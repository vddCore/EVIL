#ifndef CVIL_AST_H
#define CVIL_AST_H

#include <stdint.h>
#include <stdbool.h>
#include "cvil_diag.h"
#include "cvil_value.h"

typedef enum {
    /* Statements */
    AST_STMT_PROGRAM,
    AST_STMT_VAL,
    AST_STMT_EXPR,
    AST_STMT_BLOCK,
    AST_STMT_IF,
    AST_STMT_WHILE,
    AST_STMT_DO_WHILE,
    AST_STMT_FOR,
    AST_STMT_EACH,
    AST_STMT_TRY,
    AST_STMT_THROW,
    AST_STMT_RETRY,
    AST_STMT_RET,
    AST_STMT_BREAK,
    AST_STMT_SKIP,
    AST_STMT_FN,

    /* Expressions */
    AST_EXPR_ASSIGN,
    AST_EXPR_CONDITIONAL,
    AST_EXPR_COALESCING,
    AST_EXPR_BINARY,
    AST_EXPR_UNARY,
    AST_EXPR_POSTFIX_INC_DEC,
    AST_EXPR_INVOCATION,
    AST_EXPR_SELF_INVOCATION,
    AST_EXPR_INDEXER,
    AST_EXPR_PROPERTY,
    AST_EXPR_ELVIS_PROPERTY,
    AST_EXPR_ELVIS_INDEXER,
    AST_EXPR_TABLE,
    AST_EXPR_ARRAY,
    AST_EXPR_FN,
    AST_EXPR_SELF_FN,
    AST_EXPR_BY,
    AST_EXPR_WITH,
    AST_EXPR_TYPEOF,
    AST_EXPR_YIELD,
    AST_EXPR_ERROR,
    AST_EXPR_IDENTIFIER,
    AST_EXPR_SELF,
    AST_EXPR_ELLIPSIS,
    AST_EXPR_VARARG_FORWARD,
    AST_EXPR_LITERAL
} cvil_ast_kind_t;

typedef enum {
    BIN_ADD, BIN_SUB, BIN_MUL, BIN_DIV, BIN_MOD,
    BIN_EQ, BIN_NE, BIN_DEQ, BIN_DNE,
    BIN_LT, BIN_LTE, BIN_GT, BIN_GTE,
    BIN_IN, BIN_NOT_IN,
    BIN_IS, BIN_IS_NOT,
    BIN_AND, BIN_OR,
    BIN_BAND, BIN_BOR, BIN_BXOR,
    BIN_SHL, BIN_SHR
} cvil_bin_op_t;

typedef enum {
    UN_PLUS, UN_MINUS, UN_NOT, UN_BNOT,
    UN_LEN, UN_TOSTR, UN_TONUM,
    UN_PRE_INC, UN_PRE_DEC,
    UN_POST_INC, UN_POST_DEC
} cvil_un_op_t;

typedef enum {
    ASSIGN_EQ,
    ASSIGN_ADD, ASSIGN_SUB, ASSIGN_MUL, ASSIGN_DIV, ASSIGN_MOD,
    ASSIGN_BAND, ASSIGN_BOR, ASSIGN_BXOR,
    ASSIGN_SHL, ASSIGN_SHR,
    ASSIGN_COALESCE
} cvil_assign_op_t;

typedef struct cvil_ast_node cvil_ast_node_t;

typedef struct cvil_ast_list {
    cvil_ast_node_t** items;
    uint32_t count;
    uint32_t capacity;
} cvil_ast_list_t;

/* Node List Operations */
cvil_ast_list_t* cvil_ast_list_create(void);
void cvil_ast_list_append(cvil_ast_list_t* list, cvil_ast_node_t* item);

/* Specific Structs */
typedef struct cvil_param {
    const char* name;
    cvil_ast_node_t* default_val;
    bool is_rw;
    bool is_self;
    cvil_span_t span;
} cvil_param_t;

typedef struct cvil_val_decl {
    const char* name;
    cvil_ast_node_t* init_val;
    cvil_span_t span;
} cvil_val_decl_t;

typedef struct cvil_table_entry_node {
    cvil_ast_node_t* key;
    cvil_ast_node_t* val;
    bool is_associate; /* '=>' */
    bool is_self_fn;
} cvil_table_entry_node_t;

