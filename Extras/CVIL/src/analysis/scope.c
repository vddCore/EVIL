#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "cvil_ast.h"
#include "cvil_diag.h"

typedef struct cvil_symbol {
    const char* name;
    bool is_rw;
    bool is_captured;
    bool is_param;
    cvil_span_t decl_span;
} cvil_symbol_t;

typedef struct cvil_scope {
    struct cvil_scope* parent;
    bool is_function_boundary;
    cvil_symbol_t* symbols;
    uint32_t count;
    uint32_t capacity;
} cvil_scope_t;

static cvil_scope_t* g_current_scope = NULL;

static cvil_scope_t* scope_push(bool is_function_boundary) {
    cvil_scope_t* s = (cvil_scope_t*)calloc(1, sizeof(cvil_scope_t));
    s->parent = g_current_scope;
    s->is_function_boundary = is_function_boundary;
    s->capacity = 16;
    s->symbols = (cvil_symbol_t*)calloc(s->capacity, sizeof(cvil_symbol_t));
    g_current_scope = s;
    return s;
}

static void scope_pop(void) {
    if (g_current_scope) {
        cvil_scope_t* p = g_current_scope->parent;
        free(g_current_scope->symbols);
        free(g_current_scope);
        g_current_scope = p;
    }
}

static void scope_add_symbol(const char* name, bool is_rw, bool is_param, cvil_span_t span) {
    if (!g_current_scope || !name) return;
    cvil_scope_t* s = g_current_scope;
    if (s->count >= s->capacity) {
        s->capacity *= 2;
        s->symbols = (cvil_symbol_t*)realloc(s->symbols, s->capacity * sizeof(cvil_symbol_t));
    }
    s->symbols[s->count].name = strdup(name);
    s->symbols[s->count].is_rw = is_rw;
    s->symbols[s->count].is_captured = false;
    s->symbols[s->count].is_param = is_param;
    s->symbols[s->count].decl_span = span;
    s->count++;
}

static cvil_symbol_t* scope_lookup(const char* name, bool* crossed_fn_boundary) {
    *crossed_fn_boundary = false;
    cvil_scope_t* s = g_current_scope;
    while (s) {
        for (int i = (int)s->count - 1; i >= 0; i--) {
            if (strcmp(s->symbols[i].name, name) == 0) {
                return &s->symbols[i];
            }
        }
        if (s->is_function_boundary) {
            *crossed_fn_boundary = true;
        }
        s = s->parent;
    }
    return NULL;
}

static void analyze_node(cvil_ast_node_t* node);

