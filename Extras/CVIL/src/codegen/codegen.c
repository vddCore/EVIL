#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <stdbool.h>
#include <math.h>
#include "cvil.h"
#include "cvil_ast.h"
#include "cvil_diag.h"

/* Forward parser declaration for snippet expressions in string interpolation */
extern int yyparse(void);
typedef struct yy_buffer_state* YY_BUFFER_STATE;
extern YY_BUFFER_STATE yy_scan_string(const char *str);
extern void yy_delete_buffer(YY_BUFFER_STATE b);
extern cvil_ast_node_t* g_ast_root;

/* Symbol in codegen scope */
typedef struct sym_entry {
    const char* name;
} sym_entry_t;

typedef struct sym_scope {
    struct sym_scope* parent;
    sym_entry_t* symbols;
    uint32_t count;
    uint32_t capacity;
} sym_scope_t;

/* Captured upvalue metadata for lifted function */
typedef struct uv_entry {
    const char* name;
    bool is_from_parent_uv;
    int parent_uv_idx;
} uv_entry_t;

/* Lifted function representation */
typedef struct lifted_fn {
    uint32_t id;
    const char* name;
    cvil_ast_node_t* node;
    bool is_test;
    bool is_local;
    const char* target_name;
    cvil_ast_list_t* target_chain;
    uv_entry_t* upvalues;
    uint32_t uv_count;
    uint32_t uv_capacity;
} lifted_fn_t;

typedef struct codegen_ctx {
    FILE* out;
    const char* source_filename;
    bool test_mode;
    sym_scope_t* current_scope;
    lifted_fn_t* functions;
    uint32_t fn_count;
    uint32_t fn_capacity;
    lifted_fn_t* current_fn;
    uint32_t try_stack[64];
    uint32_t try_depth;
    uint32_t next_try_id;
} codegen_ctx_t;

/* Pass 1 Lexical Scope */
typedef struct res_sym {
    const char* name;
} res_sym_t;

typedef struct res_scope {
    struct res_scope* parent;
    lifted_fn_t* fn;
    res_sym_t* syms;
    uint32_t count;
    uint32_t cap;
} res_scope_t;

static res_scope_t* res_scope_create(res_scope_t* parent, lifted_fn_t* fn) {
    res_scope_t* s = (res_scope_t*)calloc(1, sizeof(res_scope_t));
    s->parent = parent;
    s->fn = fn;
    s->cap = 16;
    s->syms = (res_sym_t*)calloc(s->cap, sizeof(res_sym_t));
    return s;
}

static void res_scope_add(res_scope_t* s, const char* name) {
    if (!s || !name) return;
    if (s->count >= s->cap) {
        s->cap *= 2;
        s->syms = (res_sym_t*)realloc(s->syms, s->cap * sizeof(res_sym_t));
    }
    s->syms[s->count++].name = strdup(name);
}

static int fn_find_upvalue(lifted_fn_t* fn, const char* name) {
    if (!fn) return -1;
    for (uint32_t i = 0; i < fn->uv_count; i++) {
        if (strcmp(fn->upvalues[i].name, name) == 0) return (int)i;
    }
    return -1;
}

static int fn_add_upvalue(lifted_fn_t* fn, const char* name, bool is_from_parent, int parent_idx) {
    if (!fn || !name) return -1;
    int existing = fn_find_upvalue(fn, name);
    if (existing >= 0) return existing;

    if (fn->uv_count >= fn->uv_capacity) {
        fn->uv_capacity = fn->uv_capacity ? fn->uv_capacity * 2 : 8;
        fn->upvalues = (uv_entry_t*)realloc(fn->upvalues, fn->uv_capacity * sizeof(uv_entry_t));
    }
    int idx = (int)fn->uv_count++;
    fn->upvalues[idx].name = strdup(name);
    fn->upvalues[idx].is_from_parent_uv = is_from_parent;
    fn->upvalues[idx].parent_uv_idx = parent_idx;
    return idx;
}

static lifted_fn_t* find_fn_by_id(codegen_ctx_t* ctx, uint32_t id) {
    for (uint32_t i = 0; i < ctx->fn_count; i++) {
        if (ctx->functions[i].id == id) return &ctx->functions[i];
    }
    return NULL;
}

static void route_upvalue(res_scope_t* start_scope, lifted_fn_t* def_fn, const char* name) {
    lifted_fn_t* chain[64];
    int depth = 0;
    res_scope_t* s = start_scope;
    while (s && s->fn != def_fn) {
        if (depth == 0 || chain[depth - 1] != s->fn) {
            chain[depth++] = s->fn;
        }
        s = s->parent;
    }
    for (int i = depth - 1; i >= 0; i--) {
        lifted_fn_t* fn = chain[i];
        if (i == depth - 1) {
            fn_add_upvalue(fn, name, false, -1);
        } else {
            lifted_fn_t* pfn = chain[i + 1];
            int p_idx = fn_find_upvalue(pfn, name);
            fn_add_upvalue(fn, name, true, p_idx);
        }
    }
}

static void pass1_resolve(codegen_ctx_t* ctx, cvil_ast_node_t* node, res_scope_t* scope, lifted_fn_t* current_fn) {
    if (!node) return;

    switch (node->kind) {
        case AST_STMT_PROGRAM:
            if (node->as.program.stmts) {
                for (uint32_t i = 0; i < node->as.program.stmts->count; i++) {
                    pass1_resolve(ctx, node->as.program.stmts->items[i], scope, current_fn);
                }
            }
            break;

        case AST_STMT_BLOCK: {
            res_scope_t* b_scope = res_scope_create(scope, current_fn);
            if (node->as.block.stmts) {
                for (uint32_t i = 0; i < node->as.block.stmts->count; i++) {
                    pass1_resolve(ctx, node->as.block.stmts->items[i], b_scope, current_fn);
                }
            }
            break;
        }

        case AST_STMT_VAL:
            for (uint32_t i = 0; i < node->as.val_stmt.count; i++) {
                if (node->as.val_stmt.decls[i].init_val) {
                    pass1_resolve(ctx, node->as.val_stmt.decls[i].init_val, scope, current_fn);
                }
                res_scope_add(scope, node->as.val_stmt.decls[i].name);
            }
            break;

        case AST_STMT_EXPR:
            pass1_resolve(ctx, node->as.expr_stmt.expr, scope, current_fn);
            break;

        case AST_STMT_IF:
            pass1_resolve(ctx, node->as.if_stmt.condition, scope, current_fn);
            pass1_resolve(ctx, node->as.if_stmt.if_branch, scope, current_fn);
            for (uint32_t i = 0; i < node->as.if_stmt.elif_count; i++) {
                pass1_resolve(ctx, node->as.if_stmt.elifs[i].condition, scope, current_fn);
                pass1_resolve(ctx, node->as.if_stmt.elifs[i].branch, scope, current_fn);
            }
            if (node->as.if_stmt.else_branch) pass1_resolve(ctx, node->as.if_stmt.else_branch, scope, current_fn);
            break;

        case AST_STMT_WHILE:
            pass1_resolve(ctx, node->as.while_stmt.condition, scope, current_fn);
            pass1_resolve(ctx, node->as.while_stmt.body, scope, current_fn);
            break;

        case AST_STMT_DO_WHILE:
            pass1_resolve(ctx, node->as.do_while_stmt.body, scope, current_fn);
            pass1_resolve(ctx, node->as.do_while_stmt.condition, scope, current_fn);
            break;

        case AST_STMT_FOR: {
            res_scope_t* f_scope = res_scope_create(scope, current_fn);
            if (node->as.for_stmt.inits) {
                for (uint32_t i = 0; i < node->as.for_stmt.inits->count; i++)
                    pass1_resolve(ctx, node->as.for_stmt.inits->items[i], f_scope, current_fn);
            }
            if (node->as.for_stmt.condition) pass1_resolve(ctx, node->as.for_stmt.condition, f_scope, current_fn);
            if (node->as.for_stmt.steps) {
                for (uint32_t i = 0; i < node->as.for_stmt.steps->count; i++)
                    pass1_resolve(ctx, node->as.for_stmt.steps->items[i], f_scope, current_fn);
            }
            pass1_resolve(ctx, node->as.for_stmt.body, f_scope, current_fn);
            break;
        }

        case AST_STMT_EACH: {
            pass1_resolve(ctx, node->as.each_stmt.collection, scope, current_fn);
            res_scope_t* e_scope = res_scope_create(scope, current_fn);
            if (node->as.each_stmt.key_var) res_scope_add(e_scope, node->as.each_stmt.key_var);
            if (node->as.each_stmt.val_var) res_scope_add(e_scope, node->as.each_stmt.val_var);
            pass1_resolve(ctx, node->as.each_stmt.body, e_scope, current_fn);
            break;
        }

        case AST_STMT_TRY: {
            pass1_resolve(ctx, node->as.try_stmt.body, scope, current_fn);
            if (node->as.try_stmt.catch_body) {
                res_scope_t* c_scope = res_scope_create(scope, current_fn);
                if (node->as.try_stmt.err_var) res_scope_add(c_scope, node->as.try_stmt.err_var);
                pass1_resolve(ctx, node->as.try_stmt.catch_body, c_scope, current_fn);
            }
            break;
        }

        case AST_STMT_THROW:
            pass1_resolve(ctx, node->as.throw_stmt.expr, scope, current_fn);
            break;

        case AST_STMT_RET:
            if (node->as.ret_stmt.expr) pass1_resolve(ctx, node->as.ret_stmt.expr, scope, current_fn);
            break;

        case AST_STMT_FN: {
            lifted_fn_t* lf = find_fn_by_id(ctx, node->as.fn_stmt.unique_fn_id);
            if (node->as.fn_stmt.is_local && node->as.fn_stmt.name) {
                res_scope_add(scope, node->as.fn_stmt.name);
            }
            res_scope_t* fn_scope = res_scope_create(scope, lf);
            for (uint32_t i = 0; i < node->as.fn_stmt.param_count; i++) {
                if (node->as.fn_stmt.params[i].default_val) {
                    pass1_resolve(ctx, node->as.fn_stmt.params[i].default_val, scope, current_fn);
                }
                res_scope_add(fn_scope, node->as.fn_stmt.params[i].name);
            }
            pass1_resolve(ctx, node->as.fn_stmt.body, fn_scope, lf);
            break;
        }

        case AST_EXPR_FN:
        case AST_EXPR_SELF_FN: {
            lifted_fn_t* lf = find_fn_by_id(ctx, node->as.fn_expr.unique_fn_id);
            res_scope_t* fn_scope = res_scope_create(scope, lf);
            for (uint32_t i = 0; i < node->as.fn_expr.param_count; i++) {
                if (node->as.fn_expr.params[i].default_val) {
                    pass1_resolve(ctx, node->as.fn_expr.params[i].default_val, scope, current_fn);
                }
                res_scope_add(fn_scope, node->as.fn_expr.params[i].name);
            }
            pass1_resolve(ctx, node->as.fn_expr.body, fn_scope, lf);
            break;
        }

        case AST_EXPR_IDENTIFIER: {
            const char* name = node->as.ident_expr.name;
            res_scope_t* s = scope;
            while (s) {
                for (int i = (int)s->count - 1; i >= 0; i--) {
                    if (strcmp(s->syms[i].name, name) == 0) {
                        if (s->fn != current_fn) {
                            route_upvalue(scope, s->fn, name);
                        }
                        return;
                    }
                }
                s = s->parent;
            }
            break;
        }

        case AST_EXPR_SELF: {
            const char* name = "self";
            res_scope_t* s = scope;
            while (s) {
                for (int i = (int)s->count - 1; i >= 0; i--) {
                    if (strcmp(s->syms[i].name, name) == 0) {
                        if (s->fn != current_fn) {
                            route_upvalue(scope, s->fn, name);
                        }
                        return;
                    }
                }
                s = s->parent;
            }
            break;
        }

        case AST_EXPR_ASSIGN:
            pass1_resolve(ctx, node->as.assign_expr.left, scope, current_fn);
            pass1_resolve(ctx, node->as.assign_expr.right, scope, current_fn);
            break;

        case AST_EXPR_CONDITIONAL:
            pass1_resolve(ctx, node->as.conditional_expr.cond, scope, current_fn);
            pass1_resolve(ctx, node->as.conditional_expr.true_expr, scope, current_fn);
            pass1_resolve(ctx, node->as.conditional_expr.false_expr, scope, current_fn);
            break;

        case AST_EXPR_COALESCING:
            pass1_resolve(ctx, node->as.coalescing_expr.left, scope, current_fn);
            pass1_resolve(ctx, node->as.coalescing_expr.right, scope, current_fn);
            break;

        case AST_EXPR_BINARY:
            pass1_resolve(ctx, node->as.binary_expr.left, scope, current_fn);
            pass1_resolve(ctx, node->as.binary_expr.right, scope, current_fn);
            break;

        case AST_EXPR_UNARY:
            pass1_resolve(ctx, node->as.unary_expr.operand, scope, current_fn);
            break;

        case AST_EXPR_INVOCATION:
            pass1_resolve(ctx, node->as.invocation_expr.callee, scope, current_fn);
            if (node->as.invocation_expr.args) {
                for (uint32_t i = 0; i < node->as.invocation_expr.args->count; i++)
                    pass1_resolve(ctx, node->as.invocation_expr.args->items[i], scope, current_fn);
            }
            break;

        case AST_EXPR_SELF_INVOCATION:
            pass1_resolve(ctx, node->as.self_invoc_expr.receiver, scope, current_fn);
            if (node->as.self_invoc_expr.args) {
                for (uint32_t i = 0; i < node->as.self_invoc_expr.args->count; i++)
                    pass1_resolve(ctx, node->as.self_invoc_expr.args->items[i], scope, current_fn);
            }
            break;

        case AST_EXPR_INDEXER:
            pass1_resolve(ctx, node->as.indexer_expr.target, scope, current_fn);
            pass1_resolve(ctx, node->as.indexer_expr.index, scope, current_fn);
            break;

        case AST_EXPR_PROPERTY:
            pass1_resolve(ctx, node->as.prop_expr.target, scope, current_fn);
            break;

        case AST_EXPR_ELVIS_PROPERTY:
        case AST_EXPR_ELVIS_INDEXER:
            pass1_resolve(ctx, node->as.elvis_expr.target, scope, current_fn);
            if (node->as.elvis_expr.index) pass1_resolve(ctx, node->as.elvis_expr.index, scope, current_fn);
            break;

        case AST_EXPR_TABLE:
            for (uint32_t i = 0; i < node->as.table_expr.count; i++) {
                if (node->as.table_expr.entries[i].key) pass1_resolve(ctx, node->as.table_expr.entries[i].key, scope, current_fn);
                pass1_resolve(ctx, node->as.table_expr.entries[i].val, scope, current_fn);
            }
            break;

        case AST_EXPR_ARRAY:
            if (node->as.array_expr.size_expr) pass1_resolve(ctx, node->as.array_expr.size_expr, scope, current_fn);
            if (node->as.array_expr.elements) {
                for (uint32_t i = 0; i < node->as.array_expr.elements->count; i++)
                    pass1_resolve(ctx, node->as.array_expr.elements->items[i], scope, current_fn);
            }
            break;

        case AST_EXPR_BY:
            pass1_resolve(ctx, node->as.by_expr.target, scope, current_fn);
            for (uint32_t i = 0; i < node->as.by_expr.arm_count; i++) {
                if (node->as.by_expr.arms[i].pattern) pass1_resolve(ctx, node->as.by_expr.arms[i].pattern, scope, current_fn);
                pass1_resolve(ctx, node->as.by_expr.arms[i].body, scope, current_fn);
            }
            break;

        case AST_EXPR_WITH:
            pass1_resolve(ctx, node->as.with_expr.base, scope, current_fn);
            pass1_resolve(ctx, node->as.with_expr.ext_table, scope, current_fn);
            break;

        case AST_EXPR_TYPEOF:
            pass1_resolve(ctx, node->as.typeof_expr.expr, scope, current_fn);
            break;

        case AST_EXPR_YIELD:
            if (node->as.yield_expr.target_expr) pass1_resolve(ctx, node->as.yield_expr.target_expr, scope, current_fn);
            if (node->as.yield_expr.args) {
                for (uint32_t i = 0; i < node->as.yield_expr.args->count; i++)
                    pass1_resolve(ctx, node->as.yield_expr.args->items[i], scope, current_fn);
            }
            break;

        case AST_EXPR_ERROR:
            if (node->as.error_expr.message) pass1_resolve(ctx, node->as.error_expr.message, scope, current_fn);
            if (node->as.error_expr.table_data) pass1_resolve(ctx, node->as.error_expr.table_data, scope, current_fn);
            break;

        default:
            break;
    }
}