typedef struct cvil_by_arm {
    cvil_ast_node_t* pattern;
    cvil_ast_node_t* body;
    bool is_deep_match; /* '=>' */
    bool is_else;
    bool is_throw;
} cvil_by_arm_t;

typedef struct cvil_elif_branch {
    cvil_ast_node_t* condition;
    cvil_ast_node_t* branch;
} cvil_elif_branch_t;

typedef struct cvil_attribute {
    const char* name;
    cvil_ast_list_t* args; /* list of cvil_ast_node_t* */
} cvil_attribute_t;

/* The Master AST Node */
struct cvil_ast_node {
    cvil_ast_kind_t kind;
    cvil_span_t span;
    union {
        /* Program & Block */
        struct {
            cvil_ast_list_t* stmts;
        } program, block;

        /* Expression Statement */
        struct {
            cvil_ast_node_t* expr;
        } expr_stmt;

        /* Val Statement */
        struct {
            bool is_rw;
            cvil_val_decl_t* decls;
            uint32_t count;
        } val_stmt;

        /* If Statement */
        struct {
            cvil_ast_node_t* condition;
            cvil_ast_node_t* if_branch;
            cvil_elif_branch_t* elifs;
            uint32_t elif_count;
            cvil_ast_node_t* else_branch;
        } if_stmt;

        /* While & Do-While */
        struct {
            cvil_ast_node_t* condition;
            cvil_ast_node_t* body;
        } while_stmt, do_while_stmt;

        /* For Statement */
        struct {
            cvil_ast_list_t* inits;
            cvil_ast_node_t* condition;
            cvil_ast_list_t* steps;
            cvil_ast_node_t* body;
        } for_stmt;

        /* Each Statement */
        struct {
            bool is_rw;
            const char* key_var;
            const char* val_var;
            cvil_ast_node_t* collection;
            cvil_ast_node_t* body;
        } each_stmt;

        /* Try / Catch */
        struct {
            cvil_ast_node_t* body;
            const char* err_var;
            cvil_ast_node_t* catch_body;
        } try_stmt;

        /* Throw / Return */
        struct {
            cvil_ast_node_t* expr;
        } throw_stmt, ret_stmt;

        /* Function Statement */
        struct {
            bool is_local;
            const char* name;
            const char* target_name; /* for Target::method */
            cvil_ast_list_t* target_chain; /* for t.a.b indexed functions */
            cvil_param_t* params;
            uint32_t param_count;
            cvil_ast_node_t* body;
            cvil_attribute_t* attrs;
            uint32_t attr_count;
            uint32_t unique_fn_id;
        } fn_stmt;

        /* Assignment Expression */
        struct {
            cvil_assign_op_t op;
            cvil_ast_node_t* left;
            cvil_ast_node_t* right;
        } assign_expr;

        /* Ternary / Conditional */
        struct {
            cvil_ast_node_t* cond;
            cvil_ast_node_t* true_expr;
            cvil_ast_node_t* false_expr;
        } conditional_expr;

        /* Coalescing Expression (??) */
        struct {
            cvil_ast_node_t* left;
            cvil_ast_node_t* right;
            bool is_throw;
        } coalescing_expr;

        /* Binary Expression */
        struct {
            cvil_bin_op_t op;
            cvil_ast_node_t* left;
            cvil_ast_node_t* right;
            evil_type_t is_type_target;
            const char* is_native_class; /* for `is NativeObject 'ClassName'` */
        } binary_expr;

        /* Unary Expression */
        struct {
            cvil_un_op_t op;
            cvil_ast_node_t* operand;
        } unary_expr;

        /* Invocations */
        struct {
            cvil_ast_node_t* callee;
            cvil_ast_list_t* args;
            bool is_variadic_forward; /* f(*) */
        } invocation_expr;

        /* Self Invocation (t::method(args)) */
        struct {
            cvil_ast_node_t* receiver;
            const char* method;
            cvil_ast_list_t* args;
        } self_invoc_expr;

        /* Indexer (t[k]) */
        struct {
            cvil_ast_node_t* target;
            cvil_ast_node_t* index;
        } indexer_expr;

        /* Property Access (t.prop) */
        struct {
            cvil_ast_node_t* target;
            const char* property;
        } prop_expr;

