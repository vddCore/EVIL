%{
/**
 * Bison LALR(1)/GLR Grammar Specification for the EVIL programming language.
 **/

#include <stdio.h>
#include <stdlib.h>

extern int yylex(void);
extern int yylineno;
extern char *yytext;
void yyerror(const char *s);
%}

%define parse.error verbose
%locations
%glr-parser
%expect 13
%expect-rr 31

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
%token TYPE_NIL "Nil"
%token TYPE_NUMBER "Number"
%token TYPE_STRING "String"
%token TYPE_BOOLEAN "Boolean"
%token TYPE_TABLE "Table"
%token TYPE_ARRAY "Array"
%token TYPE_FIBER "Fiber"
%token TYPE_CHUNK "Function"
%token TYPE_ERROR "Error"
%token TYPE_TYPE "Type"
%token TYPE_NATIVE_FUNCTION "NativeFunction"
%token TYPE_NATIVE_OBJECT "NativeObject"

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
%token TOKEN_SAFE_SPREAD "...?"
%token TOKEN_ELLIPSIS "..."
%token TOKEN_RANGE ".."
%token TOKEN_ASSOCIATE "=>"
%token TOKEN_RIGHT_ARROW "->"
%token TOKEN_LEFT_ARROW "<-"

%token TOKEN_ELVIS "?."
%token TOKEN_ELVIS_ARRAY "?["
%token TOKEN_SAFE_COLON "?:"
%token TOKEN_ELVIS_INVOCATION "?("
%token TOKEN_DOUBLE_QUESTION "??"
%token TOKEN_DOUBLE_COLON "::"

%token TOKEN_NOT_IN "!in"
%token TOKEN_IS_NOT "!is"
%token TOKEN_ATTRIBUTE_LIST "#["

/* Literals & Identifier */
%token IDENTIFIER "identifier"
%token TOKEN_NUMBER "number"
%token TOKEN_HEX_INTEGER "hex_integer"
%token TOKEN_PLAIN_STRING "plain_string"
%token TOKEN_INTERPOLATED_STRING "interpolated_string"

/* Operator Precedence and Associativity (lowest to highest) */
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
%precedence TOKEN_YIELD
%precedence UNARY PREFIX
%precedence POSTFIX TOKEN_INCREMENT TOKEN_DECREMENT '!'
%left '.' TOKEN_ELVIS TOKEN_ELVIS_ARRAY TOKEN_SAFE_COLON TOKEN_ELVIS_INVOCATION '[' '(' TOKEN_DOUBLE_COLON

%start program

%%

/* ========================================================================= */
/* Program & Statements                                                      */
/* ========================================================================= */

program
    : statement_list_opt
    ;

statement_list_opt
    : /* empty */
    | statement_list
    ;

statement_list
    : statement
    | statement_list statement
    ;

statement
    : fn_statement
    | if_statement
    | for_statement
    | while_statement
    | do_while_statement
    | each_statement
    | block_statement
    | val_statement
    | try_statement
    | retry_statement
    | throw_statement
    | ret_statement
    | skip_statement
    | break_statement
    | expression_statement
    ;

expression_statement
    : assignment_expression ';'
    ;

block_statement
    : '{' statement_list_opt '}'
    ;

retry_statement
    : TOKEN_RETRY ';'
    ;

throw_statement
    : TOKEN_THROW assignment_expression ';'
    ;

ret_statement
    : TOKEN_RET ';'
    | TOKEN_RET assignment_expression ';'
    ;

skip_statement
    : TOKEN_SKIP ';'
    ;

break_statement
    : TOKEN_BREAK ';'
    ;

while_statement
    : TOKEN_WHILE '(' assignment_expression ')' statement
    ;

do_while_statement
    : TOKEN_DO statement TOKEN_WHILE '(' assignment_expression ')' ';'
    ;

/* Val Statement */
val_statement
    : TOKEN_VAL val_declarator_seq ';'
    | TOKEN_RW TOKEN_VAL val_declarator_seq ';'
    ;

val_declarator_seq
    : val_declarator
    | val_declarator_seq ',' val_declarator
    ;

