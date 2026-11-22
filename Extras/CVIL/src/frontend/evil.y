%{
/*
 * Bison AST Parser for CVIL Transpiler
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "cvil_ast.h"
#include "cvil_diag.h"

extern int yylex(void);
extern int yylineno;
extern int yycolumn;
extern char *yytext;
void yyerror(const char *s);

cvil_ast_node_t* g_ast_root = NULL;

#define LOC_SPAN(loc) ((cvil_span_t){ NULL, (loc).first_line, (loc).first_column, (loc).last_line, (loc).last_column })
%}

%define parse.error verbose
%locations
%glr-parser
%expect 8
%expect-rr 1

%union {
    cvil_ast_node_t* node;
    cvil_ast_list_t* list;
    const char* str;
    double num;
    int hex_val;
    evil_type_t type_code;
    cvil_param_t param;
    cvil_val_decl_t val_decl;
    cvil_table_entry_node_t table_entry;
    cvil_by_arm_t by_arm;
    cvil_attribute_t attribute;
    struct {
        cvil_val_decl_t* items;
        uint32_t count;
    } val_decl_list;
    struct {
        cvil_param_t* items;
        uint32_t count;
    } param_list;
    struct {
        cvil_table_entry_node_t* items;
        uint32_t count;
    } table_entry_list;
    struct {
        cvil_by_arm_t* items;
        uint32_t count;
    } by_arm_list;
    struct {
        cvil_attribute_t* items;
        uint32_t count;
    } attr_list;
    struct {
        cvil_elif_branch_t* items;
        uint32_t count;
    } elif_branch_list;
    struct {
        const char* name;
        const char* target_name;
        cvil_ast_list_t* chain;
    } fn_target_info;
    int fn_mod;
}

/* Keywords */
%token TOKEN_FN "fn"
%token TOKEN_LOC "loc"
%token TOKEN_FOR "for"
%token TOKEN_WHILE "while"
%token TOKEN_DO "do"
%token TOKEN_RW "rw"
%token TOKEN_VAL "val"
%token TOKEN_IN "in"
%token TOKEN_IF "if"
%token TOKEN_IS "is"
%token TOKEN_ELIF "elif"
%token TOKEN_ELSE "else"
%token TOKEN_BREAK "break"
%token TOKEN_SKIP "skip"
%token TOKEN_BY "by"
%token TOKEN_RET "ret"
%token TOKEN_TRUE "true"
%token TOKEN_FALSE "false"
%token TOKEN_NIL "nil"
%token TOKEN_TYPEOF "typeof"
%token TOKEN_TRY "try"
%token TOKEN_CATCH "catch"
%token TOKEN_THROW "throw"
%token TOKEN_YIELD "yield"
%token TOKEN_EACH "each"
%token TOKEN_ARRAY "array"
%token TOKEN_SELF "self"
%token TOKEN_ERROR "error"
%token TOKEN_RETRY "retry"
%token TOKEN_WITH "with"

/* Special numeric aliases */
%token TOKEN_NAN "NaN"
%token TOKEN_INFINITY "Infinity"

/* Type Identifiers */
%token <type_code> TYPE_NIL "Nil"
%token <type_code> TYPE_NUMBER "Number"
%token <type_code> TYPE_STRING "String"
%token <type_code> TYPE_BOOLEAN "Boolean"
%token <type_code> TYPE_TABLE "Table"
%token <type_code> TYPE_ARRAY "Array"
%token <type_code> TYPE_FIBER "Fiber"
%token <type_code> TYPE_CHUNK "Function"
%token <type_code> TYPE_ERROR "Error"
%token <type_code> TYPE_TYPE "Type"
%token <type_code> TYPE_NATIVE_FUNCTION "NativeFunction"
%token <type_code> TYPE_NATIVE_OBJECT "NativeObject"

/* Multi-character operators and symbols */
%token TOKEN_ASSIGN_ADD "+="
%token TOKEN_ASSIGN_SUBTRACT "-="
%token TOKEN_ASSIGN_MULTIPLY "*="
%token TOKEN_ASSIGN_DIVIDE "/="
%token TOKEN_ASSIGN_MODULO "%="
%token TOKEN_ASSIGN_BITWISE_AND "&="
%token TOKEN_ASSIGN_BITWISE_OR "|="
%token TOKEN_ASSIGN_BITWISE_XOR "^="
%token TOKEN_ASSIGN_SHIFT_RIGHT ">>="
%token TOKEN_ASSIGN_SHIFT_LEFT "<<="
%token TOKEN_ASSIGN_COALESCE "??="

%token TOKEN_DEEP_EQUAL "<==>"
%token TOKEN_DEEP_NOT_EQUAL "<!=>"
%token TOKEN_EQUAL "=="
%token TOKEN_NOT_EQUAL "!="
%token TOKEN_LESS_EQUAL "<="
%token TOKEN_GREATER_EQUAL ">="

%token TOKEN_SHIFT_LEFT "<<"
%token TOKEN_SHIFT_RIGHT ">>"
%token TOKEN_LOGICAL_AND "&&"
%token TOKEN_LOGICAL_OR "||"

%token TOKEN_INCREMENT "++"
%token TOKEN_DECREMENT "--"
%token TOKEN_ELLIPSIS "..."
%token TOKEN_ASSOCIATE "=>"
%token TOKEN_RIGHT_ARROW "->"

%token TOKEN_ELVIS "?."
%token TOKEN_ELVIS_ARRAY "?["
%token TOKEN_DOUBLE_QUESTION "??"
%token TOKEN_DOUBLE_COLON "::"

%token TOKEN_NOT_IN "!in"
%token TOKEN_IS_NOT "!is"
%token TOKEN_ATTRIBUTE_LIST "#["

/* Literals & Identifier */
%token <str> IDENTIFIER "identifier"
%token <num> TOKEN_NUMBER "number"
%token <hex_val> TOKEN_HEX_INTEGER "hex_integer"
%token <str> TOKEN_PLAIN_STRING "plain_string"
%token <str> TOKEN_INTERPOLATED_STRING "interpolated_string"

/* Operator Precedence and Associativity */
%precedence LOWER_THAN_ELSE
%precedence TOKEN_ELIF
%precedence TOKEN_ELSE
%precedence LOWER_THAN_CATCH
%precedence TOKEN_CATCH

%right '=' TOKEN_ASSIGN_ADD TOKEN_ASSIGN_SUBTRACT TOKEN_ASSIGN_MULTIPLY TOKEN_ASSIGN_DIVIDE TOKEN_ASSIGN_MODULO TOKEN_ASSIGN_BITWISE_AND TOKEN_ASSIGN_BITWISE_OR TOKEN_ASSIGN_BITWISE_XOR TOKEN_ASSIGN_SHIFT_RIGHT TOKEN_ASSIGN_SHIFT_LEFT TOKEN_ASSIGN_COALESCE
%right '?' ':'
%right TOKEN_DOUBLE_QUESTION
%left TOKEN_LOGICAL_OR
%left TOKEN_LOGICAL_AND
%left '|'
%left '^'
%left '&'
%left TOKEN_EQUAL TOKEN_NOT_EQUAL TOKEN_DEEP_EQUAL TOKEN_DEEP_NOT_EQUAL
%left '<' '>' TOKEN_LESS_EQUAL TOKEN_GREATER_EQUAL TOKEN_IN TOKEN_NOT_IN TOKEN_IS TOKEN_IS_NOT
%left TOKEN_SHIFT_LEFT TOKEN_SHIFT_RIGHT
%left '+' '-'
%left '*' '/' '%'
%left TOKEN_WITH
%precedence UNARY PREFIX
%precedence POSTFIX
%left '.' TOKEN_ELVIS TOKEN_ELVIS_ARRAY '[' '(' TOKEN_DOUBLE_COLON

/* Non-terminal type definitions */
%type <node> program statement expression_statement block_statement val_statement
%type <node> if_statement for_statement while_statement do_while_statement each_statement
%type <node> try_statement retry_statement throw_statement ret_statement skip_statement break_statement fn_statement
%type <node> if_branch else_opt
%type <node> assignment_expression conditional_expression coalescing_expression coalescing_rhs logical_or_expression
%type <node> logical_and_expression inclusive_or_expression exclusive_or_expression and_expression
%type <node> equality_expression relational_expression shift_expression additive_expression
%type <node> multiplicative_expression pattern_expression by_expression with_expression
%type <node> prefix_expression unary_expression postfix_expression primary_expression
%type <node> table_expression array_expression error_expression typeof_expression yield_expression
%type <node> constant_expression string_literal fn_block_expression fn_arrow_expression self_fn_expression fn_body
%type <node> by_value for_init_item

%type <list> statement_list statement_list_opt for_init_list for_init_list_opt for_step_list for_step_list_opt
%type <list> argument_list argument_list_opt arg_seq array_element_list_opt array_element_seq fn_name_chain attribute_arg_list

%type <val_decl_list> val_declarator_seq
%type <val_decl> val_declarator
%type <param_list> parameter_list parameter_list_opt param_seq
%type <param> param_item
%type <table_entry_list> table_entry_list_opt table_entry_seq
%type <table_entry> table_entry
%type <by_arm_list> by_arm_seq
%type <by_arm> by_arm
%type <elif_branch_list> elif_list
%type <attr_list> attribute_list_seq
%type <attribute> attribute_list attribute_item attribute_seq
%type <fn_target_info> fn_target
%type <fn_mod> fn_modifier
%type <node> attribute_arg
%type <node> is_native_opt
%type <type_code> type_code

%start program

%%

/* ========================================================================= */
/* Program & Statements                                                      */
/* ========================================================================= */

program
    : statement_list_opt
        { g_ast_root = cvil_ast_program($1, LOC_SPAN(@$)); $$ = g_ast_root; }
    ;

statement_list_opt
    : /* empty */
        { $$ = cvil_ast_list_create(); }
    | statement_list
        { $$ = $1; }
    ;

statement_list
    : statement
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, $1); }
    | statement_list statement
        { cvil_ast_list_append($1, $2); $$ = $1; }
    ;

