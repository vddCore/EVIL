%{
/**
 * Flex lexical scanner for the EVIL programming language.
 **/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "evil.tab.h"

int yycolumn = 1;

#define YY_USER_ACTION \
    yylloc.first_line = yylloc.last_line = yylineno; \
    yylloc.first_column = yycolumn; \
    yylloc.last_column = yycolumn + yyleng - 1; \
    yycolumn += yyleng;

void yyerror(const char *s);
%}

%option noyywrap
%option yylineno

%x IN_BLOCK_COMMENT

IDENTIFIER_CHAR   [a-zA-Z0-9_]
IDENTIFIER        [a-zA-Z_]{IDENTIFIER_CHAR}*
HEX_INTEGER       0x[0-9a-fA-F]+
NUMBER            ([0-9]+\.[0-9]*|\.[0-9]+)([eE][+-]?[0-9]+)?|[0-9]+[eE][+-]?[0-9]+|[0-9]+

%%

[ \t\r\v\f]+            { /* ignore whitespace */ }
\xEF\xBB\xBF            { /* ignore UTF-8 byte order mark */ }
\n                      { yycolumn = 1; }

"//"[^\r\n]*            { /* ignore single-line comments */ }

"/*"                    { BEGIN(IN_BLOCK_COMMENT); }
<IN_BLOCK_COMMENT>{
    "*/"                { BEGIN(INITIAL); }
    \n                  { yycolumn = 1; }
    [^*\n]+             { /* ignore inside comment */ }
    "*"                 { /* ignore solitary asterisk */ }
    <<EOF>>             { yyerror("Unterminated block comment"); BEGIN(INITIAL); return 0; }
}

    /* Keywords */
"fn"                    { return TOKEN_FN; }
"loc"                   { return TOKEN_LOC; }
"for"                   { return TOKEN_FOR; }
"while"                 { return TOKEN_WHILE; }
"do"                    { return TOKEN_DO; }
"rw"                    { return TOKEN_RW; }
"val"                   { return TOKEN_VAL; }
"in"                    { return TOKEN_IN; }
"if"                    { return TOKEN_IF; }
"is"                    { return TOKEN_IS; }
"elif"                  { return TOKEN_ELIF; }
"else"                  { return TOKEN_ELSE; }
"break"                 { return TOKEN_BREAK; }
"skip"                  { return TOKEN_SKIP; }
"by"                    { return TOKEN_BY; }
"ret"                   { return TOKEN_RET; }
"true"                  { return TOKEN_TRUE; }
"false"                 { return TOKEN_FALSE; }
"nil"                   { return TOKEN_NIL; }
"typeof"                { return TOKEN_TYPEOF; }
"try"                   { return TOKEN_TRY; }
"catch"                 { return TOKEN_CATCH; }
"throw"                 { return TOKEN_THROW; }
"yield"                 { return TOKEN_YIELD; }
"each"                  { return TOKEN_EACH; }
"array"                 { return TOKEN_ARRAY; }
"self"                  { return TOKEN_SELF; }
"error"                 { return TOKEN_ERROR; }
"retry"                 { return TOKEN_RETRY; }
"with"                  { return TOKEN_WITH; }

    /* Aliases */
"NaN"                   { return TOKEN_NAN; }
"Infinity"              { return TOKEN_INFINITY; }

    /* Type Identifiers / Keywords */
"Nil"                   { return TYPE_NIL; }
"Number"                { return TYPE_NUMBER; }
"String"                { return TYPE_STRING; }
"Boolean"               { return TYPE_BOOLEAN; }
"Table"                 { return TYPE_TABLE; }
"Array"                 { return TYPE_ARRAY; }
"Fiber"                 { return TYPE_FIBER; }
"Function"              { return TYPE_CHUNK; }
"Fn"                    { return TYPE_CHUNK; }
"Error"                 { return TYPE_ERROR; }
"Type"                  { return TYPE_TYPE; }
"NativeFunction"        { return TYPE_NATIVE_FUNCTION; }
"NativeObject"          { return TYPE_NATIVE_OBJECT; }

    /* Composite Negated Relational Operators (!in, !is) */
"!in"/{IDENTIFIER_CHAR} {
    /* If followed by an identifier character (e.g. !inside), match '!' only */
    yyless(1);
    return '!';
}
"!in"                   { return TOKEN_NOT_IN; }

"!is"/{IDENTIFIER_CHAR} {
    /* If followed by an identifier character (e.g. !island), match '!' only */
    yyless(1);
    return '!';
}
"!is"                   { return TOKEN_IS_NOT; }

    /* Composite Multi-Character Operators */