/* Scope management during C code emission (Pass 2) */
static void scope_push(codegen_ctx_t* ctx) {
    sym_scope_t* s = (sym_scope_t*)calloc(1, sizeof(sym_scope_t));
    s->parent = ctx->current_scope;
    s->capacity = 16;
    s->symbols = (sym_entry_t*)calloc(s->capacity, sizeof(sym_entry_t));
    ctx->current_scope = s;
}

static void scope_pop(codegen_ctx_t* ctx) {
    if (ctx->current_scope) {
        sym_scope_t* p = ctx->current_scope->parent;
        free(ctx->current_scope->symbols);
        free(ctx->current_scope);
        ctx->current_scope = p;
    }
}

static void scope_add(codegen_ctx_t* ctx, const char* name) {
    if (!ctx->current_scope || !name) return;
    sym_scope_t* s = ctx->current_scope;
    if (s->count >= s->capacity) {
        s->capacity *= 2;
        s->symbols = (sym_entry_t*)realloc(s->symbols, s->capacity * sizeof(sym_entry_t));
    }
    s->symbols[s->count++].name = strdup(name);
}

static bool scope_lookup_local(codegen_ctx_t* ctx, const char* name) {
    sym_scope_t* s = ctx->current_scope;
    while (s) {
        for (int i = (int)s->count - 1; i >= 0; i--) {
            if (strcmp(s->symbols[i].name, name) == 0) return true;
        }
        s = s->parent;
    }
    return false;
}

/* Forward declarations */
static void emit_expr(codegen_ctx_t* ctx, cvil_ast_node_t* node);
static void emit_stmt(codegen_ctx_t* ctx, cvil_ast_node_t* node);
static void emit_block(codegen_ctx_t* ctx, cvil_ast_node_t* node);

static void emit_line(codegen_ctx_t* ctx, int line) {
    if (line > 0 && ctx->source_filename) {
        fprintf(ctx->out, "\n#line %d \"%s\"\n", line, ctx->source_filename);
    }
}

static char* escape_str(const char* s) {
    if (!s) return strdup("");
    size_t len = strlen(s);
    char* buf = (char*)malloc(len * 4 + 1);
    size_t out_len = 0;
    for (size_t i = 0; i < len; i++) {
        char c = s[i];
        if (c == '"') { buf[out_len++] = '\\'; buf[out_len++] = '"'; }
        else if (c == '\\') {
            if (s[i + 1] == '$') {
                buf[out_len++] = '$';
                i++;
            } else {
                buf[out_len++] = '\\'; buf[out_len++] = '\\';
            }
        }
        else if (c == '\n') { buf[out_len++] = '\\'; buf[out_len++] = 'n'; }
        else if (c == '\r') { buf[out_len++] = '\\'; buf[out_len++] = 'r'; }
        else if (c == '\t') { buf[out_len++] = '\\'; buf[out_len++] = 't'; }
        else { buf[out_len++] = c; }
    }
    buf[out_len] = '\0';
    return buf;
}

/* Helper to parse small expression snippet */
static cvil_ast_node_t* parse_snippet(const char* expr_str) {
    char buf[2048];
    snprintf(buf, sizeof(buf), "%s;\n", expr_str);
    YY_BUFFER_STATE b = yy_scan_string(buf);
    g_ast_root = NULL;
    yyparse();
    yy_delete_buffer(b);
    if (g_ast_root && g_ast_root->as.program.stmts && g_ast_root->as.program.stmts->count > 0) {
        cvil_ast_node_t* stmt = g_ast_root->as.program.stmts->items[0];
        if (stmt->kind == AST_STMT_EXPR) return stmt->as.expr_stmt.expr;
    }
    return NULL;
}

/* Emitting string interpolation */
static void emit_interpolated_string(codegen_ctx_t* ctx, const char* str) {
    fprintf(ctx->out, "({ evil_strbuilder_t* _sb = evil_strbuilder_create(); ");
    const char* p = str;
    while (*p) {
        if (*p == '\\' && *(p + 1) == '$') {
            fprintf(ctx->out, "evil_strbuilder_append_str(_sb, \"$\"); ");
            p += 2;
        } else if (*p == '$') {
            if (*(p + 1) == '$') {
                fprintf(ctx->out, "evil_strbuilder_append_str(_sb, \"$\"); ");
                p += 2;
            } else if (*(p + 1) == '{') {
                p += 2;
                const char* start = p;
                int braces = 1;
                while (*p && braces > 0) {
                    if (*p == '{') braces++;
                    else if (*p == '}') braces--;
                    p++;
                }
                int expr_len = (int)(p - 1 - start);
                char expr_buf[1024];
                if (expr_len < (int)sizeof(expr_buf)) {
                    strncpy(expr_buf, start, expr_len);
                    expr_buf[expr_len] = '\0';
                    cvil_ast_node_t* en = parse_snippet(expr_buf);
                    if (en) {
                        fprintf(ctx->out, "evil_strbuilder_append_val(_sb, ");
                        emit_expr(ctx, en);
                        fprintf(ctx->out, "); ");
                    }
                }
            } else if (isalpha(*(p + 1)) || *(p + 1) == '_') {
                p++;
                const char* start = p;
                while (isalnum(*p) || *p == '_') p++;
                int id_len = (int)(p - start);
                char id_buf[256];
                strncpy(id_buf, start, id_len);
                id_buf[id_len] = '\0';

                int uv_idx = fn_find_upvalue(ctx->current_fn, id_buf);
                fprintf(ctx->out, "evil_strbuilder_append_val(_sb, ");
                if (scope_lookup_local(ctx, id_buf)) {
                    fprintf(ctx->out, "(*_uv_%s->val_ptr)", id_buf);
                } else if (uv_idx >= 0) {
                    fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr)", uv_idx);
                } else {
                    fprintf(ctx->out, "evil_runtime_get_global(g_rt, \"%s\")", id_buf);
                }
                fprintf(ctx->out, "); ");
            } else {
                fprintf(ctx->out, "evil_strbuilder_append_str(_sb, \"$\"); ");
                p++;
            }
        } else {
            const char* start = p;
            while (*p && *p != '$' && !(*p == '\\' && *(p + 1) == '$')) {
                if (*p == '\\' && *(p + 1)) p += 2;
                else p++;
            }
            int seg_len = (int)(p - start);
            char seg_buf[1024];
            if (seg_len < (int)sizeof(seg_buf)) {
                strncpy(seg_buf, start, seg_len);
                seg_buf[seg_len] = '\0';
                char* esc = escape_str(seg_buf);
                fprintf(ctx->out, "evil_strbuilder_append_str(_sb, \"%s\"); ", esc);
                free(esc);
            }
        }
    }
    fprintf(ctx->out, "evil_strbuilder_finish(_sb); })");
}