statement
    : fn_statement          { $$ = $1; }
    | if_statement          { $$ = $1; }
    | for_statement         { $$ = $1; }
    | while_statement       { $$ = $1; }
    | do_while_statement    { $$ = $1; }
    | each_statement        { $$ = $1; }
    | block_statement       { $$ = $1; }
    | val_statement         { $$ = $1; }
    | try_statement         { $$ = $1; }
    | retry_statement       { $$ = $1; }
    | throw_statement       { $$ = $1; }
    | ret_statement         { $$ = $1; }
    | skip_statement        { $$ = $1; }
    | break_statement       { $$ = $1; }
    | expression_statement  { $$ = $1; }
    ;

expression_statement
    : assignment_expression ';'
        { $$ = cvil_ast_expr_stmt($1, LOC_SPAN(@$)); }
    ;

block_statement
    : '{' statement_list_opt '}'
        { $$ = cvil_ast_block($2, LOC_SPAN(@$)); }
    ;

retry_statement
    : TOKEN_RETRY ';'
        { $$ = cvil_ast_new(AST_STMT_RETRY, LOC_SPAN(@$)); }
    ;

throw_statement
    : TOKEN_THROW assignment_expression ';'
        { $$ = cvil_ast_new(AST_STMT_THROW, LOC_SPAN(@$)); $$->as.throw_stmt.expr = $2; }
    ;

ret_statement
    : TOKEN_RET ';'
        { $$ = cvil_ast_new(AST_STMT_RET, LOC_SPAN(@$)); $$->as.ret_stmt.expr = NULL; }
    | TOKEN_RET assignment_expression ';'
        { $$ = cvil_ast_new(AST_STMT_RET, LOC_SPAN(@$)); $$->as.ret_stmt.expr = $2; }
    ;

skip_statement
    : TOKEN_SKIP ';'
        { $$ = cvil_ast_new(AST_STMT_SKIP, LOC_SPAN(@$)); }
    ;

break_statement
    : TOKEN_BREAK ';'
        { $$ = cvil_ast_new(AST_STMT_BREAK, LOC_SPAN(@$)); }
    ;

while_statement
    : TOKEN_WHILE '(' assignment_expression ')' statement
        { $$ = cvil_ast_new(AST_STMT_WHILE, LOC_SPAN(@$)); $$->as.while_stmt.condition = $3; $$->as.while_stmt.body = $5; }
    ;

do_while_statement
    : TOKEN_DO statement TOKEN_WHILE '(' assignment_expression ')' ';'
        { $$ = cvil_ast_new(AST_STMT_DO_WHILE, LOC_SPAN(@$)); $$->as.do_while_stmt.body = $2; $$->as.do_while_stmt.condition = $5; }
    ;

/* Val Statement */
val_statement
    : TOKEN_VAL val_declarator_seq ';'
        { $$ = cvil_ast_val_stmt(false, $2.items, $2.count, LOC_SPAN(@$)); }
    | TOKEN_RW TOKEN_VAL val_declarator_seq ';'
        { $$ = cvil_ast_val_stmt(true, $3.items, $3.count, LOC_SPAN(@$)); }
    ;