val_declarator
    : IDENTIFIER
    | IDENTIFIER '=' assignment_expression
    | array_pattern TOKEN_LEFT_ARROW assignment_expression
    | table_pattern TOKEN_LEFT_ARROW assignment_expression
    ;

array_pattern
    : '[' pattern_binding_seq ']'
    ;

pattern_binding_seq
    : pattern_binding
    | pattern_binding_seq ',' pattern_binding
    | pattern_binding_seq ','
    ;

pattern_binding
    : IDENTIFIER
    | TOKEN_RW IDENTIFIER
    ;

table_pattern
    : '{' table_pattern_field_seq '}'
    ;

table_pattern_field_seq
    : table_pattern_field
    | table_pattern_field_seq ',' table_pattern_field
    | table_pattern_field_seq ','
    ;

table_pattern_field
    : IDENTIFIER
    | IDENTIFIER ':' pattern_binding
    | TOKEN_RW IDENTIFIER
    | TOKEN_RW IDENTIFIER ':' pattern_binding
    | '[' TOKEN_PLAIN_STRING ']' ':' pattern_binding
    | TOKEN_RW '[' TOKEN_PLAIN_STRING ']' ':' pattern_binding
    ;

/* Each Statement */
each_statement
    : TOKEN_EACH '(' TOKEN_RW TOKEN_VAL IDENTIFIER ':' assignment_expression ')' statement
    | TOKEN_EACH '(' TOKEN_RW TOKEN_VAL IDENTIFIER ',' IDENTIFIER ':' assignment_expression ')' statement
    ;

/* For Statement */
for_statement
    : TOKEN_FOR '(' for_init_list_opt ';' assignment_expression ';' for_step_list_opt ')' statement
    ;

for_init_list_opt
    : /* empty */
    | for_init_list
    ;

for_init_list
    : for_init_item
    | for_init_list ',' for_init_item
    ;

for_init_item
    : TOKEN_RW TOKEN_VAL val_declarator_seq
    | assignment_expression
    ;

for_step_list_opt
    : /* empty */
    | for_step_list
    ;

for_step_list
    : assignment_expression
    | for_step_list ',' assignment_expression
    ;

/* If Statement */
if_statement
    : TOKEN_IF '(' assignment_expression ')' if_branch elif_list else_opt
    ;

elif_list
    : /* empty */
    | elif_list TOKEN_ELIF '(' assignment_expression ')' if_branch
    ;

else_opt
    : /* empty */ %prec LOWER_THAN_ELSE
    | TOKEN_ELSE if_branch
    ;

if_branch
    : statement
    | TOKEN_RIGHT_ARROW assignment_expression ';'
    ;

/* Try Statement */
try_statement
    : TOKEN_TRY statement catch_clause_opt
    ;

catch_clause_opt
    : /* empty */ %prec LOWER_THAN_CATCH
    | TOKEN_CATCH block_statement
    | TOKEN_CATCH '(' IDENTIFIER ')' block_statement
    ;

/* Function Statement */
fn_statement
    : attribute_list_seq fn_head fn_body
    | fn_head fn_body
    ;

attribute_list_seq
    : attribute_list
    | attribute_list_seq attribute_list
    ;

attribute_list
    : TOKEN_ATTRIBUTE_LIST attribute_seq ']'
    ;

attribute_seq
    : attribute_item
    | attribute_seq ';' attribute_item
    ;

attribute_item
    : IDENTIFIER
    | IDENTIFIER '(' ')'
    | IDENTIFIER '(' attribute_arg_list ')'
    ;

attribute_arg_list
    : attribute_arg
    | attribute_arg_list ',' attribute_arg
    ;

attribute_arg
    : assignment_expression
    ;

fn_head
    : fn_modifier fn_target parameter_list_opt
    ;

fn_modifier
    : TOKEN_FN
    | TOKEN_LOC TOKEN_FN
    ;

fn_target
    : fn_name_chain
    | fn_name_chain TOKEN_DOUBLE_COLON IDENTIFIER
    | TOKEN_SELF TOKEN_DOUBLE_COLON IDENTIFIER
    ;