/* Collect lifted functions from AST */
static void collect_functions(codegen_ctx_t* ctx, cvil_ast_node_t* node) {
    if (!node) return;

    if (node->kind == AST_STMT_FN) {
        if (ctx->fn_count >= ctx->fn_capacity) {
            ctx->fn_capacity = ctx->fn_capacity ? ctx->fn_capacity * 2 : 16;
            ctx->functions = (lifted_fn_t*)realloc(ctx->functions, ctx->fn_capacity * sizeof(lifted_fn_t));
        }
        lifted_fn_t* lf = &ctx->functions[ctx->fn_count++];
        memset(lf, 0, sizeof(lifted_fn_t));
        lf->id = node->as.fn_stmt.unique_fn_id;
        lf->name = node->as.fn_stmt.name;
        lf->node = node;
        lf->is_local = node->as.fn_stmt.is_local;
        lf->target_name = node->as.fn_stmt.target_name;
        lf->target_chain = node->as.fn_stmt.target_chain;
        lf->is_test = false;
        for (uint32_t i = 0; i < node->as.fn_stmt.attr_count; i++) {
            if (strcmp(node->as.fn_stmt.attrs[i].name, "test") == 0) {
                lf->is_test = true;
                break;
            }
        }
    } else if (node->kind == AST_EXPR_FN || node->kind == AST_EXPR_SELF_FN) {
        if (ctx->fn_count >= ctx->fn_capacity) {
            ctx->fn_capacity = ctx->fn_capacity ? ctx->fn_capacity * 2 : 16;
            ctx->functions = (lifted_fn_t*)realloc(ctx->functions, ctx->fn_capacity * sizeof(lifted_fn_t));
        }
        lifted_fn_t* lf = &ctx->functions[ctx->fn_count++];
        memset(lf, 0, sizeof(lifted_fn_t));
        lf->id = node->as.fn_expr.unique_fn_id;
        lf->name = NULL;
        lf->node = node;
        lf->is_local = true;
        lf->is_test = false;
    }

    /* Recurse */
    switch (node->kind) {
        case AST_STMT_PROGRAM:
            if (node->as.program.stmts) {
                for (uint32_t i = 0; i < node->as.program.stmts->count; i++) {
                    collect_functions(ctx, node->as.program.stmts->items[i]);
                }
            }
            break;
        case AST_STMT_BLOCK:
            if (node->as.block.stmts) {
                for (uint32_t i = 0; i < node->as.block.stmts->count; i++) {
                    collect_functions(ctx, node->as.block.stmts->items[i]);
                }
            }
            break;
        case AST_STMT_EXPR:
            collect_functions(ctx, node->as.expr_stmt.expr);
            break;
        case AST_STMT_VAL:
            for (uint32_t i = 0; i < node->as.val_stmt.count; i++) {
                if (node->as.val_stmt.decls[i].init_val) {
                    collect_functions(ctx, node->as.val_stmt.decls[i].init_val);
                }
            }
            break;
        case AST_STMT_IF:
            collect_functions(ctx, node->as.if_stmt.condition);
            collect_functions(ctx, node->as.if_stmt.if_branch);
            for (uint32_t i = 0; i < node->as.if_stmt.elif_count; i++) {
                collect_functions(ctx, node->as.if_stmt.elifs[i].condition);
                collect_functions(ctx, node->as.if_stmt.elifs[i].branch);
            }
            if (node->as.if_stmt.else_branch) collect_functions(ctx, node->as.if_stmt.else_branch);
            break;
        case AST_STMT_WHILE:
            collect_functions(ctx, node->as.while_stmt.condition);
            collect_functions(ctx, node->as.while_stmt.body);
            break;
        case AST_STMT_DO_WHILE:
            collect_functions(ctx, node->as.do_while_stmt.body);
            collect_functions(ctx, node->as.do_while_stmt.condition);
            break;
        case AST_STMT_FOR:
            if (node->as.for_stmt.inits) {
                for (uint32_t i = 0; i < node->as.for_stmt.inits->count; i++)
                    collect_functions(ctx, node->as.for_stmt.inits->items[i]);
            }
            if (node->as.for_stmt.condition) collect_functions(ctx, node->as.for_stmt.condition);
            if (node->as.for_stmt.steps) {
                for (uint32_t i = 0; i < node->as.for_stmt.steps->count; i++)
                    collect_functions(ctx, node->as.for_stmt.steps->items[i]);
            }
            collect_functions(ctx, node->as.for_stmt.body);
            break;
        case AST_STMT_EACH:
            collect_functions(ctx, node->as.each_stmt.collection);
            collect_functions(ctx, node->as.each_stmt.body);
            break;
        case AST_STMT_TRY:
            collect_functions(ctx, node->as.try_stmt.body);
            if (node->as.try_stmt.catch_body) collect_functions(ctx, node->as.try_stmt.catch_body);
            break;
        case AST_STMT_THROW:
            collect_functions(ctx, node->as.throw_stmt.expr);
            break;
        case AST_STMT_RET:
            if (node->as.ret_stmt.expr) collect_functions(ctx, node->as.ret_stmt.expr);
            break;
        case AST_STMT_FN:
            collect_functions(ctx, node->as.fn_stmt.body);
            break;
        case AST_EXPR_FN:
        case AST_EXPR_SELF_FN:
            collect_functions(ctx, node->as.fn_expr.body);
            break;
        case AST_EXPR_ASSIGN:
            collect_functions(ctx, node->as.assign_expr.left);
            collect_functions(ctx, node->as.assign_expr.right);
            break;
        case AST_EXPR_CONDITIONAL:
            collect_functions(ctx, node->as.conditional_expr.cond);
            collect_functions(ctx, node->as.conditional_expr.true_expr);
            collect_functions(ctx, node->as.conditional_expr.false_expr);
            break;
        case AST_EXPR_COALESCING:
            collect_functions(ctx, node->as.coalescing_expr.left);
            collect_functions(ctx, node->as.coalescing_expr.right);
            break;
        case AST_EXPR_BINARY:
            collect_functions(ctx, node->as.binary_expr.left);
            collect_functions(ctx, node->as.binary_expr.right);
            break;
        case AST_EXPR_UNARY:
            collect_functions(ctx, node->as.unary_expr.operand);
            break;
        case AST_EXPR_INVOCATION:
            collect_functions(ctx, node->as.invocation_expr.callee);
            if (node->as.invocation_expr.args) {
                for (uint32_t i = 0; i < node->as.invocation_expr.args->count; i++)
                    collect_functions(ctx, node->as.invocation_expr.args->items[i]);
            }
            break;
        case AST_EXPR_SELF_INVOCATION:
            collect_functions(ctx, node->as.self_invoc_expr.receiver);
            if (node->as.self_invoc_expr.args) {
                for (uint32_t i = 0; i < node->as.self_invoc_expr.args->count; i++)
                    collect_functions(ctx, node->as.self_invoc_expr.args->items[i]);
            }
            break;
        case AST_EXPR_INDEXER:
            collect_functions(ctx, node->as.indexer_expr.target);
            collect_functions(ctx, node->as.indexer_expr.index);
            break;
        case AST_EXPR_PROPERTY:
            collect_functions(ctx, node->as.prop_expr.target);
            break;
        case AST_EXPR_ELVIS_PROPERTY:
        case AST_EXPR_ELVIS_INDEXER:
            collect_functions(ctx, node->as.elvis_expr.target);
            if (node->as.elvis_expr.index) collect_functions(ctx, node->as.elvis_expr.index);
            break;
        case AST_EXPR_TABLE:
            for (uint32_t i = 0; i < node->as.table_expr.count; i++) {
                if (node->as.table_expr.entries[i].key) collect_functions(ctx, node->as.table_expr.entries[i].key);
                collect_functions(ctx, node->as.table_expr.entries[i].val);
            }
            break;
        case AST_EXPR_ARRAY:
            if (node->as.array_expr.size_expr) collect_functions(ctx, node->as.array_expr.size_expr);
            if (node->as.array_expr.elements) {
                for (uint32_t i = 0; i < node->as.array_expr.elements->count; i++)
                    collect_functions(ctx, node->as.array_expr.elements->items[i]);
            }
            break;
        case AST_EXPR_BY:
            collect_functions(ctx, node->as.by_expr.target);
            for (uint32_t i = 0; i < node->as.by_expr.arm_count; i++) {
                if (node->as.by_expr.arms[i].pattern) collect_functions(ctx, node->as.by_expr.arms[i].pattern);
                collect_functions(ctx, node->as.by_expr.arms[i].body);
            }
            break;
        case AST_EXPR_WITH:
            collect_functions(ctx, node->as.with_expr.base);
            collect_functions(ctx, node->as.with_expr.ext_table);
            break;
        case AST_EXPR_TYPEOF:
            collect_functions(ctx, node->as.typeof_expr.expr);
            break;
        case AST_EXPR_YIELD:
            if (node->as.yield_expr.target_expr) collect_functions(ctx, node->as.yield_expr.target_expr);
            if (node->as.yield_expr.args) {
                for (uint32_t i = 0; i < node->as.yield_expr.args->count; i++)
                    collect_functions(ctx, node->as.yield_expr.args->items[i]);
            }
            break;
        case AST_EXPR_ERROR:
            if (node->as.error_expr.message) collect_functions(ctx, node->as.error_expr.message);
            if (node->as.error_expr.table_data) collect_functions(ctx, node->as.error_expr.table_data);
            break;
        default:
            break;
    }
}