"#["                    { return TOKEN_ATTRIBUTE_LIST; }
"<==>"                  { return TOKEN_DEEP_EQUAL; }
"<!=>"                  { return TOKEN_DEEP_NOT_EQUAL; }
"=="                    { return TOKEN_EQUAL; }
"!="                    { return TOKEN_NOT_EQUAL; }
"<="                    { return TOKEN_LESS_EQUAL; }
">="                    { return TOKEN_GREATER_EQUAL; }
"<<"                    { return TOKEN_SHIFT_LEFT; }
">>"                    { return TOKEN_SHIFT_RIGHT; }
"&&"                    { return TOKEN_LOGICAL_AND; }
"||"                    { return TOKEN_LOGICAL_OR; }
"++"                    { return TOKEN_INCREMENT; }
"--"                    { return TOKEN_DECREMENT; }
"...?"                  { return TOKEN_SAFE_SPREAD; }
"..."                   { return TOKEN_ELLIPSIS; }
"=>"                    { return TOKEN_ASSOCIATE; }
"->"                    { return TOKEN_RIGHT_ARROW; }
"?."                    { return TOKEN_ELVIS; }
"?["                    { return TOKEN_ELVIS_ARRAY; }
"?:"                    { return TOKEN_SAFE_COLON; }
"?("                    { return TOKEN_ELVIS_INVOCATION; }
"??"                    { return TOKEN_DOUBLE_QUESTION; }
"::"                    { return TOKEN_DOUBLE_COLON; }

    /* Compound Assignment Operators */
"+="                    { return TOKEN_ASSIGN_ADD; }
"-="                    { return TOKEN_ASSIGN_SUBTRACT; }
"*="                    { return TOKEN_ASSIGN_MULTIPLY; }
"/="                    { return TOKEN_ASSIGN_DIVIDE; }
"%="                    { return TOKEN_ASSIGN_MODULO; }
"&="                    { return TOKEN_ASSIGN_BITWISE_AND; }
"|="                    { return TOKEN_ASSIGN_BITWISE_OR; }
"^="                    { return TOKEN_ASSIGN_BITWISE_XOR; }
">>="                   { return TOKEN_ASSIGN_SHIFT_RIGHT; }
"<<="                   { return TOKEN_ASSIGN_SHIFT_LEFT; }
"??="                   { return TOKEN_ASSIGN_COALESCE; }

    /* Single Character Operators and Punctuation */
"="                     { return '='; }
"+"                     { return '+'; }
"-"                     { return '-'; }
"*"                     { return '*'; }
"/"                     { return '/'; }
"%"                     { return '%'; }
"&"                     { return '&'; }
"|"                     { return '|'; }
"^"                     { return '^'; }
"~"                     { return '~'; }
"<"                     { return '<'; }
">"                     { return '>'; }
"!"                     { return '!'; }
"?"                     { return '?'; }
":"                     { return ':'; }
";"                     { return ';'; }
","                     { return ','; }
"."                     { return '.'; }
"{"                     { return '{'; }
"}"                     { return '}'; }
"["                     { return '['; }
"]"                     { return ']'; }
"("                     { return '('; }
")"                     { return ')'; }
"#"                     { return '#'; }
"$"                     { return '$'; }
"@"                     { return '@'; }

    /* Numbers & Identifiers */
{HEX_INTEGER}           { return TOKEN_HEX_INTEGER; }
{NUMBER}                { return TOKEN_NUMBER; }
{IDENTIFIER}            { return IDENTIFIER; }

    /* Single-Quoted Plain String Literals */
'([^'\\]|\\.)*'         { return TOKEN_PLAIN_STRING; }

    /* Double-Quoted Interpolated String Literals with Nested Expression Tracking */
\" {
    int c;
    int brace_depth = 0;
    while ((c = input()) != 0) {
        if (c == '\n') {
            yycolumn = 1;
        }
        if (brace_depth == 0) {
            if (c == '"') {
                return TOKEN_INTERPOLATED_STRING;
            } else if (c == '\\') {
                int next = input();
                if (next == 0) break;
                if (next == '\n') yycolumn = 1;
            } else if (c == '$') {
                int next = input();
                if (next == '{') {
                    brace_depth = 1;
                } else if (next != 0) {
                    unput(next);
                }
            }
        } else {
            /* Inside ${ ... } interpolated block: track braces, strings, comments */
            if (c == '\\') {
                int next = input();
                if (next == 0) break;
                if (next == '\n') yycolumn = 1;
            } else if (c == '/') {
                int next = input();
                if (next == '/') {
                    while ((c = input()) != 0 && c != '\n');
                    if (c == '\n') yycolumn = 1;
                } else if (next == '*') {
                    int prev = 0;
                    while ((c = input()) != 0) {
                        if (c == '\n') yycolumn = 1;
                        if (prev == '*' && c == '/') break;
                        prev = c;
                    }
                } else if (next != 0) {
                    unput(next);
                }
            } else if (c == '"' || c == '\'') {
                int quote = c;
                while ((c = input()) != 0 && c != quote) {
                    if (c == '\n') yycolumn = 1;
                    if (c == '\\') {
                        int next = input();
                        if (next == 0) break;
                        if (next == '\n') yycolumn = 1;
                    }
                }
            } else if (c == '{') {
                brace_depth++;
            } else if (c == '}') {
                brace_depth--;
            }
        }
    }
    yyerror("Unterminated string literal");
    return TOKEN_INTERPOLATED_STRING;
}

. {
    fprintf(stderr, "Unexpected character: '%c' (0x%02X) at line %d, col %d\n", 
            yytext[0], (unsigned char)yytext[0], yylineno, yycolumn);
    return yytext[0];
}

%%
