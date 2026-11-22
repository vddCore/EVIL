#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "cvil_ast.h"

static uint32_t g_next_fn_id = 1;

uint32_t cvil_next_fn_id(void) {
    return g_next_fn_id++;
}

cvil_ast_list_t* cvil_ast_list_create(void) {
    cvil_ast_list_t* l = (cvil_ast_list_t*)malloc(sizeof(cvil_ast_list_t));
    l->capacity = 8;
    l->count = 0;
    l->items = (cvil_ast_node_t**)malloc(l->capacity * sizeof(cvil_ast_node_t*));
    return l;
}

void cvil_ast_list_append(cvil_ast_list_t* list, cvil_ast_node_t* item) {
    if (!list) return;
    if (list->count >= list->capacity) {
        list->capacity *= 2;
        list->items = (cvil_ast_node_t**)realloc(list->items, list->capacity * sizeof(cvil_ast_node_t*));
    }
    list->items[list->count++] = item;
}

cvil_ast_node_t* cvil_ast_new(cvil_ast_kind_t kind, cvil_span_t span) {
    cvil_ast_node_t* n = (cvil_ast_node_t*)calloc(1, sizeof(cvil_ast_node_t));
    n->kind = kind;
    n->span = span;
    return n;
}

cvil_ast_node_t* cvil_ast_program(cvil_ast_list_t* stmts, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_STMT_PROGRAM, span);
    n->as.program.stmts = stmts ? stmts : cvil_ast_list_create();
    return n;
}

cvil_ast_node_t* cvil_ast_block(cvil_ast_list_t* stmts, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_STMT_BLOCK, span);
    n->as.block.stmts = stmts ? stmts : cvil_ast_list_create();
    return n;
}

cvil_ast_node_t* cvil_ast_expr_stmt(cvil_ast_node_t* expr, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_STMT_EXPR, span);
    n->as.expr_stmt.expr = expr;
    return n;
}

cvil_ast_node_t* cvil_ast_val_stmt(bool is_rw, cvil_val_decl_t* decls, uint32_t count, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_STMT_VAL, span);
    n->as.val_stmt.is_rw = is_rw;
    n->as.val_stmt.decls = decls;
    n->as.val_stmt.count = count;
    return n;
}

cvil_ast_node_t* cvil_ast_literal_num(double val, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_LITERAL, span);
    n->as.literal_expr.lit_type = EVIL_TYPE_NUMBER;
    n->as.literal_expr.v.number = val;
    return n;
}

cvil_ast_node_t* cvil_ast_literal_hex(int val, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_LITERAL, span);
    n->as.literal_expr.lit_type = EVIL_TYPE_NUMBER;
    n->as.literal_expr.v.number = (double)val;
    n->as.literal_expr.is_hex = true;
    return n;
}

cvil_ast_node_t* cvil_ast_literal_str(const char* val, bool is_interpolated, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_LITERAL, span);
    n->as.literal_expr.lit_type = EVIL_TYPE_STRING;
    n->as.literal_expr.v.string_val = strdup(val);
    n->as.literal_expr.is_interpolated = is_interpolated;
    return n;
}

cvil_ast_node_t* cvil_ast_literal_bool(bool val, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_LITERAL, span);
    n->as.literal_expr.lit_type = EVIL_TYPE_BOOLEAN;
    n->as.literal_expr.v.boolean = val;
    return n;
}

cvil_ast_node_t* cvil_ast_literal_nil(cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_LITERAL, span);
    n->as.literal_expr.lit_type = EVIL_TYPE_NIL;
    return n;
}

cvil_ast_node_t* cvil_ast_literal_type(evil_type_t tc, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_LITERAL, span);
    n->as.literal_expr.lit_type = EVIL_TYPE_TYPE;
    n->as.literal_expr.v.type_code = tc;
    return n;
}

cvil_ast_node_t* cvil_ast_ident(const char* name, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_IDENTIFIER, span);
    n->as.ident_expr.name = strdup(name);
    return n;
}

cvil_ast_node_t* cvil_ast_binary(cvil_bin_op_t op, cvil_ast_node_t* l, cvil_ast_node_t* r, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_BINARY, span);
    n->as.binary_expr.op = op;
    n->as.binary_expr.left = l;
    n->as.binary_expr.right = r;
    return n;
}

cvil_ast_node_t* cvil_ast_unary(cvil_un_op_t op, cvil_ast_node_t* operand, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_UNARY, span);
    n->as.unary_expr.op = op;
    n->as.unary_expr.operand = operand;
    return n;
}

cvil_ast_node_t* cvil_ast_assign(cvil_assign_op_t op, cvil_ast_node_t* l, cvil_ast_node_t* r, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_ASSIGN, span);
    n->as.assign_expr.op = op;
    n->as.assign_expr.left = l;
    n->as.assign_expr.right = r;
    return n;
}

cvil_ast_node_t* cvil_ast_invocation(cvil_ast_node_t* callee, cvil_ast_list_t* args, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_INVOCATION, span);
    n->as.invocation_expr.callee = callee;
    n->as.invocation_expr.args = args ? args : cvil_ast_list_create();
    return n;
}

cvil_ast_node_t* cvil_ast_table(cvil_table_entry_node_t* entries, uint32_t count, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_TABLE, span);
    n->as.table_expr.entries = entries;
    n->as.table_expr.count = count;
    return n;
}

cvil_ast_node_t* cvil_ast_array(cvil_ast_node_t* size_expr, cvil_ast_list_t* elements, cvil_span_t span) {
    cvil_ast_node_t* n = cvil_ast_new(AST_EXPR_ARRAY, span);
    n->as.array_expr.size_expr = size_expr;
    n->as.array_expr.elements = elements ? elements : cvil_ast_list_create();
    return n;
}