fn_name_chain
    : IDENTIFIER
    | fn_name_chain '.' IDENTIFIER
    ;

parameter_list_opt
    : /* empty */
    | '(' parameter_list ')'
    ;

parameter_list
    : /* empty */
    | TOKEN_SELF
    | TOKEN_SELF ',' param_seq
    | param_seq
    ;

param_seq
    : param_item
    | param_seq ',' param_item
    ;

param_item
    : IDENTIFIER
    | IDENTIFIER '=' assignment_expression
    | TOKEN_RW IDENTIFIER
    | TOKEN_RW IDENTIFIER '=' assignment_expression
    ;

fn_body
    : block_statement
    | TOKEN_RIGHT_ARROW assignment_expression ';'
    ;

/* ========================================================================= */
/* Expressions                                                               */
/* ========================================================================= */

/* Level 1: Assignment */
assignment_expression
    : conditional_expression
    | unary_expression assignment_operator assignment_expression
    | fn_arrow_expression
    ;

fn_arrow_expression
    : TOKEN_FN parameter_list_opt TOKEN_RIGHT_ARROW assignment_expression
    ;

assignment_operator
    : '='
    | TOKEN_ASSIGN_ADD
    | TOKEN_ASSIGN_SUBTRACT
    | TOKEN_ASSIGN_MULTIPLY
    | TOKEN_ASSIGN_DIVIDE
    | TOKEN_ASSIGN_MODULO
    | TOKEN_ASSIGN_BITWISE_AND
    | TOKEN_ASSIGN_BITWISE_OR
    | TOKEN_ASSIGN_BITWISE_XOR
    | TOKEN_ASSIGN_SHIFT_RIGHT
    | TOKEN_ASSIGN_SHIFT_LEFT
    | TOKEN_ASSIGN_COALESCE
    ;

/* Level 2: Conditional (Ternary) */
conditional_expression
    : coalescing_expression
    | coalescing_expression '?' assignment_expression ':' conditional_expression
    ;

/* Level 3: Nil-Coalescing */
coalescing_expression
    : logical_or_expression
    | logical_or_expression TOKEN_DOUBLE_QUESTION coalescing_rhs
    ;

coalescing_rhs
    : assignment_expression
    | TOKEN_THROW assignment_expression
    ;

/* Level 4: Logical OR */
logical_or_expression
    : logical_and_expression
    | logical_or_expression TOKEN_LOGICAL_OR logical_and_expression
    ;

/* Level 5: Logical AND */
logical_and_expression
    : inclusive_or_expression
    | logical_and_expression TOKEN_LOGICAL_AND inclusive_or_expression
    ;

/* Level 6: Bitwise OR */
inclusive_or_expression
    : exclusive_or_expression
    | inclusive_or_expression '|' exclusive_or_expression
    ;

/* Level 7: Bitwise XOR */
exclusive_or_expression
    : and_expression
    | exclusive_or_expression '^' and_expression
    ;

/* Level 8: Bitwise AND */
and_expression
    : equality_expression
    | and_expression '&' equality_expression
    ;

/* Level 9: Equality & Deep Equality */
equality_expression
    : relational_expression
    | equality_expression TOKEN_EQUAL relational_expression
    | equality_expression TOKEN_NOT_EQUAL relational_expression
    | equality_expression TOKEN_DEEP_EQUAL relational_expression
    | equality_expression TOKEN_DEEP_NOT_EQUAL relational_expression
    ;

/* Level 10: Relational & Type / Membership Checking */
relational_expression
    : shift_expression
    | relational_expression '<' shift_expression
    | relational_expression TOKEN_LEFT_ARROW left_arrow_shift_expression
    | relational_expression '>' shift_expression
    | relational_expression TOKEN_LESS_EQUAL shift_expression
    | relational_expression TOKEN_GREATER_EQUAL shift_expression
    | relational_expression TOKEN_IN shift_expression
    | relational_expression TOKEN_NOT_IN shift_expression
    | relational_expression TOKEN_IS type_code is_native_opt
    | relational_expression TOKEN_IS_NOT type_code is_native_opt
    ;

