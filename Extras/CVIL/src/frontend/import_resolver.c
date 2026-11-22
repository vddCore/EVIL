#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <libgen.h>
#include "cvil.h"

static char* get_dirname(const char* path) {
    if (!path) return strdup(".");
    const char* last_slash = strrchr(path, '/');
    if (!last_slash) return strdup(".");
    size_t len = last_slash - path;
    if (len == 0) return strdup("/");
    char* d = (char*)malloc(len + 1);
    memcpy(d, path, len);
    d[len] = '\0';
    return d;
}

static char* resolve_dofile_path(const char* base_dir, const char* req_path) {
    char buf[2048];
    if (req_path[0] == '/') {
        if (access(req_path, R_OK) == 0) return strdup(req_path);
    } else {
        snprintf(buf, sizeof(buf), "%s/%s", base_dir, req_path);
        if (access(buf, R_OK) == 0) return strdup(buf);

        if (access(req_path, R_OK) == 0) return strdup(req_path);

        snprintf(buf, sizeof(buf), "VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests/%s", req_path);
        if (access(buf, R_OK) == 0) return strdup(buf);

        snprintf(buf, sizeof(buf), "VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests/lib/%s", req_path);
        if (access(buf, R_OK) == 0) return strdup(buf);
    }
    return NULL;
}

static char* resolve_require_path(const char* base_dir, const char* module_spec) {
    char fs_path[512];
    size_t len = strlen(module_spec);
    if (len >= sizeof(fs_path) - 1) return NULL;
    for (size_t i = 0; i < len; i++) {
        fs_path[i] = (module_spec[i] == '.') ? '/' : module_spec[i];
    }
    fs_path[len] = '\0';

    const char* last_slash = strrchr(fs_path, '/');
    const char* base_name = last_slash ? last_slash + 1 : fs_path;

    const char* dirs[] = {
        base_dir,
        ".",
        "VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests"
    };

    char buf[2048];
    for (size_t d = 0; d < sizeof(dirs) / sizeof(dirs[0]); d++) {
        if (!dirs[d]) continue;

        /* 1. <dir>/<fs_path>.vil */
        snprintf(buf, sizeof(buf), "%s/%s.vil", dirs[d], fs_path);
        if (access(buf, R_OK) == 0) return strdup(buf);

        /* 2. <dir>/<fs_path>/init.vil */
        snprintf(buf, sizeof(buf), "%s/%s/init.vil", dirs[d], fs_path);
        if (access(buf, R_OK) == 0) return strdup(buf);

        /* 3. <dir>/<fs_path>/<basename>.vil */
        snprintf(buf, sizeof(buf), "%s/%s/%s.vil", dirs[d], fs_path, base_name);
        if (access(buf, R_OK) == 0) return strdup(buf);
    }

    return NULL;
}

static cvil_ast_node_t* resolve_node(cvil_ast_node_t* node, const char* current_file);
static void resolve_stmts(cvil_ast_list_t* stmts, const char* current_file);

static void resolve_stmts(cvil_ast_list_t* stmts, const char* current_file) {
    if (!stmts) return;

    cvil_ast_list_t* new_stmts = cvil_ast_list_create();

    for (uint32_t i = 0; i < stmts->count; i++) {
        cvil_ast_node_t* s = stmts->items[i];
        if (!s) continue;

        /* Check for dofile("...") */
        if (s->kind == AST_STMT_EXPR &&
            s->as.expr_stmt.expr &&
            s->as.expr_stmt.expr->kind == AST_EXPR_INVOCATION &&
            s->as.expr_stmt.expr->as.invocation_expr.callee &&
            s->as.expr_stmt.expr->as.invocation_expr.callee->kind == AST_EXPR_IDENTIFIER &&
            strcmp(s->as.expr_stmt.expr->as.invocation_expr.callee->as.ident_expr.name, "dofile") == 0 &&
            s->as.expr_stmt.expr->as.invocation_expr.args &&
            s->as.expr_stmt.expr->as.invocation_expr.args->count == 1 &&
            s->as.expr_stmt.expr->as.invocation_expr.args->items[0]->kind == AST_EXPR_LITERAL &&
            s->as.expr_stmt.expr->as.invocation_expr.args->items[0]->as.literal_expr.lit_type == EVIL_TYPE_STRING) {

            const char* req_path = s->as.expr_stmt.expr->as.invocation_expr.args->items[0]->as.literal_expr.v.string_val;
            char* base_dir = get_dirname(current_file);
            char* resolved = resolve_dofile_path(base_dir, req_path);
            free(base_dir);

            if (resolved) {
                cvil_ast_node_t* inc_root = cvil_parse_file(resolved);
                if (inc_root && inc_root->as.program.stmts) {
                    resolve_stmts(inc_root->as.program.stmts, resolved);
                    for (uint32_t j = 0; j < inc_root->as.program.stmts->count; j++) {
                        cvil_ast_list_append(new_stmts, inc_root->as.program.stmts->items[j]);
                    }
                }
                free(resolved);
                continue;
            }
        }

        s = resolve_node(s, current_file);
        cvil_ast_list_append(new_stmts, s);
    }

    stmts->items = new_stmts->items;
    stmts->count = new_stmts->count;
    stmts->capacity = new_stmts->capacity;
}