static void analyze_stmt(cvil_ast_node_t* node) {
    if (!node) return;

    switch (node->kind) {
        case AST_STMT_PROGRAM: {
            scope_push(false);
            if (node->as.program.stmts) {
                for (uint32_t i = 0; i < node->as.program.stmts->count; i++) {
                    analyze_stmt(node->as.program.stmts->items[i]);
                }
            }
            scope_pop();
            break;
        }

        case AST_STMT_BLOCK: {
            scope_push(false);
            if (node->as.block.stmts) {
                for (uint32_t i = 0; i < node->as.block.stmts->count; i++) {
                    analyze_stmt(node->as.block.stmts->items[i]);
                }
            }
            scope_pop();
            break;
        }

        case AST_STMT_VAL: {
            for (uint32_t i = 0; i < node->as.val_stmt.count; i++) {
                cvil_val_decl_t* d = &node->as.val_stmt.decls[i];
                if (d->init_val) {
                    analyze_node(d->init_val);
                }
                scope_add_symbol(d->name, node->as.val_stmt.is_rw, false, d->span);
            }
            break;
        }

        case AST_STMT_EXPR: {
            analyze_node(node->as.expr_stmt.expr);
            break;
        }

        case AST_STMT_IF: {
            analyze_node(node->as.if_stmt.condition);
            analyze_stmt(node->as.if_stmt.if_branch);
            for (uint32_t i = 0; i < node->as.if_stmt.elif_count; i++) {
                analyze_node(node->as.if_stmt.elifs[i].condition);
                analyze_stmt(node->as.if_stmt.elifs[i].branch);
            }
            if (node->as.if_stmt.else_branch) {
                analyze_stmt(node->as.if_stmt.else_branch);
            }
            break;
        }

        case AST_STMT_WHILE: {
            analyze_node(node->as.while_stmt.condition);
            analyze_stmt(node->as.while_stmt.body);
            break;
        }

        case AST_STMT_DO_WHILE: {
            analyze_stmt(node->as.do_while_stmt.body);
            analyze_node(node->as.do_while_stmt.condition);
            break;
        }

        case AST_STMT_FOR: {
            scope_push(false);
            if (node->as.for_stmt.inits) {
                for (uint32_t i = 0; i < node->as.for_stmt.inits->count; i++) {
                    analyze_stmt(node->as.for_stmt.inits->items[i]);
                }
            }
            if (node->as.for_stmt.condition) {
                analyze_node(node->as.for_stmt.condition);
            }
            if (node->as.for_stmt.steps) {
                for (uint32_t i = 0; i < node->as.for_stmt.steps->count; i++) {
                    analyze_node(node->as.for_stmt.steps->items[i]);
                }
            }
            analyze_stmt(node->as.for_stmt.body);
            scope_pop();
            break;
        }

        case AST_STMT_EACH: {
            analyze_node(node->as.each_stmt.collection);
            scope_push(false);
            if (node->as.each_stmt.key_var) {
                scope_add_symbol(node->as.each_stmt.key_var, node->as.each_stmt.is_rw, false, node->span);
            }
            if (node->as.each_stmt.val_var) {
                scope_add_symbol(node->as.each_stmt.val_var, node->as.each_stmt.is_rw, false, node->span);
            }
            analyze_stmt(node->as.each_stmt.body);
            scope_pop();
            break;
        }

        case AST_STMT_TRY: {
            analyze_stmt(node->as.try_stmt.body);
            if (node->as.try_stmt.catch_body) {
                scope_push(false);
                if (node->as.try_stmt.err_var) {
                    scope_add_symbol(node->as.try_stmt.err_var, false, false, node->span);
                }
                analyze_stmt(node->as.try_stmt.catch_body);
                scope_pop();
            }
            break;
        }

        case AST_STMT_THROW: {
            analyze_node(node->as.throw_stmt.expr);
            break;
        }

        case AST_STMT_RET: {
            if (node->as.ret_stmt.expr) {
                analyze_node(node->as.ret_stmt.expr);
            }
            break;
        }

        case AST_STMT_FN: {
            /* Register function name in current scope */
            if (node->as.fn_stmt.name) {
                scope_add_symbol(node->as.fn_stmt.name, false, false, node->span);
            }
            /* Enter function body scope */
            scope_push(true);
            for (uint32_t i = 0; i < node->as.fn_stmt.param_count; i++) {
                cvil_param_t* p = &node->as.fn_stmt.params[i];
                if (p->default_val) {
                    analyze_node(p->default_val);
                }
                scope_add_symbol(p->name, p->is_rw, true, p->span);
            }
            analyze_stmt(node->as.fn_stmt.body);
            scope_pop();
            break;
        }

        default:
            break;
    }
}