left_arrow_shift_expression
    : left_arrow_additive_expression
    | left_arrow_shift_expression TOKEN_SHIFT_LEFT additive_expression
    | left_arrow_shift_expression TOKEN_SHIFT_RIGHT additive_expression
    ;

left_arrow_additive_expression
    : left_arrow_multiplicative_expression
    | left_arrow_additive_expression '+' multiplicative_expression
    | left_arrow_additive_expression '-' multiplicative_expression
    ;

left_arrow_multiplicative_expression
    : left_arrow_pattern_expression
    | left_arrow_multiplicative_expression '*' pattern_expression
    | left_arrow_multiplicative_expression '/' pattern_expression
    | left_arrow_multiplicative_expression '%' pattern_expression
    ;

left_arrow_pattern_expression
    : '-' prefix_expression
    | left_arrow_pattern_expression TOKEN_WITH table_expression
    ;

is_native_opt
    : /* empty */
    | string_literal
    ;

type_code
    : TYPE_NIL
    | TYPE_NUMBER
    | TYPE_STRING
    | TYPE_BOOLEAN
    | TYPE_TABLE
    | TYPE_ARRAY
    | TYPE_FIBER
    | TYPE_CHUNK
    | TYPE_ERROR
    | TYPE_TYPE
    | TYPE_NATIVE_FUNCTION
    | TYPE_NATIVE_OBJECT
    ;

/* Level 11: Shift */
shift_expression
    : additive_expression
    | shift_expression TOKEN_SHIFT_LEFT additive_expression
    | shift_expression TOKEN_SHIFT_RIGHT additive_expression
    ;

/* Level 12: Additive */
additive_expression
    : multiplicative_expression
    | additive_expression '+' multiplicative_expression
    | additive_expression '-' multiplicative_expression
    ;

/* Level 13: Multiplicative */
multiplicative_expression
    : pattern_expression
    | multiplicative_expression '*' pattern_expression
    | multiplicative_expression '/' pattern_expression
    | multiplicative_expression '%' pattern_expression
    ;

/* Level 14: Pattern (By and With) */
pattern_expression
    : by_expression
    | with_expression
    ;

by_expression
    : TOKEN_BY assignment_expression '{' by_arm_seq comma_opt '}'
    ;

by_arm_seq
    : by_arm
    | by_arm_seq ',' by_arm
    ;

by_arm
    : assignment_expression TOKEN_RIGHT_ARROW by_value
    | assignment_expression TOKEN_ASSOCIATE by_value
    | TOKEN_ELSE ':' by_value
    ;

by_value
    : assignment_expression
    | TOKEN_THROW assignment_expression
    ;

with_expression
    : prefix_expression
    | with_expression TOKEN_WITH table_expression
    ;

/* Level 15: Prefix */
prefix_expression
    : TOKEN_INCREMENT unary_expression
    | TOKEN_DECREMENT unary_expression
    | unary_expression
    ;

/* Level 16: Unary */
unary_expression
    : postfix_expression
    | '+' unary_expression %prec UNARY
    | '-' unary_expression %prec UNARY
    | '!' unary_expression %prec UNARY
    | '~' unary_expression %prec UNARY
    | '@' unary_expression %prec UNARY
    | '#' unary_expression %prec UNARY
    | '$' unary_expression %prec UNARY
    ;

/* Level 17: Postfix */
postfix_expression
    : primary_expression
    | postfix_expression '(' argument_list_opt ')'
    | postfix_expression TOKEN_ELVIS_INVOCATION argument_list_opt ')'
    | postfix_expression '[' index_argument ']'
    | postfix_expression '.' IDENTIFIER
    | postfix_expression TOKEN_ELVIS IDENTIFIER
    | postfix_expression TOKEN_ELVIS_ARRAY index_argument ']'
    | postfix_expression ':' IDENTIFIER '(' argument_list_opt ')'
    | postfix_expression TOKEN_SAFE_COLON IDENTIFIER '(' argument_list_opt ')'
    | postfix_expression TOKEN_INCREMENT %prec POSTFIX
    | postfix_expression TOKEN_DECREMENT %prec POSTFIX
    | postfix_expression '!' %prec POSTFIX
    ;