static cvil_ast_node_t* resolve_node(cvil_ast_node_t* node, const char* current_file) {
    if (!node) return NULL;

    switch (node->kind) {
        case AST_STMT_PROGRAM:
            resolve_stmts(node->as.program.stmts, current_file);
            return node;

        case AST_STMT_BLOCK:
            resolve_stmts(node->as.block.stmts, current_file);
            return node;

        case AST_STMT_EXPR:
            node->as.expr_stmt.expr = resolve_node(node->as.expr_stmt.expr, current_file);
            return node;

        case AST_STMT_VAL:
            for (uint32_t i = 0; i < node->as.val_stmt.count; i++) {
                if (node->as.val_stmt.decls[i].init_val) {
                    node->as.val_stmt.decls[i].init_val = resolve_node(node->as.val_stmt.decls[i].init_val, current_file);
                }
            }
            return node;

        case AST_STMT_IF:
            node->as.if_stmt.condition = resolve_node(node->as.if_stmt.condition, current_file);
            node->as.if_stmt.if_branch = resolve_node(node->as.if_stmt.if_branch, current_file);
            for (uint32_t i = 0; i < node->as.if_stmt.elif_count; i++) {
                node->as.if_stmt.elifs[i].condition = resolve_node(node->as.if_stmt.elifs[i].condition, current_file);
                node->as.if_stmt.elifs[i].branch = resolve_node(node->as.if_stmt.elifs[i].branch, current_file);
            }
            if (node->as.if_stmt.else_branch) {
                node->as.if_stmt.else_branch = resolve_node(node->as.if_stmt.else_branch, current_file);
            }
            return node;

        case AST_STMT_WHILE:
            node->as.while_stmt.condition = resolve_node(node->as.while_stmt.condition, current_file);
            node->as.while_stmt.body = resolve_node(node->as.while_stmt.body, current_file);
            return node;

        case AST_STMT_DO_WHILE:
            node->as.do_while_stmt.condition = resolve_node(node->as.do_while_stmt.condition, current_file);
            node->as.do_while_stmt.body = resolve_node(node->as.do_while_stmt.body, current_file);
            return node;

        case AST_STMT_FOR:
            if (node->as.for_stmt.inits) {
                for (uint32_t i = 0; i < node->as.for_stmt.inits->count; i++)
                    node->as.for_stmt.inits->items[i] = resolve_node(node->as.for_stmt.inits->items[i], current_file);
            }
            if (node->as.for_stmt.condition) node->as.for_stmt.condition = resolve_node(node->as.for_stmt.condition, current_file);
            if (node->as.for_stmt.steps) {
                for (uint32_t i = 0; i < node->as.for_stmt.steps->count; i++)
                    node->as.for_stmt.steps->items[i] = resolve_node(node->as.for_stmt.steps->items[i], current_file);
            }
            if (node->as.for_stmt.body) node->as.for_stmt.body = resolve_node(node->as.for_stmt.body, current_file);
            return node;

        case AST_STMT_EACH:
            node->as.each_stmt.collection = resolve_node(node->as.each_stmt.collection, current_file);
            node->as.each_stmt.body = resolve_node(node->as.each_stmt.body, current_file);
            return node;

        case AST_STMT_TRY:
            node->as.try_stmt.body = resolve_node(node->as.try_stmt.body, current_file);
            if (node->as.try_stmt.catch_body) {
                node->as.try_stmt.catch_body = resolve_node(node->as.try_stmt.catch_body, current_file);
            }
            return node;

        case AST_STMT_THROW:
            node->as.throw_stmt.expr = resolve_node(node->as.throw_stmt.expr, current_file);
            return node;

        case AST_STMT_RET:
            if (node->as.ret_stmt.expr) {
                node->as.ret_stmt.expr = resolve_node(node->as.ret_stmt.expr, current_file);
            }
            return node;

        case AST_STMT_FN:
            for (uint32_t i = 0; i < node->as.fn_stmt.param_count; i++) {
                if (node->as.fn_stmt.params[i].default_val)
                    node->as.fn_stmt.params[i].default_val = resolve_node(node->as.fn_stmt.params[i].default_val, current_file);
            }
            node->as.fn_stmt.body = resolve_node(node->as.fn_stmt.body, current_file);
            return node;

        case AST_EXPR_ASSIGN:
            node->as.assign_expr.left = resolve_node(node->as.assign_expr.left, current_file);
            node->as.assign_expr.right = resolve_node(node->as.assign_expr.right, current_file);
            return node;

        case AST_EXPR_CONDITIONAL:
            node->as.conditional_expr.cond = resolve_node(node->as.conditional_expr.cond, current_file);
            node->as.conditional_expr.true_expr = resolve_node(node->as.conditional_expr.true_expr, current_file);
            node->as.conditional_expr.false_expr = resolve_node(node->as.conditional_expr.false_expr, current_file);
            return node;

        case AST_EXPR_COALESCING:
            node->as.coalescing_expr.left = resolve_node(node->as.coalescing_expr.left, current_file);
            node->as.coalescing_expr.right = resolve_node(node->as.coalescing_expr.right, current_file);
            return node;

        case AST_EXPR_BINARY:
            node->as.binary_expr.left = resolve_node(node->as.binary_expr.left, current_file);
            node->as.binary_expr.right = resolve_node(node->as.binary_expr.right, current_file);
            return node;

        case AST_EXPR_UNARY:
        case AST_EXPR_POSTFIX_INC_DEC:
            node->as.unary_expr.operand = resolve_node(node->as.unary_expr.operand, current_file);
            return node;

        case AST_EXPR_INVOCATION: {
            /* Check if require("...") */
            if (node->as.invocation_expr.callee &&
                node->as.invocation_expr.callee->kind == AST_EXPR_IDENTIFIER &&
                strcmp(node->as.invocation_expr.callee->as.ident_expr.name, "require") == 0 &&
                node->as.invocation_expr.args &&
                node->as.invocation_expr.args->count == 1 &&
                node->as.invocation_expr.args->items[0]->kind == AST_EXPR_LITERAL &&
                node->as.invocation_expr.args->items[0]->as.literal_expr.lit_type == EVIL_TYPE_STRING) {

                const char* mod_spec = node->as.invocation_expr.args->items[0]->as.literal_expr.v.string_val;
                char* base_dir = get_dirname(current_file);
                char* resolved = resolve_require_path(base_dir, mod_spec);
                free(base_dir);

                if (resolved) {
                    cvil_ast_node_t* mod_root = cvil_parse_file(resolved);
                    if (mod_root && mod_root->as.program.stmts) {
                        resolve_stmts(mod_root->as.program.stmts, resolved);

                        cvil_ast_node_t* fn_node = cvil_ast_new(AST_EXPR_FN, node->span);
                        fn_node->as.fn_expr.params = NULL;
                        fn_node->as.fn_expr.param_count = 0;
                        fn_node->as.fn_expr.body = cvil_ast_block(mod_root->as.program.stmts, mod_root->span);
                        fn_node->as.fn_expr.is_self_fn = false;
                        fn_node->as.fn_expr.unique_fn_id = cvil_next_fn_id();

                        cvil_ast_node_t* invoc = cvil_ast_invocation(fn_node, NULL, node->span);
                        free(resolved);
                        return invoc;
                    }
                    free(resolved);
                }
            }

            node->as.invocation_expr.callee = resolve_node(node->as.invocation_expr.callee, current_file);
            if (node->as.invocation_expr.args) {
                for (uint32_t i = 0; i < node->as.invocation_expr.args->count; i++) {
                    node->as.invocation_expr.args->items[i] = resolve_node(node->as.invocation_expr.args->items[i], current_file);
                }
            }
            return node;
        }

        case AST_EXPR_SELF_INVOCATION:
            node->as.self_invoc_expr.receiver = resolve_node(node->as.self_invoc_expr.receiver, current_file);
            if (node->as.self_invoc_expr.args) {
                for (uint32_t i = 0; i < node->as.self_invoc_expr.args->count; i++) {
                    node->as.self_invoc_expr.args->items[i] = resolve_node(node->as.self_invoc_expr.args->items[i], current_file);
                }
            }
            return node;

        case AST_EXPR_INDEXER:
            node->as.indexer_expr.target = resolve_node(node->as.indexer_expr.target, current_file);
            node->as.indexer_expr.index = resolve_node(node->as.indexer_expr.index, current_file);
            return node;

        case AST_EXPR_PROPERTY:
            node->as.prop_expr.target = resolve_node(node->as.prop_expr.target, current_file);
            return node;

        case AST_EXPR_ELVIS_PROPERTY:
        case AST_EXPR_ELVIS_INDEXER:
            node->as.elvis_expr.target = resolve_node(node->as.elvis_expr.target, current_file);
            if (node->as.elvis_expr.index) {
                node->as.elvis_expr.index = resolve_node(node->as.elvis_expr.index, current_file);
            }
            return node;

        case AST_EXPR_TABLE:
            for (uint32_t i = 0; i < node->as.table_expr.count; i++) {
                if (node->as.table_expr.entries[i].key)
                    node->as.table_expr.entries[i].key = resolve_node(node->as.table_expr.entries[i].key, current_file);
                if (node->as.table_expr.entries[i].val)
                    node->as.table_expr.entries[i].val = resolve_node(node->as.table_expr.entries[i].val, current_file);
            }
            return node;

        case AST_EXPR_ARRAY:
            if (node->as.array_expr.size_expr)
                node->as.array_expr.size_expr = resolve_node(node->as.array_expr.size_expr, current_file);
            if (node->as.array_expr.elements) {
                for (uint32_t i = 0; i < node->as.array_expr.elements->count; i++)
                    node->as.array_expr.elements->items[i] = resolve_node(node->as.array_expr.elements->items[i], current_file);
            }
            return node;

        case AST_EXPR_FN:
        case AST_EXPR_SELF_FN:
            for (uint32_t i = 0; i < node->as.fn_expr.param_count; i++) {
                if (node->as.fn_expr.params[i].default_val)
                    node->as.fn_expr.params[i].default_val = resolve_node(node->as.fn_expr.params[i].default_val, current_file);
            }
            node->as.fn_expr.body = resolve_node(node->as.fn_expr.body, current_file);
            return node;

        case AST_EXPR_BY:
            node->as.by_expr.target = resolve_node(node->as.by_expr.target, current_file);
            for (uint32_t i = 0; i < node->as.by_expr.arm_count; i++) {
                if (node->as.by_expr.arms[i].pattern)
                    node->as.by_expr.arms[i].pattern = resolve_node(node->as.by_expr.arms[i].pattern, current_file);
                if (node->as.by_expr.arms[i].body)
                    node->as.by_expr.arms[i].body = resolve_node(node->as.by_expr.arms[i].body, current_file);
            }
            return node;

        case AST_EXPR_WITH:
            node->as.with_expr.base = resolve_node(node->as.with_expr.base, current_file);
            node->as.with_expr.ext_table = resolve_node(node->as.with_expr.ext_table, current_file);
            return node;

        case AST_EXPR_TYPEOF:
            node->as.typeof_expr.expr = resolve_node(node->as.typeof_expr.expr, current_file);
            return node;

        case AST_EXPR_YIELD:
            if (node->as.yield_expr.target_expr)
                node->as.yield_expr.target_expr = resolve_node(node->as.yield_expr.target_expr, current_file);
            if (node->as.yield_expr.args) {
                for (uint32_t i = 0; i < node->as.yield_expr.args->count; i++)
                    node->as.yield_expr.args->items[i] = resolve_node(node->as.yield_expr.args->items[i], current_file);
            }
            return node;

        case AST_EXPR_ERROR:
            if (node->as.error_expr.message)
                node->as.error_expr.message = resolve_node(node->as.error_expr.message, current_file);
            if (node->as.error_expr.table_data)
                node->as.error_expr.table_data = resolve_node(node->as.error_expr.table_data, current_file);
            return node;

        default:
            return node;
    }
}

cvil_ast_node_t* cvil_resolve_imports(cvil_ast_node_t* root, const char* source_filename) {
    if (!root) return NULL;
    return resolve_node(root, source_filename);
}