val_declarator_seq
    : val_declarator
        {
            $$.items = (cvil_val_decl_t*)malloc(sizeof(cvil_val_decl_t));
            $$.items[0] = $1;
            $$.count = 1;
        }
    | val_declarator_seq ',' val_declarator
        {
            $$.items = (cvil_val_decl_t*)realloc($1.items, ($1.count + 1) * sizeof(cvil_val_decl_t));
            $$.items[$1.count] = $3;
            $$.count = $1.count + 1;
        }
    ;

val_declarator
    : IDENTIFIER
        { $$.name = $1; $$.init_val = NULL; $$.span = LOC_SPAN(@$); }
    | IDENTIFIER '=' assignment_expression
        { $$.name = $1; $$.init_val = $3; $$.span = LOC_SPAN(@$); }
    ;

/* Each Statement */
each_statement
    : TOKEN_EACH '(' TOKEN_RW TOKEN_VAL IDENTIFIER ':' assignment_expression ')' statement
        {
            $$ = cvil_ast_new(AST_STMT_EACH, LOC_SPAN(@$));
            $$->as.each_stmt.is_rw = true;
            $$->as.each_stmt.key_var = NULL;
            $$->as.each_stmt.val_var = $5;
            $$->as.each_stmt.collection = $7;
            $$->as.each_stmt.body = $9;
        }
    | TOKEN_EACH '(' TOKEN_RW TOKEN_VAL IDENTIFIER ',' IDENTIFIER ':' assignment_expression ')' statement
        {
            $$ = cvil_ast_new(AST_STMT_EACH, LOC_SPAN(@$));
            $$->as.each_stmt.is_rw = true;
            $$->as.each_stmt.key_var = $5;
            $$->as.each_stmt.val_var = $7;
            $$->as.each_stmt.collection = $9;
            $$->as.each_stmt.body = $11;
        }
    ;

/* For Statement */
for_statement
    : TOKEN_FOR '(' for_init_list_opt ';' assignment_expression ';' for_step_list_opt ')' statement
        {
            $$ = cvil_ast_new(AST_STMT_FOR, LOC_SPAN(@$));
            $$->as.for_stmt.inits = $3;
            $$->as.for_stmt.condition = $5;
            $$->as.for_stmt.steps = $7;
            $$->as.for_stmt.body = $9;
        }
    ;

for_init_list_opt
    : /* empty */ { $$ = cvil_ast_list_create(); }
    | for_init_list { $$ = $1; }
    ;

for_init_list
    : for_init_item
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, $1); }
    | for_init_list ',' for_init_item
        { cvil_ast_list_append($1, $3); $$ = $1; }
    ;

for_init_item
    : TOKEN_VAL val_declarator_seq
        { $$ = cvil_ast_val_stmt(false, $2.items, $2.count, LOC_SPAN(@$)); }
    | TOKEN_RW TOKEN_VAL val_declarator_seq
        { $$ = cvil_ast_val_stmt(true, $3.items, $3.count, LOC_SPAN(@$)); }
    | assignment_expression
        { $$ = $1; }
    ;

for_step_list_opt
    : /* empty */ { $$ = cvil_ast_list_create(); }
    | for_step_list { $$ = $1; }
    ;

for_step_list
    : assignment_expression
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, $1); }
    | for_step_list ',' assignment_expression
        { cvil_ast_list_append($1, $3); $$ = $1; }
    ;

/* If Statement */
if_statement
    : TOKEN_IF '(' assignment_expression ')' if_branch elif_list else_opt
        {
            $$ = cvil_ast_new(AST_STMT_IF, LOC_SPAN(@$));
            $$->as.if_stmt.condition = $3;
            $$->as.if_stmt.if_branch = $5;
            $$->as.if_stmt.elifs = $6.items;
            $$->as.if_stmt.elif_count = $6.count;
            $$->as.if_stmt.else_branch = $7;
        }
    ;

elif_list
    : /* empty */
        { $$.items = NULL; $$.count = 0; }
    | elif_list TOKEN_ELIF '(' assignment_expression ')' if_branch
        {
            $$.items = (cvil_elif_branch_t*)realloc($1.items, ($1.count + 1) * sizeof(cvil_elif_branch_t));
            $$.items[$1.count].condition = $4;
            $$.items[$1.count].branch = $6;
            $$.count = $1.count + 1;
        }
    ;

else_opt
    : /* empty */ %prec LOWER_THAN_ELSE { $$ = NULL; }
    | TOKEN_ELSE if_branch              { $$ = $2; }
    ;

if_branch
    : statement
        { $$ = $1; }
    | TOKEN_RIGHT_ARROW assignment_expression ';'
        { $$ = cvil_ast_new(AST_STMT_RET, LOC_SPAN(@$)); $$->as.ret_stmt.expr = $2; }
    ;

/* Try Statement */
try_statement
    : TOKEN_TRY statement catch_clause_opt
        {
            $$ = cvil_ast_new(AST_STMT_TRY, LOC_SPAN(@$));
            $$->as.try_stmt.body = $2;
            $$->as.try_stmt.err_var = NULL;
            $$->as.try_stmt.catch_body = NULL;
        }
    | TOKEN_TRY statement TOKEN_CATCH block_statement
        {
            $$ = cvil_ast_new(AST_STMT_TRY, LOC_SPAN(@$));
            $$->as.try_stmt.body = $2;
            $$->as.try_stmt.err_var = NULL;
            $$->as.try_stmt.catch_body = $4;
        }
    | TOKEN_TRY statement TOKEN_CATCH '(' IDENTIFIER ')' block_statement
        {
            $$ = cvil_ast_new(AST_STMT_TRY, LOC_SPAN(@$));
            $$->as.try_stmt.body = $2;
            $$->as.try_stmt.err_var = $5;
            $$->as.try_stmt.catch_body = $7;
        }
    ;

catch_clause_opt
    : /* empty */ %prec LOWER_THAN_CATCH
    ;