/* A leading '#' in an index bound means from-end; parenthesized length
   expressions remain ordinary bound expressions. Dynamic precedence makes
   the GLR reference parser select the marker interpretation. */
index_argument
    : assignment_expression %dprec 1
    | '#' assignment_expression %dprec 2
    | range_bound TOKEN_RANGE range_bound_opt
    | TOKEN_RANGE range_bound_opt
    ;

range_bound
    : assignment_expression %dprec 1
    | '#' assignment_expression %dprec 2
    ;

range_bound_opt
    : /* empty */
    | range_bound
    ;

argument_list_opt
    : /* empty */
    | argument_list
    ;

argument_list
    : arg_seq comma_opt
    ;

arg_seq
    : arg_item
    | arg_seq ',' arg_item
    ;

arg_item
    : assignment_expression
    | '*'
    ;

/* Level 18: Primary */
primary_expression
    : '(' assignment_expression ')'
    | table_expression
    | array_expression
    | error_expression
    | TOKEN_ELLIPSIS
    | fn_block_expression
    | IDENTIFIER
    | TOKEN_SELF
    | typeof_expression
    | yield_expression
    | constant_expression
    ;

table_expression
    : '{' table_entry_list_opt '}'
    ;

table_entry_list_opt
    : /* empty */
    | table_entry_seq comma_opt
    ;

table_entry_seq
    : table_entry
    | table_entry_seq ',' table_entry
    ;

table_entry
    : assignment_expression %dprec 1
    | self_fn_expression
    | IDENTIFIER ':' assignment_expression %dprec 2
    | IDENTIFIER ':' self_fn_expression %dprec 2
    | assignment_expression TOKEN_ASSOCIATE assignment_expression
    | assignment_expression TOKEN_ASSOCIATE self_fn_expression
    | TOKEN_SAFE_SPREAD assignment_expression %dprec 2
    | TOKEN_ELLIPSIS assignment_expression %dprec 2
    ;

self_fn_expression
    : TOKEN_SELF TOKEN_DOUBLE_COLON TOKEN_FN parameter_list_opt block_statement
    | TOKEN_SELF TOKEN_DOUBLE_COLON TOKEN_FN parameter_list_opt TOKEN_RIGHT_ARROW assignment_expression
    ;

array_expression
    : '[' array_element_list_opt ']'
    | TOKEN_ARRAY '(' assignment_expression ')'
    | TOKEN_ARRAY '(' assignment_expression ')' '{' array_element_list_opt '}'
    | TOKEN_ARRAY '(' ')' '{' array_element_list_opt '}'
    | TOKEN_ARRAY '{' array_element_list_opt '}'
    ;

array_element_list_opt
    : /* empty */
    | array_element_seq comma_opt
    ;

array_element_seq
    : assignment_expression
    | array_element_seq ',' assignment_expression
    ;

error_expression
    : TOKEN_ERROR '(' string_literal ')'
    | TOKEN_ERROR '(' string_literal ')' table_expression
    | TOKEN_ERROR table_expression
    ;

typeof_expression
    : TOKEN_TYPEOF '(' assignment_expression ')'
    | TOKEN_TYPEOF '!' '(' assignment_expression ')'
    ;

yield_expression
    : TOKEN_YIELD postfix_expression
    ;

fn_block_expression
    : TOKEN_FN parameter_list_opt block_statement
    ;

constant_expression
    : TOKEN_NUMBER
    | TOKEN_HEX_INTEGER
    | string_literal
    | TOKEN_TRUE
    | TOKEN_FALSE
    | TOKEN_NIL
    | TOKEN_NAN
    | TOKEN_INFINITY
    | type_code
    ;

string_literal
    : TOKEN_PLAIN_STRING
    | TOKEN_INTERPOLATED_STRING
    ;

comma_opt
    : /* empty */
    | ','
    ;

%%

void yyerror(const char *s) {
    extern int yylineno;
    extern int yycolumn;
    extern char *current_filename;
    fprintf(stderr, "%s:%d:%d: error: %s near '%s'\n", 
            current_filename ? current_filename : "<stdin>", 
            yylineno, yycolumn, s, yytext);
}