/* Expression Code Generation */
static void emit_expr(codegen_ctx_t* ctx, cvil_ast_node_t* node) {
    if (!node) {
        fprintf(ctx->out, "evil_val_nil()");
        return;
    }

    switch (node->kind) {
        case AST_EXPR_LITERAL: {
            switch (node->as.literal_expr.lit_type) {
                case EVIL_TYPE_NIL:
                    fprintf(ctx->out, "evil_val_nil()");
                    break;
                case EVIL_TYPE_NUMBER:
                    if (node->as.literal_expr.is_hex) {
                        fprintf(ctx->out, "evil_val_number(%d)", (int)node->as.literal_expr.v.number);
                    } else if (isnan(node->as.literal_expr.v.number)) {
                        fprintf(ctx->out, "evil_val_number(NAN)");
                    } else if (isinf(node->as.literal_expr.v.number)) {
                        fprintf(ctx->out, "evil_val_number(INFINITY)");
                    } else {
                        fprintf(ctx->out, "evil_val_number(%.17g)", node->as.literal_expr.v.number);
                    }
                    break;
                case EVIL_TYPE_BOOLEAN:
                    fprintf(ctx->out, "evil_val_bool(%s)", node->as.literal_expr.v.boolean ? "true" : "false");
                    break;
                case EVIL_TYPE_STRING: {
                    if (node->as.literal_expr.is_interpolated) {
                        emit_interpolated_string(ctx, node->as.literal_expr.v.string_val);
                    } else {
                        char* esc = escape_str(node->as.literal_expr.v.string_val);
                        fprintf(ctx->out, "evil_str_intern(\"%s\")", esc);
                        free(esc);
                    }
                    break;
                }
                case EVIL_TYPE_TYPE:
                    fprintf(ctx->out, "evil_val_type_code(%d)", node->as.literal_expr.v.type_code);
                    break;
                default:
                    fprintf(ctx->out, "evil_val_nil()");
                    break;
            }
            break;
        }

        case AST_EXPR_IDENTIFIER: {
            const char* name = node->as.ident_expr.name;
            int uv_idx = fn_find_upvalue(ctx->current_fn, name);
            if (scope_lookup_local(ctx, name)) {
                fprintf(ctx->out, "(*_uv_%s->val_ptr)", name);
            } else if (uv_idx >= 0) {
                fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr)", uv_idx);
            } else {
                fprintf(ctx->out, "evil_runtime_get_global(g_rt, \"%s\")", name);
            }
            break;
        }

        case AST_EXPR_SELF: {
            int uv_idx = fn_find_upvalue(ctx->current_fn, "self");
            if (scope_lookup_local(ctx, "self")) {
                fprintf(ctx->out, "(*_uv_self->val_ptr)");
            } else if (uv_idx >= 0) {
                fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr)", uv_idx);
            } else {
                fprintf(ctx->out, "self");
            }
            break;
        }

        case AST_EXPR_ELLIPSIS:
            fprintf(ctx->out, "_varargs");
            break;

        case AST_EXPR_ASSIGN: {
            cvil_ast_node_t* left = node->as.assign_expr.left;
            cvil_ast_node_t* right = node->as.assign_expr.right;
            cvil_assign_op_t op = node->as.assign_expr.op;

            if (left->kind == AST_EXPR_IDENTIFIER) {
                const char* name = left->as.ident_expr.name;
                bool is_local = scope_lookup_local(ctx, name);
                int uv_idx = is_local ? -1 : fn_find_upvalue(ctx->current_fn, name);

                if (op == ASSIGN_EQ) {
                    if (is_local) {
                        fprintf(ctx->out, "(*(_uv_%s = evil_upvalue_create(", name);
                        emit_expr(ctx, right);
                        fprintf(ctx->out, "))->val_ptr)");
                    } else if (uv_idx >= 0) {
                        fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr = ", uv_idx);
                        emit_expr(ctx, right);
                        fprintf(ctx->out, ")");
                    } else {
                        fprintf(ctx->out, "evil_runtime_set_global(g_rt, \"%s\", ", name);
                        emit_expr(ctx, right);
                        fprintf(ctx->out, ")");
                    }
                } else {
                    /* Compound assignment */
                    const char* fn_name = "evil_val_add";
                    switch (op) {
                        case ASSIGN_ADD: fn_name = "evil_val_add"; break;
                        case ASSIGN_SUB: fn_name = "evil_val_sub"; break;
                        case ASSIGN_MUL: fn_name = "evil_val_mul"; break;
                        case ASSIGN_DIV: fn_name = "evil_val_div"; break;
                        case ASSIGN_MOD: fn_name = "evil_val_mod"; break;
                        case ASSIGN_BAND: fn_name = "evil_val_band"; break;
                        case ASSIGN_BOR:  fn_name = "evil_val_bor"; break;
                        case ASSIGN_BXOR: fn_name = "evil_val_bxor"; break;
                        case ASSIGN_SHL:  fn_name = "evil_val_shl"; break;
                        case ASSIGN_SHR:  fn_name = "evil_val_shr"; break;
                        default: break;
                    }
                    if (op == ASSIGN_COALESCE) {
                        if (is_local) {
                            fprintf(ctx->out, "(evil_is_nil(*_uv_%s->val_ptr) ? (*(_uv_%s = evil_upvalue_create(", name, name);
                            emit_expr(ctx, right);
                            fprintf(ctx->out, "))->val_ptr) : *_uv_%s->val_ptr)", name);
                        } else if (uv_idx >= 0) {
                            fprintf(ctx->out, "(evil_is_nil(*_cur_closure->upvalues[%d]->val_ptr) ? (*_cur_closure->upvalues[%d]->val_ptr = ", uv_idx, uv_idx);
                            emit_expr(ctx, right);
                            fprintf(ctx->out, ") : *_cur_closure->upvalues[%d]->val_ptr)", uv_idx);
                        } else {
                            fprintf(ctx->out, "({ evil_value_t _cur = evil_runtime_get_global(g_rt, \"%s\"); if (evil_is_nil(_cur)) { evil_value_t _nv = ", name);
                            emit_expr(ctx, right);
                            fprintf(ctx->out, "; evil_runtime_set_global(g_rt, \"%s\", _nv); _cur = _nv; } _cur; })", name);
                        }
                    } else {
                        if (is_local) {
                            fprintf(ctx->out, "(*(_uv_%s = evil_upvalue_create(%s(fiber, *_uv_%s->val_ptr, ", name, fn_name, name);
                            emit_expr(ctx, right);
                            fprintf(ctx->out, ")))->val_ptr)");
                        } else if (uv_idx >= 0) {
                            fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr = %s(fiber, *_cur_closure->upvalues[%d]->val_ptr, ", uv_idx, fn_name, uv_idx);
                            emit_expr(ctx, right);
                            fprintf(ctx->out, "))");
                        } else {
                            fprintf(ctx->out, "({ evil_value_t _v = %s(fiber, evil_runtime_get_global(g_rt, \"%s\"), ", fn_name, name);
                            emit_expr(ctx, right);
                            fprintf(ctx->out, "); evil_runtime_set_global(g_rt, \"%s\", _v); _v; })", name);
                        }
                    }
                }
            } else if (left->kind == AST_EXPR_PROPERTY) {
                const char* prop = left->as.prop_expr.property;
                if (op == ASSIGN_EQ) {
                    fprintf(ctx->out, "({ evil_value_t _t = ");
                    emit_expr(ctx, left->as.prop_expr.target);
                    fprintf(ctx->out, "; evil_value_t _v = ");
                    emit_expr(ctx, right);
                    fprintf(ctx->out, "; evil_table_set_str(_t, \"%s\", _v); })", prop);
                } else if (op == ASSIGN_COALESCE) {
                    fprintf(ctx->out, "({ evil_value_t _t = ");
                    emit_expr(ctx, left->as.prop_expr.target);
                    fprintf(ctx->out, "; evil_value_t _v = evil_table_get_str(_t, \"%s\"); if (evil_is_nil(_v)) { _v = ", prop);
                    emit_expr(ctx, right);
                    fprintf(ctx->out, "; evil_table_set_str(_t, \"%s\", _v); } _v; })", prop);
                } else {
                    const char* fn_name = "evil_val_add";
                    switch (op) {
                        case ASSIGN_ADD: fn_name = "evil_val_add"; break;
                        case ASSIGN_SUB: fn_name = "evil_val_sub"; break;
                        case ASSIGN_MUL: fn_name = "evil_val_mul"; break;
                        case ASSIGN_DIV: fn_name = "evil_val_div"; break;
                        case ASSIGN_MOD: fn_name = "evil_val_mod"; break;
                        case ASSIGN_BAND: fn_name = "evil_val_band"; break;
                        case ASSIGN_BOR:  fn_name = "evil_val_bor"; break;
                        case ASSIGN_BXOR: fn_name = "evil_val_bxor"; break;
                        case ASSIGN_SHL:  fn_name = "evil_val_shl"; break;
                        case ASSIGN_SHR:  fn_name = "evil_val_shr"; break;
                        default: break;
                    }
                    fprintf(ctx->out, "({ evil_value_t _t = ");
                    emit_expr(ctx, left->as.prop_expr.target);
                    fprintf(ctx->out, "; evil_value_t _old = evil_table_get_str(_t, \"%s\"); evil_value_t _nv = %s(fiber, _old, ", prop, fn_name);
                    emit_expr(ctx, right);
                    fprintf(ctx->out, "); evil_table_set_str(_t, \"%s\", _nv); _nv; })", prop);
                }
            } else if (left->kind == AST_EXPR_INDEXER) {
                if (op == ASSIGN_EQ) {
                    fprintf(ctx->out, "({ evil_value_t _t = ");
                    emit_expr(ctx, left->as.indexer_expr.target);
                    fprintf(ctx->out, "; evil_value_t _i = ");
                    emit_expr(ctx, left->as.indexer_expr.index);
                    fprintf(ctx->out, "; evil_value_t _v = ");
                    emit_expr(ctx, right);
                    fprintf(ctx->out, "; evil_index_set(fiber, _t, _i, _v); })");
                } else if (op == ASSIGN_COALESCE) {
                    fprintf(ctx->out, "({ evil_value_t _t = ");
                    emit_expr(ctx, left->as.indexer_expr.target);
                    fprintf(ctx->out, "; evil_value_t _i = ");
                    emit_expr(ctx, left->as.indexer_expr.index);
                    fprintf(ctx->out, "; evil_value_t _v = evil_index_get(fiber, _t, _i); if (evil_is_nil(_v)) { _v = ");
                    emit_expr(ctx, right);
                    fprintf(ctx->out, "; evil_index_set(fiber, _t, _i, _v); } _v; })");
                } else {
                    const char* fn_name = "evil_val_add";
                    switch (op) {
                        case ASSIGN_ADD: fn_name = "evil_val_add"; break;
                        case ASSIGN_SUB: fn_name = "evil_val_sub"; break;
                        case ASSIGN_MUL: fn_name = "evil_val_mul"; break;
                        case ASSIGN_DIV: fn_name = "evil_val_div"; break;
                        case ASSIGN_MOD: fn_name = "evil_val_mod"; break;
                        case ASSIGN_BAND: fn_name = "evil_val_band"; break;
                        case ASSIGN_BOR:  fn_name = "evil_val_bor"; break;
                        case ASSIGN_BXOR: fn_name = "evil_val_bxor"; break;
                        case ASSIGN_SHL:  fn_name = "evil_val_shl"; break;
                        case ASSIGN_SHR:  fn_name = "evil_val_shr"; break;
                        default: break;
                    }
                    fprintf(ctx->out, "({ evil_value_t _t = ");
                    emit_expr(ctx, left->as.indexer_expr.target);
                    fprintf(ctx->out, "; evil_value_t _i = ");
                    emit_expr(ctx, left->as.indexer_expr.index);
                    fprintf(ctx->out, "; evil_value_t _old = evil_index_get(fiber, _t, _i); evil_value_t _nv = %s(fiber, _old, ", fn_name);
                    emit_expr(ctx, right);
                    fprintf(ctx->out, "); evil_index_set(fiber, _t, _i, _nv); _nv; })");
                }
            }
            break;
        }

        case AST_EXPR_CONDITIONAL:
            fprintf(ctx->out, "({ evil_value_t _c = ");
            emit_expr(ctx, node->as.conditional_expr.cond);
            fprintf(ctx->out, "; evil_is_truth(_c) ? (");
            emit_expr(ctx, node->as.conditional_expr.true_expr);
            fprintf(ctx->out, ") : (");
            emit_expr(ctx, node->as.conditional_expr.false_expr);
            fprintf(ctx->out, "); })");
            break;

        case AST_STMT_THROW:
            fprintf(ctx->out, "({ evil_throw(fiber, ");
            emit_expr(ctx, node->as.throw_stmt.expr);
            fprintf(ctx->out, "); evil_val_nil(); })");
            break;

        case AST_EXPR_COALESCING:
            fprintf(ctx->out, "({ evil_value_t _l = ");
            emit_expr(ctx, node->as.coalescing_expr.left);
            fprintf(ctx->out, "; !evil_is_nil(_l) ? _l : (");
            if (node->as.coalescing_expr.is_throw) {
                fprintf(ctx->out, "({ evil_throw(fiber, ");
                emit_expr(ctx, node->as.coalescing_expr.right->kind == AST_STMT_THROW ? node->as.coalescing_expr.right->as.throw_stmt.expr : node->as.coalescing_expr.right);
                fprintf(ctx->out, "); evil_val_nil(); })");
            } else {
                emit_expr(ctx, node->as.coalescing_expr.right);
            }
            fprintf(ctx->out, "); })");
            break;

        case AST_EXPR_BINARY: {
            cvil_bin_op_t op = node->as.binary_expr.op;
            if (op == BIN_AND) {
                fprintf(ctx->out, "({ evil_value_t _l = ");
                emit_expr(ctx, node->as.binary_expr.left);
                fprintf(ctx->out, "; (_l.type == EVIL_TYPE_TABLE && (_l.as.table->metatable && (_l.as.table->metatable->metamethod_flags & META_LAND))) ? evil_val_land(fiber, _l, ");
                emit_expr(ctx, node->as.binary_expr.right);
                fprintf(ctx->out, ") : (evil_is_truth(_l) ? (");
                emit_expr(ctx, node->as.binary_expr.right);
                fprintf(ctx->out, ") : _l); })");
            } else if (op == BIN_OR) {
                fprintf(ctx->out, "({ evil_value_t _l = ");
                emit_expr(ctx, node->as.binary_expr.left);
                fprintf(ctx->out, "; (_l.type == EVIL_TYPE_TABLE && (_l.as.table->metatable && (_l.as.table->metatable->metamethod_flags & META_LOR))) ? evil_val_lor(fiber, _l, ");
                emit_expr(ctx, node->as.binary_expr.right);
                fprintf(ctx->out, ") : (evil_is_truth(_l) ? _l : (");
                emit_expr(ctx, node->as.binary_expr.right);
                fprintf(ctx->out, ")); })");
            } else if (op == BIN_IS || op == BIN_IS_NOT) {
                fprintf(ctx->out, "evil_val_bool(%sevil_val_is_type(", op == BIN_IS_NOT ? "!" : "");
                emit_expr(ctx, node->as.binary_expr.left);
                fprintf(ctx->out, ", %d, ", node->as.binary_expr.is_type_target);
                if (node->as.binary_expr.right) {
                    fprintf(ctx->out, "evil_str_chars(");
                    emit_expr(ctx, node->as.binary_expr.right);
                    fprintf(ctx->out, ")");
                } else {
                    fprintf(ctx->out, "NULL");
                }
                fprintf(ctx->out, "))");
            } else {
                const char* fn_name = "evil_val_add";
                switch (op) {
                    case BIN_ADD: fn_name = "evil_val_add"; break;
                    case BIN_SUB: fn_name = "evil_val_sub"; break;
                    case BIN_MUL: fn_name = "evil_val_mul"; break;
                    case BIN_DIV: fn_name = "evil_val_div"; break;
                    case BIN_MOD: fn_name = "evil_val_mod"; break;
                    case BIN_EQ:  fn_name = "evil_val_eq"; break;
                    case BIN_NE:  fn_name = "evil_val_ne"; break;
                    case BIN_DEQ: fn_name = "evil_val_deq"; break;
                    case BIN_DNE: fn_name = "evil_val_dne"; break;
                    case BIN_LT:  fn_name = "evil_val_lt"; break;
                    case BIN_LTE: fn_name = "evil_val_lte"; break;
                    case BIN_GT:  fn_name = "evil_val_gt"; break;
                    case BIN_GTE: fn_name = "evil_val_gte"; break;
                    case BIN_IN:  fn_name = "evil_val_in"; break;
                    case BIN_NOT_IN: fn_name = "evil_val_in"; break;
                    case BIN_BAND: fn_name = "evil_val_band"; break;
                    case BIN_BOR:  fn_name = "evil_val_bor"; break;
                    case BIN_BXOR: fn_name = "evil_val_bxor"; break;
                    case BIN_SHL:  fn_name = "evil_val_shl"; break;
                    case BIN_SHR:  fn_name = "evil_val_shr"; break;
                    default: break;
                }
                if (op == BIN_NOT_IN) {
                    fprintf(ctx->out, "evil_val_bool(!evil_is_truth(%s(fiber, ", fn_name);
                    emit_expr(ctx, node->as.binary_expr.left);
                    fprintf(ctx->out, ", ");
                    emit_expr(ctx, node->as.binary_expr.right);
                    fprintf(ctx->out, ")))");
                } else {
                    fprintf(ctx->out, "%s(fiber, ", fn_name);
                    emit_expr(ctx, node->as.binary_expr.left);
                    fprintf(ctx->out, ", ");
                    emit_expr(ctx, node->as.binary_expr.right);
                    fprintf(ctx->out, ")");
                }
            }
            break;
        }

        case AST_EXPR_UNARY: {
            cvil_un_op_t op = node->as.unary_expr.op;
            cvil_ast_node_t* operand = node->as.unary_expr.operand;

            switch (op) {
                case UN_PLUS:
                    emit_expr(ctx, operand);
                    break;
                case UN_MINUS:
                    fprintf(ctx->out, "evil_val_neg(fiber, ");
                    emit_expr(ctx, operand);
                    fprintf(ctx->out, ")");
                    break;
                case UN_NOT:
                    fprintf(ctx->out, "evil_val_lnot(fiber, ");
                    emit_expr(ctx, operand);
                    fprintf(ctx->out, ")");
                    break;
                case UN_BNOT:
                    fprintf(ctx->out, "evil_val_bnot(fiber, ");
                    emit_expr(ctx, operand);
                    fprintf(ctx->out, ")");
                    break;
                case UN_LEN:
                    fprintf(ctx->out, "evil_val_len(fiber, ");
                    emit_expr(ctx, operand);
                    fprintf(ctx->out, ")");
                    break;
                case UN_TOSTR:
                    fprintf(ctx->out, "evil_val_tostr(fiber, ");
                    emit_expr(ctx, operand);
                    fprintf(ctx->out, ")");
                    break;
                case UN_TONUM:
                    fprintf(ctx->out, "evil_val_tonum(fiber, ");
                    emit_expr(ctx, operand);
                    fprintf(ctx->out, ")");
                    break;
                case UN_PRE_INC:
                    if (operand->kind == AST_EXPR_IDENTIFIER) {
                        const char* n = operand->as.ident_expr.name;
                        bool is_local = scope_lookup_local(ctx, n);
                        int uv_idx = is_local ? -1 : fn_find_upvalue(ctx->current_fn, n);
                        if (is_local) {
                            fprintf(ctx->out, "(*(_uv_%s = evil_upvalue_create(evil_val_inc(fiber, *_uv_%s->val_ptr)))->val_ptr)", n, n);
                        } else if (uv_idx >= 0) {
                            fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr = evil_val_inc(fiber, *_cur_closure->upvalues[%d]->val_ptr))", uv_idx, uv_idx);
                        } else {
                            fprintf(ctx->out, "({ evil_value_t _v = evil_val_inc(fiber, evil_runtime_get_global(g_rt, \"%s\")); evil_runtime_set_global(g_rt, \"%s\", _v); _v; })", n, n);
                        }
                    } else if (operand->kind == AST_EXPR_PROPERTY) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.prop_expr.target);
                        fprintf(ctx->out, "; evil_value_t _nv = evil_val_inc(fiber, evil_table_get_str(_t, \"%s\")); evil_table_set_str(_t, \"%s\", _nv); _nv; })", operand->as.prop_expr.property, operand->as.prop_expr.property);
                    } else if (operand->kind == AST_EXPR_INDEXER) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.indexer_expr.target);
                        fprintf(ctx->out, "; evil_value_t _i = ");
                        emit_expr(ctx, operand->as.indexer_expr.index);
                        fprintf(ctx->out, "; evil_value_t _nv = evil_val_inc(fiber, evil_index_get(fiber, _t, _i)); evil_index_set(fiber, _t, _i, _nv); _nv; })");
                    }
                    break;
                case UN_PRE_DEC:
                    if (operand->kind == AST_EXPR_IDENTIFIER) {
                        const char* n = operand->as.ident_expr.name;
                        bool is_local = scope_lookup_local(ctx, n);
                        int uv_idx = is_local ? -1 : fn_find_upvalue(ctx->current_fn, n);
                        if (is_local) {
                            fprintf(ctx->out, "(*(_uv_%s = evil_upvalue_create(evil_val_dec(fiber, *_uv_%s->val_ptr)))->val_ptr)", n, n);
                        } else if (uv_idx >= 0) {
                            fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr = evil_val_dec(fiber, *_cur_closure->upvalues[%d]->val_ptr))", uv_idx, uv_idx);
                        } else {
                            fprintf(ctx->out, "({ evil_value_t _v = evil_val_dec(fiber, evil_runtime_get_global(g_rt, \"%s\")); evil_runtime_set_global(g_rt, \"%s\", _v); _v; })", n, n);
                        }
                    } else if (operand->kind == AST_EXPR_PROPERTY) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.prop_expr.target);
                        fprintf(ctx->out, "; evil_value_t _nv = evil_val_dec(fiber, evil_table_get_str(_t, \"%s\")); evil_table_set_str(_t, \"%s\", _nv); _nv; })", operand->as.prop_expr.property, operand->as.prop_expr.property);
                    } else if (operand->kind == AST_EXPR_INDEXER) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.indexer_expr.target);
                        fprintf(ctx->out, "; evil_value_t _i = ");
                        emit_expr(ctx, operand->as.indexer_expr.index);
                        fprintf(ctx->out, "; evil_value_t _nv = evil_val_dec(fiber, evil_index_get(fiber, _t, _i)); evil_index_set(fiber, _t, _i, _nv); _nv; })");
                    }
                    break;
                case UN_POST_INC:
                    if (operand->kind == AST_EXPR_IDENTIFIER) {
                        const char* n = operand->as.ident_expr.name;
                        bool is_local = scope_lookup_local(ctx, n);
                        int uv_idx = is_local ? -1 : fn_find_upvalue(ctx->current_fn, n);
                        if (is_local) {
                            fprintf(ctx->out, "({ evil_value_t _old = *_uv_%s->val_ptr; _uv_%s = evil_upvalue_create(evil_val_inc(fiber, _old)); _old; })", n, n);
                        } else if (uv_idx >= 0) {
                            fprintf(ctx->out, "({ evil_value_t _old = *_cur_closure->upvalues[%d]->val_ptr; *_cur_closure->upvalues[%d]->val_ptr = evil_val_inc(fiber, _old); _old; })", uv_idx, uv_idx);
                        } else {
                            fprintf(ctx->out, "({ evil_value_t _old = evil_runtime_get_global(g_rt, \"%s\"); evil_runtime_set_global(g_rt, \"%s\", evil_val_inc(fiber, _old)); _old; })", n, n);
                        }
                    } else if (operand->kind == AST_EXPR_PROPERTY) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.prop_expr.target);
                        fprintf(ctx->out, "; evil_value_t _old = evil_table_get_str(_t, \"%s\"); evil_table_set_str(_t, \"%s\", evil_val_inc(fiber, _old)); _old; })", operand->as.prop_expr.property, operand->as.prop_expr.property);
                    } else if (operand->kind == AST_EXPR_INDEXER) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.indexer_expr.target);
                        fprintf(ctx->out, "; evil_value_t _i = ");
                        emit_expr(ctx, operand->as.indexer_expr.index);
                        fprintf(ctx->out, "; evil_value_t _old = evil_index_get(fiber, _t, _i); evil_index_set(fiber, _t, _i, evil_val_inc(fiber, _old)); _old; })");
                    }
                    break;
                case UN_POST_DEC:
                    if (operand->kind == AST_EXPR_IDENTIFIER) {
                        const char* n = operand->as.ident_expr.name;
                        bool is_local = scope_lookup_local(ctx, n);
                        int uv_idx = is_local ? -1 : fn_find_upvalue(ctx->current_fn, n);
                        if (is_local) {
                            fprintf(ctx->out, "({ evil_value_t _old = *_uv_%s->val_ptr; _uv_%s = evil_upvalue_create(evil_val_dec(fiber, _old)); _old; })", n, n);
                        } else if (uv_idx >= 0) {
                            fprintf(ctx->out, "({ evil_value_t _old = *_cur_closure->upvalues[%d]->val_ptr; *_cur_closure->upvalues[%d]->val_ptr = evil_val_dec(fiber, _old); _old; })", uv_idx, uv_idx);
                        } else {
                            fprintf(ctx->out, "({ evil_value_t _old = evil_runtime_get_global(g_rt, \"%s\"); evil_runtime_set_global(g_rt, \"%s\", evil_val_dec(fiber, _old)); _old; })", n, n);
                        }
                    } else if (operand->kind == AST_EXPR_PROPERTY) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.prop_expr.target);
                        fprintf(ctx->out, "; evil_value_t _old = evil_table_get_str(_t, \"%s\"); evil_table_set_str(_t, \"%s\", evil_val_dec(fiber, _old)); _old; })", operand->as.prop_expr.property, operand->as.prop_expr.property);
                    } else if (operand->kind == AST_EXPR_INDEXER) {
                        fprintf(ctx->out, "({ evil_value_t _t = ");
                        emit_expr(ctx, operand->as.indexer_expr.target);
                        fprintf(ctx->out, "; evil_value_t _i = ");
                        emit_expr(ctx, operand->as.indexer_expr.index);
                        fprintf(ctx->out, "; evil_value_t _old = evil_index_get(fiber, _t, _i); evil_index_set(fiber, _t, _i, evil_val_dec(fiber, _old)); _old; })");
                    }
                    break;
            }
            break;
        }

        case AST_EXPR_INVOCATION: {
            cvil_ast_node_t* callee = node->as.invocation_expr.callee;
            cvil_ast_list_t* args = node->as.invocation_expr.args;
            uint32_t arg_count = args ? args->count : 0;

            bool has_forward = false;
            for (uint32_t i = 0; i < arg_count; i++) {
                if (args->items[i]->kind == AST_EXPR_VARARG_FORWARD) {
                    has_forward = true;
                    break;
                }
            }

            if (!has_forward) {
                fprintf(ctx->out, "({ evil_value_t _callee = ");
                emit_expr(ctx, callee);
                if (arg_count > 0) {
                    fprintf(ctx->out, "; evil_value_t _args[%d] = { ", arg_count);
                    for (uint32_t i = 0; i < arg_count; i++) {
                        if (i > 0) fprintf(ctx->out, ", ");
                        emit_expr(ctx, args->items[i]);
                    }
                    fprintf(ctx->out, " }; evil_call(fiber, _callee, evil_val_nil(), %d, _args); })", arg_count);
                } else {
                    fprintf(ctx->out, "; evil_call(fiber, _callee, evil_val_nil(), 0, NULL); })");
                }
            } else {
                /* Variadic forward f(*) */
                fprintf(ctx->out, "({ evil_value_t _callee = ");
                emit_expr(ctx, callee);
                fprintf(ctx->out, "; int _n = %d + (argc > 0 ? argc : 0); evil_value_t* _a = (evil_value_t*)evil_alloc(_n * sizeof(evil_value_t)); int _idx = 0; ", arg_count);
                for (uint32_t i = 0; i < arg_count; i++) {
                    if (args->items[i]->kind == AST_EXPR_VARARG_FORWARD) {
                        fprintf(ctx->out, "for (int _j = 0; _j < argc; _j++) _a[_idx++] = argv[_j]; ");
                    } else {
                        fprintf(ctx->out, "_a[_idx++] = ");
                        emit_expr(ctx, args->items[i]);
                        fprintf(ctx->out, "; ");
                    }
                }
                fprintf(ctx->out, "evil_call(fiber, _callee, evil_val_nil(), _idx, _a); })");
            }
            break;
        }

        case AST_EXPR_SELF_INVOCATION: {
            cvil_ast_node_t* receiver = node->as.self_invoc_expr.receiver;
            const char* method = node->as.self_invoc_expr.method;
            cvil_ast_list_t* args = node->as.self_invoc_expr.args;
            uint32_t arg_count = args ? args->count : 0;

            fprintf(ctx->out, "({ evil_value_t _rec = ");
            emit_expr(ctx, receiver);
            if (arg_count > 0) {
                fprintf(ctx->out, "; evil_value_t _args[%d] = { ", arg_count);
                for (uint32_t i = 0; i < arg_count; i++) {
                    if (i > 0) fprintf(ctx->out, ", ");
                    emit_expr(ctx, args->items[i]);
                }
                fprintf(ctx->out, " }; evil_call_method(fiber, _rec, \"%s\", %d, _args); })", method, arg_count);
            } else {
                fprintf(ctx->out, "; evil_call_method(fiber, _rec, \"%s\", 0, NULL); })", method);
            }
            break;
        }

        case AST_EXPR_INDEXER:
            fprintf(ctx->out, "evil_index_get(fiber, ");
            emit_expr(ctx, node->as.indexer_expr.target);
            fprintf(ctx->out, ", ");
            emit_expr(ctx, node->as.indexer_expr.index);
            fprintf(ctx->out, ")");
            break;

        case AST_EXPR_PROPERTY:
            fprintf(ctx->out, "evil_table_get_str(");
            emit_expr(ctx, node->as.prop_expr.target);
            fprintf(ctx->out, ", \"%s\")", node->as.prop_expr.property);
            break;

        case AST_EXPR_ELVIS_PROPERTY:
            fprintf(ctx->out, "({ evil_value_t _tgt = ");
            emit_expr(ctx, node->as.elvis_expr.target);
            fprintf(ctx->out, "; evil_is_nil(_tgt) ? evil_val_nil() : evil_table_get_str(_tgt, \"%s\"); })", node->as.elvis_expr.property);
            break;

        case AST_EXPR_ELVIS_INDEXER:
            fprintf(ctx->out, "({ evil_value_t _tgt = ");
            emit_expr(ctx, node->as.elvis_expr.target);
            fprintf(ctx->out, "; evil_is_nil(_tgt) ? evil_val_nil() : evil_index_get(fiber, _tgt, ");
            emit_expr(ctx, node->as.elvis_expr.index);
            fprintf(ctx->out, "); })");
            break;

        case AST_EXPR_TABLE: {
            uint32_t count = node->as.table_expr.count;
            fprintf(ctx->out, "({ evil_value_t _t = evil_table_create(%d, %d); ", count / 2, count + 2);
            for (uint32_t i = 0; i < count; i++) {
                cvil_table_entry_node_t* ent = &node->as.table_expr.entries[i];
                if (ent->key) {
                    fprintf(ctx->out, "evil_table_set(_t, ");
                    emit_expr(ctx, ent->key);
                    fprintf(ctx->out, ", ");
                    emit_expr(ctx, ent->val);
                    fprintf(ctx->out, "); ");
                } else {
                    /* Array-like entry or method */
                    fprintf(ctx->out, "evil_table_set(_t, evil_val_number(%d), ", i);
                    emit_expr(ctx, ent->val);
                    fprintf(ctx->out, "); ");
                }
            }
            fprintf(ctx->out, "_t; })");
            break;
        }

        case AST_EXPR_ARRAY: {
            uint32_t count = node->as.array_expr.elements ? node->as.array_expr.elements->count : 0;
            if (node->as.array_expr.size_expr) {
                fprintf(ctx->out, "({ evil_value_t _sz = ");
                emit_expr(ctx, node->as.array_expr.size_expr);
                fprintf(ctx->out, "; evil_value_t _a = evil_array_create_sized((uint32_t)evil_val_tonum(fiber, _sz).as.number, evil_val_nil()); ");
                for (uint32_t i = 0; i < count; i++) {
                    fprintf(ctx->out, "evil_array_set(_a, %d, ", i);
                    emit_expr(ctx, node->as.array_expr.elements->items[i]);
                    fprintf(ctx->out, "); ");
                }
                fprintf(ctx->out, "_a; })");
            } else {
                fprintf(ctx->out, "({ evil_value_t _a = evil_array_create(%d); ", count);
                for (uint32_t i = 0; i < count; i++) {
                    fprintf(ctx->out, "evil_array_push(_a, ");
                    emit_expr(ctx, node->as.array_expr.elements->items[i]);
                    fprintf(ctx->out, "); ");
                }
                fprintf(ctx->out, "_a; })");
            }
            break;
        }

        case AST_EXPR_FN:
        case AST_EXPR_SELF_FN: {
            uint32_t id = node->as.fn_expr.unique_fn_id;
            lifted_fn_t* lf = find_fn_by_id(ctx, id);
            uint32_t uv_count = lf ? lf->uv_count : 0;
            if (uv_count > 0) {
                fprintf(ctx->out, "({ evil_upvalue_t* _uvs[%d] = { ", uv_count);
                for (uint32_t i = 0; i < uv_count; i++) {
                    if (i > 0) fprintf(ctx->out, ", ");
                    if (lf->upvalues[i].is_from_parent_uv) {
                        fprintf(ctx->out, "_cur_closure->upvalues[%d]", lf->upvalues[i].parent_uv_idx);
                    } else {
                        fprintf(ctx->out, "_uv_%s", lf->upvalues[i].name);
                    }
                }
                fprintf(ctx->out, " }; evil_closure_create_arr(evil_fn_%d, evil_val_nil(), %d, _uvs); })",
                        id, uv_count);
            } else {
                fprintf(ctx->out, "evil_closure_create_arr(evil_fn_%d, evil_val_nil(), 0, NULL)",
                        id);
            }
            break;
        }

        case AST_EXPR_BY: {
            cvil_ast_node_t* target = node->as.by_expr.target;
            uint32_t arm_count = node->as.by_expr.arm_count;
            fprintf(ctx->out, "({ evil_value_t _matched = evil_val_nil(); evil_value_t _tgt = ");
            emit_expr(ctx, target);
            fprintf(ctx->out, "; ");

            bool has_else = false;
            for (uint32_t i = 0; i < arm_count; i++) {
                cvil_by_arm_t* arm = &node->as.by_expr.arms[i];
                if (arm->is_else) {
                    has_else = true;
                    if (i > 0) fprintf(ctx->out, " else { ");
                    else fprintf(ctx->out, "{ ");
                    if (arm->is_throw) {
                        fprintf(ctx->out, "evil_throw(fiber, ");
                        emit_expr(ctx, arm->body->kind == AST_STMT_THROW ? arm->body->as.throw_stmt.expr : arm->body);
                        fprintf(ctx->out, "); ");
                    } else {
                        fprintf(ctx->out, "_matched = ");
                        emit_expr(ctx, arm->body);
                        fprintf(ctx->out, "; ");
                    }
                    fprintf(ctx->out, "} ");
                } else {
                    const char* cmp_fn = arm->is_deep_match ? "evil_val_deq" : "evil_val_eq";
                    if (i == 0) fprintf(ctx->out, "if (evil_is_truth(%s(fiber, _tgt, ", cmp_fn);
                    else fprintf(ctx->out, "else if (evil_is_truth(%s(fiber, _tgt, ", cmp_fn);
                    emit_expr(ctx, arm->pattern);
                    if (arm->is_throw) {
                        fprintf(ctx->out, "))) { evil_throw(fiber, ");
                        emit_expr(ctx, arm->body->kind == AST_STMT_THROW ? arm->body->as.throw_stmt.expr : arm->body);
                        fprintf(ctx->out, "); } ");
                    } else {
                        fprintf(ctx->out, "))) { _matched = ");
                        emit_expr(ctx, arm->body);
                        fprintf(ctx->out, "; } ");
                    }
                }
            }
            if (!has_else && arm_count > 0) {
                fprintf(ctx->out, "else { _matched = evil_val_nil(); } ");
            }
            fprintf(ctx->out, "_matched; })");
            break;
        }

        case AST_EXPR_WITH:
            fprintf(ctx->out, "({ evil_value_t _b = ");
            emit_expr(ctx, node->as.with_expr.base);
            fprintf(ctx->out, "; evil_value_t _c = evil_table_clone(_b, false); evil_value_t _ext = ");
            emit_expr(ctx, node->as.with_expr.ext_table);
            fprintf(ctx->out, "; if (_ext.type == EVIL_TYPE_TABLE && _ext.as.table) { evil_table_iter_t _it = evil_table_iter_begin(_ext.as.table); evil_value_t _k, _v; while (evil_table_iter_next(&_it, &_k, &_v)) { evil_table_set(_c, _k, _v); } } _c; })");
            break;

        case AST_EXPR_TYPEOF:
            if (node->as.typeof_expr.invert) {
                fprintf(ctx->out, "evil_val_native_typeof(");
                emit_expr(ctx, node->as.typeof_expr.expr);
                fprintf(ctx->out, ")");
            } else {
                fprintf(ctx->out, "evil_val_typeof(");
                emit_expr(ctx, node->as.typeof_expr.expr);
                fprintf(ctx->out, ")");
            }
            break;

        case AST_EXPR_YIELD: {
            cvil_ast_node_t* target = node->as.yield_expr.target_expr;
            cvil_ast_list_t* args = node->as.yield_expr.args;
            uint32_t arg_count = args ? args->count : 0;
            fprintf(ctx->out, "({ evil_value_t _tgt = ");
            emit_expr(ctx, target);
            if (arg_count > 0) {
                fprintf(ctx->out, "; evil_value_t _args[%d] = { ", arg_count);
                for (uint32_t i = 0; i < arg_count; i++) {
                    if (i > 0) fprintf(ctx->out, ", ");
                    emit_expr(ctx, args->items[i]);
                }
                fprintf(ctx->out, " }; evil_fiber_yield_call(fiber, _tgt, %d, _args); })", arg_count);
            } else {
                fprintf(ctx->out, "; evil_fiber_yield_call(fiber, _tgt, 0, NULL); })");
            }
            break;
        }

        case AST_EXPR_ERROR:
            fprintf(ctx->out, "evil_error_create(");
            if (node->as.error_expr.message) emit_expr(ctx, node->as.error_expr.message);
            else fprintf(ctx->out, "evil_val_nil()");
            fprintf(ctx->out, ", ");
            if (node->as.error_expr.table_data) emit_expr(ctx, node->as.error_expr.table_data);
            else fprintf(ctx->out, "evil_val_nil()");
            fprintf(ctx->out, ")");
            break;

        default:
            fprintf(ctx->out, "evil_val_nil()");
            break;
    }
}

