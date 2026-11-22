%{
/*
 * Flex Scanner for CVIL Transpiler
 */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "cvil_ast.h"
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
\xEF\xBB\xBF            { /* ignore UTF-8 BOM */ }
\n                      { yycolumn = 1; }

"//"[^\r\n]*            { /* ignore line comments */ }

"/*"                    { BEGIN(IN_BLOCK_COMMENT); }
<IN_BLOCK_COMMENT>{
    "*/"                { BEGIN(INITIAL); }
    \n                  { yycolumn = 1; }
    [^*\n]+             { /* ignore inside comment */ }
    "*"                 { /* ignore solitary asterisk */ }
    <<EOF>>             { yyerror("Unterminated block comment"); BEGIN(INITIAL); return 0; }
}

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

"NaN"                   { return TOKEN_NAN; }
"Infinity"              { return TOKEN_INFINITY; }

"Nil"                   { yylval.type_code = EVIL_TYPE_NIL; return TYPE_NIL; }
"Number"                { yylval.type_code = EVIL_TYPE_NUMBER; return TYPE_NUMBER; }
"String"                { yylval.type_code = EVIL_TYPE_STRING; return TYPE_STRING; }
"Boolean"               { yylval.type_code = EVIL_TYPE_BOOLEAN; return TYPE_BOOLEAN; }
"Table"                 { yylval.type_code = EVIL_TYPE_TABLE; return TYPE_TABLE; }
"Array"                 { yylval.type_code = EVIL_TYPE_ARRAY; return TYPE_ARRAY; }
"Fiber"                 { yylval.type_code = EVIL_TYPE_FIBER; return TYPE_FIBER; }
"Function"              { yylval.type_code = EVIL_TYPE_FUNCTION; return TYPE_CHUNK; }
"Fn"                    { yylval.type_code = EVIL_TYPE_FUNCTION; return TYPE_CHUNK; }
"Error"                 { yylval.type_code = EVIL_TYPE_ERROR; return TYPE_ERROR; }
"Type"                  { yylval.type_code = EVIL_TYPE_TYPE; return TYPE_TYPE; }
"NativeFunction"        { yylval.type_code = EVIL_TYPE_NATIVE_FUNCTION; return TYPE_NATIVE_FUNCTION; }
"NativeObject"          { yylval.type_code = EVIL_TYPE_NATIVE_OBJECT; return TYPE_NATIVE_OBJECT; }

"!in"/{IDENTIFIER_CHAR} { yyless(1); return '!'; }
"!in"                   { return TOKEN_NOT_IN; }

"!is"/{IDENTIFIER_CHAR} { yyless(1); return '!'; }
"!is"                   { return TOKEN_IS_NOT; }

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
"..."                   { return TOKEN_ELLIPSIS; }
"=>"                    { return TOKEN_ASSOCIATE; }
"->"                    { return TOKEN_RIGHT_ARROW; }
"?."                    { return TOKEN_ELVIS; }
"?["                    { return TOKEN_ELVIS_ARRAY; }
"??"                    { return TOKEN_DOUBLE_QUESTION; }
"::"                    { return TOKEN_DOUBLE_COLON; }

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

{HEX_INTEGER}           { yylval.hex_val = (int)strtol(yytext, NULL, 0); return TOKEN_HEX_INTEGER; }
{NUMBER}                { yylval.num = atof(yytext); return TOKEN_NUMBER; }
{IDENTIFIER}            { yylval.str = strdup(yytext); return IDENTIFIER; }

'([^'\\]|\\.)*'         {
                            /* Single-quoted plain string */
                            size_t len = strlen(yytext);
                            char* buf = (char*)malloc(len);
                            /* unescape */
                            size_t bi = 0;
                            for (size_t i = 1; i < len - 1; i++) {
                                if (yytext[i] == '\\' && i + 1 < len - 1) {
                                    i++;
                                    if (yytext[i] == 'n') buf[bi++] = '\n';
                                    else if (yytext[i] == 't') buf[bi++] = '\t';
                                    else if (yytext[i] == 'r') buf[bi++] = '\r';
                                    else buf[bi++] = yytext[i];
                                } else {
                                    buf[bi++] = yytext[i];
                                }
                            }
                            buf[bi] = '\0';
                            yylval.str = buf;
                            return TOKEN_PLAIN_STRING;
                        }

\" {
    /* Double-quoted string (plain or interpolated) */
    size_t cap = 128;
    size_t len = 0;
    char* buf = (char*)malloc(cap);
    int c;
    int brace_depth = 0;
    bool has_interp = false;

    while ((c = input()) != 0) {
        if (c == '\n') yycolumn = 1;

        if (brace_depth == 0) {
            if (c == '"') {
                buf[len] = '\0';
                yylval.str = buf;
                return has_interp ? TOKEN_INTERPOLATED_STRING : TOKEN_PLAIN_STRING;
            } else if (c == '\\') {
                int next = input();
                if (next == 0) break;
                if (next == '\n') yycolumn = 1;
                if (len + 3 >= cap) { cap *= 2; buf = (char*)realloc(buf, cap); }
                if (next == 'n') buf[len++] = '\n';
                else if (next == 't') buf[len++] = '\t';
                else if (next == 'r') buf[len++] = '\r';
                else if (next == '"') buf[len++] = '"';
                else if (next == '\\') buf[len++] = '\\';
                else { buf[len++] = '\\'; buf[len++] = (char)next; }
            } else if (c == '$') {
                int next = input();
                if (next == '{') {
                    has_interp = true;
                    brace_depth = 1;
                    if (len + 3 >= cap) { cap *= 2; buf = (char*)realloc(buf, cap); }
                    buf[len++] = '$';
                    buf[len++] = '{';
                } else if (next != 0) {
                    has_interp = true;
                    if (len + 3 >= cap) { cap *= 2; buf = (char*)realloc(buf, cap); }
                    buf[len++] = '$';
                    buf[len++] = (char)next;
                }
            } else {
                if (len + 2 >= cap) { cap *= 2; buf = (char*)realloc(buf, cap); }
                buf[len++] = (char)c;
            }
        } else {
            /* Inside ${ ... } */
            if (len + 2 >= cap) { cap *= 2; buf = (char*)realloc(buf, cap); }
            buf[len++] = (char)c;
            if (c == '{') brace_depth++;
            else if (c == '}') brace_depth--;
        }
    }
    buf[len] = '\0';
    yylval.str = buf;
    return has_interp ? TOKEN_INTERPOLATED_STRING : TOKEN_PLAIN_STRING;
}

. {
    return yytext[0];
}

%%