static void analyze_node(cvil_ast_node_t* node) {
    if (!node) return;

    switch (node->kind) {
        case AST_EXPR_ASSIGN: {
            /* Check immutability */
            if (node->as.assign_expr.left->kind == AST_EXPR_IDENTIFIER) {
                const char* name = node->as.assign_expr.left->as.ident_expr.name;
                bool crossed = false;
                cvil_symbol_t* sym = scope_lookup(name, &crossed);
                if (sym) {
                    if (!sym->is_rw) {
                        cvil_diag_error_with_help(
                            node->as.assign_expr.left->span,
                            "CVIL0102",
                            "declare variable as mutable using 'rw val'",
                            "cannot assign to immutable binding '%s'",
                            name
                        );
                    }
                    if (crossed) {
                        sym->is_captured = true;
                    }
                }
            }
            analyze_node(node->as.assign_expr.left);
            analyze_node(node->as.assign_expr.right);
            break;
        }

        case AST_EXPR_IDENTIFIER: {
            const char* name = node->as.ident_expr.name;
            bool crossed = false;
            cvil_symbol_t* sym = scope_lookup(name, &crossed);
            if (sym && crossed) {
                sym->is_captured = true;
            }
            break;
        }

        case AST_EXPR_CONDITIONAL: {
            analyze_node(node->as.conditional_expr.cond);
            analyze_node(node->as.conditional_expr.true_expr);
            analyze_node(node->as.conditional_expr.false_expr);
            break;
        }

        case AST_EXPR_COALESCING: {
            analyze_node(node->as.coalescing_expr.left);
            analyze_node(node->as.coalescing_expr.right);
            break;
        }

        case AST_EXPR_BINARY: {
            analyze_node(node->as.binary_expr.left);
            if (node->as.binary_expr.right) {
                analyze_node(node->as.binary_expr.right);
            }
            break;
        }

        case AST_EXPR_UNARY: {
            analyze_node(node->as.unary_expr.operand);
            break;
        }

        case AST_EXPR_INVOCATION: {
            analyze_node(node->as.invocation_expr.callee);
            if (node->as.invocation_expr.args) {
                for (uint32_t i = 0; i < node->as.invocation_expr.args->count; i++) {
                    analyze_node(node->as.invocation_expr.args->items[i]);
                }
            }
            break;
        }

        case AST_EXPR_SELF_INVOCATION: {
            analyze_node(node->as.self_invoc_expr.receiver);
            if (node->as.self_invoc_expr.args) {
                for (uint32_t i = 0; i < node->as.self_invoc_expr.args->count; i++) {
                    analyze_node(node->as.self_invoc_expr.args->items[i]);
                }
            }
            break;
        }

        case AST_EXPR_INDEXER: {
            analyze_node(node->as.indexer_expr.target);
            analyze_node(node->as.indexer_expr.index);
            break;
        }

        case AST_EXPR_PROPERTY: {
            analyze_node(node->as.prop_expr.target);
            break;
        }

        case AST_EXPR_ELVIS_PROPERTY: {
            analyze_node(node->as.elvis_expr.target);
            break;
        }

        case AST_EXPR_ELVIS_INDEXER: {
            analyze_node(node->as.elvis_expr.target);
            analyze_node(node->as.elvis_expr.index);
            break;
        }

        case AST_EXPR_TABLE: {
            for (uint32_t i = 0; i < node->as.table_expr.count; i++) {
                if (node->as.table_expr.entries[i].key) {
                    analyze_node(node->as.table_expr.entries[i].key);
                }
                if (node->as.table_expr.entries[i].val) {
                    analyze_node(node->as.table_expr.entries[i].val);
                }
            }
            break;
        }

        case AST_EXPR_ARRAY: {
            if (node->as.array_expr.size_expr) {
                analyze_node(node->as.array_expr.size_expr);
            }
            if (node->as.array_expr.elements) {
                for (uint32_t i = 0; i < node->as.array_expr.elements->count; i++) {
                    analyze_node(node->as.array_expr.elements->items[i]);
                }
            }
            break;
        }

        case AST_EXPR_FN:
        case AST_EXPR_SELF_FN: {
            scope_push(true);
            for (uint32_t i = 0; i < node->as.fn_expr.param_count; i++) {
                cvil_param_t* p = &node->as.fn_expr.params[i];
                if (p->default_val) {
                    analyze_node(p->default_val);
                }
                scope_add_symbol(p->name, p->is_rw, true, p->span);
            }
            analyze_stmt(node->as.fn_expr.body);
            scope_pop();
            break;
        }

        case AST_EXPR_BY: {
            analyze_node(node->as.by_expr.target);
            for (uint32_t i = 0; i < node->as.by_expr.arm_count; i++) {
                if (node->as.by_expr.arms[i].pattern) {
                    analyze_node(node->as.by_expr.arms[i].pattern);
                }
                if (node->as.by_expr.arms[i].body) {
                    analyze_node(node->as.by_expr.arms[i].body);
                }
            }
            break;
        }

        case AST_EXPR_WITH: {
            analyze_node(node->as.with_expr.base);
            analyze_node(node->as.with_expr.ext_table);
            break;
        }

        case AST_EXPR_TYPEOF: {
            analyze_node(node->as.typeof_expr.expr);
            break;
        }

        case AST_EXPR_YIELD: {
            if (node->as.yield_expr.target_expr) {
                analyze_node(node->as.yield_expr.target_expr);
            }
            if (node->as.yield_expr.args) {
                for (uint32_t i = 0; i < node->as.yield_expr.args->count; i++) {
                    analyze_node(node->as.yield_expr.args->items[i]);
                }
            }
            break;
        }

        case AST_EXPR_ERROR: {
            if (node->as.error_expr.message) {
                analyze_node(node->as.error_expr.message);
            }
            if (node->as.error_expr.table_data) {
                analyze_node(node->as.error_expr.table_data);
            }
            break;
        }

        default:
            break;
    }
}

bool cvil_analyze_semantics(cvil_ast_node_t* root) {
    if (!root) return false;
    analyze_stmt(root);
    return g_diag_engine.error_count == 0;
}