/* Function Statement */
fn_statement
    : attribute_list_seq fn_modifier fn_target parameter_list_opt fn_body
        {
            $$ = cvil_ast_new(AST_STMT_FN, LOC_SPAN(@$));
            $$->as.fn_stmt.is_local = $2;
            $$->as.fn_stmt.name = $3.name;
            $$->as.fn_stmt.target_name = $3.target_name;
            $$->as.fn_stmt.target_chain = $3.chain;
            if ($3.target_name) {
                cvil_param_t* p = (cvil_param_t*)calloc($4.count + 1, sizeof(cvil_param_t));
                p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN(@3) };
                for (uint32_t i = 0; i < $4.count; i++) p[i + 1] = $4.items[i];
                $$->as.fn_stmt.params = p;
                $$->as.fn_stmt.param_count = $4.count + 1;
            } else {
                $$->as.fn_stmt.params = $4.items;
                $$->as.fn_stmt.param_count = $4.count;
            }
            $$->as.fn_stmt.body = $5;
            $$->as.fn_stmt.attrs = $1.items;
            $$->as.fn_stmt.attr_count = $1.count;
            $$->as.fn_stmt.unique_fn_id = cvil_next_fn_id();
        }
    | fn_modifier fn_target parameter_list_opt fn_body
        {
            $$ = cvil_ast_new(AST_STMT_FN, LOC_SPAN(@$));
            $$->as.fn_stmt.is_local = $1;
            $$->as.fn_stmt.name = $2.name;
            $$->as.fn_stmt.target_name = $2.target_name;
            $$->as.fn_stmt.target_chain = $2.chain;
            if ($2.target_name) {
                cvil_param_t* p = (cvil_param_t*)calloc($3.count + 1, sizeof(cvil_param_t));
                p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN(@2) };
                for (uint32_t i = 0; i < $3.count; i++) p[i + 1] = $3.items[i];
                $$->as.fn_stmt.params = p;
                $$->as.fn_stmt.param_count = $3.count + 1;
            } else {
                $$->as.fn_stmt.params = $3.items;
                $$->as.fn_stmt.param_count = $3.count;
            }
            $$->as.fn_stmt.body = $4;
            $$->as.fn_stmt.attrs = NULL;
            $$->as.fn_stmt.attr_count = 0;
            $$->as.fn_stmt.unique_fn_id = cvil_next_fn_id();
        }
    ;

attribute_list_seq
    : attribute_list
        {
            $$.items = (cvil_attribute_t*)malloc(sizeof(cvil_attribute_t));
            $$.items[0] = $1;
            $$.count = 1;
        }
    | attribute_list_seq attribute_list
        {
            $$.items = (cvil_attribute_t*)realloc($1.items, ($1.count + 1) * sizeof(cvil_attribute_t));
            $$.items[$1.count] = $2;
            $$.count = $1.count + 1;
        }
    ;

attribute_list
    : TOKEN_ATTRIBUTE_LIST attribute_seq ']'
        { $$ = $2; }
    ;

attribute_seq
    : attribute_item { $$ = $1; }
    | attribute_seq ';' attribute_item { $$ = $3; }
    ;

attribute_item
    : IDENTIFIER
        { $$.name = $1; $$.args = NULL; }
    | IDENTIFIER '(' ')'
        { $$.name = $1; $$.args = NULL; }
    | IDENTIFIER '(' attribute_arg_list ')'
        { $$.name = $1; $$.args = $3; }
    ;

attribute_arg_list
    : attribute_arg
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, $1); }
    | attribute_arg_list ',' attribute_arg
        { cvil_ast_list_append($1, $3); $$ = $1; }
    ;

attribute_arg
    : assignment_expression { $$ = $1; }
    | IDENTIFIER '=' assignment_expression { $$ = $3; }
    ;

fn_modifier
    : TOKEN_FN { $$ = 0; }
    | TOKEN_LOC TOKEN_FN { $$ = 1; }
    ;

fn_target
    : fn_name_chain
        {
            if ($1->count == 1) {
                $$.name = $1->items[0]->as.ident_expr.name;
                $$.target_name = NULL;
                $$.chain = NULL;
            } else {
                $$.name = $1->items[$1->count - 1]->as.ident_expr.name;
                $$.target_name = NULL;
                $$.chain = $1;
            }
        }
    | fn_name_chain TOKEN_DOUBLE_COLON IDENTIFIER
        {
            $$.target_name = $1->items[0]->as.ident_expr.name;
            $$.name = $3;
            $$.chain = NULL;
        }
    | TOKEN_SELF TOKEN_DOUBLE_COLON IDENTIFIER
        {
            $$.target_name = "self";
            $$.name = $3;
            $$.chain = NULL;
        }
    ;

fn_name_chain
    : IDENTIFIER
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, cvil_ast_ident($1, LOC_SPAN(@$))); }
    | fn_name_chain '.' IDENTIFIER
        { cvil_ast_list_append($1, cvil_ast_ident($3, LOC_SPAN(@$))); $$ = $1; }
    ;

parameter_list_opt
    : /* empty */ { $$.items = NULL; $$.count = 0; }
    | '(' parameter_list ')' { $$ = $2; }
    ;

parameter_list
    : /* empty */
        { $$.items = NULL; $$.count = 0; }
    | TOKEN_SELF
        {
            $$.items = (cvil_param_t*)malloc(sizeof(cvil_param_t));
            $$.items[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN(@$) };
            $$.count = 1;
        }
    | TOKEN_SELF ',' param_seq
        {
            $$.items = (cvil_param_t*)realloc($3.items, ($3.count + 1) * sizeof(cvil_param_t));
            memmove(&$$.items[1], &$$.items[0], $3.count * sizeof(cvil_param_t));
            $$.items[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN(@1) };
            $$.count = $3.count + 1;
        }
    | param_seq
        { $$ = $1; }
    ;

param_seq
    : param_item
        {
            $$.items = (cvil_param_t*)malloc(sizeof(cvil_param_t));
            $$.items[0] = $1;
            $$.count = 1;
        }
    | param_seq ',' param_item
        {
            $$.items = (cvil_param_t*)realloc($1.items, ($1.count + 1) * sizeof(cvil_param_t));
            $$.items[$1.count] = $3;
            $$.count = $1.count + 1;
        }
    ;

param_item
    : IDENTIFIER
        { $$ = (cvil_param_t){ .name = $1, .default_val = NULL, .is_rw = false, .is_self = false, .span = LOC_SPAN(@$) }; }
    | IDENTIFIER '=' assignment_expression
        { $$ = (cvil_param_t){ .name = $1, .default_val = $3, .is_rw = false, .is_self = false, .span = LOC_SPAN(@$) }; }
    | TOKEN_RW IDENTIFIER
        { $$ = (cvil_param_t){ .name = $2, .default_val = NULL, .is_rw = true, .is_self = false, .span = LOC_SPAN(@$) }; }
    | TOKEN_RW IDENTIFIER '=' assignment_expression
        { $$ = (cvil_param_t){ .name = $2, .default_val = $4, .is_rw = true, .is_self = false, .span = LOC_SPAN(@$) }; }
    ;

fn_body
    : block_statement
        { $$ = $1; }
    | TOKEN_RIGHT_ARROW assignment_expression ';'
        { $$ = cvil_ast_new(AST_STMT_RET, LOC_SPAN(@$)); $$->as.ret_stmt.expr = $2; }
    ;