/* Statement Code Generation */
static void emit_stmt(codegen_ctx_t* ctx, cvil_ast_node_t* node) {
    if (!node) return;

    emit_line(ctx, node->span.first_line);

    switch (node->kind) {
        case AST_STMT_EXPR:
            emit_expr(ctx, node->as.expr_stmt.expr);
            fprintf(ctx->out, ";\n");
            break;

        case AST_STMT_BLOCK:
            emit_block(ctx, node);
            break;

        case AST_STMT_VAL: {
            for (uint32_t i = 0; i < node->as.val_stmt.count; i++) {
                cvil_val_decl_t* d = &node->as.val_stmt.decls[i];
                scope_add(ctx, d->name);

                fprintf(ctx->out, "evil_upvalue_t* volatile _uv_%s = evil_upvalue_create(", d->name);
                if (d->init_val) {
                    emit_expr(ctx, d->init_val);
                } else {
                    fprintf(ctx->out, "evil_val_nil()");
                }
                fprintf(ctx->out, ");\n");
            }
            break;
        }

        case AST_STMT_IF: {
            fprintf(ctx->out, "if (evil_is_truth(");
            emit_expr(ctx, node->as.if_stmt.condition);
            fprintf(ctx->out, ")) {\n");
            emit_stmt(ctx, node->as.if_stmt.if_branch);
            fprintf(ctx->out, "}\n");

            for (uint32_t i = 0; i < node->as.if_stmt.elif_count; i++) {
                fprintf(ctx->out, "else if (evil_is_truth(");
                emit_expr(ctx, node->as.if_stmt.elifs[i].condition);
                fprintf(ctx->out, ")) {\n");
                emit_stmt(ctx, node->as.if_stmt.elifs[i].branch);
                fprintf(ctx->out, "}\n");
            }

            if (node->as.if_stmt.else_branch) {
                fprintf(ctx->out, "else {\n");
                emit_stmt(ctx, node->as.if_stmt.else_branch);
                fprintf(ctx->out, "}\n");
            }
            break;
        }

        case AST_STMT_WHILE:
            fprintf(ctx->out, "while (evil_is_truth(");
            emit_expr(ctx, node->as.while_stmt.condition);
            fprintf(ctx->out, ")) {\n");
            emit_stmt(ctx, node->as.while_stmt.body);
            fprintf(ctx->out, "}\n");
            break;

        case AST_STMT_DO_WHILE:
            fprintf(ctx->out, "do {\n");
            emit_stmt(ctx, node->as.do_while_stmt.body);
            fprintf(ctx->out, "} while (evil_is_truth(");
            emit_expr(ctx, node->as.do_while_stmt.condition);
            fprintf(ctx->out, "));\n");
            break;

        case AST_STMT_FOR: {
            scope_push(ctx);
            fprintf(ctx->out, "{\n");
            if (node->as.for_stmt.inits) {
                for (uint32_t i = 0; i < node->as.for_stmt.inits->count; i++) {
                    cvil_ast_node_t* init = node->as.for_stmt.inits->items[i];
                    if (init->kind == AST_STMT_VAL || init->kind == AST_STMT_EXPR) {
                        emit_stmt(ctx, init);
                    } else {
                        emit_expr(ctx, init);
                        fprintf(ctx->out, ";\n");
                    }
                }
            }
            fprintf(ctx->out, "for ( ; ");
            if (node->as.for_stmt.condition) {
                fprintf(ctx->out, "evil_is_truth(");
                emit_expr(ctx, node->as.for_stmt.condition);
                fprintf(ctx->out, ")");
            }
            fprintf(ctx->out, " ; ");
            if (node->as.for_stmt.steps && node->as.for_stmt.steps->count > 0) {
                for (uint32_t i = 0; i < node->as.for_stmt.steps->count; i++) {
                    if (i > 0) fprintf(ctx->out, ", ");
                    fprintf(ctx->out, "(");
                    emit_expr(ctx, node->as.for_stmt.steps->items[i]);
                    fprintf(ctx->out, ")");
                }
            }
            fprintf(ctx->out, ") {\n");
            emit_stmt(ctx, node->as.for_stmt.body);
            fprintf(ctx->out, "}\n}\n");
            scope_pop(ctx);
            break;
        }

        case AST_STMT_EACH: {
            scope_push(ctx);
            const char* k_var = node->as.each_stmt.key_var;
            const char* v_var = node->as.each_stmt.val_var;
            if (k_var) scope_add(ctx, k_var);
            if (v_var) scope_add(ctx, v_var);

            fprintf(ctx->out, "{\n");
            fprintf(ctx->out, "evil_value_t _coll = ");
            emit_expr(ctx, node->as.each_stmt.collection);
            fprintf(ctx->out, ";\n");

            if (k_var) fprintf(ctx->out, "evil_upvalue_t* volatile _uv_%s = evil_upvalue_create(evil_val_nil());\n", k_var);
            if (v_var) fprintf(ctx->out, "evil_upvalue_t* volatile _uv_%s = evil_upvalue_create(evil_val_nil());\n", v_var);

            fprintf(ctx->out, "if (_coll.type == EVIL_TYPE_ARRAY && _coll.as.array) {\n");
            fprintf(ctx->out, "    for (uint32_t _i = 0; _i < _coll.as.array->length; _i++) {\n");
            if (k_var) fprintf(ctx->out, "        *_uv_%s->val_ptr = evil_val_number((double)_i);\n", k_var);
            if (v_var) fprintf(ctx->out, "        *_uv_%s->val_ptr = _coll.as.array->items[_i];\n", v_var);
            emit_stmt(ctx, node->as.each_stmt.body);
            fprintf(ctx->out, "    }\n");
            fprintf(ctx->out, "} else if (_coll.type == EVIL_TYPE_TABLE && _coll.as.table) {\n");
            fprintf(ctx->out, "    evil_table_iter_t _it = evil_table_iter_begin(_coll.as.table);\n");
            fprintf(ctx->out, "    evil_value_t _k, _v;\n");
            fprintf(ctx->out, "    while (evil_table_iter_next(&_it, &_k, &_v)) {\n");
            if (k_var) fprintf(ctx->out, "        *_uv_%s->val_ptr = _k;\n", k_var);
            if (v_var) fprintf(ctx->out, "        *_uv_%s->val_ptr = _v;\n", v_var);
            emit_stmt(ctx, node->as.each_stmt.body);
            fprintf(ctx->out, "    }\n");
            fprintf(ctx->out, "}\n");
            fprintf(ctx->out, "}\n");
            scope_pop(ctx);
            break;
        }

        case AST_STMT_TRY: {
            scope_push(ctx);
            uint32_t try_id = ++ctx->next_try_id;
            ctx->try_stack[ctx->try_depth++] = try_id;
            fprintf(ctx->out, "{\n");
            fprintf(ctx->out, "evil_try_frame_t _frame_%d;\n", try_id);
            fprintf(ctx->out, "_try_retry_%d:\n", try_id);
            fprintf(ctx->out, "_frame_%d.prev = fiber->try_top;\n", try_id);
            fprintf(ctx->out, "fiber->try_top = &_frame_%d;\n", try_id);
            fprintf(ctx->out, "int _jmp_%d = setjmp(_frame_%d.jmp);\n", try_id, try_id);
            fprintf(ctx->out, "if (_jmp_%d == EVIL_JMP_INIT) {\n", try_id);
            emit_stmt(ctx, node->as.try_stmt.body);
            fprintf(ctx->out, "fiber->try_top = _frame_%d.prev;\n", try_id);
            fprintf(ctx->out, "} else if (_jmp_%d == EVIL_JMP_THROW) {\n", try_id);
            fprintf(ctx->out, "fiber->try_top = _frame_%d.prev;\n", try_id);
            if (node->as.try_stmt.err_var) {
                scope_add(ctx, node->as.try_stmt.err_var);
                fprintf(ctx->out, "evil_upvalue_t* volatile _uv_%s = evil_upvalue_create(fiber->current_exception);\n", node->as.try_stmt.err_var);
            }
            if (node->as.try_stmt.catch_body) {
                emit_stmt(ctx, node->as.try_stmt.catch_body);
            }
            fprintf(ctx->out, "}\n");
            fprintf(ctx->out, "}\n");
            ctx->try_depth--;
            scope_pop(ctx);
            break;
        }

        case AST_STMT_THROW:
            fprintf(ctx->out, "evil_throw(fiber, ");
            emit_expr(ctx, node->as.throw_stmt.expr);
            fprintf(ctx->out, ");\n");
            break;

        case AST_STMT_RETRY:
            if (ctx->try_depth > 0) {
                uint32_t tid = ctx->try_stack[ctx->try_depth - 1];
                fprintf(ctx->out, "fiber->try_top = _frame_%d.prev; goto _try_retry_%d;\n", tid, tid);
            } else {
                fprintf(ctx->out, "evil_retry(fiber);\n");
            }
            break;

        case AST_STMT_RET:
            fprintf(ctx->out, "return ");
            if (node->as.ret_stmt.expr) {
                emit_expr(ctx, node->as.ret_stmt.expr);
            } else {
                fprintf(ctx->out, "evil_val_nil()");
            }
            fprintf(ctx->out, ";\n");
            break;

        case AST_STMT_BREAK:
            fprintf(ctx->out, "break;\n");
            break;

        case AST_STMT_SKIP:
            fprintf(ctx->out, "continue;\n");
            break;

        case AST_STMT_FN: {
            uint32_t id = node->as.fn_stmt.unique_fn_id;
            lifted_fn_t* lf = find_fn_by_id(ctx, id);
            uint32_t uv_count = lf ? lf->uv_count : 0;

            if (node->as.fn_stmt.target_name) {
                /* Target::method */
                const char* target = node->as.fn_stmt.target_name;
                const char* method = node->as.fn_stmt.name;
                int uv_idx = fn_find_upvalue(ctx->current_fn, target);

                fprintf(ctx->out, "evil_table_set_str(");
                if (scope_lookup_local(ctx, target)) {
                    fprintf(ctx->out, "(*_uv_%s->val_ptr)", target);
                } else if (uv_idx >= 0) {
                    fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr)", uv_idx);
                } else {
                    fprintf(ctx->out, "evil_runtime_get_global(g_rt, \"%s\")", target);
                }
                fprintf(ctx->out, ", \"%s\", ", method);
                if (uv_count > 0) {
                    fprintf(ctx->out, "({ evil_upvalue_t* _uvs[%d] = { ", uv_count);
                    for (uint32_t i = 0; i < uv_count; i++) {
                        if (i > 0) fprintf(ctx->out, ", ");
                        if (lf->upvalues[i].is_from_parent_uv) {
                            fprintf(ctx->out, "_cur_closure->upvalues[%d]", lf->upvalues[i].parent_uv_idx);
                        } else {
                            fprintf(ctx->out, "_uv_%s", lf->upvalues[i].name);
                        }
                    }
                    fprintf(ctx->out, " }; evil_closure_create_arr(evil_fn_%d, evil_val_nil(), %d, _uvs); }));\n", id, uv_count);
                } else {
                    fprintf(ctx->out, "evil_closure_create_arr(evil_fn_%d, evil_val_nil(), 0, NULL));\n", id);
                }
            } else if (node->as.fn_stmt.target_chain) {
                /* Indexed function t.a.b.c */
                cvil_ast_list_t* chain = node->as.fn_stmt.target_chain;
                uint32_t count = chain->count;
                const char* first = chain->items[0]->as.ident_expr.name;
                int uv_idx = fn_find_upvalue(ctx->current_fn, first);

                fprintf(ctx->out, "{\n evil_value_t _cur = ");
                if (scope_lookup_local(ctx, first)) {
                    fprintf(ctx->out, "(*_uv_%s->val_ptr);\n", first);
                } else if (uv_idx >= 0) {
                    fprintf(ctx->out, "(*_cur_closure->upvalues[%d]->val_ptr);\n", uv_idx);
                } else {
                    fprintf(ctx->out, "evil_runtime_get_global(g_rt, \"%s\");\n", first);
                }
                for (uint32_t i = 1; i < count - 1; i++) {
                    const char* seg = chain->items[i]->as.ident_expr.name;
                    fprintf(ctx->out, "evil_value_t _next_%d = evil_table_get_str(_cur, \"%s\");\n", i, seg);
                    fprintf(ctx->out, "if (evil_is_nil(_next_%d)) { _next_%d = evil_table_create(0, 4); evil_table_set_str(_cur, \"%s\", _next_%d); }\n", i, i, seg, i);
                    fprintf(ctx->out, "_cur = _next_%d;\n", i);
                }
                const char* last = chain->items[count - 1]->as.ident_expr.name;
                fprintf(ctx->out, "evil_table_set_str(_cur, \"%s\", ", last);
                if (uv_count > 0) {
                    fprintf(ctx->out, "({ evil_upvalue_t* _uvs[%d] = { ", uv_count);
                    for (uint32_t i = 0; i < uv_count; i++) {
                        if (i > 0) fprintf(ctx->out, ", ");
                        if (lf->upvalues[i].is_from_parent_uv) {
                            fprintf(ctx->out, "_cur_closure->upvalues[%d]", lf->upvalues[i].parent_uv_idx);
                        } else {
                            fprintf(ctx->out, "_uv_%s", lf->upvalues[i].name);
                        }
                    }
                    fprintf(ctx->out, " }; evil_closure_create_arr(evil_fn_%d, evil_val_nil(), %d, _uvs); }));\n}\n", id, uv_count);
                } else {
                    fprintf(ctx->out, "evil_closure_create_arr(evil_fn_%d, evil_val_nil(), 0, NULL));\n}\n", id);
                }
            } else if (node->as.fn_stmt.is_local) {
                scope_add(ctx, node->as.fn_stmt.name);
                if (uv_count > 0) {
                    fprintf(ctx->out, "evil_upvalue_t* volatile _uv_%s = evil_upvalue_create(evil_val_nil());\n", node->as.fn_stmt.name);
                    fprintf(ctx->out, "{\n evil_upvalue_t* _uvs[%d] = { ", uv_count);
                    for (uint32_t i = 0; i < uv_count; i++) {
                        if (i > 0) fprintf(ctx->out, ", ");
                        if (lf->upvalues[i].is_from_parent_uv) {
                            fprintf(ctx->out, "_cur_closure->upvalues[%d]", lf->upvalues[i].parent_uv_idx);
                        } else {
                            fprintf(ctx->out, "_uv_%s", lf->upvalues[i].name);
                        }
                    }
                    fprintf(ctx->out, " }; *_uv_%s->val_ptr = evil_closure_create_arr(evil_fn_%d, evil_val_nil(), %d, _uvs);\n}\n",
                            node->as.fn_stmt.name, id, uv_count);
                } else {
                    fprintf(ctx->out, "evil_upvalue_t* volatile _uv_%s = evil_upvalue_create(evil_closure_create_arr(evil_fn_%d, evil_val_nil(), 0, NULL));\n",
                            node->as.fn_stmt.name, id);
                }
            } else {
                /* Global function */
                if (uv_count > 0) {
                    fprintf(ctx->out, "{\n evil_upvalue_t* _uvs[%d] = { ", uv_count);
                    for (uint32_t i = 0; i < uv_count; i++) {
                        if (i > 0) fprintf(ctx->out, ", ");
                        if (lf->upvalues[i].is_from_parent_uv) {
                            fprintf(ctx->out, "_cur_closure->upvalues[%d]", lf->upvalues[i].parent_uv_idx);
                        } else {
                            fprintf(ctx->out, "_uv_%s", lf->upvalues[i].name);
                        }
                    }
                    fprintf(ctx->out, " }; evil_runtime_set_global(g_rt, \"%s\", evil_closure_create_arr(evil_fn_%d, evil_val_nil(), %d, _uvs));\n}\n",
                            node->as.fn_stmt.name, id, uv_count);
                } else {
                    fprintf(ctx->out, "evil_runtime_set_global(g_rt, \"%s\", evil_closure_create_arr(evil_fn_%d, evil_val_nil(), 0, NULL));\n",
                            node->as.fn_stmt.name, id);
                }
            }
            break;
        }

        default:
            break;
    }
}