        /* Elvis / Safe Navigation (t?.prop, t?[idx]) */
        struct {
            cvil_ast_node_t* target;
            const char* property;
            cvil_ast_node_t* index;
            bool is_array;
        } elvis_expr;

        /* Table Expression */
        struct {
            cvil_table_entry_node_t* entries;
            uint32_t count;
        } table_expr;

        /* Array Expression */
        struct {
            cvil_ast_node_t* size_expr;
            cvil_ast_list_t* elements;
        } array_expr;

        /* Anonymous Function (fn (...) { ... } / fn (...) -> ...) */
        struct {
            cvil_param_t* params;
            uint32_t param_count;
            cvil_ast_node_t* body;
            bool is_self_fn;
            uint32_t unique_fn_id;
        } fn_expr;

        /* Pattern Matching (by) */
        struct {
            cvil_ast_node_t* target;
            cvil_by_arm_t* arms;
            uint32_t arm_count;
        } by_expr;

        /* Table Expansion (with) */
        struct {
            cvil_ast_node_t* base;
            cvil_ast_node_t* ext_table;
        } with_expr;

        /* TypeOf */
        struct {
            cvil_ast_node_t* expr;
            bool invert;
        } typeof_expr;

        /* Yield */
        struct {
            const char* target_name;
            cvil_ast_node_t* target_expr;
            cvil_ast_list_t* args;
        } yield_expr;

        /* Error */
        struct {
            cvil_ast_node_t* message;
            cvil_ast_node_t* table_data;
        } error_expr;

        /* Identifier */
        struct {
            const char* name;
        } ident_expr;

        /* Literal */
        struct {
            evil_type_t lit_type;
            union {
                double number;
                bool boolean;
                const char* string_val;
                int hex_int;
                evil_type_t type_code;
            } v;
            bool is_interpolated;
            bool is_hex;
        } literal_expr;
    } as;
};

/* AST Constructors */
uint32_t cvil_next_fn_id(void);
cvil_ast_node_t* cvil_ast_new(cvil_ast_kind_t kind, cvil_span_t span);
cvil_ast_node_t* cvil_ast_program(cvil_ast_list_t* stmts, cvil_span_t span);
cvil_ast_node_t* cvil_ast_block(cvil_ast_list_t* stmts, cvil_span_t span);
cvil_ast_node_t* cvil_ast_expr_stmt(cvil_ast_node_t* expr, cvil_span_t span);
cvil_ast_node_t* cvil_ast_val_stmt(bool is_rw, cvil_val_decl_t* decls, uint32_t count, cvil_span_t span);
cvil_ast_node_t* cvil_ast_literal_num(double val, cvil_span_t span);
cvil_ast_node_t* cvil_ast_literal_hex(int val, cvil_span_t span);
cvil_ast_node_t* cvil_ast_literal_str(const char* val, bool is_interpolated, cvil_span_t span);
cvil_ast_node_t* cvil_ast_literal_bool(bool val, cvil_span_t span);
cvil_ast_node_t* cvil_ast_literal_nil(cvil_span_t span);
cvil_ast_node_t* cvil_ast_literal_type(evil_type_t tc, cvil_span_t span);
cvil_ast_node_t* cvil_ast_ident(const char* name, cvil_span_t span);
cvil_ast_node_t* cvil_ast_binary(cvil_bin_op_t op, cvil_ast_node_t* l, cvil_ast_node_t* r, cvil_span_t span);
cvil_ast_node_t* cvil_ast_unary(cvil_un_op_t op, cvil_ast_node_t* operand, cvil_span_t span);
cvil_ast_node_t* cvil_ast_assign(cvil_assign_op_t op, cvil_ast_node_t* l, cvil_ast_node_t* r, cvil_span_t span);
cvil_ast_node_t* cvil_ast_invocation(cvil_ast_node_t* callee, cvil_ast_list_t* args, cvil_span_t span);
cvil_ast_node_t* cvil_ast_table(cvil_table_entry_node_t* entries, uint32_t count, cvil_span_t span);
cvil_ast_node_t* cvil_ast_array(cvil_ast_node_t* size_expr, cvil_ast_list_t* elements, cvil_span_t span);

#endif /* CVIL_AST_H */