/* ========================================================================= */
/* Expressions                                                               */
/* ========================================================================= */

assignment_expression
    : conditional_expression
        { $$ = $1; }
    | unary_expression '=' assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_EQ, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_ADD assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_ADD, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_SUBTRACT assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_SUB, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_MULTIPLY assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_MUL, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_DIVIDE assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_DIV, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_MODULO assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_MOD, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_BITWISE_AND assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_BAND, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_BITWISE_OR assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_BOR, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_BITWISE_XOR assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_BXOR, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_SHIFT_LEFT assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_SHL, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_SHIFT_RIGHT assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_SHR, $1, $3, LOC_SPAN(@$)); }
    | unary_expression TOKEN_ASSIGN_COALESCE assignment_expression
        { $$ = cvil_ast_assign(ASSIGN_COALESCE, $1, $3, LOC_SPAN(@$)); }
    | fn_arrow_expression
        { $$ = $1; }
    ;

fn_arrow_expression
    : TOKEN_FN parameter_list_opt TOKEN_RIGHT_ARROW assignment_expression
        {
            $$ = cvil_ast_new(AST_EXPR_FN, LOC_SPAN(@$));
            $$->as.fn_expr.params = $2.items;
            $$->as.fn_expr.param_count = $2.count;
            cvil_ast_node_t* ret = cvil_ast_new(AST_STMT_RET, LOC_SPAN(@4));
            ret->as.ret_stmt.expr = $4;
            cvil_ast_list_t* stmts = cvil_ast_list_create();
            cvil_ast_list_append(stmts, ret);
            $$->as.fn_expr.body = cvil_ast_block(stmts, LOC_SPAN(@4));
            $$->as.fn_expr.is_self_fn = false;
            $$->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
    ;

conditional_expression
    : coalescing_expression
        { $$ = $1; }
    | coalescing_expression '?' assignment_expression ':' conditional_expression
        {
            $$ = cvil_ast_new(AST_EXPR_CONDITIONAL, LOC_SPAN(@$));
            $$->as.conditional_expr.cond = $1;
            $$->as.conditional_expr.true_expr = $3;
            $$->as.conditional_expr.false_expr = $5;
        }
    ;

coalescing_expression
    : logical_or_expression
        { $$ = $1; }
    | logical_or_expression TOKEN_DOUBLE_QUESTION coalescing_rhs
        {
            $$ = cvil_ast_new(AST_EXPR_COALESCING, LOC_SPAN(@$));
            $$->as.coalescing_expr.left = $1;
            $$->as.coalescing_expr.right = $3;
            $$->as.coalescing_expr.is_throw = ($3->kind == AST_STMT_THROW);
        }
    ;

coalescing_rhs
    : assignment_expression { $$ = $1; }
    | TOKEN_THROW assignment_expression
        {
            $$ = cvil_ast_new(AST_STMT_THROW, LOC_SPAN(@$));
            $$->as.throw_stmt.expr = $2;
        }
    ;

logical_or_expression
    : logical_and_expression { $$ = $1; }
    | logical_or_expression TOKEN_LOGICAL_OR logical_and_expression
        { $$ = cvil_ast_binary(BIN_OR, $1, $3, LOC_SPAN(@$)); }
    ;

logical_and_expression
    : inclusive_or_expression { $$ = $1; }
    | logical_and_expression TOKEN_LOGICAL_AND inclusive_or_expression
        { $$ = cvil_ast_binary(BIN_AND, $1, $3, LOC_SPAN(@$)); }
    ;

inclusive_or_expression
    : exclusive_or_expression { $$ = $1; }
    | inclusive_or_expression '|' exclusive_or_expression
        { $$ = cvil_ast_binary(BIN_BOR, $1, $3, LOC_SPAN(@$)); }
    ;

exclusive_or_expression
    : and_expression { $$ = $1; }
    | exclusive_or_expression '^' and_expression
        { $$ = cvil_ast_binary(BIN_BXOR, $1, $3, LOC_SPAN(@$)); }
    ;

and_expression
    : equality_expression { $$ = $1; }
    | and_expression '&' equality_expression
        { $$ = cvil_ast_binary(BIN_BAND, $1, $3, LOC_SPAN(@$)); }
    ;

equality_expression
    : relational_expression { $$ = $1; }
    | equality_expression TOKEN_EQUAL relational_expression
        { $$ = cvil_ast_binary(BIN_EQ, $1, $3, LOC_SPAN(@$)); }
    | equality_expression TOKEN_NOT_EQUAL relational_expression
        { $$ = cvil_ast_binary(BIN_NE, $1, $3, LOC_SPAN(@$)); }
    | equality_expression TOKEN_DEEP_EQUAL relational_expression
        { $$ = cvil_ast_binary(BIN_DEQ, $1, $3, LOC_SPAN(@$)); }
    | equality_expression TOKEN_DEEP_NOT_EQUAL relational_expression
        { $$ = cvil_ast_binary(BIN_DNE, $1, $3, LOC_SPAN(@$)); }
    ;

relational_expression
    : shift_expression { $$ = $1; }
    | relational_expression '<' shift_expression
        { $$ = cvil_ast_binary(BIN_LT, $1, $3, LOC_SPAN(@$)); }
    | relational_expression '>' shift_expression
        { $$ = cvil_ast_binary(BIN_GT, $1, $3, LOC_SPAN(@$)); }
    | relational_expression TOKEN_LESS_EQUAL shift_expression
        { $$ = cvil_ast_binary(BIN_LTE, $1, $3, LOC_SPAN(@$)); }
    | relational_expression TOKEN_GREATER_EQUAL shift_expression
        { $$ = cvil_ast_binary(BIN_GTE, $1, $3, LOC_SPAN(@$)); }
    | relational_expression TOKEN_IN shift_expression
        { $$ = cvil_ast_binary(BIN_IN, $1, $3, LOC_SPAN(@$)); }
    | relational_expression TOKEN_NOT_IN shift_expression
        { $$ = cvil_ast_binary(BIN_NOT_IN, $1, $3, LOC_SPAN(@$)); }
    | relational_expression TOKEN_IS type_code is_native_opt
        {
            $$ = cvil_ast_binary(BIN_IS, $1, $4, LOC_SPAN(@$));
            $$->as.binary_expr.is_type_target = $3;
        }
    | relational_expression TOKEN_IS_NOT type_code is_native_opt
        {
            $$ = cvil_ast_binary(BIN_IS_NOT, $1, $4, LOC_SPAN(@$));
            $$->as.binary_expr.is_type_target = $3;
        }
    ;

is_native_opt
    : /* empty */ { $$ = NULL; }
    | string_literal
        { $$ = $1; }
    ;

type_code
    : TYPE_NIL            { $$ = EVIL_TYPE_NIL; }
    | TYPE_NUMBER         { $$ = EVIL_TYPE_NUMBER; }
    | TYPE_STRING         { $$ = EVIL_TYPE_STRING; }
    | TYPE_BOOLEAN        { $$ = EVIL_TYPE_BOOLEAN; }
    | TYPE_TABLE          { $$ = EVIL_TYPE_TABLE; }
    | TYPE_ARRAY          { $$ = EVIL_TYPE_ARRAY; }
    | TYPE_FIBER          { $$ = EVIL_TYPE_FIBER; }
    | TYPE_CHUNK          { $$ = EVIL_TYPE_FUNCTION; }
    | TYPE_ERROR          { $$ = EVIL_TYPE_ERROR; }
    | TYPE_TYPE           { $$ = EVIL_TYPE_TYPE; }
    | TYPE_NATIVE_FUNCTION { $$ = EVIL_TYPE_NATIVE_FUNCTION; }
    | TYPE_NATIVE_OBJECT  { $$ = EVIL_TYPE_NATIVE_OBJECT; }
    ;

shift_expression
    : additive_expression { $$ = $1; }
    | shift_expression TOKEN_SHIFT_LEFT additive_expression
        { $$ = cvil_ast_binary(BIN_SHL, $1, $3, LOC_SPAN(@$)); }
    | shift_expression TOKEN_SHIFT_RIGHT additive_expression
        { $$ = cvil_ast_binary(BIN_SHR, $1, $3, LOC_SPAN(@$)); }
    ;

additive_expression
    : multiplicative_expression { $$ = $1; }
    | additive_expression '+' multiplicative_expression
        { $$ = cvil_ast_binary(BIN_ADD, $1, $3, LOC_SPAN(@$)); }
    | additive_expression '-' multiplicative_expression
        { $$ = cvil_ast_binary(BIN_SUB, $1, $3, LOC_SPAN(@$)); }
    ;

multiplicative_expression
    : pattern_expression { $$ = $1; }
    | multiplicative_expression '*' pattern_expression
        { $$ = cvil_ast_binary(BIN_MUL, $1, $3, LOC_SPAN(@$)); }
    | multiplicative_expression '/' pattern_expression
        { $$ = cvil_ast_binary(BIN_DIV, $1, $3, LOC_SPAN(@$)); }
    | multiplicative_expression '%' pattern_expression
        { $$ = cvil_ast_binary(BIN_MOD, $1, $3, LOC_SPAN(@$)); }
    ;

pattern_expression
    : by_expression   { $$ = $1; }
    | with_expression { $$ = $1; }
    ;

by_expression
    : TOKEN_BY assignment_expression '{' by_arm_seq comma_opt '}'
        {
            $$ = cvil_ast_new(AST_EXPR_BY, LOC_SPAN(@$));
            $$->as.by_expr.target = $2;
            $$->as.by_expr.arms = $4.items;
            $$->as.by_expr.arm_count = $4.count;
        }
    ;

by_arm_seq
    : by_arm
        {
            $$.items = (cvil_by_arm_t*)malloc(sizeof(cvil_by_arm_t));
            $$.items[0] = $1;
            $$.count = 1;
        }
    | by_arm_seq ',' by_arm
        {
            $$.items = (cvil_by_arm_t*)realloc($1.items, ($1.count + 1) * sizeof(cvil_by_arm_t));
            $$.items[$1.count] = $3;
            $$.count = $1.count + 1;
        }
    ;

by_arm
    : assignment_expression TOKEN_RIGHT_ARROW by_value
        { $$ = (cvil_by_arm_t){ .pattern = $1, .body = $3, .is_deep_match = false, .is_else = false, .is_throw = ($3->kind == AST_STMT_THROW) }; }
    | assignment_expression TOKEN_ASSOCIATE by_value
        { $$ = (cvil_by_arm_t){ .pattern = $1, .body = $3, .is_deep_match = true, .is_else = false, .is_throw = ($3->kind == AST_STMT_THROW) }; }
    | TOKEN_ELSE ':' by_value
        { $$ = (cvil_by_arm_t){ .pattern = NULL, .body = $3, .is_deep_match = false, .is_else = true, .is_throw = ($3->kind == AST_STMT_THROW) }; }
    ;

by_value
    : assignment_expression { $$ = $1; }
    | TOKEN_THROW assignment_expression
        {
            $$ = cvil_ast_new(AST_STMT_THROW, LOC_SPAN(@$));
            $$->as.throw_stmt.expr = $2;
        }
    ;

with_expression
    : prefix_expression { $$ = $1; }
    | with_expression TOKEN_WITH table_expression
        {
            $$ = cvil_ast_new(AST_EXPR_WITH, LOC_SPAN(@$));
            $$->as.with_expr.base = $1;
            $$->as.with_expr.ext_table = $3;
        }
    ;

prefix_expression
    : TOKEN_INCREMENT unary_expression
        { $$ = cvil_ast_unary(UN_PRE_INC, $2, LOC_SPAN(@$)); }
    | TOKEN_DECREMENT unary_expression
        { $$ = cvil_ast_unary(UN_PRE_DEC, $2, LOC_SPAN(@$)); }
    | unary_expression
        { $$ = $1; }
    ;

unary_expression
    : postfix_expression { $$ = $1; }
    | '+' unary_expression %prec UNARY
        { $$ = cvil_ast_unary(UN_PLUS, $2, LOC_SPAN(@$)); }
    | '-' unary_expression %prec UNARY
        { $$ = cvil_ast_unary(UN_MINUS, $2, LOC_SPAN(@$)); }
    | '!' unary_expression %prec UNARY
        { $$ = cvil_ast_unary(UN_NOT, $2, LOC_SPAN(@$)); }
    | '~' unary_expression %prec UNARY
        { $$ = cvil_ast_unary(UN_BNOT, $2, LOC_SPAN(@$)); }
    | '@' unary_expression %prec UNARY
        { $$ = cvil_ast_unary(UN_TOSTR, $2, LOC_SPAN(@$)); }
    | '#' unary_expression %prec UNARY
        { $$ = cvil_ast_unary(UN_LEN, $2, LOC_SPAN(@$)); }
    | '$' unary_expression %prec UNARY
        { $$ = cvil_ast_unary(UN_TONUM, $2, LOC_SPAN(@$)); }
    ;

postfix_expression
    : primary_expression { $$ = $1; }
    | postfix_expression '(' argument_list_opt ')'
        { $$ = cvil_ast_invocation($1, $3, LOC_SPAN(@$)); }
    | postfix_expression '[' assignment_expression ']'
        {
            $$ = cvil_ast_new(AST_EXPR_INDEXER, LOC_SPAN(@$));
            $$->as.indexer_expr.target = $1;
            $$->as.indexer_expr.index = $3;
        }
    | postfix_expression '.' IDENTIFIER
        {
            $$ = cvil_ast_new(AST_EXPR_PROPERTY, LOC_SPAN(@$));
            $$->as.prop_expr.target = $1;
            $$->as.prop_expr.property = $3;
        }
    | postfix_expression TOKEN_ELVIS IDENTIFIER
        {
            $$ = cvil_ast_new(AST_EXPR_ELVIS_PROPERTY, LOC_SPAN(@$));
            $$->as.elvis_expr.target = $1;
            $$->as.elvis_expr.property = $3;
            $$->as.elvis_expr.is_array = false;
        }
    | postfix_expression TOKEN_ELVIS_ARRAY assignment_expression ']'
        {
            $$ = cvil_ast_new(AST_EXPR_ELVIS_INDEXER, LOC_SPAN(@$));
            $$->as.elvis_expr.target = $1;
            $$->as.elvis_expr.index = $3;
            $$->as.elvis_expr.is_array = true;
        }
    | postfix_expression TOKEN_DOUBLE_COLON IDENTIFIER '(' argument_list_opt ')'
        {
            $$ = cvil_ast_new(AST_EXPR_SELF_INVOCATION, LOC_SPAN(@$));
            $$->as.self_invoc_expr.receiver = $1;
            $$->as.self_invoc_expr.method = $3;
            $$->as.self_invoc_expr.args = $5;
        }
    | postfix_expression TOKEN_INCREMENT %prec POSTFIX
        { $$ = cvil_ast_unary(UN_POST_INC, $1, LOC_SPAN(@$)); }
    | postfix_expression TOKEN_DECREMENT %prec POSTFIX
        { $$ = cvil_ast_unary(UN_POST_DEC, $1, LOC_SPAN(@$)); }
    ;

argument_list_opt
    : /* empty */ { $$ = cvil_ast_list_create(); }
    | argument_list { $$ = $1; }
    ;

argument_list
    : arg_seq comma_opt { $$ = $1; }
    ;

arg_seq
    : assignment_expression
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, $1); }
    | '*'
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, cvil_ast_new(AST_EXPR_VARARG_FORWARD, LOC_SPAN(@$))); }
    | arg_seq ',' assignment_expression
        { cvil_ast_list_append($1, $3); $$ = $1; }
    | arg_seq ',' '*'
        { cvil_ast_list_append($1, cvil_ast_new(AST_EXPR_VARARG_FORWARD, LOC_SPAN(@$))); $$ = $1; }
    ;