static void emit_block(codegen_ctx_t* ctx, cvil_ast_node_t* node) {
    if (!node) return;
    scope_push(ctx);
    fprintf(ctx->out, "{\n");
    if (node->as.block.stmts) {
        for (uint32_t i = 0; i < node->as.block.stmts->count; i++) {
            emit_stmt(ctx, node->as.block.stmts->items[i]);
        }
    }
    fprintf(ctx->out, "}\n");
    scope_pop(ctx);
}

/* Emit Function Body */
static void emit_function_impl(codegen_ctx_t* ctx, lifted_fn_t* lf) {
    ctx->current_fn = lf;
    scope_push(ctx);

    fprintf(ctx->out, "\n/* Lifted Function ID %d: %s */\n", lf->id, lf->name ? lf->name : "anonymous");
    fprintf(ctx->out, "static evil_value_t evil_fn_%d(evil_fiber_t* fiber, evil_value_t self, int argc, evil_value_t* argv) {\n", lf->id);
    fprintf(ctx->out, "evil_closure_t* _cur_closure = fiber ? fiber->current_closure : NULL;\n");

    cvil_param_t* params = NULL;
    uint32_t param_count = 0;
    cvil_ast_node_t* body = NULL;

    if (lf->node->kind == AST_STMT_FN) {
        params = lf->node->as.fn_stmt.params;
        param_count = lf->node->as.fn_stmt.param_count;
        body = lf->node->as.fn_stmt.body;
    } else {
        params = lf->node->as.fn_expr.params;
        param_count = lf->node->as.fn_expr.param_count;
        body = lf->node->as.fn_expr.body;
    }

    /* Set up formal parameters */
    for (uint32_t i = 0; i < param_count; i++) {
        cvil_param_t* p = &params[i];
        scope_add(ctx, p->name);

        fprintf(ctx->out, "evil_value_t _arg_%d = (argc > %d) ? argv[%d] : (", i, i, i);
        if (p->default_val) {
            emit_expr(ctx, p->default_val);
        } else {
            fprintf(ctx->out, "evil_val_nil()");
        }
        fprintf(ctx->out, ");\n");
        fprintf(ctx->out, "evil_upvalue_t* volatile _uv_%s = evil_upvalue_create(_arg_%d);\n", p->name, i);
    }

    /* Ellipsis array for extra arguments */
    fprintf(ctx->out, "evil_value_t _varargs = evil_array_create((argc > %d) ? (argc - %d) : 0);\n", param_count, param_count);
    fprintf(ctx->out, "for (int _i = %d; _i < argc; _i++) evil_array_push(_varargs, argv[_i]);\n", param_count);

    /* Body */
    if (body) {
        if (body->kind == AST_STMT_BLOCK) {
            if (body->as.block.stmts) {
                for (uint32_t i = 0; i < body->as.block.stmts->count; i++) {
                    emit_stmt(ctx, body->as.block.stmts->items[i]);
                }
            }
        } else {
            emit_stmt(ctx, body);
        }
    }

    fprintf(ctx->out, "return evil_val_nil();\n");
    fprintf(ctx->out, "}\n");

    scope_pop(ctx);
    ctx->current_fn = NULL;
}