primary_expression
    : '(' assignment_expression ')' { $$ = $2; }
    | table_expression             { $$ = $1; }
    | array_expression             { $$ = $1; }
    | error_expression             { $$ = $1; }
    | TOKEN_ELLIPSIS               { $$ = cvil_ast_new(AST_EXPR_ELLIPSIS, LOC_SPAN(@$)); }
    | fn_block_expression          { $$ = $1; }
    | IDENTIFIER                   { $$ = cvil_ast_ident($1, LOC_SPAN(@$)); }
    | TOKEN_SELF                   { $$ = cvil_ast_new(AST_EXPR_SELF, LOC_SPAN(@$)); }
    | typeof_expression            { $$ = $1; }
    | yield_expression             { $$ = $1; }
    | constant_expression          { $$ = $1; }
    ;

table_expression
    : '{' table_entry_list_opt '}'
        { $$ = cvil_ast_table($2.items, $2.count, LOC_SPAN(@$)); }
    ;

table_entry_list_opt
    : /* empty */
        { $$.items = NULL; $$.count = 0; }
    | table_entry_seq comma_opt
        { $$ = $1; }
    ;

table_entry_seq
    : table_entry
        {
            $$.items = (cvil_table_entry_node_t*)malloc(sizeof(cvil_table_entry_node_t));
            $$.items[0] = $1;
            $$.count = 1;
        }
    | table_entry_seq ',' table_entry
        {
            $$.items = (cvil_table_entry_node_t*)realloc($1.items, ($1.count + 1) * sizeof(cvil_table_entry_node_t));
            $$.items[$1.count] = $3;
            $$.count = $1.count + 1;
        }
    ;

table_entry
    : assignment_expression
        { $$ = (cvil_table_entry_node_t){ .key = NULL, .val = $1, .is_associate = false, .is_self_fn = false }; }
    | self_fn_expression
        { $$ = (cvil_table_entry_node_t){ .key = NULL, .val = $1, .is_associate = false, .is_self_fn = true }; }
    | IDENTIFIER ':' assignment_expression
        { $$ = (cvil_table_entry_node_t){ .key = cvil_ast_literal_str($1, false, LOC_SPAN(@1)), .val = $3, .is_associate = false, .is_self_fn = false }; }
    | IDENTIFIER ':' self_fn_expression
        { $$ = (cvil_table_entry_node_t){ .key = cvil_ast_literal_str($1, false, LOC_SPAN(@1)), .val = $3, .is_associate = false, .is_self_fn = true }; }
    | '[' assignment_expression ']' TOKEN_ASSOCIATE assignment_expression %dprec 2
        { $$ = (cvil_table_entry_node_t){ .key = $2, .val = $5, .is_associate = true, .is_self_fn = false }; }
    | '[' assignment_expression ']' TOKEN_ASSOCIATE self_fn_expression %dprec 2
        { $$ = (cvil_table_entry_node_t){ .key = $2, .val = $5, .is_associate = true, .is_self_fn = true }; }
    | assignment_expression TOKEN_ASSOCIATE assignment_expression %dprec 1
        { $$ = (cvil_table_entry_node_t){ .key = $1, .val = $3, .is_associate = true, .is_self_fn = false }; }
    | assignment_expression TOKEN_ASSOCIATE self_fn_expression %dprec 1
        { $$ = (cvil_table_entry_node_t){ .key = $1, .val = $3, .is_associate = true, .is_self_fn = true }; }
    ;