/* Master Codegen Entrypoint */
bool cvil_generate_c(cvil_ast_node_t* root, FILE* out_c_file, const char* source_filename, bool test_mode) {
    if (!root || !out_c_file) return false;

    codegen_ctx_t ctx;
    memset(&ctx, 0, sizeof(codegen_ctx_t));
    ctx.out = out_c_file;
    ctx.source_filename = source_filename;
    ctx.test_mode = test_mode;

    /* 0. Pre-Pass: Collect all functions */
    collect_functions(&ctx, root);

    /* 1. Pass 1: Resolve all lexical scopes and compute all upvalues */
    lifted_fn_t toplevel_fn;
    memset(&toplevel_fn, 0, sizeof(lifted_fn_t));
    toplevel_fn.id = 0;
    res_scope_t* root_scope = res_scope_create(NULL, &toplevel_fn);
    pass1_resolve(&ctx, root, root_scope, &toplevel_fn);

    /* 2. Header & Prelude */
    fprintf(ctx.out, "/* Generated by CVIL (EVIL-to-C Transpiler) */\n");
    fprintf(ctx.out, "#define _GNU_SOURCE\n");
    fprintf(ctx.out, "#include <stdio.h>\n");
    fprintf(ctx.out, "#include <stdlib.h>\n");
    fprintf(ctx.out, "#include <stdint.h>\n");
    fprintf(ctx.out, "#include <stdbool.h>\n");
    fprintf(ctx.out, "#include <string.h>\n");
    fprintf(ctx.out, "#include <math.h>\n");
    fprintf(ctx.out, "#include \"cvil_value.h\"\n");
    fprintf(ctx.out, "#include \"cvil_runtime.h\"\n\n");
    fprintf(ctx.out, "static evil_runtime_t* g_rt = NULL;\n\n");

    /* 3. Forward Declarations of all lifted functions */
    fprintf(ctx.out, "/* Forward Declarations of Lifted Functions */\n");
    for (uint32_t i = 0; i < ctx.fn_count; i++) {
        fprintf(ctx.out, "static evil_value_t evil_fn_%d(evil_fiber_t* fiber, evil_value_t self, int argc, evil_value_t* argv);\n", ctx.functions[i].id);
    }
    fprintf(ctx.out, "\n");

    /* 4. Emit lifted function definitions */
    for (uint32_t i = 0; i < ctx.fn_count; i++) {
        emit_function_impl(&ctx, &ctx.functions[i]);
    }

    /* 5. Top-level script chunk */
    ctx.current_fn = &toplevel_fn;
    fprintf(ctx.out, "\n/* Top-Level Script Execution */\n");
    fprintf(ctx.out, "static evil_value_t evil_toplevel(evil_fiber_t* fiber, evil_value_t self, int argc, evil_value_t* argv) {\n");
    fprintf(ctx.out, "(void)self; (void)argc; (void)argv;\n");
    fprintf(ctx.out, "evil_closure_t* _cur_closure = fiber ? fiber->current_closure : NULL;\n");
    fprintf(ctx.out, "evil_value_t _varargs = evil_val_nil();\n");

    scope_push(&ctx);
    if (root->kind == AST_STMT_PROGRAM && root->as.program.stmts) {
        for (uint32_t i = 0; i < root->as.program.stmts->count; i++) {
            emit_stmt(&ctx, root->as.program.stmts->items[i]);
        }
    }
    scope_pop(&ctx);
    ctx.current_fn = NULL;

    fprintf(ctx.out, "return evil_val_nil();\n");
    fprintf(ctx.out, "}\n\n");

    /* 6. Main Entrypoint */
    fprintf(ctx.out, "/* Main Entrypoint */\n");
    fprintf(ctx.out, "int main(int argc, char** argv) {\n");
    fprintf(ctx.out, "evil_runtime_init_gc();\n");
    fprintf(ctx.out, "g_rt = evil_runtime_create();\n");
    fprintf(ctx.out, "evil_runtime_init_globals(g_rt, argc, argv);\n");
    fprintf(ctx.out, "\n/* Run Top-Level Script */\n");
    fprintf(ctx.out, "evil_call(g_rt->main_fiber, evil_val_fn(evil_toplevel), evil_val_nil(), 0, NULL);\n\n");

    /* Test mode runner */
    if (test_mode) {
        uint32_t test_count = 0;
        for (uint32_t i = 0; i < ctx.fn_count; i++) {
            if (ctx.functions[i].is_test) test_count++;
        }

        fprintf(ctx.out, "/* Test Execution Mode */\n");
        fprintf(ctx.out, "static const char* _tests[] = {\n");
        for (uint32_t i = 0; i < ctx.fn_count; i++) {
            if (ctx.functions[i].is_test) {
                fprintf(ctx.out, "    \"%s\",\n", ctx.functions[i].name ? ctx.functions[i].name : "test");
            }
        }
        fprintf(ctx.out, "};\n");
        fprintf(ctx.out, "int total_tests = %d;\n", test_count);
        fprintf(ctx.out, "int passed = 0;\nint failed = 0;\n");
        fprintf(ctx.out, "printf(\"Running %%d tests...\\n\", total_tests);\n");
        fprintf(ctx.out, "for (int i = 0; i < total_tests; i++) {\n");
        fprintf(ctx.out, "    printf(\"  test %%s ... \", _tests[i]); fflush(stdout);\n");
        fprintf(ctx.out, "    evil_value_t _test_fn = evil_runtime_get_global(g_rt, _tests[i]);\n");
        fprintf(ctx.out, "    evil_try_frame_t _tf;\n");
        fprintf(ctx.out, "    _tf.prev = g_rt->main_fiber->try_top;\n");
        fprintf(ctx.out, "    g_rt->main_fiber->try_top = &_tf;\n");
        fprintf(ctx.out, "    if (setjmp(_tf.jmp) == 0) {\n");
        fprintf(ctx.out, "        evil_call(g_rt->main_fiber, _test_fn, evil_val_nil(), 0, NULL);\n");
        fprintf(ctx.out, "        g_rt->main_fiber->try_top = _tf.prev;\n");
        fprintf(ctx.out, "        printf(\"\\033[1;32mok\\033[0m\\n\"); passed++;\n");
        fprintf(ctx.out, "    } else {\n");
        fprintf(ctx.out, "        g_rt->main_fiber->try_top = _tf.prev;\n");
        fprintf(ctx.out, "        printf(\"\\033[1;31mFAILED\\033[0m\\n\");\n");
        fprintf(ctx.out, "        if (g_rt->main_fiber->current_exception.type == EVIL_TYPE_ERROR && g_rt->main_fiber->current_exception.as.error) {\n");
        fprintf(ctx.out, "            evil_value_t m = g_rt->main_fiber->current_exception.as.error->message;\n");
        fprintf(ctx.out, "            if (m.type == EVIL_TYPE_STRING) printf(\"    Reason: %%s\\n\", evil_str_chars(m));\n");
        fprintf(ctx.out, "        } else if (g_rt->main_fiber->current_exception.type == EVIL_TYPE_STRING) {\n");
        fprintf(ctx.out, "            printf(\"    Reason: %%s\\n\", evil_str_chars(g_rt->main_fiber->current_exception));\n");
        fprintf(ctx.out, "        }\n");
        fprintf(ctx.out, "        failed++;\n");
        fprintf(ctx.out, "    }\n");
        fprintf(ctx.out, "}\n");
        fprintf(ctx.out, "printf(\"\\nTest result: %%s. %%d passed, %%d failed.\\n\", failed == 0 ? \"\\033[1;32mSUCCESS\\033[0m\" : \"\\033[1;31mFAILURE\\033[0m\", passed, failed);\n");
        fprintf(ctx.out, "return failed > 0 ? 1 : 0;\n");
    } else {
        fprintf(ctx.out, "return 0;\n");
    }
    fprintf(ctx.out, "}\n");

    return true;
}