self_fn_expression
    : TOKEN_SELF TOKEN_DOUBLE_COLON TOKEN_FN parameter_list_opt block_statement
        {
            $$ = cvil_ast_new(AST_EXPR_SELF_FN, LOC_SPAN(@$));
            cvil_param_t* p = (cvil_param_t*)calloc($4.count + 1, sizeof(cvil_param_t));
            p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN(@1) };
            for (uint32_t i = 0; i < $4.count; i++) p[i + 1] = $4.items[i];
            $$->as.fn_expr.params = p;
            $$->as.fn_expr.param_count = $4.count + 1;
            $$->as.fn_expr.body = $5;
            $$->as.fn_expr.is_self_fn = true;
            $$->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
    | TOKEN_SELF TOKEN_DOUBLE_COLON TOKEN_FN parameter_list_opt TOKEN_RIGHT_ARROW assignment_expression
        {
            $$ = cvil_ast_new(AST_EXPR_SELF_FN, LOC_SPAN(@$));
            cvil_param_t* p = (cvil_param_t*)calloc($4.count + 1, sizeof(cvil_param_t));
            p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN(@1) };
            for (uint32_t i = 0; i < $4.count; i++) p[i + 1] = $4.items[i];
            $$->as.fn_expr.params = p;
            $$->as.fn_expr.param_count = $4.count + 1;
            cvil_ast_node_t* ret = cvil_ast_new(AST_STMT_RET, LOC_SPAN(@6));
            ret->as.ret_stmt.expr = $6;
            cvil_ast_list_t* stmts = cvil_ast_list_create();
            cvil_ast_list_append(stmts, ret);
            $$->as.fn_expr.body = cvil_ast_block(stmts, LOC_SPAN(@6));
            $$->as.fn_expr.is_self_fn = true;
            $$->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
    ;

array_expression
    : '[' array_element_list_opt ']'
        { $$ = cvil_ast_array(NULL, $2, LOC_SPAN(@$)); }
    | TOKEN_ARRAY '(' assignment_expression ')'
        { $$ = cvil_ast_array($3, NULL, LOC_SPAN(@$)); }
    | TOKEN_ARRAY '(' assignment_expression ')' '{' array_element_list_opt '}'
        { $$ = cvil_ast_array($3, $6, LOC_SPAN(@$)); }
    | TOKEN_ARRAY '(' ')' '{' array_element_list_opt '}'
        { $$ = cvil_ast_array(NULL, $5, LOC_SPAN(@$)); }
    | TOKEN_ARRAY '{' array_element_list_opt '}'
        { $$ = cvil_ast_array(NULL, $3, LOC_SPAN(@$)); }
    ;

array_element_list_opt
    : /* empty */ { $$ = cvil_ast_list_create(); }
    | array_element_seq comma_opt { $$ = $1; }
    ;

array_element_seq
    : assignment_expression
        { $$ = cvil_ast_list_create(); cvil_ast_list_append($$, $1); }
    | array_element_seq ',' assignment_expression
        { cvil_ast_list_append($1, $3); $$ = $1; }
    ;

error_expression
    : TOKEN_ERROR '(' string_literal ')'
        { $$ = cvil_ast_new(AST_EXPR_ERROR, LOC_SPAN(@$)); $$->as.error_expr.message = $3; $$->as.error_expr.table_data = NULL; }
    | TOKEN_ERROR '(' string_literal ')' table_expression
        { $$ = cvil_ast_new(AST_EXPR_ERROR, LOC_SPAN(@$)); $$->as.error_expr.message = $3; $$->as.error_expr.table_data = $5; }
    | TOKEN_ERROR table_expression
        { $$ = cvil_ast_new(AST_EXPR_ERROR, LOC_SPAN(@$)); $$->as.error_expr.message = NULL; $$->as.error_expr.table_data = $2; }
    ;

typeof_expression
    : TOKEN_TYPEOF '(' assignment_expression ')'
        { $$ = cvil_ast_new(AST_EXPR_TYPEOF, LOC_SPAN(@$)); $$->as.typeof_expr.expr = $3; $$->as.typeof_expr.invert = false; }
    | TOKEN_TYPEOF '!' '(' assignment_expression ')'
        { $$ = cvil_ast_new(AST_EXPR_TYPEOF, LOC_SPAN(@$)); $$->as.typeof_expr.expr = $4; $$->as.typeof_expr.invert = true; }
    ;

yield_expression
    : TOKEN_YIELD '<' IDENTIFIER '>' '(' argument_list_opt ')'
        {
            $$ = cvil_ast_new(AST_EXPR_YIELD, LOC_SPAN(@$));
            $$->as.yield_expr.target_name = $3;
            $$->as.yield_expr.target_expr = cvil_ast_ident($3, LOC_SPAN(@3));
            $$->as.yield_expr.args = $6;
        }
    | TOKEN_YIELD '<' '[' assignment_expression ']' '>' '(' argument_list_opt ')'
        {
            $$ = cvil_ast_new(AST_EXPR_YIELD, LOC_SPAN(@$));
            $$->as.yield_expr.target_name = NULL;
            $$->as.yield_expr.target_expr = $4;
            $$->as.yield_expr.args = $8;
        }
    ;

fn_block_expression
    : TOKEN_FN parameter_list_opt block_statement
        {
            $$ = cvil_ast_new(AST_EXPR_FN, LOC_SPAN(@$));
            $$->as.fn_expr.params = $2.items;
            $$->as.fn_expr.param_count = $2.count;
            $$->as.fn_expr.body = $3;
            $$->as.fn_expr.is_self_fn = false;
            $$->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
    ;

constant_expression
    : TOKEN_NUMBER          { $$ = cvil_ast_literal_num($1, LOC_SPAN(@$)); }
    | TOKEN_HEX_INTEGER     { $$ = cvil_ast_literal_hex($1, LOC_SPAN(@$)); }
    | string_literal        { $$ = $1; }
    | TOKEN_TRUE            { $$ = cvil_ast_literal_bool(true, LOC_SPAN(@$)); }
    | TOKEN_FALSE           { $$ = cvil_ast_literal_bool(false, LOC_SPAN(@$)); }
    | TOKEN_NIL             { $$ = cvil_ast_literal_nil(LOC_SPAN(@$)); }
    | TOKEN_NAN             { $$ = cvil_ast_literal_num(NAN, LOC_SPAN(@$)); }
    | TOKEN_INFINITY        { $$ = cvil_ast_literal_num(INFINITY, LOC_SPAN(@$)); }
    | type_code             { $$ = cvil_ast_literal_type($1, LOC_SPAN(@$)); }
    ;

string_literal
    : TOKEN_PLAIN_STRING        { $$ = cvil_ast_literal_str($1, false, LOC_SPAN(@$)); }
    | TOKEN_INTERPOLATED_STRING { $$ = cvil_ast_literal_str($1, true, LOC_SPAN(@$)); }
    ;

comma_opt
    : /* empty */
    | ','
    ;

%%

void yyerror(const char *s) {
    cvil_span_t span = { NULL, yylineno, yycolumn, yylineno, yycolumn };
    cvil_diag_error(span, "CVIL0100", "%s near '%s'", s, yytext ? yytext : "");
}
