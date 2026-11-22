/* A Bison parser, made by GNU Bison 3.8.2.  */

/* Skeleton implementation for Bison GLR parsers in C

   Copyright (C) 2002-2015, 2018-2021 Free Software Foundation, Inc.

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <https://www.gnu.org/licenses/>.  */

/* As a special exception, you may create a larger work that contains
   part or all of the Bison parser skeleton and distribute that work
   under terms of your choice, so long as that work isn't itself a
   parser generator using the skeleton or a modified version thereof
   as a parser skeleton.  Alternatively, if you modify or redistribute
   the parser skeleton itself, you may (at your option) remove this
   special exception, which will cause the skeleton and the resulting
   Bison output files to be licensed under the GNU General Public
   License without this special exception.

   This special exception was added by the Free Software Foundation in
   version 2.2 of Bison.  */

/* C GLR parser skeleton written by Paul Hilfinger.  */

/* DO NOT RELY ON FEATURES THAT ARE NOT DOCUMENTED in the manual,
   especially those whose name start with YY_ or yy_.  They are
   private implementation details that can be changed or removed.  */

/* Identify Bison output, and Bison version.  */
#define YYBISON 30802

/* Bison version string.  */
#define YYBISON_VERSION "3.8.2"

/* Skeleton name.  */
#define YYSKELETON_NAME "glr.c"

/* Pure parsers.  */
#define YYPURE 0






/* First part of user prologue.  */
#line 1 "src/frontend/evil.y"

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

#line 81 "src/frontend/evil.tab.c"

# ifndef YY_CAST
#  ifdef __cplusplus
#   define YY_CAST(Type, Val) static_cast<Type> (Val)
#   define YY_REINTERPRET_CAST(Type, Val) reinterpret_cast<Type> (Val)
#  else
#   define YY_CAST(Type, Val) ((Type) (Val))
#   define YY_REINTERPRET_CAST(Type, Val) ((Type) (Val))
#  endif
# endif
# ifndef YY_NULLPTR
#  if defined __cplusplus
#   if 201103L <= __cplusplus
#    define YY_NULLPTR nullptr
#   else
#    define YY_NULLPTR 0
#   endif
#  else
#   define YY_NULLPTR ((void*)0)
#  endif
# endif

#include "evil.tab.h"

/* Symbol kind.  */
enum yysymbol_kind_t
{
  YYSYMBOL_YYEMPTY = -2,
  YYSYMBOL_YYEOF = 0,                      /* "end of file"  */
  YYSYMBOL_YYerror = 1,                    /* error  */
  YYSYMBOL_YYUNDEF = 2,                    /* "invalid token"  */
  YYSYMBOL_TOKEN_FN = 3,                   /* "fn"  */
  YYSYMBOL_TOKEN_LOC = 4,                  /* "loc"  */
  YYSYMBOL_TOKEN_FOR = 5,                  /* "for"  */
  YYSYMBOL_TOKEN_WHILE = 6,                /* "while"  */
  YYSYMBOL_TOKEN_DO = 7,                   /* "do"  */
  YYSYMBOL_TOKEN_RW = 8,                   /* "rw"  */
  YYSYMBOL_TOKEN_VAL = 9,                  /* "val"  */
  YYSYMBOL_TOKEN_IN = 10,                  /* "in"  */
  YYSYMBOL_TOKEN_IF = 11,                  /* "if"  */
  YYSYMBOL_TOKEN_IS = 12,                  /* "is"  */
  YYSYMBOL_TOKEN_ELIF = 13,                /* "elif"  */
  YYSYMBOL_TOKEN_ELSE = 14,                /* "else"  */
  YYSYMBOL_TOKEN_BREAK = 15,               /* "break"  */
  YYSYMBOL_TOKEN_SKIP = 16,                /* "skip"  */
  YYSYMBOL_TOKEN_BY = 17,                  /* "by"  */
  YYSYMBOL_TOKEN_RET = 18,                 /* "ret"  */
  YYSYMBOL_TOKEN_TRUE = 19,                /* "true"  */
  YYSYMBOL_TOKEN_FALSE = 20,               /* "false"  */
  YYSYMBOL_TOKEN_NIL = 21,                 /* "nil"  */
  YYSYMBOL_TOKEN_TYPEOF = 22,              /* "typeof"  */
  YYSYMBOL_TOKEN_TRY = 23,                 /* "try"  */
  YYSYMBOL_TOKEN_CATCH = 24,               /* "catch"  */
  YYSYMBOL_TOKEN_THROW = 25,               /* "throw"  */
  YYSYMBOL_TOKEN_YIELD = 26,               /* "yield"  */
  YYSYMBOL_TOKEN_EACH = 27,                /* "each"  */
  YYSYMBOL_TOKEN_ARRAY = 28,               /* "array"  */
  YYSYMBOL_TOKEN_SELF = 29,                /* "self"  */
  YYSYMBOL_TOKEN_ERROR = 30,               /* "error"  */
  YYSYMBOL_TOKEN_RETRY = 31,               /* "retry"  */
  YYSYMBOL_TOKEN_WITH = 32,                /* "with"  */
  YYSYMBOL_TOKEN_NAN = 33,                 /* "NaN"  */
  YYSYMBOL_TOKEN_INFINITY = 34,            /* "Infinity"  */
  YYSYMBOL_TYPE_NIL = 35,                  /* "Nil"  */
  YYSYMBOL_TYPE_NUMBER = 36,               /* "Number"  */
  YYSYMBOL_TYPE_STRING = 37,               /* "String"  */
  YYSYMBOL_TYPE_BOOLEAN = 38,              /* "Boolean"  */
  YYSYMBOL_TYPE_TABLE = 39,                /* "Table"  */
  YYSYMBOL_TYPE_ARRAY = 40,                /* "Array"  */
  YYSYMBOL_TYPE_FIBER = 41,                /* "Fiber"  */
  YYSYMBOL_TYPE_CHUNK = 42,                /* "Function"  */
  YYSYMBOL_TYPE_ERROR = 43,                /* "Error"  */
  YYSYMBOL_TYPE_TYPE = 44,                 /* "Type"  */
  YYSYMBOL_TYPE_NATIVE_FUNCTION = 45,      /* "NativeFunction"  */
  YYSYMBOL_TYPE_NATIVE_OBJECT = 46,        /* "NativeObject"  */
  YYSYMBOL_TOKEN_ASSIGN_ADD = 47,          /* "+="  */
  YYSYMBOL_TOKEN_ASSIGN_SUBTRACT = 48,     /* "-="  */
  YYSYMBOL_TOKEN_ASSIGN_MULTIPLY = 49,     /* "*="  */
  YYSYMBOL_TOKEN_ASSIGN_DIVIDE = 50,       /* "/="  */
  YYSYMBOL_TOKEN_ASSIGN_MODULO = 51,       /* "%="  */
  YYSYMBOL_TOKEN_ASSIGN_BITWISE_AND = 52,  /* "&="  */
  YYSYMBOL_TOKEN_ASSIGN_BITWISE_OR = 53,   /* "|="  */
  YYSYMBOL_TOKEN_ASSIGN_BITWISE_XOR = 54,  /* "^="  */
  YYSYMBOL_TOKEN_ASSIGN_SHIFT_RIGHT = 55,  /* ">>="  */
  YYSYMBOL_TOKEN_ASSIGN_SHIFT_LEFT = 56,   /* "<<="  */
  YYSYMBOL_TOKEN_ASSIGN_COALESCE = 57,     /* "??="  */
  YYSYMBOL_TOKEN_DEEP_EQUAL = 58,          /* "<==>"  */
  YYSYMBOL_TOKEN_DEEP_NOT_EQUAL = 59,      /* "<!=>"  */
  YYSYMBOL_TOKEN_EQUAL = 60,               /* "=="  */
  YYSYMBOL_TOKEN_NOT_EQUAL = 61,           /* "!="  */
  YYSYMBOL_TOKEN_LESS_EQUAL = 62,          /* "<="  */
  YYSYMBOL_TOKEN_GREATER_EQUAL = 63,       /* ">="  */
  YYSYMBOL_TOKEN_SHIFT_LEFT = 64,          /* "<<"  */
  YYSYMBOL_TOKEN_SHIFT_RIGHT = 65,         /* ">>"  */
  YYSYMBOL_TOKEN_LOGICAL_AND = 66,         /* "&&"  */
  YYSYMBOL_TOKEN_LOGICAL_OR = 67,          /* "||"  */
  YYSYMBOL_TOKEN_INCREMENT = 68,           /* "++"  */
  YYSYMBOL_TOKEN_DECREMENT = 69,           /* "--"  */
  YYSYMBOL_TOKEN_ELLIPSIS = 70,            /* "..."  */
  YYSYMBOL_TOKEN_ASSOCIATE = 71,           /* "=>"  */
  YYSYMBOL_TOKEN_RIGHT_ARROW = 72,         /* "->"  */
  YYSYMBOL_TOKEN_ELVIS = 73,               /* "?."  */
  YYSYMBOL_TOKEN_ELVIS_ARRAY = 74,         /* "?["  */
  YYSYMBOL_TOKEN_DOUBLE_QUESTION = 75,     /* "??"  */
  YYSYMBOL_TOKEN_DOUBLE_COLON = 76,        /* "::"  */
  YYSYMBOL_TOKEN_NOT_IN = 77,              /* "!in"  */
  YYSYMBOL_TOKEN_IS_NOT = 78,              /* "!is"  */
  YYSYMBOL_TOKEN_ATTRIBUTE_LIST = 79,      /* "#["  */
  YYSYMBOL_IDENTIFIER = 80,                /* "identifier"  */
  YYSYMBOL_TOKEN_NUMBER = 81,              /* "number"  */
  YYSYMBOL_TOKEN_HEX_INTEGER = 82,         /* "hex_integer"  */
  YYSYMBOL_TOKEN_PLAIN_STRING = 83,        /* "plain_string"  */
  YYSYMBOL_TOKEN_INTERPOLATED_STRING = 84, /* "interpolated_string"  */
  YYSYMBOL_LOWER_THAN_ELSE = 85,           /* LOWER_THAN_ELSE  */
  YYSYMBOL_LOWER_THAN_CATCH = 86,          /* LOWER_THAN_CATCH  */
  YYSYMBOL_87_ = 87,                       /* '='  */
  YYSYMBOL_88_ = 88,                       /* '?'  */
  YYSYMBOL_89_ = 89,                       /* ':'  */
  YYSYMBOL_90_ = 90,                       /* '|'  */
  YYSYMBOL_91_ = 91,                       /* '^'  */
  YYSYMBOL_92_ = 92,                       /* '&'  */
  YYSYMBOL_93_ = 93,                       /* '<'  */
  YYSYMBOL_94_ = 94,                       /* '>'  */
  YYSYMBOL_95_ = 95,                       /* '+'  */
  YYSYMBOL_96_ = 96,                       /* '-'  */
  YYSYMBOL_97_ = 97,                       /* '*'  */
  YYSYMBOL_98_ = 98,                       /* '/'  */
  YYSYMBOL_99_ = 99,                       /* '%'  */
  YYSYMBOL_UNARY = 100,                    /* UNARY  */
  YYSYMBOL_PREFIX = 101,                   /* PREFIX  */
  YYSYMBOL_POSTFIX = 102,                  /* POSTFIX  */
  YYSYMBOL_103_ = 103,                     /* '.'  */
  YYSYMBOL_104_ = 104,                     /* '['  */
  YYSYMBOL_105_ = 105,                     /* '('  */
  YYSYMBOL_106_ = 106,                     /* ';'  */
  YYSYMBOL_107_ = 107,                     /* '{'  */
  YYSYMBOL_108_ = 108,                     /* '}'  */
  YYSYMBOL_109_ = 109,                     /* ')'  */
  YYSYMBOL_110_ = 110,                     /* ','  */
  YYSYMBOL_111_ = 111,                     /* ']'  */
  YYSYMBOL_112_ = 112,                     /* '!'  */
  YYSYMBOL_113_ = 113,                     /* '~'  */
  YYSYMBOL_114_ = 114,                     /* '@'  */
  YYSYMBOL_115_ = 115,                     /* '#'  */
  YYSYMBOL_116_ = 116,                     /* '$'  */
  YYSYMBOL_YYACCEPT = 117,                 /* $accept  */
  YYSYMBOL_program = 118,                  /* program  */
  YYSYMBOL_statement_list_opt = 119,       /* statement_list_opt  */
  YYSYMBOL_statement_list = 120,           /* statement_list  */
  YYSYMBOL_statement = 121,                /* statement  */
  YYSYMBOL_expression_statement = 122,     /* expression_statement  */
  YYSYMBOL_block_statement = 123,          /* block_statement  */
  YYSYMBOL_retry_statement = 124,          /* retry_statement  */
  YYSYMBOL_throw_statement = 125,          /* throw_statement  */
  YYSYMBOL_ret_statement = 126,            /* ret_statement  */
  YYSYMBOL_skip_statement = 127,           /* skip_statement  */
  YYSYMBOL_break_statement = 128,          /* break_statement  */
  YYSYMBOL_while_statement = 129,          /* while_statement  */
  YYSYMBOL_do_while_statement = 130,       /* do_while_statement  */
  YYSYMBOL_val_statement = 131,            /* val_statement  */
  YYSYMBOL_val_declarator_seq = 132,       /* val_declarator_seq  */
  YYSYMBOL_val_declarator = 133,           /* val_declarator  */
  YYSYMBOL_each_statement = 134,           /* each_statement  */
  YYSYMBOL_for_statement = 135,            /* for_statement  */
  YYSYMBOL_for_init_list_opt = 136,        /* for_init_list_opt  */
  YYSYMBOL_for_init_list = 137,            /* for_init_list  */
  YYSYMBOL_for_init_item = 138,            /* for_init_item  */
  YYSYMBOL_for_step_list_opt = 139,        /* for_step_list_opt  */
  YYSYMBOL_for_step_list = 140,            /* for_step_list  */
  YYSYMBOL_if_statement = 141,             /* if_statement  */
  YYSYMBOL_elif_list = 142,                /* elif_list  */
  YYSYMBOL_else_opt = 143,                 /* else_opt  */
  YYSYMBOL_if_branch = 144,                /* if_branch  */
  YYSYMBOL_try_statement = 145,            /* try_statement  */
  YYSYMBOL_catch_clause_opt = 146,         /* catch_clause_opt  */
  YYSYMBOL_fn_statement = 147,             /* fn_statement  */
  YYSYMBOL_attribute_list_seq = 148,       /* attribute_list_seq  */
  YYSYMBOL_attribute_list = 149,           /* attribute_list  */
  YYSYMBOL_attribute_seq = 150,            /* attribute_seq  */
  YYSYMBOL_attribute_item = 151,           /* attribute_item  */
  YYSYMBOL_attribute_arg_list = 152,       /* attribute_arg_list  */
  YYSYMBOL_attribute_arg = 153,            /* attribute_arg  */
  YYSYMBOL_fn_modifier = 154,              /* fn_modifier  */
  YYSYMBOL_fn_target = 155,                /* fn_target  */
  YYSYMBOL_fn_name_chain = 156,            /* fn_name_chain  */
  YYSYMBOL_parameter_list_opt = 157,       /* parameter_list_opt  */
  YYSYMBOL_parameter_list = 158,           /* parameter_list  */
  YYSYMBOL_param_seq = 159,                /* param_seq  */
  YYSYMBOL_param_item = 160,               /* param_item  */
  YYSYMBOL_fn_body = 161,                  /* fn_body  */
  YYSYMBOL_assignment_expression = 162,    /* assignment_expression  */
  YYSYMBOL_fn_arrow_expression = 163,      /* fn_arrow_expression  */
  YYSYMBOL_conditional_expression = 164,   /* conditional_expression  */
  YYSYMBOL_coalescing_expression = 165,    /* coalescing_expression  */
  YYSYMBOL_coalescing_rhs = 166,           /* coalescing_rhs  */
  YYSYMBOL_logical_or_expression = 167,    /* logical_or_expression  */
  YYSYMBOL_logical_and_expression = 168,   /* logical_and_expression  */
  YYSYMBOL_inclusive_or_expression = 169,  /* inclusive_or_expression  */
  YYSYMBOL_exclusive_or_expression = 170,  /* exclusive_or_expression  */
  YYSYMBOL_and_expression = 171,           /* and_expression  */
  YYSYMBOL_equality_expression = 172,      /* equality_expression  */
  YYSYMBOL_relational_expression = 173,    /* relational_expression  */
  YYSYMBOL_is_native_opt = 174,            /* is_native_opt  */
  YYSYMBOL_type_code = 175,                /* type_code  */
  YYSYMBOL_shift_expression = 176,         /* shift_expression  */
  YYSYMBOL_additive_expression = 177,      /* additive_expression  */
  YYSYMBOL_multiplicative_expression = 178, /* multiplicative_expression  */
  YYSYMBOL_pattern_expression = 179,       /* pattern_expression  */
  YYSYMBOL_by_expression = 180,            /* by_expression  */
  YYSYMBOL_by_arm_seq = 181,               /* by_arm_seq  */
  YYSYMBOL_by_arm = 182,                   /* by_arm  */
  YYSYMBOL_by_value = 183,                 /* by_value  */
  YYSYMBOL_with_expression = 184,          /* with_expression  */
  YYSYMBOL_prefix_expression = 185,        /* prefix_expression  */
  YYSYMBOL_unary_expression = 186,         /* unary_expression  */
  YYSYMBOL_postfix_expression = 187,       /* postfix_expression  */
  YYSYMBOL_argument_list_opt = 188,        /* argument_list_opt  */
  YYSYMBOL_argument_list = 189,            /* argument_list  */
  YYSYMBOL_arg_seq = 190,                  /* arg_seq  */
  YYSYMBOL_primary_expression = 191,       /* primary_expression  */
  YYSYMBOL_table_expression = 192,         /* table_expression  */
  YYSYMBOL_table_entry_list_opt = 193,     /* table_entry_list_opt  */
  YYSYMBOL_table_entry_seq = 194,          /* table_entry_seq  */
  YYSYMBOL_table_entry = 195,              /* table_entry  */
  YYSYMBOL_self_fn_expression = 196,       /* self_fn_expression  */
  YYSYMBOL_array_expression = 197,         /* array_expression  */
  YYSYMBOL_array_element_list_opt = 198,   /* array_element_list_opt  */
  YYSYMBOL_array_element_seq = 199,        /* array_element_seq  */
  YYSYMBOL_error_expression = 200,         /* error_expression  */
  YYSYMBOL_typeof_expression = 201,        /* typeof_expression  */
  YYSYMBOL_yield_expression = 202,         /* yield_expression  */
  YYSYMBOL_fn_block_expression = 203,      /* fn_block_expression  */
  YYSYMBOL_constant_expression = 204,      /* constant_expression  */
  YYSYMBOL_string_literal = 205,           /* string_literal  */
  YYSYMBOL_comma_opt = 206                 /* comma_opt  */
};
typedef enum yysymbol_kind_t yysymbol_kind_t;


/* Default (constant) value used for initialization for null
   right-hand sides.  Unlike the standard yacc.c template, here we set
   the default value of $$ to a zeroed-out value.  Since the default
   value is undefined, this behavior is technically correct.  */
static YYSTYPE yyval_default;
static YYLTYPE yyloc_default
# if defined YYLTYPE_IS_TRIVIAL && YYLTYPE_IS_TRIVIAL
  = { 1, 1, 1, 1 }
# endif
;



#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef short
# undef short
#endif

/* On compilers that do not define __PTRDIFF_MAX__ etc., make sure
   <limits.h> and (if available) <stdint.h> are included
   so that the code can choose integer types of a good width.  */

#ifndef __PTRDIFF_MAX__
# include <limits.h> /* INFRINGES ON USER NAME SPACE */
# if defined __STDC_VERSION__ && 199901 <= __STDC_VERSION__
#  include <stdint.h> /* INFRINGES ON USER NAME SPACE */
#  define YY_STDINT_H
# endif
#endif

/* Narrow types that promote to a signed type and that can represent a
   signed or unsigned integer of at least N bits.  In tables they can
   save space and decrease cache pressure.  Promoting to a signed type
   helps avoid bugs in integer arithmetic.  */

#ifdef __INT_LEAST8_MAX__
typedef __INT_LEAST8_TYPE__ yytype_int8;
#elif defined YY_STDINT_H
typedef int_least8_t yytype_int8;
#else
typedef signed char yytype_int8;
#endif

#ifdef __INT_LEAST16_MAX__
typedef __INT_LEAST16_TYPE__ yytype_int16;
#elif defined YY_STDINT_H
typedef int_least16_t yytype_int16;
#else
typedef short yytype_int16;
#endif

/* Work around bug in HP-UX 11.23, which defines these macros
   incorrectly for preprocessor constants.  This workaround can likely
   be removed in 2023, as HPE has promised support for HP-UX 11.23
   (aka HP-UX 11i v2) only through the end of 2022; see Table 2 of
   <https://h20195.www2.hpe.com/V2/getpdf.aspx/4AA4-7673ENW.pdf>.  */
#ifdef __hpux
# undef UINT_LEAST8_MAX
# undef UINT_LEAST16_MAX
# define UINT_LEAST8_MAX 255
# define UINT_LEAST16_MAX 65535
#endif

#if defined __UINT_LEAST8_MAX__ && __UINT_LEAST8_MAX__ <= __INT_MAX__
typedef __UINT_LEAST8_TYPE__ yytype_uint8;
#elif (!defined __UINT_LEAST8_MAX__ && defined YY_STDINT_H \
       && UINT_LEAST8_MAX <= INT_MAX)
typedef uint_least8_t yytype_uint8;
#elif !defined __UINT_LEAST8_MAX__ && UCHAR_MAX <= INT_MAX
typedef unsigned char yytype_uint8;
#else
typedef short yytype_uint8;
#endif

#if defined __UINT_LEAST16_MAX__ && __UINT_LEAST16_MAX__ <= __INT_MAX__
typedef __UINT_LEAST16_TYPE__ yytype_uint16;
#elif (!defined __UINT_LEAST16_MAX__ && defined YY_STDINT_H \
       && UINT_LEAST16_MAX <= INT_MAX)
typedef uint_least16_t yytype_uint16;
#elif !defined __UINT_LEAST16_MAX__ && USHRT_MAX <= INT_MAX
typedef unsigned short yytype_uint16;
#else
typedef int yytype_uint16;
#endif
#ifndef YYPTRDIFF_T
# if defined __PTRDIFF_TYPE__ && defined __PTRDIFF_MAX__
#  define YYPTRDIFF_T __PTRDIFF_TYPE__
#  define YYPTRDIFF_MAXIMUM __PTRDIFF_MAX__
# elif defined PTRDIFF_MAX
#  ifndef ptrdiff_t
#   include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  endif
#  define YYPTRDIFF_T ptrdiff_t
#  define YYPTRDIFF_MAXIMUM PTRDIFF_MAX
# else
#  define YYPTRDIFF_T long
#  define YYPTRDIFF_MAXIMUM LONG_MAX
# endif
#endif

#ifndef YYSIZE_T
# ifdef __SIZE_TYPE__
#  define YYSIZE_T __SIZE_TYPE__
# elif defined size_t
#  define YYSIZE_T size_t
# elif defined __STDC_VERSION__ && 199901 <= __STDC_VERSION__
#  include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  define YYSIZE_T size_t
# else
#  define YYSIZE_T unsigned
# endif
#endif

#define YYSIZE_MAXIMUM                                  \
  YY_CAST (YYPTRDIFF_T,                                 \
           (YYPTRDIFF_MAXIMUM < YY_CAST (YYSIZE_T, -1)  \
            ? YYPTRDIFF_MAXIMUM                         \
            : YY_CAST (YYSIZE_T, -1)))

#define YYSIZEOF(X) YY_CAST (YYPTRDIFF_T, sizeof (X))


#ifndef YY_
# if defined YYENABLE_NLS && YYENABLE_NLS
#  if ENABLE_NLS
#   include <libintl.h> /* INFRINGES ON USER NAME SPACE */
#   define YY_(Msgid) dgettext ("bison-runtime", Msgid)
#  endif
# endif
# ifndef YY_
#  define YY_(Msgid) Msgid
# endif
#endif


#ifndef YYFREE
# define YYFREE free
#endif
#ifndef YYMALLOC
# define YYMALLOC malloc
#endif
#ifndef YYREALLOC
# define YYREALLOC realloc
#endif

#ifdef __cplusplus
  typedef bool yybool;
# define yytrue true
# define yyfalse false
#else
  /* When we move to stdbool, get rid of the various casts to yybool.  */
  typedef signed char yybool;
# define yytrue 1
# define yyfalse 0
#endif

#ifndef YYSETJMP
# include <setjmp.h>
# define YYJMP_BUF jmp_buf
# define YYSETJMP(Env) setjmp (Env)
/* Pacify Clang and ICC.  */
# define YYLONGJMP(Env, Val)                    \
 do {                                           \
   longjmp (Env, Val);                          \
   YY_ASSERT (0);                               \
 } while (yyfalse)
#endif

#ifndef YY_ATTRIBUTE_PURE
# if defined __GNUC__ && 2 < __GNUC__ + (96 <= __GNUC_MINOR__)
#  define YY_ATTRIBUTE_PURE __attribute__ ((__pure__))
# else
#  define YY_ATTRIBUTE_PURE
# endif
#endif

#ifndef YY_ATTRIBUTE_UNUSED
# if defined __GNUC__ && 2 < __GNUC__ + (7 <= __GNUC_MINOR__)
#  define YY_ATTRIBUTE_UNUSED __attribute__ ((__unused__))
# else
#  define YY_ATTRIBUTE_UNUSED
# endif
#endif

/* The _Noreturn keyword of C11.  */
#ifndef _Noreturn
# if (defined __cplusplus \
      && ((201103 <= __cplusplus && !(__GNUC__ == 4 && __GNUC_MINOR__ == 7)) \
          || (defined _MSC_VER && 1900 <= _MSC_VER)))
#  define _Noreturn [[noreturn]]
# elif ((!defined __cplusplus || defined __clang__) \
        && (201112 <= (defined __STDC_VERSION__ ? __STDC_VERSION__ : 0) \
            || (!defined __STRICT_ANSI__ \
                && (4 < __GNUC__ + (7 <= __GNUC_MINOR__) \
                    || (defined __apple_build_version__ \
                        ? 6000000 <= __apple_build_version__ \
                        : 3 < __clang_major__ + (5 <= __clang_minor__))))))
   /* _Noreturn works as-is.  */
# elif (2 < __GNUC__ + (8 <= __GNUC_MINOR__) || defined __clang__ \
        || 0x5110 <= __SUNPRO_C)
#  define _Noreturn __attribute__ ((__noreturn__))
# elif 1200 <= (defined _MSC_VER ? _MSC_VER : 0)
#  define _Noreturn __declspec (noreturn)
# else
#  define _Noreturn
# endif
#endif

/* Suppress unused-variable warnings by "using" E.  */
#if ! defined lint || defined __GNUC__
# define YY_USE(E) ((void) (E))
#else
# define YY_USE(E) /* empty */
#endif

/* Suppress an incorrect diagnostic about yylval being uninitialized.  */
#if defined __GNUC__ && ! defined __ICC && 406 <= __GNUC__ * 100 + __GNUC_MINOR__
# if __GNUC__ * 100 + __GNUC_MINOR__ < 407
#  define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN                           \
    _Pragma ("GCC diagnostic push")                                     \
    _Pragma ("GCC diagnostic ignored \"-Wuninitialized\"")
# else
#  define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN                           \
    _Pragma ("GCC diagnostic push")                                     \
    _Pragma ("GCC diagnostic ignored \"-Wuninitialized\"")              \
    _Pragma ("GCC diagnostic ignored \"-Wmaybe-uninitialized\"")
# endif
# define YY_IGNORE_MAYBE_UNINITIALIZED_END      \
    _Pragma ("GCC diagnostic pop")
#else
# define YY_INITIAL_VALUE(Value) Value
#endif
#ifndef YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_END
#endif
#ifndef YY_INITIAL_VALUE
# define YY_INITIAL_VALUE(Value) /* Nothing. */
#endif

#if defined __cplusplus && defined __GNUC__ && ! defined __ICC && 6 <= __GNUC__
# define YY_IGNORE_USELESS_CAST_BEGIN                          \
    _Pragma ("GCC diagnostic push")                            \
    _Pragma ("GCC diagnostic ignored \"-Wuseless-cast\"")
# define YY_IGNORE_USELESS_CAST_END            \
    _Pragma ("GCC diagnostic pop")
#endif
#ifndef YY_IGNORE_USELESS_CAST_BEGIN
# define YY_IGNORE_USELESS_CAST_BEGIN
# define YY_IGNORE_USELESS_CAST_END
#endif


#define YY_ASSERT(E) ((void) (0 && (E)))

/* YYFINAL -- State number of the termination state.  */
#define YYFINAL  165
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   2326

/* YYNTOKENS -- Number of terminals.  */
#define YYNTOKENS  117
/* YYNNTS -- Number of nonterminals.  */
#define YYNNTS  90
/* YYNRULES -- Number of rules.  */
#define YYNRULES  261
/* YYNSTATES -- Number of states.  */
#define YYNSTATES  491
/* YYMAXRHS -- Maximum number of symbols on right-hand side of rule.  */
#define YYMAXRHS 11
/* YYMAXLEFT -- Maximum number of symbols to the left of a handle
   accessed by $0, $-1, etc., in any rule.  */
#define YYMAXLEFT 0

/* YYMAXUTOK -- Last valid token kind.  */
#define YYMAXUTOK   344

/* YYTRANSLATE(TOKEN-NUM) -- Symbol number corresponding to TOKEN-NUM
   as returned by yylex, with out-of-bounds checking.  */
#define YYTRANSLATE(YYX)                                \
  (0 <= (YYX) && (YYX) <= YYMAXUTOK                     \
   ? YY_CAST (yysymbol_kind_t, yytranslate[YYX])        \
   : YYSYMBOL_YYUNDEF)

/* YYTRANSLATE[TOKEN-NUM] -- Symbol number corresponding to TOKEN-NUM
   as returned by yylex.  */
static const yytype_int8 yytranslate[] =
{
       0,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,   112,     2,   115,   116,    99,    92,     2,
     105,   109,    97,    95,   110,    96,   103,    98,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,    89,   106,
      93,    87,    94,    88,   114,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,   104,     2,   111,    91,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,   107,    90,   108,   113,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     1,     2,     3,     4,
       5,     6,     7,     8,     9,    10,    11,    12,    13,    14,
      15,    16,    17,    18,    19,    20,    21,    22,    23,    24,
      25,    26,    27,    28,    29,    30,    31,    32,    33,    34,
      35,    36,    37,    38,    39,    40,    41,    42,    43,    44,
      45,    46,    47,    48,    49,    50,    51,    52,    53,    54,
      55,    56,    57,    58,    59,    60,    61,    62,    63,    64,
      65,    66,    67,    68,    69,    70,    71,    72,    73,    74,
      75,    76,    77,    78,    79,    80,    81,    82,    83,    84,
      85,    86,   100,   101,   102
};

#if YYDEBUG
/* YYRLINE[YYN] -- source line where rule number YYN was defined.  */
static const yytype_int16 yyrline[] =
{
       0,   239,   239,   245,   246,   251,   253,   258,   259,   260,
     261,   262,   263,   264,   265,   266,   267,   268,   269,   270,
     271,   272,   276,   281,   286,   291,   296,   298,   303,   308,
     313,   318,   324,   326,   331,   337,   346,   348,   354,   363,
     376,   387,   388,   392,   394,   399,   401,   403,   408,   409,
     413,   415,   421,   434,   435,   445,   446,   450,   452,   458,
     465,   472,   482,   487,   509,   534,   540,   549,   554,   555,
     559,   561,   563,   568,   570,   575,   576,   580,   581,   585,
     597,   603,   612,   614,   619,   620,   625,   626,   632,   639,
     644,   650,   659,   661,   663,   665,   670,   672,   681,   683,
     685,   687,   689,   691,   693,   695,   697,   699,   701,   703,
     705,   707,   712,   728,   730,   740,   742,   752,   753,   761,
     762,   767,   768,   773,   774,   779,   780,   785,   786,   791,
     792,   794,   796,   798,   803,   804,   806,   808,   810,   812,
     814,   816,   821,   829,   830,   835,   836,   837,   838,   839,
     840,   841,   842,   843,   844,   845,   846,   850,   851,   853,
     858,   859,   861,   866,   867,   869,   871,   876,   877,   881,
     891,   897,   906,   908,   910,   915,   916,   924,   925,   934,
     936,   938,   943,   944,   946,   948,   950,   952,   954,   956,
     961,   962,   964,   970,   976,   983,   990,   997,   999,  1004,
    1005,  1009,  1013,  1015,  1017,  1019,  1024,  1025,  1026,  1027,
    1028,  1029,  1030,  1031,  1032,  1033,  1034,  1038,  1044,  1045,
    1050,  1056,  1065,  1067,  1069,  1071,  1073,  1075,  1077,  1079,
    1084,  1096,  1115,  1117,  1119,  1121,  1123,  1128,  1129,  1133,
    1135,  1140,  1142,  1144,  1149,  1151,  1156,  1163,  1173,  1185,
    1186,  1187,  1188,  1189,  1190,  1191,  1192,  1193,  1197,  1198,
    1202,  1203
};
#endif

#define YYPACT_NINF (-411)
#define YYTABLE_NINF (-85)

/* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
   STATE-NUM.  */
static const yytype_int16 yypact[] =
{
     709,   -40,    44,   -48,   -29,   709,    57,    -6,    15,   -18,
       8,  1712,   909,  -411,  -411,  -411,   -53,   709,  1712,     3,
      19,   -47,  -411,    16,    29,  -411,  -411,  -411,  -411,  -411,
    -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  2210,
    2210,  -411,    58,  -411,  -411,  -411,  -411,  -411,  2210,  2210,
    1712,  1712,   481,  2210,  2210,  2210,  2210,  2210,   149,  -411,
     709,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,
    -411,  -411,  -411,  -411,  -411,  -411,  -411,    12,  -411,   -10,
      66,  -411,  -411,    85,   -19,   108,    86,    87,    83,    69,
       1,  -411,    36,    47,   -13,  -411,  -411,   148,  -411,   -26,
     -23,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,
       9,   -39,  -411,   809,  1712,   175,    -6,    95,    -8,  -411,
    1712,  -411,  -411,    78,  1812,    77,  -411,    79,  1712,    81,
     165,   100,   -31,   182,  1009,  1712,    64,  -411,  -411,    78,
    -411,  -411,   104,   -14,  -411,  -411,  -411,  -411,    80,    82,
     101,   137,   125,  1712,   107,   -37,   109,   106,  -411,  -411,
    -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,   -10,
     142,  -411,    78,   -41,  -411,  1712,  1912,  1109,  1912,  1912,
    1912,  1912,  1912,  1912,  1912,  1912,  1912,   271,  1912,  1912,
    1912,   271,  1912,  1912,  1912,  1912,  1912,  1912,  1912,  1912,
    1912,   113,  1712,  1712,  1712,  1712,  1712,  1712,  1712,  1712,
    1712,  1712,  1712,  1712,  -411,  -411,   141,  1712,   143,   144,
    1712,  1209,   145,   112,   139,   118,   120,  -411,  1712,   709,
    -411,   223,    -6,   127,   124,  -411,  -411,   128,   133,    -3,
    1712,  -411,    -6,   131,   170,  1312,  -411,   134,  1712,    34,
    -411,  -411,   151,  1712,   233,   140,   147,   138,   152,   150,
    1412,    58,  -411,  -411,  1712,  -411,  -411,   245,  2012,   154,
    -411,  2012,  -411,  1812,  -411,    78,   169,   -36,   172,   173,
     161,   108,  -411,  1712,  -411,  -411,    86,    87,    83,    69,
       1,     1,     1,     1,    36,    64,    36,    36,    36,    64,
      36,    36,    47,    47,   -13,   -13,  -411,  -411,  -411,  -411,
    -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,  -411,
    -411,  -411,  -411,   155,   153,  -411,   156,  -411,  -411,   159,
    -411,   160,   167,    10,  1712,  -411,    10,  -411,    -6,   162,
    1712,   809,   709,  1712,  -411,  -411,  -411,   595,   171,    84,
     174,  -411,  -411,   189,   183,  -411,   164,   188,   191,  1712,
     193,  -411,   194,   215,  -411,    48,  -411,  -411,  -411,  -411,
      78,  -411,  -411,   232,  -411,  -411,  -411,   -36,  -411,  1712,
    -411,  -411,  -411,  -411,  1912,  -411,  -411,  -411,  -411,  -411,
    1209,  -411,  -411,  1512,  -411,  1712,   120,  -411,  -411,   208,
     198,  -411,  -411,   210,  1712,  -411,  -411,  1612,  1612,  1612,
    1312,   212,  -411,   213,  1209,   227,   -17,   216,  1712,  -411,
    1712,  -411,  2112,   -32,  2012,  -411,   217,  -411,   218,  -411,
    -411,  -411,  1712,   220,   222,   146,  1712,  -411,  -411,  -411,
    -411,  -411,  -411,   150,   221,   224,  1712,   253,  -411,   228,
    -411,  -411,  1712,  -411,  -411,  -411,  -411,  -411,   226,   229,
    -411,  -411,  -411,   236,   595,  -411,  -411,  -411,  -411,  1209,
     234,   249,  -411,  -411,   709,  1712,  1712,  -411,   235,   709,
    1712,  -411,  -411,   237,  -411,  -411,   238,   595,   709,  -411,
    -411
};

/* YYDEFACT[STATE-NUM] -- Default reduction number in state STATE-NUM.
   Performed when YYTABLE does not specify something else to do.  Zero
   means the default is an error.  */
static const yytype_int16 yydefact[] =
{
       3,    77,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,   252,   253,   254,     0,     0,     0,     0,
       0,     0,   213,     0,     0,   255,   256,   145,   146,   147,
     148,   149,   150,   151,   152,   153,   154,   155,   156,     0,
       0,   210,     0,   212,   249,   250,   258,   259,     0,     0,
     237,     0,     3,     0,     0,     0,     0,     0,     0,     2,
       4,     5,    21,    13,    16,    17,    18,    19,    20,    10,
      11,    14,    12,     9,     8,    15,     7,     0,    65,     0,
       0,   111,    98,   113,   115,   119,   121,   123,   125,   127,
     129,   257,   134,   157,   160,   163,   167,   168,   177,   181,
     182,   190,   207,   208,   209,   214,   215,   211,   216,   251,
      86,     0,    78,    41,     0,     0,     0,    36,     0,    34,
       0,    29,    28,    84,   218,     0,    26,     0,     0,     0,
      62,     0,     0,     0,     0,   237,     0,   243,    24,    84,
     179,   180,    70,     0,    68,   183,   184,   239,     0,   260,
       0,   213,   212,   237,     0,   222,     0,   260,   220,   223,
     185,   186,   187,   188,   189,     1,     6,    77,    66,     0,
       0,    82,    84,    79,    22,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,   197,   198,     0,     0,     0,     0,
       0,   199,     0,    87,    92,     0,    89,    90,     0,     3,
     248,     0,     0,     0,    42,    43,    47,     0,     0,     0,
       0,    32,     0,     0,   222,     0,    27,     0,     0,     0,
      59,    25,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,    67,   232,   261,   238,   206,     0,     0,   239,
      23,     0,   217,   261,   219,    84,     0,     0,     0,     0,
       0,   120,   181,     0,   117,   116,   122,   124,   126,   128,
     132,   133,   130,   131,   139,   143,   137,   138,   140,   143,
     135,   136,   158,   159,   161,   162,   164,   165,   166,   178,
     100,   101,   102,   103,   104,   105,   106,   107,   109,   108,
     110,    99,   194,     0,     0,   193,     0,   203,   202,     0,
     200,   260,    94,     0,     0,    85,     0,   112,     0,    45,
       0,     0,     0,     0,    33,    37,    35,     0,     0,     0,
     260,   170,   244,     0,     0,    60,     0,     0,     0,   237,
     233,   236,   241,   212,    71,     0,    73,    75,    69,   240,
      84,   224,   225,     0,   228,   229,   221,     0,    81,     0,
      96,    64,    80,    83,     0,   118,   141,   144,   142,   195,
     199,   192,   191,   261,   201,     0,    88,    93,    91,    46,
       0,    44,    30,     0,     0,    57,    53,     0,     0,     0,
     261,     0,   245,     0,   199,     0,     0,     0,   237,   242,
       0,    72,     0,     0,     0,    63,     0,   114,     0,   205,
     204,    95,    48,     0,     0,    55,     0,   175,   174,   173,
     172,   171,   169,     0,     0,     0,     0,     0,   235,     0,
      76,    74,     0,   230,   226,   227,    97,   196,     0,    49,
      50,    31,    58,     0,     0,    52,   176,    61,   246,   199,
       0,     0,   234,   231,     0,     0,     0,    56,     0,     0,
       0,    40,    51,     0,   247,    38,     0,     0,     0,    54,
      39
};

/* YYPGOTO[NTERM-NUM].  */
static const yytype_int16 yypgoto[] =
{
    -411,  -411,   345,  -411,    -5,  -411,   -72,  -411,  -411,  -411,
    -411,  -411,  -411,  -411,  -411,  -110,   110,  -411,  -411,  -411,
    -411,     7,  -411,  -411,  -411,  -411,  -411,  -410,  -411,  -411,
    -411,  -411,   272,  -411,    92,  -411,   -68,   278,   187,  -411,
    -135,  -411,    24,    22,   -16,    -9,  -411,   -25,  -411,  -411,
    -411,   184,   185,   186,   192,   181,   -51,    65,  -104,   -80,
     -33,   -27,   -82,  -411,  -411,   -44,  -244,  -411,  -411,    97,
    -411,  -370,  -411,  -411,  -411,   -22,  -411,  -411,    94,  -261,
    -411,  -130,  -411,  -411,  -411,  -411,  -411,  -411,  -128,  -143
};

/* YYDEFGOTO[NTERM-NUM].  */
static const yytype_int16 yydefgoto[] =
{
       0,    58,   154,    60,    61,    62,    63,    64,    65,    66,
      67,    68,    69,    70,    71,   118,   119,    72,    73,   233,
     234,   235,   458,   459,    74,   435,   465,   406,    75,   250,
      76,    77,    78,   143,   144,   365,   366,    79,   172,   173,
     111,   225,   226,   227,   381,    80,    81,    82,    83,   285,
      84,    85,    86,    87,    88,    89,    90,   386,    91,    92,
      93,    94,    95,    96,   350,   351,   438,    97,    98,    99,
     100,   329,   330,   331,   101,   102,   156,   157,   158,   159,
     103,   148,   149,   104,   105,   106,   107,   108,   109,   265
};

/* YYTABLE[YYPACT[STATE-NUM]] -- What to do in state STATE-NUM.  If
   positive, shift that token.  If negative, reduce the rule whose
   number is the opposite.  If YYTABLE_NINF, syntax error.  */
static const yytype_int16 yytable[] =
{
     115,   137,   125,   127,   259,   257,   239,   372,   258,   131,
     375,   186,   130,   187,   274,   167,     2,   222,   222,   170,
     428,   202,   203,   204,   205,   206,   207,   208,   209,   210,
     211,   212,   -84,   228,   271,   278,   379,   277,   223,   230,
     452,   147,   150,   155,   444,   214,   215,   112,   176,   252,
     216,   217,   128,   218,   477,   166,   177,   113,   134,   129,
     135,   213,   279,   188,   189,   110,   116,   -84,   229,   174,
     171,   229,   446,   253,   117,   229,   114,   489,   190,   191,
     219,   220,   221,   295,   198,   199,   200,   299,   121,   224,
     224,    42,   261,   447,   192,   193,   132,   262,   241,   478,
     194,   195,   242,   344,   236,   237,   294,   242,   296,   297,
     298,   243,   300,   301,   122,   244,   306,   307,   308,   247,
     120,   136,   339,   124,   133,   256,   147,   182,   183,   184,
     185,   290,   291,   292,   293,   138,   140,   141,   142,   354,
     377,   229,   196,   197,   269,   145,   146,    46,    47,   165,
     160,   161,   162,   163,   164,   408,   409,   421,   422,   463,
     464,   302,   303,   455,   439,   440,   280,   387,   284,   304,
     305,   387,   174,   175,   178,   181,   179,   355,   180,   309,
     201,   238,   240,   110,   245,   246,   248,   230,   394,   249,
     254,   263,   264,   310,   311,   312,   313,   314,   315,   316,
     317,   318,   319,   320,   321,   380,   251,   411,   323,   260,
     266,   326,   328,   267,   268,   270,   273,   272,   276,   337,
     124,   322,   333,   324,   325,   332,   334,   335,   399,   417,
     336,   345,   338,   340,   341,   423,   349,   342,   343,   353,
     347,   271,   358,   352,   357,   356,   361,   359,   370,   378,
     384,   367,   382,   383,   395,   369,   360,   229,   390,   371,
     407,   362,   374,   413,   244,   373,   389,   391,   392,   414,
     393,   416,   242,   282,   385,   282,   282,   282,   282,   282,
     282,   282,   282,   282,   410,   282,   282,   282,   449,   282,
     282,   282,   282,   282,   282,   282,   282,   282,   412,   415,
     418,   124,   420,   424,   432,   380,    27,    28,    29,    30,
      31,    32,    33,    34,    35,    36,    37,    38,   242,   433,
     442,   445,   443,   456,   448,   397,   461,   457,   462,   469,
     468,   400,   236,   471,   403,   474,   472,   402,   480,   475,
     419,   476,   405,   479,   484,    59,   487,   488,   401,   168,
     147,   453,   346,   368,   451,   169,   275,   396,   398,   427,
     281,   425,   289,   286,   388,   287,   441,   376,     0,     0,
     426,   467,   288,     0,     0,     0,     0,     0,     0,     0,
       0,   328,     0,     0,   430,     0,   431,     0,     0,     0,
       0,     0,     0,     0,     0,   434,     0,     0,   437,   437,
     437,   349,     0,     0,     0,   328,     0,     0,     0,   147,
       0,   450,     0,   367,     0,   454,     0,     0,     0,     0,
       0,     0,     0,   460,     0,     0,     0,   466,     0,     0,
       0,     0,     0,     0,     0,     0,     0,   470,     0,     0,
       0,     0,     0,   473,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,   405,
     328,     0,     0,     0,     0,     0,   482,   483,     0,   481,
       0,   486,     0,     0,   485,     0,     0,     0,     0,     0,
       0,   282,   405,   490,     1,     2,     3,     4,     5,     6,
       7,     0,     8,     0,     0,     0,     9,    10,    11,    12,
      13,    14,    15,    16,    17,     0,    18,    19,    20,    21,
     151,    23,    24,     0,    25,    26,    27,    28,    29,    30,
      31,    32,    33,    34,    35,    36,    37,    38,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    39,
      40,    41,     0,     0,     0,     0,     0,     0,     0,     0,
      42,   152,    44,    45,    46,    47,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,    48,    49,     0,     0,
       0,     0,     0,     0,     0,   153,    51,     0,    52,    -3,
       0,     0,     0,    53,    54,    55,    56,    57,     1,     2,
       3,     4,     5,     6,     7,     0,     8,     0,     0,     0,
       9,    10,    11,    12,    13,    14,    15,    16,    17,     0,
      18,    19,    20,    21,    22,    23,    24,     0,    25,    26,
      27,    28,    29,    30,    31,    32,    33,    34,    35,    36,
      37,    38,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,    39,    40,    41,     0,   404,     0,     0,
       0,     0,     0,     0,    42,    43,    44,    45,    46,    47,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      48,    49,     0,     0,     0,     0,     0,     0,     0,    50,
      51,     0,    52,     0,     0,     0,     0,    53,    54,    55,
      56,    57,     1,     2,     3,     4,     5,     6,     7,     0,
       8,     0,     0,     0,     9,    10,    11,    12,    13,    14,
      15,    16,    17,     0,    18,    19,    20,    21,    22,    23,
      24,     0,    25,    26,    27,    28,    29,    30,    31,    32,
      33,    34,    35,    36,    37,    38,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    39,    40,    41,
       0,     0,     0,     0,     0,     0,     0,     0,    42,    43,
      44,    45,    46,    47,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,    48,    49,     0,     0,     0,     0,
       0,     0,   123,    50,    51,     0,    52,   231,   232,     0,
       0,    53,    54,    55,    56,    57,    11,     0,    13,    14,
      15,    16,     0,     0,     0,    19,     0,    21,    22,    23,
       0,     0,    25,    26,    27,    28,    29,    30,    31,    32,
      33,    34,    35,    36,    37,    38,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    39,    40,    41,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    43,
      44,    45,    46,    47,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,    48,    49,     0,     0,     0,     0,
       0,     0,   123,    50,    51,     0,   124,     0,     0,     0,
       0,    53,    54,    55,    56,    57,    11,     0,    13,    14,
      15,    16,     0,     0,     0,    19,     0,    21,    22,    23,
       0,     0,    25,    26,    27,    28,    29,    30,    31,    32,
      33,    34,    35,    36,    37,    38,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    39,    40,    41,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    43,
      44,    45,    46,    47,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,    48,    49,     0,     0,     0,     0,
       0,     0,   123,    50,    51,   126,   124,     0,     0,     0,
       0,    53,    54,    55,    56,    57,    11,     0,    13,    14,
      15,    16,     0,     0,     0,    19,     0,    21,    22,    23,
       0,     0,    25,    26,    27,    28,    29,    30,    31,    32,
      33,    34,    35,    36,    37,    38,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    39,    40,    41,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    43,
      44,    45,    46,    47,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,    48,    49,     0,     0,     0,     0,
       0,     0,   123,    50,    51,     0,   124,     0,   255,     0,
       0,    53,    54,    55,    56,    57,    11,     0,    13,    14,
      15,    16,     0,     0,   283,    19,     0,    21,    22,    23,
       0,     0,    25,    26,    27,    28,    29,    30,    31,    32,
      33,    34,    35,    36,    37,    38,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    39,    40,    41,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    43,
      44,    45,    46,    47,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,    48,    49,     0,     0,     0,     0,
       0,     0,   123,    50,    51,     0,   124,     0,     0,     0,
       0,    53,    54,    55,    56,    57,    11,     0,    13,    14,
      15,    16,     0,     0,     0,    19,     0,    21,    22,    23,
       0,     0,    25,    26,    27,    28,    29,    30,    31,    32,
      33,    34,    35,    36,    37,    38,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    39,    40,    41,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    43,
      44,    45,    46,    47,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,    48,    49,   327,     0,     0,     0,
       0,     0,     0,    50,    51,   123,   124,     0,     0,     0,
       0,    53,    54,    55,    56,    57,   348,     0,     0,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,    22,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,    43,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,     0,     0,   123,    50,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,    22,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,   363,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,     0,     0,   123,    50,    51,     0,   124,
       0,   364,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,    22,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,    43,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,   429,
       0,     0,     0,     0,     0,   123,    50,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,   436,    19,     0,
      21,    22,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,    43,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,     0,     0,   123,    50,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,    22,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,    43,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,     0,     0,   123,    50,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,   151,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,   152,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,     0,     0,   139,   153,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,    22,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,    43,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,     0,     0,   123,    50,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,   151,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,    43,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,     0,     0,   123,    50,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    11,
       0,    13,    14,    15,    16,     0,     0,     0,    19,     0,
      21,    22,    23,     0,     0,    25,    26,    27,    28,    29,
      30,    31,    32,    33,    34,    35,    36,    37,    38,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      39,    40,    41,     0,     0,     0,     0,     0,     0,     0,
       0,     0,   363,    44,    45,    46,    47,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    48,    49,     0,
       0,     0,     0,   139,     0,     0,    50,    51,     0,   124,
       0,     0,     0,     0,    53,    54,    55,    56,    57,    13,
      14,    15,    16,     0,     0,     0,    19,     0,    21,    22,
      23,     0,     0,    25,    26,    27,    28,    29,    30,    31,
      32,    33,    34,    35,    36,    37,    38,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      41,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      43,    44,    45,    46,    47,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,    48,    49,     0,     0,     0,
       0,     0,     0,     0,    50,    51,     0,   124,     0,     0,
       0,     0,    53,    54,    55,    56,    57
};

static const yytype_int16 yycheck[] =
{
       5,    23,    11,    12,   139,   135,   116,   268,   136,    18,
     271,    10,    17,    12,   157,     3,     4,     8,     8,    29,
     390,    47,    48,    49,    50,    51,    52,    53,    54,    55,
      56,    57,    72,    72,    71,    76,    72,   172,    29,   111,
      72,    50,    51,    52,   414,    68,    69,     3,    67,    80,
      73,    74,   105,    76,   464,    60,    75,   105,   105,   112,
     107,    87,   103,    62,    63,   105,     9,   107,   107,   106,
      80,   107,    89,   104,    80,   107,   105,   487,    77,    78,
     103,   104,   105,   187,    97,    98,    99,   191,   106,    80,
      80,    79,   106,   110,    93,    94,    93,   111,   106,   469,
      64,    65,   110,   106,   113,   114,   186,   110,   188,   189,
     190,   120,   192,   193,   106,   124,   198,   199,   200,   128,
     105,   105,   232,   107,   105,   134,   135,    58,    59,    60,
      61,   182,   183,   184,   185,   106,    39,    40,    80,   105,
     275,   107,    95,    96,   153,    48,    49,    83,    84,     0,
      53,    54,    55,    56,    57,    71,    72,   109,   110,    13,
      14,   194,   195,   424,   408,   409,   175,   295,   177,   196,
     197,   299,   106,    88,    66,    92,    90,   249,    91,   201,
      32,     6,    87,   105,   107,   106,   105,   259,   331,    24,
       8,   111,   110,   202,   203,   204,   205,   206,   207,   208,
     209,   210,   211,   212,   213,   277,   106,   350,   217,   105,
     109,   220,   221,    76,    89,   108,   110,   108,    76,   228,
     107,    80,   110,    80,    80,    80,    87,   109,   338,   359,
     110,   240,     9,   106,   110,   370,   245,   109,   105,   248,
     109,    71,     9,   109,   253,    94,   108,   107,     3,    80,
      89,   260,    80,    80,    87,   264,   109,   107,   105,   268,
      89,   109,   271,    80,   273,   111,   111,   111,   109,   105,
     110,    80,   110,   176,   283,   178,   179,   180,   181,   182,
     183,   184,   185,   186,   110,   188,   189,   190,   418,   192,
     193,   194,   195,   196,   197,   198,   199,   200,   109,   111,
     107,   107,    87,    71,   106,   377,    35,    36,    37,    38,
      39,    40,    41,    42,    43,    44,    45,    46,   110,   109,
     108,    94,   109,   106,   108,   334,   106,   109,   106,   105,
     109,   340,   341,    80,   343,   109,   108,   342,    89,   110,
     362,   105,   347,   109,   109,     0,   109,   109,   341,    77,
     359,   423,   242,   261,   422,    77,   169,   333,   336,   384,
     176,   377,   181,   178,   299,   179,   410,   273,    -1,    -1,
     379,   443,   180,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,   390,    -1,    -1,   393,    -1,   395,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,   404,    -1,    -1,   407,   408,
     409,   410,    -1,    -1,    -1,   414,    -1,    -1,    -1,   418,
      -1,   420,    -1,   422,    -1,   424,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,   432,    -1,    -1,    -1,   436,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,   446,    -1,    -1,
      -1,    -1,    -1,   452,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,   464,
     469,    -1,    -1,    -1,    -1,    -1,   475,   476,    -1,   474,
      -1,   480,    -1,    -1,   479,    -1,    -1,    -1,    -1,    -1,
      -1,   384,   487,   488,     3,     4,     5,     6,     7,     8,
       9,    -1,    11,    -1,    -1,    -1,    15,    16,    17,    18,
      19,    20,    21,    22,    23,    -1,    25,    26,    27,    28,
      29,    30,    31,    -1,    33,    34,    35,    36,    37,    38,
      39,    40,    41,    42,    43,    44,    45,    46,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    68,
      69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      79,    80,    81,    82,    83,    84,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,   104,   105,    -1,   107,   108,
      -1,    -1,    -1,   112,   113,   114,   115,   116,     3,     4,
       5,     6,     7,     8,     9,    -1,    11,    -1,    -1,    -1,
      15,    16,    17,    18,    19,    20,    21,    22,    23,    -1,
      25,    26,    27,    28,    29,    30,    31,    -1,    33,    34,
      35,    36,    37,    38,    39,    40,    41,    42,    43,    44,
      45,    46,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    68,    69,    70,    -1,    72,    -1,    -1,
      -1,    -1,    -1,    -1,    79,    80,    81,    82,    83,    84,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      95,    96,    -1,    -1,    -1,    -1,    -1,    -1,    -1,   104,
     105,    -1,   107,    -1,    -1,    -1,    -1,   112,   113,   114,
     115,   116,     3,     4,     5,     6,     7,     8,     9,    -1,
      11,    -1,    -1,    -1,    15,    16,    17,    18,    19,    20,
      21,    22,    23,    -1,    25,    26,    27,    28,    29,    30,
      31,    -1,    33,    34,    35,    36,    37,    38,    39,    40,
      41,    42,    43,    44,    45,    46,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    68,    69,    70,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    79,    80,
      81,    82,    83,    84,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    95,    96,    -1,    -1,    -1,    -1,
      -1,    -1,     3,   104,   105,    -1,   107,     8,     9,    -1,
      -1,   112,   113,   114,   115,   116,    17,    -1,    19,    20,
      21,    22,    -1,    -1,    -1,    26,    -1,    28,    29,    30,
      -1,    -1,    33,    34,    35,    36,    37,    38,    39,    40,
      41,    42,    43,    44,    45,    46,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    68,    69,    70,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    80,
      81,    82,    83,    84,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    95,    96,    -1,    -1,    -1,    -1,
      -1,    -1,     3,   104,   105,    -1,   107,    -1,    -1,    -1,
      -1,   112,   113,   114,   115,   116,    17,    -1,    19,    20,
      21,    22,    -1,    -1,    -1,    26,    -1,    28,    29,    30,
      -1,    -1,    33,    34,    35,    36,    37,    38,    39,    40,
      41,    42,    43,    44,    45,    46,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    68,    69,    70,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    80,
      81,    82,    83,    84,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    95,    96,    -1,    -1,    -1,    -1,
      -1,    -1,     3,   104,   105,   106,   107,    -1,    -1,    -1,
      -1,   112,   113,   114,   115,   116,    17,    -1,    19,    20,
      21,    22,    -1,    -1,    -1,    26,    -1,    28,    29,    30,
      -1,    -1,    33,    34,    35,    36,    37,    38,    39,    40,
      41,    42,    43,    44,    45,    46,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    68,    69,    70,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    80,
      81,    82,    83,    84,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    95,    96,    -1,    -1,    -1,    -1,
      -1,    -1,     3,   104,   105,    -1,   107,    -1,   109,    -1,
      -1,   112,   113,   114,   115,   116,    17,    -1,    19,    20,
      21,    22,    -1,    -1,    25,    26,    -1,    28,    29,    30,
      -1,    -1,    33,    34,    35,    36,    37,    38,    39,    40,
      41,    42,    43,    44,    45,    46,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    68,    69,    70,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    80,
      81,    82,    83,    84,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    95,    96,    -1,    -1,    -1,    -1,
      -1,    -1,     3,   104,   105,    -1,   107,    -1,    -1,    -1,
      -1,   112,   113,   114,   115,   116,    17,    -1,    19,    20,
      21,    22,    -1,    -1,    -1,    26,    -1,    28,    29,    30,
      -1,    -1,    33,    34,    35,    36,    37,    38,    39,    40,
      41,    42,    43,    44,    45,    46,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    68,    69,    70,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    80,
      81,    82,    83,    84,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    95,    96,    97,    -1,    -1,    -1,
      -1,    -1,    -1,   104,   105,     3,   107,    -1,    -1,    -1,
      -1,   112,   113,   114,   115,   116,    14,    -1,    -1,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,   109,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    97,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    25,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,    -1,    -1,     3,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    17,
      -1,    19,    20,    21,    22,    -1,    -1,    -1,    26,    -1,
      28,    29,    30,    -1,    -1,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      68,    69,    70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    80,    81,    82,    83,    84,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    -1,
      -1,    -1,    -1,     3,    -1,    -1,   104,   105,    -1,   107,
      -1,    -1,    -1,    -1,   112,   113,   114,   115,   116,    19,
      20,    21,    22,    -1,    -1,    -1,    26,    -1,    28,    29,
      30,    -1,    -1,    33,    34,    35,    36,    37,    38,    39,
      40,    41,    42,    43,    44,    45,    46,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      70,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      80,    81,    82,    83,    84,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    95,    96,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,   104,   105,    -1,   107,    -1,    -1,
      -1,    -1,   112,   113,   114,   115,   116
};

/* YYSTOS[STATE-NUM] -- The symbol kind of the accessing symbol of
   state STATE-NUM.  */
static const yytype_uint8 yystos[] =
{
       0,     3,     4,     5,     6,     7,     8,     9,    11,    15,
      16,    17,    18,    19,    20,    21,    22,    23,    25,    26,
      27,    28,    29,    30,    31,    33,    34,    35,    36,    37,
      38,    39,    40,    41,    42,    43,    44,    45,    46,    68,
      69,    70,    79,    80,    81,    82,    83,    84,    95,    96,
     104,   105,   107,   112,   113,   114,   115,   116,   118,   119,
     120,   121,   122,   123,   124,   125,   126,   127,   128,   129,
     130,   131,   134,   135,   141,   145,   147,   148,   149,   154,
     162,   163,   164,   165,   167,   168,   169,   170,   171,   172,
     173,   175,   176,   177,   178,   179,   180,   184,   185,   186,
     187,   191,   192,   197,   200,   201,   202,   203,   204,   205,
     105,   157,     3,   105,   105,   121,     9,    80,   132,   133,
     105,   106,   106,     3,   107,   162,   106,   162,   105,   112,
     121,   162,    93,   105,   105,   107,   105,   192,   106,     3,
     186,   186,    80,   150,   151,   186,   186,   162,   198,   199,
     162,    29,    80,   104,   119,   162,   193,   194,   195,   196,
     186,   186,   186,   186,   186,     0,   121,     3,   149,   154,
      29,    80,   155,   156,   106,    88,    67,    75,    66,    90,
      91,    92,    58,    59,    60,    61,    10,    12,    62,    63,
      77,    78,    93,    94,    64,    65,    95,    96,    97,    98,
      99,    32,    47,    48,    49,    50,    51,    52,    53,    54,
      55,    56,    57,    87,    68,    69,    73,    74,    76,   103,
     104,   105,     8,    29,    80,   158,   159,   160,    72,   107,
     123,     8,     9,   136,   137,   138,   162,   162,     6,   132,
      87,   106,   110,   162,   162,   107,   106,   162,   105,    24,
     146,   106,    80,   104,     8,   109,   162,   198,   205,   157,
     105,   106,   111,   111,   110,   206,   109,    76,    89,   162,
     108,    71,   108,   110,   206,   155,    76,   157,    76,   103,
     162,   168,   186,    25,   162,   166,   169,   170,   171,   172,
     173,   173,   173,   173,   176,   175,   176,   176,   176,   175,
     176,   176,   177,   177,   178,   178,   179,   179,   179,   192,
     162,   162,   162,   162,   162,   162,   162,   162,   162,   162,
     162,   162,    80,   162,    80,    80,   162,    97,   162,   188,
     189,   190,    80,   110,    87,   109,   110,   162,     9,   132,
     106,   110,   109,   105,   106,   162,   133,   109,    14,   162,
     181,   182,   109,   162,   105,   123,    94,   162,     9,   107,
     109,   108,   109,    80,   109,   152,   153,   162,   151,   162,
       3,   162,   196,   111,   162,   196,   195,   157,    80,    72,
     123,   161,    80,    80,    89,   162,   174,   205,   174,   111,
     105,   111,   109,   110,   206,    87,   159,   162,   160,   132,
     162,   138,   121,   162,    72,   121,   144,    89,    71,    72,
     110,   206,   109,    80,   105,   111,    80,   198,   107,   192,
      87,   109,   110,   157,    71,   161,   162,   164,   188,    97,
     162,   162,   106,   109,   162,   142,    25,   162,   183,   183,
     183,   182,   108,   109,   188,    94,    89,   110,   108,   198,
     162,   153,    72,   123,   162,   196,   106,   109,   139,   140,
     162,   106,   106,    13,    14,   143,   162,   123,   109,   105,
     162,    80,   108,   162,   109,   110,   105,   144,   188,   109,
      89,   121,   162,   162,   109,   121,   162,   109,   109,   144,
     121
};

/* YYR1[RULE-NUM] -- Symbol kind of the left-hand side of rule RULE-NUM.  */
static const yytype_uint8 yyr1[] =
{
       0,   117,   118,   119,   119,   120,   120,   121,   121,   121,
     121,   121,   121,   121,   121,   121,   121,   121,   121,   121,
     121,   121,   122,   123,   124,   125,   126,   126,   127,   128,
     129,   130,   131,   131,   132,   132,   133,   133,   134,   134,
     135,   136,   136,   137,   137,   138,   138,   138,   139,   139,
     140,   140,   141,   142,   142,   143,   143,   144,   144,   145,
     145,   145,   146,   147,   147,   148,   148,   149,   150,   150,
     151,   151,   151,   152,   152,   153,   153,   154,   154,   155,
     155,   155,   156,   156,   157,   157,   158,   158,   158,   158,
     159,   159,   160,   160,   160,   160,   161,   161,   162,   162,
     162,   162,   162,   162,   162,   162,   162,   162,   162,   162,
     162,   162,   163,   164,   164,   165,   165,   166,   166,   167,
     167,   168,   168,   169,   169,   170,   170,   171,   171,   172,
     172,   172,   172,   172,   173,   173,   173,   173,   173,   173,
     173,   173,   173,   174,   174,   175,   175,   175,   175,   175,
     175,   175,   175,   175,   175,   175,   175,   176,   176,   176,
     177,   177,   177,   178,   178,   178,   178,   179,   179,   180,
     181,   181,   182,   182,   182,   183,   183,   184,   184,   185,
     185,   185,   186,   186,   186,   186,   186,   186,   186,   186,
     187,   187,   187,   187,   187,   187,   187,   187,   187,   188,
     188,   189,   190,   190,   190,   190,   191,   191,   191,   191,
     191,   191,   191,   191,   191,   191,   191,   192,   193,   193,
     194,   194,   195,   195,   195,   195,   195,   195,   195,   195,
     196,   196,   197,   197,   197,   197,   197,   198,   198,   199,
     199,   200,   200,   200,   201,   201,   202,   202,   203,   204,
     204,   204,   204,   204,   204,   204,   204,   204,   205,   205,
     206,   206
};

/* YYR2[RULE-NUM] -- Number of symbols on the right-hand side of rule RULE-NUM.  */
static const yytype_int8 yyr2[] =
{
       0,     2,     1,     0,     1,     1,     2,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     2,     3,     2,     3,     2,     3,     2,     2,
       5,     7,     3,     4,     1,     3,     1,     3,     9,    11,
       9,     0,     1,     1,     3,     2,     3,     1,     0,     1,
       1,     3,     7,     0,     6,     0,     2,     1,     3,     3,
       4,     7,     0,     5,     4,     1,     2,     3,     1,     3,
       1,     3,     4,     1,     3,     1,     3,     1,     2,     1,
       3,     3,     1,     3,     0,     3,     0,     1,     3,     1,
       1,     3,     1,     3,     2,     4,     1,     3,     1,     3,
       3,     3,     3,     3,     3,     3,     3,     3,     3,     3,
       3,     1,     4,     1,     5,     1,     3,     1,     2,     1,
       3,     1,     3,     1,     3,     1,     3,     1,     3,     1,
       3,     3,     3,     3,     1,     3,     3,     3,     3,     3,
       3,     4,     4,     0,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     3,     3,
       1,     3,     3,     1,     3,     3,     3,     1,     1,     6,
       1,     3,     3,     3,     3,     1,     2,     1,     3,     2,
       2,     1,     1,     2,     2,     2,     2,     2,     2,     2,
       1,     4,     4,     3,     3,     4,     6,     2,     2,     0,
       1,     2,     1,     1,     3,     3,     3,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     3,     0,     2,
       1,     3,     1,     1,     3,     3,     5,     5,     3,     3,
       5,     6,     3,     4,     7,     6,     4,     0,     2,     1,
       3,     4,     5,     2,     4,     5,     7,     9,     3,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       0,     1
};


/* YYDPREC[RULE-NUM] -- Dynamic precedence of rule #RULE-NUM (0 if none).  */
static const yytype_int8 yydprec[] =
{
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     2,     2,     1,     1,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0
};

/* YYMERGER[RULE-NUM] -- Index of merging function for rule #RULE-NUM.  */
static const yytype_int8 yymerger[] =
{
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0
};

/* YYIMMEDIATE[RULE-NUM] -- True iff rule #RULE-NUM is not to be deferred, as
   in the case of predicates.  */
static const yybool yyimmediate[] =
{
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0
};

/* YYCONFLP[YYPACT[STATE-NUM]] -- Pointer into YYCONFL of start of
   list of conflicting reductions corresponding to action entry for
   state STATE-NUM in yytable.  0 means no conflicts.  The list in
   yyconfl is terminated by a rule number of 0.  */
static const yytype_int8 yyconflp[] =
{
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     3,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     5,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     7,     0,     0,     0,     0,
       0,     0,     9,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      11,    13,    15,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,    17,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     1,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0
};

/* YYCONFL[I] -- lists of conflicting rule numbers, each terminated by
   0, pointed into by YYCONFLP.  */
static const short yyconfl[] =
{
       0,   218,     0,   113,     0,   213,     0,   239,     0,    45,
       0,   233,     0,   241,     0,   212,     0,    46,     0
};


/* YYLLOC_DEFAULT -- Set CURRENT to span from RHS[1] to RHS[N].
   If N is 0, then set CURRENT to the empty location which ends
   the previous symbol: RHS[0] (always defined).  */

#ifndef YYLLOC_DEFAULT
# define YYLLOC_DEFAULT(Current, Rhs, N)                                \
    do                                                                  \
      if (N)                                                            \
        {                                                               \
          (Current).first_line   = YYRHSLOC (Rhs, 1).first_line;        \
          (Current).first_column = YYRHSLOC (Rhs, 1).first_column;      \
          (Current).last_line    = YYRHSLOC (Rhs, N).last_line;         \
          (Current).last_column  = YYRHSLOC (Rhs, N).last_column;       \
        }                                                               \
      else                                                              \
        {                                                               \
          (Current).first_line   = (Current).last_line   =              \
            YYRHSLOC (Rhs, 0).last_line;                                \
          (Current).first_column = (Current).last_column =              \
            YYRHSLOC (Rhs, 0).last_column;                              \
        }                                                               \
    while (0)
#endif

# define YYRHSLOC(Rhs, K) ((Rhs)[K].yystate.yyloc)


YYSTYPE yylval;
YYLTYPE yylloc;

int yynerrs;
int yychar;

enum { YYENOMEM = -2 };

typedef enum { yyok, yyaccept, yyabort, yyerr, yynomem } YYRESULTTAG;

#define YYCHK(YYE)                              \
  do {                                          \
    YYRESULTTAG yychk_flag = YYE;               \
    if (yychk_flag != yyok)                     \
      return yychk_flag;                        \
  } while (0)

/* YYINITDEPTH -- initial size of the parser's stacks.  */
#ifndef YYINITDEPTH
# define YYINITDEPTH 200
#endif

/* YYMAXDEPTH -- maximum size the stacks can grow to (effective only
   if the built-in stack extension method is used).

   Do not make this value too large; the results are undefined if
   SIZE_MAX < YYMAXDEPTH * sizeof (GLRStackItem)
   evaluated with infinite-precision integer arithmetic.  */

#ifndef YYMAXDEPTH
# define YYMAXDEPTH 10000
#endif

/* Minimum number of free items on the stack allowed after an
   allocation.  This is to allow allocation and initialization
   to be completed by functions that call yyexpandGLRStack before the
   stack is expanded, thus insuring that all necessary pointers get
   properly redirected to new data.  */
#define YYHEADROOM 2

#ifndef YYSTACKEXPANDABLE
#  define YYSTACKEXPANDABLE 1
#endif

#if YYSTACKEXPANDABLE
# define YY_RESERVE_GLRSTACK(Yystack)                   \
  do {                                                  \
    if (Yystack->yyspaceLeft < YYHEADROOM)              \
      yyexpandGLRStack (Yystack);                       \
  } while (0)
#else
# define YY_RESERVE_GLRSTACK(Yystack)                   \
  do {                                                  \
    if (Yystack->yyspaceLeft < YYHEADROOM)              \
      yyMemoryExhausted (Yystack);                      \
  } while (0)
#endif

/** State numbers. */
typedef int yy_state_t;

/** Rule numbers. */
typedef int yyRuleNum;

/** Item references. */
typedef short yyItemNum;

typedef struct yyGLRState yyGLRState;
typedef struct yyGLRStateSet yyGLRStateSet;
typedef struct yySemanticOption yySemanticOption;
typedef union yyGLRStackItem yyGLRStackItem;
typedef struct yyGLRStack yyGLRStack;

struct yyGLRState
{
  /** Type tag: always true.  */
  yybool yyisState;
  /** Type tag for yysemantics.  If true, yyval applies, otherwise
   *  yyfirstVal applies.  */
  yybool yyresolved;
  /** Number of corresponding LALR(1) machine state.  */
  yy_state_t yylrState;
  /** Preceding state in this stack */
  yyGLRState* yypred;
  /** Source position of the last token produced by my symbol */
  YYPTRDIFF_T yyposn;
  union {
    /** First in a chain of alternative reductions producing the
     *  nonterminal corresponding to this state, threaded through
     *  yynext.  */
    yySemanticOption* yyfirstVal;
    /** Semantic value for this state.  */
    YYSTYPE yyval;
  } yysemantics;
  /** Source location for this state.  */
  YYLTYPE yyloc;
};

struct yyGLRStateSet
{
  yyGLRState** yystates;
  /** During nondeterministic operation, yylookaheadNeeds tracks which
   *  stacks have actually needed the current lookahead.  During deterministic
   *  operation, yylookaheadNeeds[0] is not maintained since it would merely
   *  duplicate yychar != YYEMPTY.  */
  yybool* yylookaheadNeeds;
  YYPTRDIFF_T yysize;
  YYPTRDIFF_T yycapacity;
};

struct yySemanticOption
{
  /** Type tag: always false.  */
  yybool yyisState;
  /** Rule number for this reduction */
  yyRuleNum yyrule;
  /** The last RHS state in the list of states to be reduced.  */
  yyGLRState* yystate;
  /** The lookahead for this reduction.  */
  int yyrawchar;
  YYSTYPE yyval;
  YYLTYPE yyloc;
  /** Next sibling in chain of options.  To facilitate merging,
   *  options are chained in decreasing order by address.  */
  yySemanticOption* yynext;
};

/** Type of the items in the GLR stack.  The yyisState field
 *  indicates which item of the union is valid.  */
union yyGLRStackItem {
  yyGLRState yystate;
  yySemanticOption yyoption;
};

struct yyGLRStack {
  int yyerrState;
  /* To compute the location of the error token.  */
  yyGLRStackItem yyerror_range[3];

  YYJMP_BUF yyexception_buffer;
  yyGLRStackItem* yyitems;
  yyGLRStackItem* yynextFree;
  YYPTRDIFF_T yyspaceLeft;
  yyGLRState* yysplitPoint;
  yyGLRState* yylastDeleted;
  yyGLRStateSet yytops;
};

#if YYSTACKEXPANDABLE
static void yyexpandGLRStack (yyGLRStack* yystackp);
#endif

_Noreturn static void
yyFail (yyGLRStack* yystackp, const char* yymsg)
{
  if (yymsg != YY_NULLPTR)
    yyerror (yymsg);
  YYLONGJMP (yystackp->yyexception_buffer, 1);
}

_Noreturn static void
yyMemoryExhausted (yyGLRStack* yystackp)
{
  YYLONGJMP (yystackp->yyexception_buffer, 2);
}

/** Accessing symbol of state YYSTATE.  */
static inline yysymbol_kind_t
yy_accessing_symbol (yy_state_t yystate)
{
  return YY_CAST (yysymbol_kind_t, yystos[yystate]);
}

#if 1
/* The user-facing name of the symbol whose (internal) number is
   YYSYMBOL.  No bounds checking.  */
static const char *yysymbol_name (yysymbol_kind_t yysymbol) YY_ATTRIBUTE_UNUSED;

/* YYTNAME[SYMBOL-NUM] -- String name of the symbol SYMBOL-NUM.
   First, the terminals, then, starting at YYNTOKENS, nonterminals.  */
static const char *const yytname[] =
{
  "\"end of file\"", "error", "\"invalid token\"", "\"fn\"", "\"loc\"",
  "\"for\"", "\"while\"", "\"do\"", "\"rw\"", "\"val\"", "\"in\"",
  "\"if\"", "\"is\"", "\"elif\"", "\"else\"", "\"break\"", "\"skip\"",
  "\"by\"", "\"ret\"", "\"true\"", "\"false\"", "\"nil\"", "\"typeof\"",
  "\"try\"", "\"catch\"", "\"throw\"", "\"yield\"", "\"each\"",
  "\"array\"", "\"self\"", "\"error\"", "\"retry\"", "\"with\"", "\"NaN\"",
  "\"Infinity\"", "\"Nil\"", "\"Number\"", "\"String\"", "\"Boolean\"",
  "\"Table\"", "\"Array\"", "\"Fiber\"", "\"Function\"", "\"Error\"",
  "\"Type\"", "\"NativeFunction\"", "\"NativeObject\"", "\"+=\"", "\"-=\"",
  "\"*=\"", "\"/=\"", "\"%=\"", "\"&=\"", "\"|=\"", "\"^=\"", "\">>=\"",
  "\"<<=\"", "\"?""?=\"", "\"<==>\"", "\"<!=>\"", "\"==\"", "\"!=\"",
  "\"<=\"", "\">=\"", "\"<<\"", "\">>\"", "\"&&\"", "\"||\"", "\"++\"",
  "\"--\"", "\"...\"", "\"=>\"", "\"->\"", "\"?.\"", "\"?[\"", "\"??\"",
  "\"::\"", "\"!in\"", "\"!is\"", "\"#[\"", "\"identifier\"", "\"number\"",
  "\"hex_integer\"", "\"plain_string\"", "\"interpolated_string\"",
  "LOWER_THAN_ELSE", "LOWER_THAN_CATCH", "'='", "'?'", "':'", "'|'", "'^'",
  "'&'", "'<'", "'>'", "'+'", "'-'", "'*'", "'/'", "'%'", "UNARY",
  "PREFIX", "POSTFIX", "'.'", "'['", "'('", "';'", "'{'", "'}'", "')'",
  "','", "']'", "'!'", "'~'", "'@'", "'#'", "'$'", "$accept", "program",
  "statement_list_opt", "statement_list", "statement",
  "expression_statement", "block_statement", "retry_statement",
  "throw_statement", "ret_statement", "skip_statement", "break_statement",
  "while_statement", "do_while_statement", "val_statement",
  "val_declarator_seq", "val_declarator", "each_statement",
  "for_statement", "for_init_list_opt", "for_init_list", "for_init_item",
  "for_step_list_opt", "for_step_list", "if_statement", "elif_list",
  "else_opt", "if_branch", "try_statement", "catch_clause_opt",
  "fn_statement", "attribute_list_seq", "attribute_list", "attribute_seq",
  "attribute_item", "attribute_arg_list", "attribute_arg", "fn_modifier",
  "fn_target", "fn_name_chain", "parameter_list_opt", "parameter_list",
  "param_seq", "param_item", "fn_body", "assignment_expression",
  "fn_arrow_expression", "conditional_expression", "coalescing_expression",
  "coalescing_rhs", "logical_or_expression", "logical_and_expression",
  "inclusive_or_expression", "exclusive_or_expression", "and_expression",
  "equality_expression", "relational_expression", "is_native_opt",
  "type_code", "shift_expression", "additive_expression",
  "multiplicative_expression", "pattern_expression", "by_expression",
  "by_arm_seq", "by_arm", "by_value", "with_expression",
  "prefix_expression", "unary_expression", "postfix_expression",
  "argument_list_opt", "argument_list", "arg_seq", "primary_expression",
  "table_expression", "table_entry_list_opt", "table_entry_seq",
  "table_entry", "self_fn_expression", "array_expression",
  "array_element_list_opt", "array_element_seq", "error_expression",
  "typeof_expression", "yield_expression", "fn_block_expression",
  "constant_expression", "string_literal", "comma_opt", YY_NULLPTR
};

static const char *
yysymbol_name (yysymbol_kind_t yysymbol)
{
  return yytname[yysymbol];
}
#endif

/** Left-hand-side symbol for rule #YYRULE.  */
static inline yysymbol_kind_t
yylhsNonterm (yyRuleNum yyrule)
{
  return YY_CAST (yysymbol_kind_t, yyr1[yyrule]);
}

#if YYDEBUG

# ifndef YYFPRINTF
#  define YYFPRINTF fprintf
# endif

# define YY_FPRINTF                             \
  YY_IGNORE_USELESS_CAST_BEGIN YY_FPRINTF_

# define YY_FPRINTF_(Args)                      \
  do {                                          \
    YYFPRINTF Args;                             \
    YY_IGNORE_USELESS_CAST_END                  \
  } while (0)

# define YY_DPRINTF                             \
  YY_IGNORE_USELESS_CAST_BEGIN YY_DPRINTF_

# define YY_DPRINTF_(Args)                      \
  do {                                          \
    if (yydebug)                                \
      YYFPRINTF Args;                           \
    YY_IGNORE_USELESS_CAST_END                  \
  } while (0)


/* YYLOCATION_PRINT -- Print the location on the stream.
   This macro was not mandated originally: define only if we know
   we won't break user code: when these are the locations we know.  */

# ifndef YYLOCATION_PRINT

#  if defined YY_LOCATION_PRINT

   /* Temporary convenience wrapper in case some people defined the
      undocumented and private YY_LOCATION_PRINT macros.  */
#   define YYLOCATION_PRINT(File, Loc)  YY_LOCATION_PRINT(File, *(Loc))

#  elif defined YYLTYPE_IS_TRIVIAL && YYLTYPE_IS_TRIVIAL

/* Print *YYLOCP on YYO.  Private, do not rely on its existence. */

YY_ATTRIBUTE_UNUSED
static int
yy_location_print_ (FILE *yyo, YYLTYPE const * const yylocp)
{
  int res = 0;
  int end_col = 0 != yylocp->last_column ? yylocp->last_column - 1 : 0;
  if (0 <= yylocp->first_line)
    {
      res += YYFPRINTF (yyo, "%d", yylocp->first_line);
      if (0 <= yylocp->first_column)
        res += YYFPRINTF (yyo, ".%d", yylocp->first_column);
    }
  if (0 <= yylocp->last_line)
    {
      if (yylocp->first_line < yylocp->last_line)
        {
          res += YYFPRINTF (yyo, "-%d", yylocp->last_line);
          if (0 <= end_col)
            res += YYFPRINTF (yyo, ".%d", end_col);
        }
      else if (0 <= end_col && yylocp->first_column < end_col)
        res += YYFPRINTF (yyo, "-%d", end_col);
    }
  return res;
}

#   define YYLOCATION_PRINT  yy_location_print_

    /* Temporary convenience wrapper in case some people defined the
       undocumented and private YY_LOCATION_PRINT macros.  */
#   define YY_LOCATION_PRINT(File, Loc)  YYLOCATION_PRINT(File, &(Loc))

#  else

#   define YYLOCATION_PRINT(File, Loc) ((void) 0)
    /* Temporary convenience wrapper in case some people defined the
       undocumented and private YY_LOCATION_PRINT macros.  */
#   define YY_LOCATION_PRINT  YYLOCATION_PRINT

#  endif
# endif /* !defined YYLOCATION_PRINT */



/*-----------------------------------.
| Print this symbol's value on YYO.  |
`-----------------------------------*/

static void
yy_symbol_value_print (FILE *yyo,
                       yysymbol_kind_t yykind, YYSTYPE const * const yyvaluep, YYLTYPE const * const yylocationp)
{
  FILE *yyoutput = yyo;
  YY_USE (yyoutput);
  YY_USE (yylocationp);
  if (!yyvaluep)
    return;
  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  YY_USE (yykind);
  YY_IGNORE_MAYBE_UNINITIALIZED_END
}


/*---------------------------.
| Print this symbol on YYO.  |
`---------------------------*/

static void
yy_symbol_print (FILE *yyo,
                 yysymbol_kind_t yykind, YYSTYPE const * const yyvaluep, YYLTYPE const * const yylocationp)
{
  YYFPRINTF (yyo, "%s %s (",
             yykind < YYNTOKENS ? "token" : "nterm", yysymbol_name (yykind));

  YYLOCATION_PRINT (yyo, yylocationp);
  YYFPRINTF (yyo, ": ");
  yy_symbol_value_print (yyo, yykind, yyvaluep, yylocationp);
  YYFPRINTF (yyo, ")");
}

# define YY_SYMBOL_PRINT(Title, Kind, Value, Location)                  \
  do {                                                                  \
    if (yydebug)                                                        \
      {                                                                 \
        YY_FPRINTF ((stderr, "%s ", Title));                            \
        yy_symbol_print (stderr, Kind, Value, Location);        \
        YY_FPRINTF ((stderr, "\n"));                                    \
      }                                                                 \
  } while (0)

static inline void
yy_reduce_print (yybool yynormal, yyGLRStackItem* yyvsp, YYPTRDIFF_T yyk,
                 yyRuleNum yyrule);

# define YY_REDUCE_PRINT(Args)          \
  do {                                  \
    if (yydebug)                        \
      yy_reduce_print Args;             \
  } while (0)

/* Nonzero means print parse trace.  It is left uninitialized so that
   multiple parsers can coexist.  */
int yydebug;

static void yypstack (yyGLRStack* yystackp, YYPTRDIFF_T yyk)
  YY_ATTRIBUTE_UNUSED;
static void yypdumpstack (yyGLRStack* yystackp)
  YY_ATTRIBUTE_UNUSED;

#else /* !YYDEBUG */

# define YY_DPRINTF(Args) do {} while (yyfalse)
# define YY_SYMBOL_PRINT(Title, Kind, Value, Location)
# define YY_REDUCE_PRINT(Args)

#endif /* !YYDEBUG */

#ifndef yystrlen
# define yystrlen(S) (YY_CAST (YYPTRDIFF_T, strlen (S)))
#endif

#ifndef yystpcpy
# if defined __GLIBC__ && defined _STRING_H && defined _GNU_SOURCE
#  define yystpcpy stpcpy
# else
/* Copy YYSRC to YYDEST, returning the address of the terminating '\0' in
   YYDEST.  */
static char *
yystpcpy (char *yydest, const char *yysrc)
{
  char *yyd = yydest;
  const char *yys = yysrc;

  while ((*yyd++ = *yys++) != '\0')
    continue;

  return yyd - 1;
}
# endif
#endif

#ifndef yytnamerr
/* Copy to YYRES the contents of YYSTR after stripping away unnecessary
   quotes and backslashes, so that it's suitable for yyerror.  The
   heuristic is that double-quoting is unnecessary unless the string
   contains an apostrophe, a comma, or backslash (other than
   backslash-backslash).  YYSTR is taken from yytname.  If YYRES is
   null, do not copy; instead, return the length of what the result
   would have been.  */
static YYPTRDIFF_T
yytnamerr (char *yyres, const char *yystr)
{
  if (*yystr == '"')
    {
      YYPTRDIFF_T yyn = 0;
      char const *yyp = yystr;

      for (;;)
        switch (*++yyp)
          {
          case '\'':
          case ',':
            goto do_not_strip_quotes;

          case '\\':
            if (*++yyp != '\\')
              goto do_not_strip_quotes;
            else
              goto append;

          append:
          default:
            if (yyres)
              yyres[yyn] = *yyp;
            yyn++;
            break;

          case '"':
            if (yyres)
              yyres[yyn] = '\0';
            return yyn;
          }
    do_not_strip_quotes: ;
    }

  if (yyres)
    return yystpcpy (yyres, yystr) - yyres;
  else
    return yystrlen (yystr);
}
#endif


/** Fill in YYVSP[YYLOW1 .. YYLOW0-1] from the chain of states starting
 *  at YYVSP[YYLOW0].yystate.yypred.  Leaves YYVSP[YYLOW1].yystate.yypred
 *  containing the pointer to the next state in the chain.  */
static void yyfillin (yyGLRStackItem *, int, int) YY_ATTRIBUTE_UNUSED;
static void
yyfillin (yyGLRStackItem *yyvsp, int yylow0, int yylow1)
{
  int i;
  yyGLRState *s = yyvsp[yylow0].yystate.yypred;
  for (i = yylow0-1; i >= yylow1; i -= 1)
    {
#if YYDEBUG
      yyvsp[i].yystate.yylrState = s->yylrState;
#endif
      yyvsp[i].yystate.yyresolved = s->yyresolved;
      if (s->yyresolved)
        yyvsp[i].yystate.yysemantics.yyval = s->yysemantics.yyval;
      else
        /* The effect of using yyval or yyloc (in an immediate rule) is
         * undefined.  */
        yyvsp[i].yystate.yysemantics.yyfirstVal = YY_NULLPTR;
      yyvsp[i].yystate.yyloc = s->yyloc;
      s = yyvsp[i].yystate.yypred = s->yypred;
    }
}


/** If yychar is empty, fetch the next token.  */
static inline yysymbol_kind_t
yygetToken (int *yycharp)
{
  yysymbol_kind_t yytoken;
  if (*yycharp == YYEMPTY)
    {
      YY_DPRINTF ((stderr, "Reading a token\n"));
      *yycharp = yylex ();
    }
  if (*yycharp <= YYEOF)
    {
      *yycharp = YYEOF;
      yytoken = YYSYMBOL_YYEOF;
      YY_DPRINTF ((stderr, "Now at end of input.\n"));
    }
  else
    {
      yytoken = YYTRANSLATE (*yycharp);
      YY_SYMBOL_PRINT ("Next token is", yytoken, &yylval, &yylloc);
    }
  return yytoken;
}

/* Do nothing if YYNORMAL or if *YYLOW <= YYLOW1.  Otherwise, fill in
 * YYVSP[YYLOW1 .. *YYLOW-1] as in yyfillin and set *YYLOW = YYLOW1.
 * For convenience, always return YYLOW1.  */
static inline int yyfill (yyGLRStackItem *, int *, int, yybool)
     YY_ATTRIBUTE_UNUSED;
static inline int
yyfill (yyGLRStackItem *yyvsp, int *yylow, int yylow1, yybool yynormal)
{
  if (!yynormal && yylow1 < *yylow)
    {
      yyfillin (yyvsp, *yylow, yylow1);
      *yylow = yylow1;
    }
  return yylow1;
}

/** Perform user action for rule number YYN, with RHS length YYRHSLEN,
 *  and top stack item YYVSP.  YYLVALP points to place to put semantic
 *  value ($$), and yylocp points to place for location information
 *  (@$).  Returns yyok for normal return, yyaccept for YYACCEPT,
 *  yyerr for YYERROR, yyabort for YYABORT, yynomem for YYNOMEM.  */
static YYRESULTTAG
yyuserAction (yyRuleNum yyrule, int yyrhslen, yyGLRStackItem* yyvsp,
              yyGLRStack* yystackp, YYPTRDIFF_T yyk,
              YYSTYPE* yyvalp, YYLTYPE *yylocp)
{
  const yybool yynormal YY_ATTRIBUTE_UNUSED = yystackp->yysplitPoint == YY_NULLPTR;
  int yylow = 1;
  YY_USE (yyvalp);
  YY_USE (yylocp);
  YY_USE (yyk);
  YY_USE (yyrhslen);
# undef yyerrok
# define yyerrok (yystackp->yyerrState = 0)
# undef YYACCEPT
# define YYACCEPT return yyaccept
# undef YYABORT
# define YYABORT return yyabort
# undef YYNOMEM
# define YYNOMEM return yynomem
# undef YYERROR
# define YYERROR return yyerrok, yyerr
# undef YYRECOVERING
# define YYRECOVERING() (yystackp->yyerrState != 0)
# undef yyclearin
# define yyclearin (yychar = YYEMPTY)
# undef YYFILL
# define YYFILL(N) yyfill (yyvsp, &yylow, (N), yynormal)
# undef YYBACKUP
# define YYBACKUP(Token, Value)                                              \
  return yyerror (YY_("syntax error: cannot back up")),     \
         yyerrok, yyerr

  if (yyrhslen == 0)
    *yyvalp = yyval_default;
  else
    *yyvalp = yyvsp[YYFILL (1-yyrhslen)].yystate.yysemantics.yyval;
  /* Default location. */
  YYLLOC_DEFAULT ((*yylocp), (yyvsp - yyrhslen), yyrhslen);
  yystackp->yyerror_range[1].yystate.yyloc = *yylocp;
  /* If yyk == -1, we are running a deferred action on a temporary
     stack.  In that case, YY_REDUCE_PRINT must not play with YYFILL,
     so pretend the stack is "normal". */
  YY_REDUCE_PRINT ((yynormal || yyk == -1, yyvsp, yyk, yyrule));
  switch (yyrule)
    {
  case 2: /* program: statement_list_opt  */
#line 240 "src/frontend/evil.y"
        { g_ast_root = cvil_ast_program((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list), LOC_SPAN((*yylocp))); ((*yyvalp).node) = g_ast_root; }
#line 2399 "src/frontend/evil.tab.c"
    break;

  case 3: /* statement_list_opt: %empty  */
#line 245 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); }
#line 2405 "src/frontend/evil.tab.c"
    break;

  case 4: /* statement_list_opt: statement_list  */
#line 247 "src/frontend/evil.y"
        { ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list); }
#line 2411 "src/frontend/evil.tab.c"
    break;

  case 5: /* statement_list: statement  */
#line 252 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); }
#line 2417 "src/frontend/evil.tab.c"
    break;

  case 6: /* statement_list: statement_list statement  */
#line 254 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list); }
#line 2423 "src/frontend/evil.tab.c"
    break;

  case 7: /* statement: fn_statement  */
#line 258 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2429 "src/frontend/evil.tab.c"
    break;

  case 8: /* statement: if_statement  */
#line 259 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2435 "src/frontend/evil.tab.c"
    break;

  case 9: /* statement: for_statement  */
#line 260 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2441 "src/frontend/evil.tab.c"
    break;

  case 10: /* statement: while_statement  */
#line 261 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2447 "src/frontend/evil.tab.c"
    break;

  case 11: /* statement: do_while_statement  */
#line 262 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2453 "src/frontend/evil.tab.c"
    break;

  case 12: /* statement: each_statement  */
#line 263 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2459 "src/frontend/evil.tab.c"
    break;

  case 13: /* statement: block_statement  */
#line 264 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2465 "src/frontend/evil.tab.c"
    break;

  case 14: /* statement: val_statement  */
#line 265 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2471 "src/frontend/evil.tab.c"
    break;

  case 15: /* statement: try_statement  */
#line 266 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2477 "src/frontend/evil.tab.c"
    break;

  case 16: /* statement: retry_statement  */
#line 267 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2483 "src/frontend/evil.tab.c"
    break;

  case 17: /* statement: throw_statement  */
#line 268 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2489 "src/frontend/evil.tab.c"
    break;

  case 18: /* statement: ret_statement  */
#line 269 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2495 "src/frontend/evil.tab.c"
    break;

  case 19: /* statement: skip_statement  */
#line 270 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2501 "src/frontend/evil.tab.c"
    break;

  case 20: /* statement: break_statement  */
#line 271 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2507 "src/frontend/evil.tab.c"
    break;

  case 21: /* statement: expression_statement  */
#line 272 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2513 "src/frontend/evil.tab.c"
    break;

  case 22: /* expression_statement: assignment_expression ';'  */
#line 277 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_expr_stmt((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 2519 "src/frontend/evil.tab.c"
    break;

  case 23: /* block_statement: '{' statement_list_opt '}'  */
#line 282 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_block((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list), LOC_SPAN((*yylocp))); }
#line 2525 "src/frontend/evil.tab.c"
    break;

  case 24: /* retry_statement: "retry" ';'  */
#line 287 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_RETRY, LOC_SPAN((*yylocp))); }
#line 2531 "src/frontend/evil.tab.c"
    break;

  case 25: /* throw_statement: "throw" assignment_expression ';'  */
#line 292 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_THROW, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.throw_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); }
#line 2537 "src/frontend/evil.tab.c"
    break;

  case 26: /* ret_statement: "ret" ';'  */
#line 297 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_RET, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.ret_stmt.expr = NULL; }
#line 2543 "src/frontend/evil.tab.c"
    break;

  case 27: /* ret_statement: "ret" assignment_expression ';'  */
#line 299 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_RET, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.ret_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); }
#line 2549 "src/frontend/evil.tab.c"
    break;

  case 28: /* skip_statement: "skip" ';'  */
#line 304 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_SKIP, LOC_SPAN((*yylocp))); }
#line 2555 "src/frontend/evil.tab.c"
    break;

  case 29: /* break_statement: "break" ';'  */
#line 309 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_BREAK, LOC_SPAN((*yylocp))); }
#line 2561 "src/frontend/evil.tab.c"
    break;

  case 30: /* while_statement: "while" '(' assignment_expression ')' statement  */
#line 314 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_WHILE, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.while_stmt.condition = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node); ((*yyvalp).node)->as.while_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2567 "src/frontend/evil.tab.c"
    break;

  case 31: /* do_while_statement: "do" statement "while" '(' assignment_expression ')' ';'  */
#line 319 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_DO_WHILE, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.do_while_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.node); ((*yyvalp).node)->as.do_while_stmt.condition = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node); }
#line 2573 "src/frontend/evil.tab.c"
    break;

  case 32: /* val_statement: "val" val_declarator_seq ';'  */
#line 325 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_val_stmt(false, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.val_decl_list).items, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.val_decl_list).count, LOC_SPAN((*yylocp))); }
#line 2579 "src/frontend/evil.tab.c"
    break;

  case 33: /* val_statement: "rw" "val" val_declarator_seq ';'  */
#line 327 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_val_stmt(true, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.val_decl_list).items, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.val_decl_list).count, LOC_SPAN((*yylocp))); }
#line 2585 "src/frontend/evil.tab.c"
    break;

  case 34: /* val_declarator_seq: val_declarator  */
#line 332 "src/frontend/evil.y"
        {
            ((*yyvalp).val_decl_list).items = (cvil_val_decl_t*)malloc(sizeof(cvil_val_decl_t));
            ((*yyvalp).val_decl_list).items[0] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.val_decl);
            ((*yyvalp).val_decl_list).count = 1;
        }
#line 2595 "src/frontend/evil.tab.c"
    break;

  case 35: /* val_declarator_seq: val_declarator_seq ',' val_declarator  */
#line 338 "src/frontend/evil.y"
        {
            ((*yyvalp).val_decl_list).items = (cvil_val_decl_t*)realloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.val_decl_list).items, ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.val_decl_list).count + 1) * sizeof(cvil_val_decl_t));
            ((*yyvalp).val_decl_list).items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.val_decl_list).count] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.val_decl);
            ((*yyvalp).val_decl_list).count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.val_decl_list).count + 1;
        }
#line 2605 "src/frontend/evil.tab.c"
    break;

  case 36: /* val_declarator: "identifier"  */
#line 347 "src/frontend/evil.y"
        { ((*yyvalp).val_decl).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str); ((*yyvalp).val_decl).init_val = NULL; ((*yyvalp).val_decl).span = LOC_SPAN((*yylocp)); }
#line 2611 "src/frontend/evil.tab.c"
    break;

  case 37: /* val_declarator: "identifier" '=' assignment_expression  */
#line 349 "src/frontend/evil.y"
        { ((*yyvalp).val_decl).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.str); ((*yyvalp).val_decl).init_val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); ((*yyvalp).val_decl).span = LOC_SPAN((*yylocp)); }
#line 2617 "src/frontend/evil.tab.c"
    break;

  case 38: /* each_statement: "each" '(' "rw" "val" "identifier" ':' assignment_expression ')' statement  */
#line 355 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_EACH, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.each_stmt.is_rw = true;
            ((*yyvalp).node)->as.each_stmt.key_var = NULL;
            ((*yyvalp).node)->as.each_stmt.val_var = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.str);
            ((*yyvalp).node)->as.each_stmt.collection = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.each_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 2630 "src/frontend/evil.tab.c"
    break;

  case 39: /* each_statement: "each" '(' "rw" "val" "identifier" ',' "identifier" ':' assignment_expression ')' statement  */
#line 364 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_EACH, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.each_stmt.is_rw = true;
            ((*yyvalp).node)->as.each_stmt.key_var = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-6)].yystate.yysemantics.yyval.str);
            ((*yyvalp).node)->as.each_stmt.val_var = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.str);
            ((*yyvalp).node)->as.each_stmt.collection = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.each_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 2643 "src/frontend/evil.tab.c"
    break;

  case 40: /* for_statement: "for" '(' for_init_list_opt ';' assignment_expression ';' for_step_list_opt ')' statement  */
#line 377 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_FOR, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.for_stmt.inits = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-6)].yystate.yysemantics.yyval.list);
            ((*yyvalp).node)->as.for_stmt.condition = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.for_stmt.steps = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list);
            ((*yyvalp).node)->as.for_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 2655 "src/frontend/evil.tab.c"
    break;

  case 41: /* for_init_list_opt: %empty  */
#line 387 "src/frontend/evil.y"
                  { ((*yyvalp).list) = cvil_ast_list_create(); }
#line 2661 "src/frontend/evil.tab.c"
    break;

  case 42: /* for_init_list_opt: for_init_list  */
#line 388 "src/frontend/evil.y"
                    { ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list); }
#line 2667 "src/frontend/evil.tab.c"
    break;

  case 43: /* for_init_list: for_init_item  */
#line 393 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); }
#line 2673 "src/frontend/evil.tab.c"
    break;

  case 44: /* for_init_list: for_init_list ',' for_init_item  */
#line 395 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list); }
#line 2679 "src/frontend/evil.tab.c"
    break;

  case 45: /* for_init_item: "val" val_declarator_seq  */
#line 400 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_val_stmt(false, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.val_decl_list).items, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.val_decl_list).count, LOC_SPAN((*yylocp))); }
#line 2685 "src/frontend/evil.tab.c"
    break;

  case 46: /* for_init_item: "rw" "val" val_declarator_seq  */
#line 402 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_val_stmt(true, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.val_decl_list).items, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.val_decl_list).count, LOC_SPAN((*yylocp))); }
#line 2691 "src/frontend/evil.tab.c"
    break;

  case 47: /* for_init_item: assignment_expression  */
#line 404 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2697 "src/frontend/evil.tab.c"
    break;

  case 48: /* for_step_list_opt: %empty  */
#line 408 "src/frontend/evil.y"
                  { ((*yyvalp).list) = cvil_ast_list_create(); }
#line 2703 "src/frontend/evil.tab.c"
    break;

  case 49: /* for_step_list_opt: for_step_list  */
#line 409 "src/frontend/evil.y"
                    { ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list); }
#line 2709 "src/frontend/evil.tab.c"
    break;

  case 50: /* for_step_list: assignment_expression  */
#line 414 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); }
#line 2715 "src/frontend/evil.tab.c"
    break;

  case 51: /* for_step_list: for_step_list ',' assignment_expression  */
#line 416 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list); }
#line 2721 "src/frontend/evil.tab.c"
    break;

  case 52: /* if_statement: "if" '(' assignment_expression ')' if_branch elif_list else_opt  */
#line 422 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_IF, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.if_stmt.condition = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.if_stmt.if_branch = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.if_stmt.elifs = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.elif_branch_list).items;
            ((*yyvalp).node)->as.if_stmt.elif_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.elif_branch_list).count;
            ((*yyvalp).node)->as.if_stmt.else_branch = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 2734 "src/frontend/evil.tab.c"
    break;

  case 53: /* elif_list: %empty  */
#line 434 "src/frontend/evil.y"
        { ((*yyvalp).elif_branch_list).items = NULL; ((*yyvalp).elif_branch_list).count = 0; }
#line 2740 "src/frontend/evil.tab.c"
    break;

  case 54: /* elif_list: elif_list "elif" '(' assignment_expression ')' if_branch  */
#line 436 "src/frontend/evil.y"
        {
            ((*yyvalp).elif_branch_list).items = (cvil_elif_branch_t*)realloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.elif_branch_list).items, ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.elif_branch_list).count + 1) * sizeof(cvil_elif_branch_t));
            ((*yyvalp).elif_branch_list).items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.elif_branch_list).count].condition = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).elif_branch_list).items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.elif_branch_list).count].branch = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            ((*yyvalp).elif_branch_list).count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.elif_branch_list).count + 1;
        }
#line 2751 "src/frontend/evil.tab.c"
    break;

  case 55: /* else_opt: %empty  */
#line 445 "src/frontend/evil.y"
                                        { ((*yyvalp).node) = NULL; }
#line 2757 "src/frontend/evil.tab.c"
    break;

  case 56: /* else_opt: "else" if_branch  */
#line 446 "src/frontend/evil.y"
                                        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2763 "src/frontend/evil.tab.c"
    break;

  case 57: /* if_branch: statement  */
#line 451 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2769 "src/frontend/evil.tab.c"
    break;

  case 58: /* if_branch: "->" assignment_expression ';'  */
#line 453 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_RET, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.ret_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); }
#line 2775 "src/frontend/evil.tab.c"
    break;

  case 59: /* try_statement: "try" statement catch_clause_opt  */
#line 459 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_TRY, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.try_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.try_stmt.err_var = NULL;
            ((*yyvalp).node)->as.try_stmt.catch_body = NULL;
        }
#line 2786 "src/frontend/evil.tab.c"
    break;

  case 60: /* try_statement: "try" statement "catch" block_statement  */
#line 466 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_TRY, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.try_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.try_stmt.err_var = NULL;
            ((*yyvalp).node)->as.try_stmt.catch_body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 2797 "src/frontend/evil.tab.c"
    break;

  case 61: /* try_statement: "try" statement "catch" '(' "identifier" ')' block_statement  */
#line 473 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_TRY, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.try_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.try_stmt.err_var = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.str);
            ((*yyvalp).node)->as.try_stmt.catch_body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 2808 "src/frontend/evil.tab.c"
    break;

  case 63: /* fn_statement: attribute_list_seq fn_modifier fn_target parameter_list_opt fn_body  */
#line 488 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_FN, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.fn_stmt.is_local = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.fn_mod);
            ((*yyvalp).node)->as.fn_stmt.name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).name;
            ((*yyvalp).node)->as.fn_stmt.target_name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).target_name;
            ((*yyvalp).node)->as.fn_stmt.target_chain = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).chain;
            if ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).target_name) {
                cvil_param_t* p = (cvil_param_t*)calloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count + 1, sizeof(cvil_param_t));
                p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yyloc)) };
                for (uint32_t i = 0; i < (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count; i++) p[i + 1] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).items[i];
                ((*yyvalp).node)->as.fn_stmt.params = p;
                ((*yyvalp).node)->as.fn_stmt.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count + 1;
            } else {
                ((*yyvalp).node)->as.fn_stmt.params = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).items;
                ((*yyvalp).node)->as.fn_stmt.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count;
            }
            ((*yyvalp).node)->as.fn_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.fn_stmt.attrs = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.attr_list).items;
            ((*yyvalp).node)->as.fn_stmt.attr_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.attr_list).count;
            ((*yyvalp).node)->as.fn_stmt.unique_fn_id = cvil_next_fn_id();
        }
#line 2834 "src/frontend/evil.tab.c"
    break;

  case 64: /* fn_statement: fn_modifier fn_target parameter_list_opt fn_body  */
#line 510 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_FN, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.fn_stmt.is_local = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.fn_mod);
            ((*yyvalp).node)->as.fn_stmt.name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).name;
            ((*yyvalp).node)->as.fn_stmt.target_name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).target_name;
            ((*yyvalp).node)->as.fn_stmt.target_chain = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).chain;
            if ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.fn_target_info).target_name) {
                cvil_param_t* p = (cvil_param_t*)calloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count + 1, sizeof(cvil_param_t));
                p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yyloc)) };
                for (uint32_t i = 0; i < (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count; i++) p[i + 1] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).items[i];
                ((*yyvalp).node)->as.fn_stmt.params = p;
                ((*yyvalp).node)->as.fn_stmt.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count + 1;
            } else {
                ((*yyvalp).node)->as.fn_stmt.params = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).items;
                ((*yyvalp).node)->as.fn_stmt.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count;
            }
            ((*yyvalp).node)->as.fn_stmt.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.fn_stmt.attrs = NULL;
            ((*yyvalp).node)->as.fn_stmt.attr_count = 0;
            ((*yyvalp).node)->as.fn_stmt.unique_fn_id = cvil_next_fn_id();
        }
#line 2860 "src/frontend/evil.tab.c"
    break;

  case 65: /* attribute_list_seq: attribute_list  */
#line 535 "src/frontend/evil.y"
        {
            ((*yyvalp).attr_list).items = (cvil_attribute_t*)malloc(sizeof(cvil_attribute_t));
            ((*yyvalp).attr_list).items[0] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.attribute);
            ((*yyvalp).attr_list).count = 1;
        }
#line 2870 "src/frontend/evil.tab.c"
    break;

  case 66: /* attribute_list_seq: attribute_list_seq attribute_list  */
#line 541 "src/frontend/evil.y"
        {
            ((*yyvalp).attr_list).items = (cvil_attribute_t*)realloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.attr_list).items, ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.attr_list).count + 1) * sizeof(cvil_attribute_t));
            ((*yyvalp).attr_list).items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.attr_list).count] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.attribute);
            ((*yyvalp).attr_list).count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.attr_list).count + 1;
        }
#line 2880 "src/frontend/evil.tab.c"
    break;

  case 67: /* attribute_list: "#[" attribute_seq ']'  */
#line 550 "src/frontend/evil.y"
        { ((*yyvalp).attribute) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.attribute); }
#line 2886 "src/frontend/evil.tab.c"
    break;

  case 68: /* attribute_seq: attribute_item  */
#line 554 "src/frontend/evil.y"
                     { ((*yyvalp).attribute) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.attribute); }
#line 2892 "src/frontend/evil.tab.c"
    break;

  case 69: /* attribute_seq: attribute_seq ';' attribute_item  */
#line 555 "src/frontend/evil.y"
                                       { ((*yyvalp).attribute) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.attribute); }
#line 2898 "src/frontend/evil.tab.c"
    break;

  case 70: /* attribute_item: "identifier"  */
#line 560 "src/frontend/evil.y"
        { ((*yyvalp).attribute).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str); ((*yyvalp).attribute).args = NULL; }
#line 2904 "src/frontend/evil.tab.c"
    break;

  case 71: /* attribute_item: "identifier" '(' ')'  */
#line 562 "src/frontend/evil.y"
        { ((*yyvalp).attribute).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.str); ((*yyvalp).attribute).args = NULL; }
#line 2910 "src/frontend/evil.tab.c"
    break;

  case 72: /* attribute_item: "identifier" '(' attribute_arg_list ')'  */
#line 564 "src/frontend/evil.y"
        { ((*yyvalp).attribute).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.str); ((*yyvalp).attribute).args = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list); }
#line 2916 "src/frontend/evil.tab.c"
    break;

  case 73: /* attribute_arg_list: attribute_arg  */
#line 569 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); }
#line 2922 "src/frontend/evil.tab.c"
    break;

  case 74: /* attribute_arg_list: attribute_arg_list ',' attribute_arg  */
#line 571 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list); }
#line 2928 "src/frontend/evil.tab.c"
    break;

  case 75: /* attribute_arg: assignment_expression  */
#line 575 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2934 "src/frontend/evil.tab.c"
    break;

  case 76: /* attribute_arg: "identifier" '=' assignment_expression  */
#line 576 "src/frontend/evil.y"
                                           { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 2940 "src/frontend/evil.tab.c"
    break;

  case 77: /* fn_modifier: "fn"  */
#line 580 "src/frontend/evil.y"
               { ((*yyvalp).fn_mod) = 0; }
#line 2946 "src/frontend/evil.tab.c"
    break;

  case 78: /* fn_modifier: "loc" "fn"  */
#line 581 "src/frontend/evil.y"
                         { ((*yyvalp).fn_mod) = 1; }
#line 2952 "src/frontend/evil.tab.c"
    break;

  case 79: /* fn_target: fn_name_chain  */
#line 586 "src/frontend/evil.y"
        {
            if ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list)->count == 1) {
                ((*yyvalp).fn_target_info).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list)->items[0]->as.ident_expr.name;
                ((*yyvalp).fn_target_info).target_name = NULL;
                ((*yyvalp).fn_target_info).chain = NULL;
            } else {
                ((*yyvalp).fn_target_info).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list)->items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list)->count - 1]->as.ident_expr.name;
                ((*yyvalp).fn_target_info).target_name = NULL;
                ((*yyvalp).fn_target_info).chain = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list);
            }
        }
#line 2968 "src/frontend/evil.tab.c"
    break;

  case 80: /* fn_target: fn_name_chain "::" "identifier"  */
#line 598 "src/frontend/evil.y"
        {
            ((*yyvalp).fn_target_info).target_name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list)->items[0]->as.ident_expr.name;
            ((*yyvalp).fn_target_info).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str);
            ((*yyvalp).fn_target_info).chain = NULL;
        }
#line 2978 "src/frontend/evil.tab.c"
    break;

  case 81: /* fn_target: "self" "::" "identifier"  */
#line 604 "src/frontend/evil.y"
        {
            ((*yyvalp).fn_target_info).target_name = "self";
            ((*yyvalp).fn_target_info).name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str);
            ((*yyvalp).fn_target_info).chain = NULL;
        }
#line 2988 "src/frontend/evil.tab.c"
    break;

  case 82: /* fn_name_chain: "identifier"  */
#line 613 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), cvil_ast_ident((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str), LOC_SPAN((*yylocp)))); }
#line 2994 "src/frontend/evil.tab.c"
    break;

  case 83: /* fn_name_chain: fn_name_chain '.' "identifier"  */
#line 615 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list), cvil_ast_ident((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str), LOC_SPAN((*yylocp)))); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list); }
#line 3000 "src/frontend/evil.tab.c"
    break;

  case 84: /* parameter_list_opt: %empty  */
#line 619 "src/frontend/evil.y"
                  { ((*yyvalp).param_list).items = NULL; ((*yyvalp).param_list).count = 0; }
#line 3006 "src/frontend/evil.tab.c"
    break;

  case 85: /* parameter_list_opt: '(' parameter_list ')'  */
#line 620 "src/frontend/evil.y"
                             { ((*yyvalp).param_list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list); }
#line 3012 "src/frontend/evil.tab.c"
    break;

  case 86: /* parameter_list: %empty  */
#line 625 "src/frontend/evil.y"
        { ((*yyvalp).param_list).items = NULL; ((*yyvalp).param_list).count = 0; }
#line 3018 "src/frontend/evil.tab.c"
    break;

  case 87: /* parameter_list: "self"  */
#line 627 "src/frontend/evil.y"
        {
            ((*yyvalp).param_list).items = (cvil_param_t*)malloc(sizeof(cvil_param_t));
            ((*yyvalp).param_list).items[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN((*yylocp)) };
            ((*yyvalp).param_list).count = 1;
        }
#line 3028 "src/frontend/evil.tab.c"
    break;

  case 88: /* parameter_list: "self" ',' param_seq  */
#line 633 "src/frontend/evil.y"
        {
            ((*yyvalp).param_list).items = (cvil_param_t*)realloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.param_list).items, ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.param_list).count + 1) * sizeof(cvil_param_t));
            memmove(&((*yyvalp).param_list).items[1], &((*yyvalp).param_list).items[0], (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.param_list).count * sizeof(cvil_param_t));
            ((*yyvalp).param_list).items[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yyloc)) };
            ((*yyvalp).param_list).count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.param_list).count + 1;
        }
#line 3039 "src/frontend/evil.tab.c"
    break;

  case 89: /* parameter_list: param_seq  */
#line 640 "src/frontend/evil.y"
        { ((*yyvalp).param_list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.param_list); }
#line 3045 "src/frontend/evil.tab.c"
    break;

  case 90: /* param_seq: param_item  */
#line 645 "src/frontend/evil.y"
        {
            ((*yyvalp).param_list).items = (cvil_param_t*)malloc(sizeof(cvil_param_t));
            ((*yyvalp).param_list).items[0] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.param);
            ((*yyvalp).param_list).count = 1;
        }
#line 3055 "src/frontend/evil.tab.c"
    break;

  case 91: /* param_seq: param_seq ',' param_item  */
#line 651 "src/frontend/evil.y"
        {
            ((*yyvalp).param_list).items = (cvil_param_t*)realloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).items, ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).count + 1) * sizeof(cvil_param_t));
            ((*yyvalp).param_list).items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).count] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.param);
            ((*yyvalp).param_list).count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).count + 1;
        }
#line 3065 "src/frontend/evil.tab.c"
    break;

  case 92: /* param_item: "identifier"  */
#line 660 "src/frontend/evil.y"
        { ((*yyvalp).param) = (cvil_param_t){ .name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str), .default_val = NULL, .is_rw = false, .is_self = false, .span = LOC_SPAN((*yylocp)) }; }
#line 3071 "src/frontend/evil.tab.c"
    break;

  case 93: /* param_item: "identifier" '=' assignment_expression  */
#line 662 "src/frontend/evil.y"
        { ((*yyvalp).param) = (cvil_param_t){ .name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.str), .default_val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_rw = false, .is_self = false, .span = LOC_SPAN((*yylocp)) }; }
#line 3077 "src/frontend/evil.tab.c"
    break;

  case 94: /* param_item: "rw" "identifier"  */
#line 664 "src/frontend/evil.y"
        { ((*yyvalp).param) = (cvil_param_t){ .name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str), .default_val = NULL, .is_rw = true, .is_self = false, .span = LOC_SPAN((*yylocp)) }; }
#line 3083 "src/frontend/evil.tab.c"
    break;

  case 95: /* param_item: "rw" "identifier" '=' assignment_expression  */
#line 666 "src/frontend/evil.y"
        { ((*yyvalp).param) = (cvil_param_t){ .name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.str), .default_val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_rw = true, .is_self = false, .span = LOC_SPAN((*yylocp)) }; }
#line 3089 "src/frontend/evil.tab.c"
    break;

  case 96: /* fn_body: block_statement  */
#line 671 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3095 "src/frontend/evil.tab.c"
    break;

  case 97: /* fn_body: "->" assignment_expression ';'  */
#line 673 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_STMT_RET, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.ret_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); }
#line 3101 "src/frontend/evil.tab.c"
    break;

  case 98: /* assignment_expression: conditional_expression  */
#line 682 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3107 "src/frontend/evil.tab.c"
    break;

  case 99: /* assignment_expression: unary_expression '=' assignment_expression  */
#line 684 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_EQ, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3113 "src/frontend/evil.tab.c"
    break;

  case 100: /* assignment_expression: unary_expression "+=" assignment_expression  */
#line 686 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_ADD, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3119 "src/frontend/evil.tab.c"
    break;

  case 101: /* assignment_expression: unary_expression "-=" assignment_expression  */
#line 688 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_SUB, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3125 "src/frontend/evil.tab.c"
    break;

  case 102: /* assignment_expression: unary_expression "*=" assignment_expression  */
#line 690 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_MUL, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3131 "src/frontend/evil.tab.c"
    break;

  case 103: /* assignment_expression: unary_expression "/=" assignment_expression  */
#line 692 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_DIV, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3137 "src/frontend/evil.tab.c"
    break;

  case 104: /* assignment_expression: unary_expression "%=" assignment_expression  */
#line 694 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_MOD, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3143 "src/frontend/evil.tab.c"
    break;

  case 105: /* assignment_expression: unary_expression "&=" assignment_expression  */
#line 696 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_BAND, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3149 "src/frontend/evil.tab.c"
    break;

  case 106: /* assignment_expression: unary_expression "|=" assignment_expression  */
#line 698 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_BOR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3155 "src/frontend/evil.tab.c"
    break;

  case 107: /* assignment_expression: unary_expression "^=" assignment_expression  */
#line 700 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_BXOR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3161 "src/frontend/evil.tab.c"
    break;

  case 108: /* assignment_expression: unary_expression "<<=" assignment_expression  */
#line 702 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_SHL, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3167 "src/frontend/evil.tab.c"
    break;

  case 109: /* assignment_expression: unary_expression ">>=" assignment_expression  */
#line 704 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_SHR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3173 "src/frontend/evil.tab.c"
    break;

  case 110: /* assignment_expression: unary_expression "??=" assignment_expression  */
#line 706 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_assign(ASSIGN_COALESCE, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3179 "src/frontend/evil.tab.c"
    break;

  case 111: /* assignment_expression: fn_arrow_expression  */
#line 708 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3185 "src/frontend/evil.tab.c"
    break;

  case 112: /* fn_arrow_expression: "fn" parameter_list_opt "->" assignment_expression  */
#line 713 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_FN, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.fn_expr.params = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).items;
            ((*yyvalp).node)->as.fn_expr.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).count;
            cvil_ast_node_t* ret = cvil_ast_new(AST_STMT_RET, LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yyloc)));
            ret->as.ret_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            cvil_ast_list_t* stmts = cvil_ast_list_create();
            cvil_ast_list_append(stmts, ret);
            ((*yyvalp).node)->as.fn_expr.body = cvil_ast_block(stmts, LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yyloc)));
            ((*yyvalp).node)->as.fn_expr.is_self_fn = false;
            ((*yyvalp).node)->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
#line 3202 "src/frontend/evil.tab.c"
    break;

  case 113: /* conditional_expression: coalescing_expression  */
#line 729 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3208 "src/frontend/evil.tab.c"
    break;

  case 114: /* conditional_expression: coalescing_expression '?' assignment_expression ':' conditional_expression  */
#line 731 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_CONDITIONAL, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.conditional_expr.cond = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.conditional_expr.true_expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.conditional_expr.false_expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 3219 "src/frontend/evil.tab.c"
    break;

  case 115: /* coalescing_expression: logical_or_expression  */
#line 741 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3225 "src/frontend/evil.tab.c"
    break;

  case 116: /* coalescing_expression: logical_or_expression "??" coalescing_rhs  */
#line 743 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_COALESCING, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.coalescing_expr.left = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.coalescing_expr.right = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.coalescing_expr.is_throw = ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)->kind == AST_STMT_THROW);
        }
#line 3236 "src/frontend/evil.tab.c"
    break;

  case 117: /* coalescing_rhs: assignment_expression  */
#line 752 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3242 "src/frontend/evil.tab.c"
    break;

  case 118: /* coalescing_rhs: "throw" assignment_expression  */
#line 754 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_THROW, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.throw_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 3251 "src/frontend/evil.tab.c"
    break;

  case 119: /* logical_or_expression: logical_and_expression  */
#line 761 "src/frontend/evil.y"
                             { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3257 "src/frontend/evil.tab.c"
    break;

  case 120: /* logical_or_expression: logical_or_expression "||" logical_and_expression  */
#line 763 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_OR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3263 "src/frontend/evil.tab.c"
    break;

  case 121: /* logical_and_expression: inclusive_or_expression  */
#line 767 "src/frontend/evil.y"
                              { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3269 "src/frontend/evil.tab.c"
    break;

  case 122: /* logical_and_expression: logical_and_expression "&&" inclusive_or_expression  */
#line 769 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_AND, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3275 "src/frontend/evil.tab.c"
    break;

  case 123: /* inclusive_or_expression: exclusive_or_expression  */
#line 773 "src/frontend/evil.y"
                              { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3281 "src/frontend/evil.tab.c"
    break;

  case 124: /* inclusive_or_expression: inclusive_or_expression '|' exclusive_or_expression  */
#line 775 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_BOR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3287 "src/frontend/evil.tab.c"
    break;

  case 125: /* exclusive_or_expression: and_expression  */
#line 779 "src/frontend/evil.y"
                     { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3293 "src/frontend/evil.tab.c"
    break;

  case 126: /* exclusive_or_expression: exclusive_or_expression '^' and_expression  */
#line 781 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_BXOR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3299 "src/frontend/evil.tab.c"
    break;

  case 127: /* and_expression: equality_expression  */
#line 785 "src/frontend/evil.y"
                          { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3305 "src/frontend/evil.tab.c"
    break;

  case 128: /* and_expression: and_expression '&' equality_expression  */
#line 787 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_BAND, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3311 "src/frontend/evil.tab.c"
    break;

  case 129: /* equality_expression: relational_expression  */
#line 791 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3317 "src/frontend/evil.tab.c"
    break;

  case 130: /* equality_expression: equality_expression "==" relational_expression  */
#line 793 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_EQ, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3323 "src/frontend/evil.tab.c"
    break;

  case 131: /* equality_expression: equality_expression "!=" relational_expression  */
#line 795 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_NE, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3329 "src/frontend/evil.tab.c"
    break;

  case 132: /* equality_expression: equality_expression "<==>" relational_expression  */
#line 797 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_DEQ, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3335 "src/frontend/evil.tab.c"
    break;

  case 133: /* equality_expression: equality_expression "<!=>" relational_expression  */
#line 799 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_DNE, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3341 "src/frontend/evil.tab.c"
    break;

  case 134: /* relational_expression: shift_expression  */
#line 803 "src/frontend/evil.y"
                       { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3347 "src/frontend/evil.tab.c"
    break;

  case 135: /* relational_expression: relational_expression '<' shift_expression  */
#line 805 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_LT, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3353 "src/frontend/evil.tab.c"
    break;

  case 136: /* relational_expression: relational_expression '>' shift_expression  */
#line 807 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_GT, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3359 "src/frontend/evil.tab.c"
    break;

  case 137: /* relational_expression: relational_expression "<=" shift_expression  */
#line 809 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_LTE, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3365 "src/frontend/evil.tab.c"
    break;

  case 138: /* relational_expression: relational_expression ">=" shift_expression  */
#line 811 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_GTE, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3371 "src/frontend/evil.tab.c"
    break;

  case 139: /* relational_expression: relational_expression "in" shift_expression  */
#line 813 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_IN, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3377 "src/frontend/evil.tab.c"
    break;

  case 140: /* relational_expression: relational_expression "!in" shift_expression  */
#line 815 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_NOT_IN, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3383 "src/frontend/evil.tab.c"
    break;

  case 141: /* relational_expression: relational_expression "is" type_code is_native_opt  */
#line 817 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_binary(BIN_IS, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.binary_expr.is_type_target = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.type_code);
        }
#line 3392 "src/frontend/evil.tab.c"
    break;

  case 142: /* relational_expression: relational_expression "!is" type_code is_native_opt  */
#line 822 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_binary(BIN_IS_NOT, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.binary_expr.is_type_target = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.type_code);
        }
#line 3401 "src/frontend/evil.tab.c"
    break;

  case 143: /* is_native_opt: %empty  */
#line 829 "src/frontend/evil.y"
                  { ((*yyvalp).node) = NULL; }
#line 3407 "src/frontend/evil.tab.c"
    break;

  case 144: /* is_native_opt: string_literal  */
#line 831 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3413 "src/frontend/evil.tab.c"
    break;

  case 145: /* type_code: "Nil"  */
#line 835 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_NIL; }
#line 3419 "src/frontend/evil.tab.c"
    break;

  case 146: /* type_code: "Number"  */
#line 836 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_NUMBER; }
#line 3425 "src/frontend/evil.tab.c"
    break;

  case 147: /* type_code: "String"  */
#line 837 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_STRING; }
#line 3431 "src/frontend/evil.tab.c"
    break;

  case 148: /* type_code: "Boolean"  */
#line 838 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_BOOLEAN; }
#line 3437 "src/frontend/evil.tab.c"
    break;

  case 149: /* type_code: "Table"  */
#line 839 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_TABLE; }
#line 3443 "src/frontend/evil.tab.c"
    break;

  case 150: /* type_code: "Array"  */
#line 840 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_ARRAY; }
#line 3449 "src/frontend/evil.tab.c"
    break;

  case 151: /* type_code: "Fiber"  */
#line 841 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_FIBER; }
#line 3455 "src/frontend/evil.tab.c"
    break;

  case 152: /* type_code: "Function"  */
#line 842 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_FUNCTION; }
#line 3461 "src/frontend/evil.tab.c"
    break;

  case 153: /* type_code: "Error"  */
#line 843 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_ERROR; }
#line 3467 "src/frontend/evil.tab.c"
    break;

  case 154: /* type_code: "Type"  */
#line 844 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_TYPE; }
#line 3473 "src/frontend/evil.tab.c"
    break;

  case 155: /* type_code: "NativeFunction"  */
#line 845 "src/frontend/evil.y"
                           { ((*yyvalp).type_code) = EVIL_TYPE_NATIVE_FUNCTION; }
#line 3479 "src/frontend/evil.tab.c"
    break;

  case 156: /* type_code: "NativeObject"  */
#line 846 "src/frontend/evil.y"
                          { ((*yyvalp).type_code) = EVIL_TYPE_NATIVE_OBJECT; }
#line 3485 "src/frontend/evil.tab.c"
    break;

  case 157: /* shift_expression: additive_expression  */
#line 850 "src/frontend/evil.y"
                          { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3491 "src/frontend/evil.tab.c"
    break;

  case 158: /* shift_expression: shift_expression "<<" additive_expression  */
#line 852 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_SHL, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3497 "src/frontend/evil.tab.c"
    break;

  case 159: /* shift_expression: shift_expression ">>" additive_expression  */
#line 854 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_SHR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3503 "src/frontend/evil.tab.c"
    break;

  case 160: /* additive_expression: multiplicative_expression  */
#line 858 "src/frontend/evil.y"
                                { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3509 "src/frontend/evil.tab.c"
    break;

  case 161: /* additive_expression: additive_expression '+' multiplicative_expression  */
#line 860 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_ADD, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3515 "src/frontend/evil.tab.c"
    break;

  case 162: /* additive_expression: additive_expression '-' multiplicative_expression  */
#line 862 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_SUB, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3521 "src/frontend/evil.tab.c"
    break;

  case 163: /* multiplicative_expression: pattern_expression  */
#line 866 "src/frontend/evil.y"
                         { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3527 "src/frontend/evil.tab.c"
    break;

  case 164: /* multiplicative_expression: multiplicative_expression '*' pattern_expression  */
#line 868 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_MUL, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3533 "src/frontend/evil.tab.c"
    break;

  case 165: /* multiplicative_expression: multiplicative_expression '/' pattern_expression  */
#line 870 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_DIV, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3539 "src/frontend/evil.tab.c"
    break;

  case 166: /* multiplicative_expression: multiplicative_expression '%' pattern_expression  */
#line 872 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_binary(BIN_MOD, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3545 "src/frontend/evil.tab.c"
    break;

  case 167: /* pattern_expression: by_expression  */
#line 876 "src/frontend/evil.y"
                      { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3551 "src/frontend/evil.tab.c"
    break;

  case 168: /* pattern_expression: with_expression  */
#line 877 "src/frontend/evil.y"
                      { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3557 "src/frontend/evil.tab.c"
    break;

  case 169: /* by_expression: "by" assignment_expression '{' by_arm_seq comma_opt '}'  */
#line 882 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_BY, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.by_expr.target = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.by_expr.arms = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.by_arm_list).items;
            ((*yyvalp).node)->as.by_expr.arm_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.by_arm_list).count;
        }
#line 3568 "src/frontend/evil.tab.c"
    break;

  case 170: /* by_arm_seq: by_arm  */
#line 892 "src/frontend/evil.y"
        {
            ((*yyvalp).by_arm_list).items = (cvil_by_arm_t*)malloc(sizeof(cvil_by_arm_t));
            ((*yyvalp).by_arm_list).items[0] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.by_arm);
            ((*yyvalp).by_arm_list).count = 1;
        }
#line 3578 "src/frontend/evil.tab.c"
    break;

  case 171: /* by_arm_seq: by_arm_seq ',' by_arm  */
#line 898 "src/frontend/evil.y"
        {
            ((*yyvalp).by_arm_list).items = (cvil_by_arm_t*)realloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.by_arm_list).items, ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.by_arm_list).count + 1) * sizeof(cvil_by_arm_t));
            ((*yyvalp).by_arm_list).items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.by_arm_list).count] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.by_arm);
            ((*yyvalp).by_arm_list).count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.by_arm_list).count + 1;
        }
#line 3588 "src/frontend/evil.tab.c"
    break;

  case 172: /* by_arm: assignment_expression "->" by_value  */
#line 907 "src/frontend/evil.y"
        { ((*yyvalp).by_arm) = (cvil_by_arm_t){ .pattern = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), .body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_deep_match = false, .is_else = false, .is_throw = ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)->kind == AST_STMT_THROW) }; }
#line 3594 "src/frontend/evil.tab.c"
    break;

  case 173: /* by_arm: assignment_expression "=>" by_value  */
#line 909 "src/frontend/evil.y"
        { ((*yyvalp).by_arm) = (cvil_by_arm_t){ .pattern = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), .body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_deep_match = true, .is_else = false, .is_throw = ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)->kind == AST_STMT_THROW) }; }
#line 3600 "src/frontend/evil.tab.c"
    break;

  case 174: /* by_arm: "else" ':' by_value  */
#line 911 "src/frontend/evil.y"
        { ((*yyvalp).by_arm) = (cvil_by_arm_t){ .pattern = NULL, .body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_deep_match = false, .is_else = true, .is_throw = ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)->kind == AST_STMT_THROW) }; }
#line 3606 "src/frontend/evil.tab.c"
    break;

  case 175: /* by_value: assignment_expression  */
#line 915 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3612 "src/frontend/evil.tab.c"
    break;

  case 176: /* by_value: "throw" assignment_expression  */
#line 917 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_STMT_THROW, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.throw_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 3621 "src/frontend/evil.tab.c"
    break;

  case 177: /* with_expression: prefix_expression  */
#line 924 "src/frontend/evil.y"
                        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3627 "src/frontend/evil.tab.c"
    break;

  case 178: /* with_expression: with_expression "with" table_expression  */
#line 926 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_WITH, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.with_expr.base = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.with_expr.ext_table = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
        }
#line 3637 "src/frontend/evil.tab.c"
    break;

  case 179: /* prefix_expression: "++" unary_expression  */
#line 935 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_PRE_INC, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3643 "src/frontend/evil.tab.c"
    break;

  case 180: /* prefix_expression: "--" unary_expression  */
#line 937 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_PRE_DEC, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3649 "src/frontend/evil.tab.c"
    break;

  case 181: /* prefix_expression: unary_expression  */
#line 939 "src/frontend/evil.y"
        { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3655 "src/frontend/evil.tab.c"
    break;

  case 182: /* unary_expression: postfix_expression  */
#line 943 "src/frontend/evil.y"
                         { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3661 "src/frontend/evil.tab.c"
    break;

  case 183: /* unary_expression: '+' unary_expression  */
#line 945 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_PLUS, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3667 "src/frontend/evil.tab.c"
    break;

  case 184: /* unary_expression: '-' unary_expression  */
#line 947 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_MINUS, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3673 "src/frontend/evil.tab.c"
    break;

  case 185: /* unary_expression: '!' unary_expression  */
#line 949 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_NOT, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3679 "src/frontend/evil.tab.c"
    break;

  case 186: /* unary_expression: '~' unary_expression  */
#line 951 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_BNOT, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3685 "src/frontend/evil.tab.c"
    break;

  case 187: /* unary_expression: '@' unary_expression  */
#line 953 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_TOSTR, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3691 "src/frontend/evil.tab.c"
    break;

  case 188: /* unary_expression: '#' unary_expression  */
#line 955 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_LEN, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3697 "src/frontend/evil.tab.c"
    break;

  case 189: /* unary_expression: '$' unary_expression  */
#line 957 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_TONUM, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3703 "src/frontend/evil.tab.c"
    break;

  case 190: /* postfix_expression: primary_expression  */
#line 961 "src/frontend/evil.y"
                         { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3709 "src/frontend/evil.tab.c"
    break;

  case 191: /* postfix_expression: postfix_expression '(' argument_list_opt ')'  */
#line 963 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_invocation((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list), LOC_SPAN((*yylocp))); }
#line 3715 "src/frontend/evil.tab.c"
    break;

  case 192: /* postfix_expression: postfix_expression '[' assignment_expression ']'  */
#line 965 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_INDEXER, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.indexer_expr.target = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.indexer_expr.index = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node);
        }
#line 3725 "src/frontend/evil.tab.c"
    break;

  case 193: /* postfix_expression: postfix_expression '.' "identifier"  */
#line 971 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_PROPERTY, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.prop_expr.target = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.prop_expr.property = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str);
        }
#line 3735 "src/frontend/evil.tab.c"
    break;

  case 194: /* postfix_expression: postfix_expression "?." "identifier"  */
#line 977 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_ELVIS_PROPERTY, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.elvis_expr.target = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.elvis_expr.property = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str);
            ((*yyvalp).node)->as.elvis_expr.is_array = false;
        }
#line 3746 "src/frontend/evil.tab.c"
    break;

  case 195: /* postfix_expression: postfix_expression "?[" assignment_expression ']'  */
#line 984 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_ELVIS_INDEXER, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.elvis_expr.target = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.elvis_expr.index = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.elvis_expr.is_array = true;
        }
#line 3757 "src/frontend/evil.tab.c"
    break;

  case 196: /* postfix_expression: postfix_expression "::" "identifier" '(' argument_list_opt ')'  */
#line 991 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_SELF_INVOCATION, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.self_invoc_expr.receiver = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.self_invoc_expr.method = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.str);
            ((*yyvalp).node)->as.self_invoc_expr.args = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list);
        }
#line 3768 "src/frontend/evil.tab.c"
    break;

  case 197: /* postfix_expression: postfix_expression "++"  */
#line 998 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_POST_INC, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3774 "src/frontend/evil.tab.c"
    break;

  case 198: /* postfix_expression: postfix_expression "--"  */
#line 1000 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_unary(UN_POST_DEC, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node), LOC_SPAN((*yylocp))); }
#line 3780 "src/frontend/evil.tab.c"
    break;

  case 199: /* argument_list_opt: %empty  */
#line 1004 "src/frontend/evil.y"
                  { ((*yyvalp).list) = cvil_ast_list_create(); }
#line 3786 "src/frontend/evil.tab.c"
    break;

  case 200: /* argument_list_opt: argument_list  */
#line 1005 "src/frontend/evil.y"
                    { ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.list); }
#line 3792 "src/frontend/evil.tab.c"
    break;

  case 201: /* argument_list: arg_seq comma_opt  */
#line 1009 "src/frontend/evil.y"
                        { ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list); }
#line 3798 "src/frontend/evil.tab.c"
    break;

  case 202: /* arg_seq: assignment_expression  */
#line 1014 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); }
#line 3804 "src/frontend/evil.tab.c"
    break;

  case 203: /* arg_seq: '*'  */
#line 1016 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), cvil_ast_new(AST_EXPR_VARARG_FORWARD, LOC_SPAN((*yylocp)))); }
#line 3810 "src/frontend/evil.tab.c"
    break;

  case 204: /* arg_seq: arg_seq ',' assignment_expression  */
#line 1018 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list); }
#line 3816 "src/frontend/evil.tab.c"
    break;

  case 205: /* arg_seq: arg_seq ',' '*'  */
#line 1020 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list), cvil_ast_new(AST_EXPR_VARARG_FORWARD, LOC_SPAN((*yylocp)))); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list); }
#line 3822 "src/frontend/evil.tab.c"
    break;

  case 206: /* primary_expression: '(' assignment_expression ')'  */
#line 1024 "src/frontend/evil.y"
                                    { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); }
#line 3828 "src/frontend/evil.tab.c"
    break;

  case 207: /* primary_expression: table_expression  */
#line 1025 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3834 "src/frontend/evil.tab.c"
    break;

  case 208: /* primary_expression: array_expression  */
#line 1026 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3840 "src/frontend/evil.tab.c"
    break;

  case 209: /* primary_expression: error_expression  */
#line 1027 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3846 "src/frontend/evil.tab.c"
    break;

  case 210: /* primary_expression: "..."  */
#line 1028 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = cvil_ast_new(AST_EXPR_ELLIPSIS, LOC_SPAN((*yylocp))); }
#line 3852 "src/frontend/evil.tab.c"
    break;

  case 211: /* primary_expression: fn_block_expression  */
#line 1029 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3858 "src/frontend/evil.tab.c"
    break;

  case 212: /* primary_expression: "identifier"  */
#line 1030 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = cvil_ast_ident((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str), LOC_SPAN((*yylocp))); }
#line 3864 "src/frontend/evil.tab.c"
    break;

  case 213: /* primary_expression: "self"  */
#line 1031 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = cvil_ast_new(AST_EXPR_SELF, LOC_SPAN((*yylocp))); }
#line 3870 "src/frontend/evil.tab.c"
    break;

  case 214: /* primary_expression: typeof_expression  */
#line 1032 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3876 "src/frontend/evil.tab.c"
    break;

  case 215: /* primary_expression: yield_expression  */
#line 1033 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3882 "src/frontend/evil.tab.c"
    break;

  case 216: /* primary_expression: constant_expression  */
#line 1034 "src/frontend/evil.y"
                                   { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 3888 "src/frontend/evil.tab.c"
    break;

  case 217: /* table_expression: '{' table_entry_list_opt '}'  */
#line 1039 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_table((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.table_entry_list).items, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.table_entry_list).count, LOC_SPAN((*yylocp))); }
#line 3894 "src/frontend/evil.tab.c"
    break;

  case 218: /* table_entry_list_opt: %empty  */
#line 1044 "src/frontend/evil.y"
        { ((*yyvalp).table_entry_list).items = NULL; ((*yyvalp).table_entry_list).count = 0; }
#line 3900 "src/frontend/evil.tab.c"
    break;

  case 219: /* table_entry_list_opt: table_entry_seq comma_opt  */
#line 1046 "src/frontend/evil.y"
        { ((*yyvalp).table_entry_list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.table_entry_list); }
#line 3906 "src/frontend/evil.tab.c"
    break;

  case 220: /* table_entry_seq: table_entry  */
#line 1051 "src/frontend/evil.y"
        {
            ((*yyvalp).table_entry_list).items = (cvil_table_entry_node_t*)malloc(sizeof(cvil_table_entry_node_t));
            ((*yyvalp).table_entry_list).items[0] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.table_entry);
            ((*yyvalp).table_entry_list).count = 1;
        }
#line 3916 "src/frontend/evil.tab.c"
    break;

  case 221: /* table_entry_seq: table_entry_seq ',' table_entry  */
#line 1057 "src/frontend/evil.y"
        {
            ((*yyvalp).table_entry_list).items = (cvil_table_entry_node_t*)realloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.table_entry_list).items, ((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.table_entry_list).count + 1) * sizeof(cvil_table_entry_node_t));
            ((*yyvalp).table_entry_list).items[(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.table_entry_list).count] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.table_entry);
            ((*yyvalp).table_entry_list).count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.table_entry_list).count + 1;
        }
#line 3926 "src/frontend/evil.tab.c"
    break;

  case 222: /* table_entry: assignment_expression  */
#line 1066 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = NULL, .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = false, .is_self_fn = false }; }
#line 3932 "src/frontend/evil.tab.c"
    break;

  case 223: /* table_entry: self_fn_expression  */
#line 1068 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = NULL, .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = false, .is_self_fn = true }; }
#line 3938 "src/frontend/evil.tab.c"
    break;

  case 224: /* table_entry: "identifier" ':' assignment_expression  */
#line 1070 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = cvil_ast_literal_str((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.str), false, LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yyloc))), .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = false, .is_self_fn = false }; }
#line 3944 "src/frontend/evil.tab.c"
    break;

  case 225: /* table_entry: "identifier" ':' self_fn_expression  */
#line 1072 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = cvil_ast_literal_str((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.str), false, LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yyloc))), .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = false, .is_self_fn = true }; }
#line 3950 "src/frontend/evil.tab.c"
    break;

  case 226: /* table_entry: '[' assignment_expression ']' "=>" assignment_expression  */
#line 1074 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.node), .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = true, .is_self_fn = false }; }
#line 3956 "src/frontend/evil.tab.c"
    break;

  case 227: /* table_entry: '[' assignment_expression ']' "=>" self_fn_expression  */
#line 1076 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-3)].yystate.yysemantics.yyval.node), .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = true, .is_self_fn = true }; }
#line 3962 "src/frontend/evil.tab.c"
    break;

  case 228: /* table_entry: assignment_expression "=>" assignment_expression  */
#line 1078 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = true, .is_self_fn = false }; }
#line 3968 "src/frontend/evil.tab.c"
    break;

  case 229: /* table_entry: assignment_expression "=>" self_fn_expression  */
#line 1080 "src/frontend/evil.y"
        { ((*yyvalp).table_entry) = (cvil_table_entry_node_t){ .key = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node), .val = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node), .is_associate = true, .is_self_fn = true }; }
#line 3974 "src/frontend/evil.tab.c"
    break;

  case 230: /* self_fn_expression: "self" "::" "fn" parameter_list_opt block_statement  */
#line 1085 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_SELF_FN, LOC_SPAN((*yylocp)));
            cvil_param_t* p = (cvil_param_t*)calloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count + 1, sizeof(cvil_param_t));
            p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yyloc)) };
            for (uint32_t i = 0; i < (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count; i++) p[i + 1] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).items[i];
            ((*yyvalp).node)->as.fn_expr.params = p;
            ((*yyvalp).node)->as.fn_expr.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count + 1;
            ((*yyvalp).node)->as.fn_expr.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.fn_expr.is_self_fn = true;
            ((*yyvalp).node)->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
#line 3990 "src/frontend/evil.tab.c"
    break;

  case 231: /* self_fn_expression: "self" "::" "fn" parameter_list_opt "->" assignment_expression  */
#line 1097 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_SELF_FN, LOC_SPAN((*yylocp)));
            cvil_param_t* p = (cvil_param_t*)calloc((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).count + 1, sizeof(cvil_param_t));
            p[0] = (cvil_param_t){ .name = "self", .default_val = NULL, .is_rw = false, .is_self = true, .span = LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yyloc)) };
            for (uint32_t i = 0; i < (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).count; i++) p[i + 1] = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).items[i];
            ((*yyvalp).node)->as.fn_expr.params = p;
            ((*yyvalp).node)->as.fn_expr.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.param_list).count + 1;
            cvil_ast_node_t* ret = cvil_ast_new(AST_STMT_RET, LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yyloc)));
            ret->as.ret_stmt.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            cvil_ast_list_t* stmts = cvil_ast_list_create();
            cvil_ast_list_append(stmts, ret);
            ((*yyvalp).node)->as.fn_expr.body = cvil_ast_block(stmts, LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yyloc)));
            ((*yyvalp).node)->as.fn_expr.is_self_fn = true;
            ((*yyvalp).node)->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
#line 4010 "src/frontend/evil.tab.c"
    break;

  case 232: /* array_expression: '[' array_element_list_opt ']'  */
#line 1116 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_array(NULL, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list), LOC_SPAN((*yylocp))); }
#line 4016 "src/frontend/evil.tab.c"
    break;

  case 233: /* array_expression: "array" '(' assignment_expression ')'  */
#line 1118 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_array((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node), NULL, LOC_SPAN((*yylocp))); }
#line 4022 "src/frontend/evil.tab.c"
    break;

  case 234: /* array_expression: "array" '(' assignment_expression ')' '{' array_element_list_opt '}'  */
#line 1120 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_array((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.node), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list), LOC_SPAN((*yylocp))); }
#line 4028 "src/frontend/evil.tab.c"
    break;

  case 235: /* array_expression: "array" '(' ')' '{' array_element_list_opt '}'  */
#line 1122 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_array(NULL, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list), LOC_SPAN((*yylocp))); }
#line 4034 "src/frontend/evil.tab.c"
    break;

  case 236: /* array_expression: "array" '{' array_element_list_opt '}'  */
#line 1124 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_array(NULL, (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list), LOC_SPAN((*yylocp))); }
#line 4040 "src/frontend/evil.tab.c"
    break;

  case 237: /* array_element_list_opt: %empty  */
#line 1128 "src/frontend/evil.y"
                  { ((*yyvalp).list) = cvil_ast_list_create(); }
#line 4046 "src/frontend/evil.tab.c"
    break;

  case 238: /* array_element_list_opt: array_element_seq comma_opt  */
#line 1129 "src/frontend/evil.y"
                                  { ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list); }
#line 4052 "src/frontend/evil.tab.c"
    break;

  case 239: /* array_element_seq: assignment_expression  */
#line 1134 "src/frontend/evil.y"
        { ((*yyvalp).list) = cvil_ast_list_create(); cvil_ast_list_append(((*yyvalp).list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); }
#line 4058 "src/frontend/evil.tab.c"
    break;

  case 240: /* array_element_seq: array_element_seq ',' assignment_expression  */
#line 1136 "src/frontend/evil.y"
        { cvil_ast_list_append((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list), (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node)); ((*yyvalp).list) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.list); }
#line 4064 "src/frontend/evil.tab.c"
    break;

  case 241: /* error_expression: "error" '(' string_literal ')'  */
#line 1141 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_EXPR_ERROR, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.error_expr.message = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); ((*yyvalp).node)->as.error_expr.table_data = NULL; }
#line 4070 "src/frontend/evil.tab.c"
    break;

  case 242: /* error_expression: "error" '(' string_literal ')' table_expression  */
#line 1143 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_EXPR_ERROR, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.error_expr.message = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-2)].yystate.yysemantics.yyval.node); ((*yyvalp).node)->as.error_expr.table_data = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 4076 "src/frontend/evil.tab.c"
    break;

  case 243: /* error_expression: "error" table_expression  */
#line 1145 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_EXPR_ERROR, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.error_expr.message = NULL; ((*yyvalp).node)->as.error_expr.table_data = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 4082 "src/frontend/evil.tab.c"
    break;

  case 244: /* typeof_expression: "typeof" '(' assignment_expression ')'  */
#line 1150 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_EXPR_TYPEOF, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.typeof_expr.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); ((*yyvalp).node)->as.typeof_expr.invert = false; }
#line 4088 "src/frontend/evil.tab.c"
    break;

  case 245: /* typeof_expression: "typeof" '!' '(' assignment_expression ')'  */
#line 1152 "src/frontend/evil.y"
        { ((*yyvalp).node) = cvil_ast_new(AST_EXPR_TYPEOF, LOC_SPAN((*yylocp))); ((*yyvalp).node)->as.typeof_expr.expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.node); ((*yyvalp).node)->as.typeof_expr.invert = true; }
#line 4094 "src/frontend/evil.tab.c"
    break;

  case 246: /* yield_expression: "yield" '<' "identifier" '>' '(' argument_list_opt ')'  */
#line 1157 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_YIELD, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.yield_expr.target_name = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.str);
            ((*yyvalp).node)->as.yield_expr.target_expr = cvil_ast_ident((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yysemantics.yyval.str), LOC_SPAN((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-4)].yystate.yyloc)));
            ((*yyvalp).node)->as.yield_expr.args = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list);
        }
#line 4105 "src/frontend/evil.tab.c"
    break;

  case 247: /* yield_expression: "yield" '<' '[' assignment_expression ']' '>' '(' argument_list_opt ')'  */
#line 1164 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_YIELD, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.yield_expr.target_name = NULL;
            ((*yyvalp).node)->as.yield_expr.target_expr = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-5)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.yield_expr.args = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.list);
        }
#line 4116 "src/frontend/evil.tab.c"
    break;

  case 248: /* fn_block_expression: "fn" parameter_list_opt block_statement  */
#line 1174 "src/frontend/evil.y"
        {
            ((*yyvalp).node) = cvil_ast_new(AST_EXPR_FN, LOC_SPAN((*yylocp)));
            ((*yyvalp).node)->as.fn_expr.params = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).items;
            ((*yyvalp).node)->as.fn_expr.param_count = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (-1)].yystate.yysemantics.yyval.param_list).count;
            ((*yyvalp).node)->as.fn_expr.body = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node);
            ((*yyvalp).node)->as.fn_expr.is_self_fn = false;
            ((*yyvalp).node)->as.fn_expr.unique_fn_id = cvil_next_fn_id();
        }
#line 4129 "src/frontend/evil.tab.c"
    break;

  case 249: /* constant_expression: "number"  */
#line 1185 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_num((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.num), LOC_SPAN((*yylocp))); }
#line 4135 "src/frontend/evil.tab.c"
    break;

  case 250: /* constant_expression: "hex_integer"  */
#line 1186 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_hex((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.hex_val), LOC_SPAN((*yylocp))); }
#line 4141 "src/frontend/evil.tab.c"
    break;

  case 251: /* constant_expression: string_literal  */
#line 1187 "src/frontend/evil.y"
                            { ((*yyvalp).node) = (YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.node); }
#line 4147 "src/frontend/evil.tab.c"
    break;

  case 252: /* constant_expression: "true"  */
#line 1188 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_bool(true, LOC_SPAN((*yylocp))); }
#line 4153 "src/frontend/evil.tab.c"
    break;

  case 253: /* constant_expression: "false"  */
#line 1189 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_bool(false, LOC_SPAN((*yylocp))); }
#line 4159 "src/frontend/evil.tab.c"
    break;

  case 254: /* constant_expression: "nil"  */
#line 1190 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_nil(LOC_SPAN((*yylocp))); }
#line 4165 "src/frontend/evil.tab.c"
    break;

  case 255: /* constant_expression: "NaN"  */
#line 1191 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_num(NAN, LOC_SPAN((*yylocp))); }
#line 4171 "src/frontend/evil.tab.c"
    break;

  case 256: /* constant_expression: "Infinity"  */
#line 1192 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_num(INFINITY, LOC_SPAN((*yylocp))); }
#line 4177 "src/frontend/evil.tab.c"
    break;

  case 257: /* constant_expression: type_code  */
#line 1193 "src/frontend/evil.y"
                            { ((*yyvalp).node) = cvil_ast_literal_type((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.type_code), LOC_SPAN((*yylocp))); }
#line 4183 "src/frontend/evil.tab.c"
    break;

  case 258: /* string_literal: "plain_string"  */
#line 1197 "src/frontend/evil.y"
                                { ((*yyvalp).node) = cvil_ast_literal_str((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str), false, LOC_SPAN((*yylocp))); }
#line 4189 "src/frontend/evil.tab.c"
    break;

  case 259: /* string_literal: "interpolated_string"  */
#line 1198 "src/frontend/evil.y"
                                { ((*yyvalp).node) = cvil_ast_literal_str((YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL (0)].yystate.yysemantics.yyval.str), true, LOC_SPAN((*yylocp))); }
#line 4195 "src/frontend/evil.tab.c"
    break;


#line 4199 "src/frontend/evil.tab.c"

      default: break;
    }
  YY_SYMBOL_PRINT ("-> $$ =", yylhsNonterm (yyrule), yyvalp, yylocp);

  return yyok;
# undef yyerrok
# undef YYABORT
# undef YYACCEPT
# undef YYNOMEM
# undef YYERROR
# undef YYBACKUP
# undef yyclearin
# undef YYRECOVERING
}


static void
yyuserMerge (int yyn, YYSTYPE* yy0, YYSTYPE* yy1)
{
  YY_USE (yy0);
  YY_USE (yy1);

  switch (yyn)
    {

      default: break;
    }
}

                              /* Bison grammar-table manipulation.  */

/*-----------------------------------------------.
| Release the memory associated to this symbol.  |
`-----------------------------------------------*/

static void
yydestruct (const char *yymsg,
            yysymbol_kind_t yykind, YYSTYPE *yyvaluep, YYLTYPE *yylocationp)
{
  YY_USE (yyvaluep);
  YY_USE (yylocationp);
  if (!yymsg)
    yymsg = "Deleting";
  YY_SYMBOL_PRINT (yymsg, yykind, yyvaluep, yylocationp);

  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  YY_USE (yykind);
  YY_IGNORE_MAYBE_UNINITIALIZED_END
}

/** Number of symbols composing the right hand side of rule #RULE.  */
static inline int
yyrhsLength (yyRuleNum yyrule)
{
  return yyr2[yyrule];
}

static void
yydestroyGLRState (char const *yymsg, yyGLRState *yys)
{
  if (yys->yyresolved)
    yydestruct (yymsg, yy_accessing_symbol (yys->yylrState),
                &yys->yysemantics.yyval, &yys->yyloc);
  else
    {
#if YYDEBUG
      if (yydebug)
        {
          if (yys->yysemantics.yyfirstVal)
            YY_FPRINTF ((stderr, "%s unresolved", yymsg));
          else
            YY_FPRINTF ((stderr, "%s incomplete", yymsg));
          YY_SYMBOL_PRINT ("", yy_accessing_symbol (yys->yylrState), YY_NULLPTR, &yys->yyloc);
        }
#endif

      if (yys->yysemantics.yyfirstVal)
        {
          yySemanticOption *yyoption = yys->yysemantics.yyfirstVal;
          yyGLRState *yyrh;
          int yyn;
          for (yyrh = yyoption->yystate, yyn = yyrhsLength (yyoption->yyrule);
               yyn > 0;
               yyrh = yyrh->yypred, yyn -= 1)
            yydestroyGLRState (yymsg, yyrh);
        }
    }
}

#define yypact_value_is_default(Yyn) \
  ((Yyn) == YYPACT_NINF)

/** True iff LR state YYSTATE has only a default reduction (regardless
 *  of token).  */
static inline yybool
yyisDefaultedState (yy_state_t yystate)
{
  return yypact_value_is_default (yypact[yystate]);
}

/** The default reduction for YYSTATE, assuming it has one.  */
static inline yyRuleNum
yydefaultAction (yy_state_t yystate)
{
  return yydefact[yystate];
}

#define yytable_value_is_error(Yyn) \
  0

/** The action to take in YYSTATE on seeing YYTOKEN.
 *  Result R means
 *    R < 0:  Reduce on rule -R.
 *    R = 0:  Error.
 *    R > 0:  Shift to state R.
 *  Set *YYCONFLICTS to a pointer into yyconfl to a 0-terminated list
 *  of conflicting reductions.
 */
static inline int
yygetLRActions (yy_state_t yystate, yysymbol_kind_t yytoken, const short** yyconflicts)
{
  int yyindex = yypact[yystate] + yytoken;
  if (yytoken == YYSYMBOL_YYerror)
    {
      // This is the error token.
      *yyconflicts = yyconfl;
      return 0;
    }
  else if (yyisDefaultedState (yystate)
           || yyindex < 0 || YYLAST < yyindex || yycheck[yyindex] != yytoken)
    {
      *yyconflicts = yyconfl;
      return -yydefact[yystate];
    }
  else if (! yytable_value_is_error (yytable[yyindex]))
    {
      *yyconflicts = yyconfl + yyconflp[yyindex];
      return yytable[yyindex];
    }
  else
    {
      *yyconflicts = yyconfl + yyconflp[yyindex];
      return 0;
    }
}

/** Compute post-reduction state.
 * \param yystate   the current state
 * \param yysym     the nonterminal to push on the stack
 */
static inline yy_state_t
yyLRgotoState (yy_state_t yystate, yysymbol_kind_t yysym)
{
  int yyr = yypgoto[yysym - YYNTOKENS] + yystate;
  if (0 <= yyr && yyr <= YYLAST && yycheck[yyr] == yystate)
    return yytable[yyr];
  else
    return yydefgoto[yysym - YYNTOKENS];
}

static inline yybool
yyisShiftAction (int yyaction)
{
  return 0 < yyaction;
}

static inline yybool
yyisErrorAction (int yyaction)
{
  return yyaction == 0;
}

                                /* GLRStates */

/** Return a fresh GLRStackItem in YYSTACKP.  The item is an LR state
 *  if YYISSTATE, and otherwise a semantic option.  Callers should call
 *  YY_RESERVE_GLRSTACK afterwards to make sure there is sufficient
 *  headroom.  */

static inline yyGLRStackItem*
yynewGLRStackItem (yyGLRStack* yystackp, yybool yyisState)
{
  yyGLRStackItem* yynewItem = yystackp->yynextFree;
  yystackp->yyspaceLeft -= 1;
  yystackp->yynextFree += 1;
  yynewItem->yystate.yyisState = yyisState;
  return yynewItem;
}

/** Add a new semantic action that will execute the action for rule
 *  YYRULE on the semantic values in YYRHS to the list of
 *  alternative actions for YYSTATE.  Assumes that YYRHS comes from
 *  stack #YYK of *YYSTACKP. */
static void
yyaddDeferredAction (yyGLRStack* yystackp, YYPTRDIFF_T yyk, yyGLRState* yystate,
                     yyGLRState* yyrhs, yyRuleNum yyrule)
{
  yySemanticOption* yynewOption =
    &yynewGLRStackItem (yystackp, yyfalse)->yyoption;
  YY_ASSERT (!yynewOption->yyisState);
  yynewOption->yystate = yyrhs;
  yynewOption->yyrule = yyrule;
  if (yystackp->yytops.yylookaheadNeeds[yyk])
    {
      yynewOption->yyrawchar = yychar;
      yynewOption->yyval = yylval;
      yynewOption->yyloc = yylloc;
    }
  else
    yynewOption->yyrawchar = YYEMPTY;
  yynewOption->yynext = yystate->yysemantics.yyfirstVal;
  yystate->yysemantics.yyfirstVal = yynewOption;

  YY_RESERVE_GLRSTACK (yystackp);
}

                                /* GLRStacks */

/** Initialize YYSET to a singleton set containing an empty stack.  */
static yybool
yyinitStateSet (yyGLRStateSet* yyset)
{
  yyset->yysize = 1;
  yyset->yycapacity = 16;
  yyset->yystates
    = YY_CAST (yyGLRState**,
               YYMALLOC (YY_CAST (YYSIZE_T, yyset->yycapacity)
                         * sizeof yyset->yystates[0]));
  if (! yyset->yystates)
    return yyfalse;
  yyset->yystates[0] = YY_NULLPTR;
  yyset->yylookaheadNeeds
    = YY_CAST (yybool*,
               YYMALLOC (YY_CAST (YYSIZE_T, yyset->yycapacity)
                         * sizeof yyset->yylookaheadNeeds[0]));
  if (! yyset->yylookaheadNeeds)
    {
      YYFREE (yyset->yystates);
      return yyfalse;
    }
  memset (yyset->yylookaheadNeeds,
          0,
          YY_CAST (YYSIZE_T, yyset->yycapacity) * sizeof yyset->yylookaheadNeeds[0]);
  return yytrue;
}

static void yyfreeStateSet (yyGLRStateSet* yyset)
{
  YYFREE (yyset->yystates);
  YYFREE (yyset->yylookaheadNeeds);
}

/** Initialize *YYSTACKP to a single empty stack, with total maximum
 *  capacity for all stacks of YYSIZE.  */
static yybool
yyinitGLRStack (yyGLRStack* yystackp, YYPTRDIFF_T yysize)
{
  yystackp->yyerrState = 0;
  yynerrs = 0;
  yystackp->yyspaceLeft = yysize;
  yystackp->yyitems
    = YY_CAST (yyGLRStackItem*,
               YYMALLOC (YY_CAST (YYSIZE_T, yysize)
                         * sizeof yystackp->yynextFree[0]));
  if (!yystackp->yyitems)
    return yyfalse;
  yystackp->yynextFree = yystackp->yyitems;
  yystackp->yysplitPoint = YY_NULLPTR;
  yystackp->yylastDeleted = YY_NULLPTR;
  return yyinitStateSet (&yystackp->yytops);
}


#if YYSTACKEXPANDABLE
# define YYRELOC(YYFROMITEMS, YYTOITEMS, YYX, YYTYPE)                   \
  &((YYTOITEMS)                                                         \
    - ((YYFROMITEMS) - YY_REINTERPRET_CAST (yyGLRStackItem*, (YYX))))->YYTYPE

/** If *YYSTACKP is expandable, extend it.  WARNING: Pointers into the
    stack from outside should be considered invalid after this call.
    We always expand when there are 1 or fewer items left AFTER an
    allocation, so that we can avoid having external pointers exist
    across an allocation.  */
static void
yyexpandGLRStack (yyGLRStack* yystackp)
{
  yyGLRStackItem* yynewItems;
  yyGLRStackItem* yyp0, *yyp1;
  YYPTRDIFF_T yynewSize;
  YYPTRDIFF_T yyn;
  YYPTRDIFF_T yysize = yystackp->yynextFree - yystackp->yyitems;
  if (YYMAXDEPTH - YYHEADROOM < yysize)
    yyMemoryExhausted (yystackp);
  yynewSize = 2*yysize;
  if (YYMAXDEPTH < yynewSize)
    yynewSize = YYMAXDEPTH;
  yynewItems
    = YY_CAST (yyGLRStackItem*,
               YYMALLOC (YY_CAST (YYSIZE_T, yynewSize)
                         * sizeof yynewItems[0]));
  if (! yynewItems)
    yyMemoryExhausted (yystackp);
  for (yyp0 = yystackp->yyitems, yyp1 = yynewItems, yyn = yysize;
       0 < yyn;
       yyn -= 1, yyp0 += 1, yyp1 += 1)
    {
      *yyp1 = *yyp0;
      if (*YY_REINTERPRET_CAST (yybool *, yyp0))
        {
          yyGLRState* yys0 = &yyp0->yystate;
          yyGLRState* yys1 = &yyp1->yystate;
          if (yys0->yypred != YY_NULLPTR)
            yys1->yypred =
              YYRELOC (yyp0, yyp1, yys0->yypred, yystate);
          if (! yys0->yyresolved && yys0->yysemantics.yyfirstVal != YY_NULLPTR)
            yys1->yysemantics.yyfirstVal =
              YYRELOC (yyp0, yyp1, yys0->yysemantics.yyfirstVal, yyoption);
        }
      else
        {
          yySemanticOption* yyv0 = &yyp0->yyoption;
          yySemanticOption* yyv1 = &yyp1->yyoption;
          if (yyv0->yystate != YY_NULLPTR)
            yyv1->yystate = YYRELOC (yyp0, yyp1, yyv0->yystate, yystate);
          if (yyv0->yynext != YY_NULLPTR)
            yyv1->yynext = YYRELOC (yyp0, yyp1, yyv0->yynext, yyoption);
        }
    }
  if (yystackp->yysplitPoint != YY_NULLPTR)
    yystackp->yysplitPoint = YYRELOC (yystackp->yyitems, yynewItems,
                                      yystackp->yysplitPoint, yystate);

  for (yyn = 0; yyn < yystackp->yytops.yysize; yyn += 1)
    if (yystackp->yytops.yystates[yyn] != YY_NULLPTR)
      yystackp->yytops.yystates[yyn] =
        YYRELOC (yystackp->yyitems, yynewItems,
                 yystackp->yytops.yystates[yyn], yystate);
  YYFREE (yystackp->yyitems);
  yystackp->yyitems = yynewItems;
  yystackp->yynextFree = yynewItems + yysize;
  yystackp->yyspaceLeft = yynewSize - yysize;
}
#endif

static void
yyfreeGLRStack (yyGLRStack* yystackp)
{
  YYFREE (yystackp->yyitems);
  yyfreeStateSet (&yystackp->yytops);
}

/** Assuming that YYS is a GLRState somewhere on *YYSTACKP, update the
 *  splitpoint of *YYSTACKP, if needed, so that it is at least as deep as
 *  YYS.  */
static inline void
yyupdateSplit (yyGLRStack* yystackp, yyGLRState* yys)
{
  if (yystackp->yysplitPoint != YY_NULLPTR && yystackp->yysplitPoint > yys)
    yystackp->yysplitPoint = yys;
}

/** Invalidate stack #YYK in *YYSTACKP.  */
static inline void
yymarkStackDeleted (yyGLRStack* yystackp, YYPTRDIFF_T yyk)
{
  if (yystackp->yytops.yystates[yyk] != YY_NULLPTR)
    yystackp->yylastDeleted = yystackp->yytops.yystates[yyk];
  yystackp->yytops.yystates[yyk] = YY_NULLPTR;
}

/** Undelete the last stack in *YYSTACKP that was marked as deleted.  Can
    only be done once after a deletion, and only when all other stacks have
    been deleted.  */
static void
yyundeleteLastStack (yyGLRStack* yystackp)
{
  if (yystackp->yylastDeleted == YY_NULLPTR || yystackp->yytops.yysize != 0)
    return;
  yystackp->yytops.yystates[0] = yystackp->yylastDeleted;
  yystackp->yytops.yysize = 1;
  YY_DPRINTF ((stderr, "Restoring last deleted stack as stack #0.\n"));
  yystackp->yylastDeleted = YY_NULLPTR;
}

static inline void
yyremoveDeletes (yyGLRStack* yystackp)
{
  YYPTRDIFF_T yyi, yyj;
  yyi = yyj = 0;
  while (yyj < yystackp->yytops.yysize)
    {
      if (yystackp->yytops.yystates[yyi] == YY_NULLPTR)
        {
          if (yyi == yyj)
            YY_DPRINTF ((stderr, "Removing dead stacks.\n"));
          yystackp->yytops.yysize -= 1;
        }
      else
        {
          yystackp->yytops.yystates[yyj] = yystackp->yytops.yystates[yyi];
          /* In the current implementation, it's unnecessary to copy
             yystackp->yytops.yylookaheadNeeds[yyi] since, after
             yyremoveDeletes returns, the parser immediately either enters
             deterministic operation or shifts a token.  However, it doesn't
             hurt, and the code might evolve to need it.  */
          yystackp->yytops.yylookaheadNeeds[yyj] =
            yystackp->yytops.yylookaheadNeeds[yyi];
          if (yyj != yyi)
            YY_DPRINTF ((stderr, "Rename stack %ld -> %ld.\n",
                        YY_CAST (long, yyi), YY_CAST (long, yyj)));
          yyj += 1;
        }
      yyi += 1;
    }
}

/** Shift to a new state on stack #YYK of *YYSTACKP, corresponding to LR
 * state YYLRSTATE, at input position YYPOSN, with (resolved) semantic
 * value *YYVALP and source location *YYLOCP.  */
static inline void
yyglrShift (yyGLRStack* yystackp, YYPTRDIFF_T yyk, yy_state_t yylrState,
            YYPTRDIFF_T yyposn,
            YYSTYPE* yyvalp, YYLTYPE* yylocp)
{
  yyGLRState* yynewState = &yynewGLRStackItem (yystackp, yytrue)->yystate;

  yynewState->yylrState = yylrState;
  yynewState->yyposn = yyposn;
  yynewState->yyresolved = yytrue;
  yynewState->yypred = yystackp->yytops.yystates[yyk];
  yynewState->yysemantics.yyval = *yyvalp;
  yynewState->yyloc = *yylocp;
  yystackp->yytops.yystates[yyk] = yynewState;

  YY_RESERVE_GLRSTACK (yystackp);
}

/** Shift stack #YYK of *YYSTACKP, to a new state corresponding to LR
 *  state YYLRSTATE, at input position YYPOSN, with the (unresolved)
 *  semantic value of YYRHS under the action for YYRULE.  */
static inline void
yyglrShiftDefer (yyGLRStack* yystackp, YYPTRDIFF_T yyk, yy_state_t yylrState,
                 YYPTRDIFF_T yyposn, yyGLRState* yyrhs, yyRuleNum yyrule)
{
  yyGLRState* yynewState = &yynewGLRStackItem (yystackp, yytrue)->yystate;
  YY_ASSERT (yynewState->yyisState);

  yynewState->yylrState = yylrState;
  yynewState->yyposn = yyposn;
  yynewState->yyresolved = yyfalse;
  yynewState->yypred = yystackp->yytops.yystates[yyk];
  yynewState->yysemantics.yyfirstVal = YY_NULLPTR;
  yystackp->yytops.yystates[yyk] = yynewState;

  /* Invokes YY_RESERVE_GLRSTACK.  */
  yyaddDeferredAction (yystackp, yyk, yynewState, yyrhs, yyrule);
}

#if YYDEBUG

/*----------------------------------------------------------------------.
| Report that stack #YYK of *YYSTACKP is going to be reduced by YYRULE. |
`----------------------------------------------------------------------*/

static inline void
yy_reduce_print (yybool yynormal, yyGLRStackItem* yyvsp, YYPTRDIFF_T yyk,
                 yyRuleNum yyrule)
{
  int yynrhs = yyrhsLength (yyrule);
  int yylow = 1;
  int yyi;
  YY_FPRINTF ((stderr, "Reducing stack %ld by rule %d (line %d):\n",
               YY_CAST (long, yyk), yyrule - 1, yyrline[yyrule]));
  if (! yynormal)
    yyfillin (yyvsp, 1, -yynrhs);
  /* The symbols being reduced.  */
  for (yyi = 0; yyi < yynrhs; yyi++)
    {
      YY_FPRINTF ((stderr, "   $%d = ", yyi + 1));
      yy_symbol_print (stderr,
                       yy_accessing_symbol (yyvsp[yyi - yynrhs + 1].yystate.yylrState),
                       &yyvsp[yyi - yynrhs + 1].yystate.yysemantics.yyval,
                       &(YY_CAST (yyGLRStackItem const *, yyvsp)[YYFILL ((yyi + 1) - (yynrhs))].yystate.yyloc)                       );
      if (!yyvsp[yyi - yynrhs + 1].yystate.yyresolved)
        YY_FPRINTF ((stderr, " (unresolved)"));
      YY_FPRINTF ((stderr, "\n"));
    }
}
#endif

/** Pop the symbols consumed by reduction #YYRULE from the top of stack
 *  #YYK of *YYSTACKP, and perform the appropriate semantic action on their
 *  semantic values.  Assumes that all ambiguities in semantic values
 *  have been previously resolved.  Set *YYVALP to the resulting value,
 *  and *YYLOCP to the computed location (if any).  Return value is as
 *  for userAction.  */
static inline YYRESULTTAG
yydoAction (yyGLRStack* yystackp, YYPTRDIFF_T yyk, yyRuleNum yyrule,
            YYSTYPE* yyvalp, YYLTYPE *yylocp)
{
  int yynrhs = yyrhsLength (yyrule);

  if (yystackp->yysplitPoint == YY_NULLPTR)
    {
      /* Standard special case: single stack.  */
      yyGLRStackItem* yyrhs
        = YY_REINTERPRET_CAST (yyGLRStackItem*, yystackp->yytops.yystates[yyk]);
      YY_ASSERT (yyk == 0);
      yystackp->yynextFree -= yynrhs;
      yystackp->yyspaceLeft += yynrhs;
      yystackp->yytops.yystates[0] = & yystackp->yynextFree[-1].yystate;
      return yyuserAction (yyrule, yynrhs, yyrhs, yystackp, yyk,
                           yyvalp, yylocp);
    }
  else
    {
      yyGLRStackItem yyrhsVals[YYMAXRHS + YYMAXLEFT + 1];
      yyGLRState* yys = yyrhsVals[YYMAXRHS + YYMAXLEFT].yystate.yypred
        = yystackp->yytops.yystates[yyk];
      int yyi;
      if (yynrhs == 0)
        /* Set default location.  */
        yyrhsVals[YYMAXRHS + YYMAXLEFT - 1].yystate.yyloc = yys->yyloc;
      for (yyi = 0; yyi < yynrhs; yyi += 1)
        {
          yys = yys->yypred;
          YY_ASSERT (yys);
        }
      yyupdateSplit (yystackp, yys);
      yystackp->yytops.yystates[yyk] = yys;
      return yyuserAction (yyrule, yynrhs, yyrhsVals + YYMAXRHS + YYMAXLEFT - 1,
                           yystackp, yyk, yyvalp, yylocp);
    }
}

/** Pop items off stack #YYK of *YYSTACKP according to grammar rule YYRULE,
 *  and push back on the resulting nonterminal symbol.  Perform the
 *  semantic action associated with YYRULE and store its value with the
 *  newly pushed state, if YYFORCEEVAL or if *YYSTACKP is currently
 *  unambiguous.  Otherwise, store the deferred semantic action with
 *  the new state.  If the new state would have an identical input
 *  position, LR state, and predecessor to an existing state on the stack,
 *  it is identified with that existing state, eliminating stack #YYK from
 *  *YYSTACKP.  In this case, the semantic value is
 *  added to the options for the existing state's semantic value.
 */
static inline YYRESULTTAG
yyglrReduce (yyGLRStack* yystackp, YYPTRDIFF_T yyk, yyRuleNum yyrule,
             yybool yyforceEval)
{
  YYPTRDIFF_T yyposn = yystackp->yytops.yystates[yyk]->yyposn;

  if (yyforceEval || yystackp->yysplitPoint == YY_NULLPTR)
    {
      YYSTYPE yyval;
      YYLTYPE yyloc;

      YYRESULTTAG yyflag = yydoAction (yystackp, yyk, yyrule, &yyval, &yyloc);
      if (yyflag == yyerr && yystackp->yysplitPoint != YY_NULLPTR)
        YY_DPRINTF ((stderr,
                     "Parse on stack %ld rejected by rule %d (line %d).\n",
                     YY_CAST (long, yyk), yyrule - 1, yyrline[yyrule]));
      if (yyflag != yyok)
        return yyflag;
      yyglrShift (yystackp, yyk,
                  yyLRgotoState (yystackp->yytops.yystates[yyk]->yylrState,
                                 yylhsNonterm (yyrule)),
                  yyposn, &yyval, &yyloc);
    }
  else
    {
      YYPTRDIFF_T yyi;
      int yyn;
      yyGLRState* yys, *yys0 = yystackp->yytops.yystates[yyk];
      yy_state_t yynewLRState;

      for (yys = yystackp->yytops.yystates[yyk], yyn = yyrhsLength (yyrule);
           0 < yyn; yyn -= 1)
        {
          yys = yys->yypred;
          YY_ASSERT (yys);
        }
      yyupdateSplit (yystackp, yys);
      yynewLRState = yyLRgotoState (yys->yylrState, yylhsNonterm (yyrule));
      YY_DPRINTF ((stderr,
                   "Reduced stack %ld by rule %d (line %d); action deferred.  "
                   "Now in state %d.\n",
                   YY_CAST (long, yyk), yyrule - 1, yyrline[yyrule],
                   yynewLRState));
      for (yyi = 0; yyi < yystackp->yytops.yysize; yyi += 1)
        if (yyi != yyk && yystackp->yytops.yystates[yyi] != YY_NULLPTR)
          {
            yyGLRState *yysplit = yystackp->yysplitPoint;
            yyGLRState *yyp = yystackp->yytops.yystates[yyi];
            while (yyp != yys && yyp != yysplit && yyp->yyposn >= yyposn)
              {
                if (yyp->yylrState == yynewLRState && yyp->yypred == yys)
                  {
                    yyaddDeferredAction (yystackp, yyk, yyp, yys0, yyrule);
                    yymarkStackDeleted (yystackp, yyk);
                    YY_DPRINTF ((stderr, "Merging stack %ld into stack %ld.\n",
                                 YY_CAST (long, yyk), YY_CAST (long, yyi)));
                    return yyok;
                  }
                yyp = yyp->yypred;
              }
          }
      yystackp->yytops.yystates[yyk] = yys;
      yyglrShiftDefer (yystackp, yyk, yynewLRState, yyposn, yys0, yyrule);
    }
  return yyok;
}

static YYPTRDIFF_T
yysplitStack (yyGLRStack* yystackp, YYPTRDIFF_T yyk)
{
  if (yystackp->yysplitPoint == YY_NULLPTR)
    {
      YY_ASSERT (yyk == 0);
      yystackp->yysplitPoint = yystackp->yytops.yystates[yyk];
    }
  if (yystackp->yytops.yycapacity <= yystackp->yytops.yysize)
    {
      YYPTRDIFF_T state_size = YYSIZEOF (yystackp->yytops.yystates[0]);
      YYPTRDIFF_T half_max_capacity = YYSIZE_MAXIMUM / 2 / state_size;
      if (half_max_capacity < yystackp->yytops.yycapacity)
        yyMemoryExhausted (yystackp);
      yystackp->yytops.yycapacity *= 2;

      {
        yyGLRState** yynewStates
          = YY_CAST (yyGLRState**,
                     YYREALLOC (yystackp->yytops.yystates,
                                (YY_CAST (YYSIZE_T, yystackp->yytops.yycapacity)
                                 * sizeof yynewStates[0])));
        if (yynewStates == YY_NULLPTR)
          yyMemoryExhausted (yystackp);
        yystackp->yytops.yystates = yynewStates;
      }

      {
        yybool* yynewLookaheadNeeds
          = YY_CAST (yybool*,
                     YYREALLOC (yystackp->yytops.yylookaheadNeeds,
                                (YY_CAST (YYSIZE_T, yystackp->yytops.yycapacity)
                                 * sizeof yynewLookaheadNeeds[0])));
        if (yynewLookaheadNeeds == YY_NULLPTR)
          yyMemoryExhausted (yystackp);
        yystackp->yytops.yylookaheadNeeds = yynewLookaheadNeeds;
      }
    }
  yystackp->yytops.yystates[yystackp->yytops.yysize]
    = yystackp->yytops.yystates[yyk];
  yystackp->yytops.yylookaheadNeeds[yystackp->yytops.yysize]
    = yystackp->yytops.yylookaheadNeeds[yyk];
  yystackp->yytops.yysize += 1;
  return yystackp->yytops.yysize - 1;
}

/** True iff YYY0 and YYY1 represent identical options at the top level.
 *  That is, they represent the same rule applied to RHS symbols
 *  that produce the same terminal symbols.  */
static yybool
yyidenticalOptions (yySemanticOption* yyy0, yySemanticOption* yyy1)
{
  if (yyy0->yyrule == yyy1->yyrule)
    {
      yyGLRState *yys0, *yys1;
      int yyn;
      for (yys0 = yyy0->yystate, yys1 = yyy1->yystate,
           yyn = yyrhsLength (yyy0->yyrule);
           yyn > 0;
           yys0 = yys0->yypred, yys1 = yys1->yypred, yyn -= 1)
        if (yys0->yyposn != yys1->yyposn)
          return yyfalse;
      return yytrue;
    }
  else
    return yyfalse;
}

/** Assuming identicalOptions (YYY0,YYY1), destructively merge the
 *  alternative semantic values for the RHS-symbols of YYY1 and YYY0.  */
static void
yymergeOptionSets (yySemanticOption* yyy0, yySemanticOption* yyy1)
{
  yyGLRState *yys0, *yys1;
  int yyn;
  for (yys0 = yyy0->yystate, yys1 = yyy1->yystate,
       yyn = yyrhsLength (yyy0->yyrule);
       0 < yyn;
       yys0 = yys0->yypred, yys1 = yys1->yypred, yyn -= 1)
    {
      if (yys0 == yys1)
        break;
      else if (yys0->yyresolved)
        {
          yys1->yyresolved = yytrue;
          yys1->yysemantics.yyval = yys0->yysemantics.yyval;
        }
      else if (yys1->yyresolved)
        {
          yys0->yyresolved = yytrue;
          yys0->yysemantics.yyval = yys1->yysemantics.yyval;
        }
      else
        {
          yySemanticOption** yyz0p = &yys0->yysemantics.yyfirstVal;
          yySemanticOption* yyz1 = yys1->yysemantics.yyfirstVal;
          while (yytrue)
            {
              if (yyz1 == *yyz0p || yyz1 == YY_NULLPTR)
                break;
              else if (*yyz0p == YY_NULLPTR)
                {
                  *yyz0p = yyz1;
                  break;
                }
              else if (*yyz0p < yyz1)
                {
                  yySemanticOption* yyz = *yyz0p;
                  *yyz0p = yyz1;
                  yyz1 = yyz1->yynext;
                  (*yyz0p)->yynext = yyz;
                }
              yyz0p = &(*yyz0p)->yynext;
            }
          yys1->yysemantics.yyfirstVal = yys0->yysemantics.yyfirstVal;
        }
    }
}

/** Y0 and Y1 represent two possible actions to take in a given
 *  parsing state; return 0 if no combination is possible,
 *  1 if user-mergeable, 2 if Y0 is preferred, 3 if Y1 is preferred.  */
static int
yypreference (yySemanticOption* y0, yySemanticOption* y1)
{
  yyRuleNum r0 = y0->yyrule, r1 = y1->yyrule;
  int p0 = yydprec[r0], p1 = yydprec[r1];

  if (p0 == p1)
    {
      if (yymerger[r0] == 0 || yymerger[r0] != yymerger[r1])
        return 0;
      else
        return 1;
    }
  if (p0 == 0 || p1 == 0)
    return 0;
  if (p0 < p1)
    return 3;
  if (p1 < p0)
    return 2;
  return 0;
}

static YYRESULTTAG
yyresolveValue (yyGLRState* yys, yyGLRStack* yystackp);


/** Resolve the previous YYN states starting at and including state YYS
 *  on *YYSTACKP. If result != yyok, some states may have been left
 *  unresolved possibly with empty semantic option chains.  Regardless
 *  of whether result = yyok, each state has been left with consistent
 *  data so that yydestroyGLRState can be invoked if necessary.  */
static YYRESULTTAG
yyresolveStates (yyGLRState* yys, int yyn,
                 yyGLRStack* yystackp)
{
  if (0 < yyn)
    {
      YY_ASSERT (yys->yypred);
      YYCHK (yyresolveStates (yys->yypred, yyn-1, yystackp));
      if (! yys->yyresolved)
        YYCHK (yyresolveValue (yys, yystackp));
    }
  return yyok;
}

/** Resolve the states for the RHS of YYOPT on *YYSTACKP, perform its
 *  user action, and return the semantic value and location in *YYVALP
 *  and *YYLOCP.  Regardless of whether result = yyok, all RHS states
 *  have been destroyed (assuming the user action destroys all RHS
 *  semantic values if invoked).  */
static YYRESULTTAG
yyresolveAction (yySemanticOption* yyopt, yyGLRStack* yystackp,
                 YYSTYPE* yyvalp, YYLTYPE *yylocp)
{
  yyGLRStackItem yyrhsVals[YYMAXRHS + YYMAXLEFT + 1];
  int yynrhs = yyrhsLength (yyopt->yyrule);
  YYRESULTTAG yyflag =
    yyresolveStates (yyopt->yystate, yynrhs, yystackp);
  if (yyflag != yyok)
    {
      yyGLRState *yys;
      for (yys = yyopt->yystate; yynrhs > 0; yys = yys->yypred, yynrhs -= 1)
        yydestroyGLRState ("Cleanup: popping", yys);
      return yyflag;
    }

  yyrhsVals[YYMAXRHS + YYMAXLEFT].yystate.yypred = yyopt->yystate;
  if (yynrhs == 0)
    /* Set default location.  */
    yyrhsVals[YYMAXRHS + YYMAXLEFT - 1].yystate.yyloc = yyopt->yystate->yyloc;
  {
    int yychar_current = yychar;
    YYSTYPE yylval_current = yylval;
    YYLTYPE yylloc_current = yylloc;
    yychar = yyopt->yyrawchar;
    yylval = yyopt->yyval;
    yylloc = yyopt->yyloc;
    yyflag = yyuserAction (yyopt->yyrule, yynrhs,
                           yyrhsVals + YYMAXRHS + YYMAXLEFT - 1,
                           yystackp, -1, yyvalp, yylocp);
    yychar = yychar_current;
    yylval = yylval_current;
    yylloc = yylloc_current;
  }
  return yyflag;
}

#if YYDEBUG
static void
yyreportTree (yySemanticOption* yyx, int yyindent)
{
  int yynrhs = yyrhsLength (yyx->yyrule);
  int yyi;
  yyGLRState* yys;
  yyGLRState* yystates[1 + YYMAXRHS];
  yyGLRState yyleftmost_state;

  for (yyi = yynrhs, yys = yyx->yystate; 0 < yyi; yyi -= 1, yys = yys->yypred)
    yystates[yyi] = yys;
  if (yys == YY_NULLPTR)
    {
      yyleftmost_state.yyposn = 0;
      yystates[0] = &yyleftmost_state;
    }
  else
    yystates[0] = yys;

  if (yyx->yystate->yyposn < yys->yyposn + 1)
    YY_FPRINTF ((stderr, "%*s%s -> <Rule %d, empty>\n",
                 yyindent, "", yysymbol_name (yylhsNonterm (yyx->yyrule)),
                 yyx->yyrule - 1));
  else
    YY_FPRINTF ((stderr, "%*s%s -> <Rule %d, tokens %ld .. %ld>\n",
                 yyindent, "", yysymbol_name (yylhsNonterm (yyx->yyrule)),
                 yyx->yyrule - 1, YY_CAST (long, yys->yyposn + 1),
                 YY_CAST (long, yyx->yystate->yyposn)));
  for (yyi = 1; yyi <= yynrhs; yyi += 1)
    {
      if (yystates[yyi]->yyresolved)
        {
          if (yystates[yyi-1]->yyposn+1 > yystates[yyi]->yyposn)
            YY_FPRINTF ((stderr, "%*s%s <empty>\n", yyindent+2, "",
                         yysymbol_name (yy_accessing_symbol (yystates[yyi]->yylrState))));
          else
            YY_FPRINTF ((stderr, "%*s%s <tokens %ld .. %ld>\n", yyindent+2, "",
                         yysymbol_name (yy_accessing_symbol (yystates[yyi]->yylrState)),
                         YY_CAST (long, yystates[yyi-1]->yyposn + 1),
                         YY_CAST (long, yystates[yyi]->yyposn)));
        }
      else
        yyreportTree (yystates[yyi]->yysemantics.yyfirstVal, yyindent+2);
    }
}
#endif

static YYRESULTTAG
yyreportAmbiguity (yySemanticOption* yyx0,
                   yySemanticOption* yyx1)
{
  YY_USE (yyx0);
  YY_USE (yyx1);

#if YYDEBUG
  YY_FPRINTF ((stderr, "Ambiguity detected.\n"));
  YY_FPRINTF ((stderr, "Option 1,\n"));
  yyreportTree (yyx0, 2);
  YY_FPRINTF ((stderr, "\nOption 2,\n"));
  yyreportTree (yyx1, 2);
  YY_FPRINTF ((stderr, "\n"));
#endif

  yyerror (YY_("syntax is ambiguous"));
  return yyabort;
}

/** Resolve the locations for each of the YYN1 states in *YYSTACKP,
 *  ending at YYS1.  Has no effect on previously resolved states.
 *  The first semantic option of a state is always chosen.  */
static void
yyresolveLocations (yyGLRState *yys1, int yyn1,
                    yyGLRStack *yystackp)
{
  if (0 < yyn1)
    {
      yyresolveLocations (yys1->yypred, yyn1 - 1, yystackp);
      if (!yys1->yyresolved)
        {
          yyGLRStackItem yyrhsloc[1 + YYMAXRHS];
          int yynrhs;
          yySemanticOption *yyoption = yys1->yysemantics.yyfirstVal;
          YY_ASSERT (yyoption);
          yynrhs = yyrhsLength (yyoption->yyrule);
          if (0 < yynrhs)
            {
              yyGLRState *yys;
              int yyn;
              yyresolveLocations (yyoption->yystate, yynrhs,
                                  yystackp);
              for (yys = yyoption->yystate, yyn = yynrhs;
                   yyn > 0;
                   yys = yys->yypred, yyn -= 1)
                yyrhsloc[yyn].yystate.yyloc = yys->yyloc;
            }
          else
            {
              /* Both yyresolveAction and yyresolveLocations traverse the GSS
                 in reverse rightmost order.  It is only necessary to invoke
                 yyresolveLocations on a subforest for which yyresolveAction
                 would have been invoked next had an ambiguity not been
                 detected.  Thus the location of the previous state (but not
                 necessarily the previous state itself) is guaranteed to be
                 resolved already.  */
              yyGLRState *yyprevious = yyoption->yystate;
              yyrhsloc[0].yystate.yyloc = yyprevious->yyloc;
            }
          YYLLOC_DEFAULT ((yys1->yyloc), yyrhsloc, yynrhs);
        }
    }
}

/** Resolve the ambiguity represented in state YYS in *YYSTACKP,
 *  perform the indicated actions, and set the semantic value of YYS.
 *  If result != yyok, the chain of semantic options in YYS has been
 *  cleared instead or it has been left unmodified except that
 *  redundant options may have been removed.  Regardless of whether
 *  result = yyok, YYS has been left with consistent data so that
 *  yydestroyGLRState can be invoked if necessary.  */
static YYRESULTTAG
yyresolveValue (yyGLRState* yys, yyGLRStack* yystackp)
{
  yySemanticOption* yyoptionList = yys->yysemantics.yyfirstVal;
  yySemanticOption* yybest = yyoptionList;
  yySemanticOption** yypp;
  yybool yymerge = yyfalse;
  YYSTYPE yyval;
  YYRESULTTAG yyflag;
  YYLTYPE *yylocp = &yys->yyloc;

  for (yypp = &yyoptionList->yynext; *yypp != YY_NULLPTR; )
    {
      yySemanticOption* yyp = *yypp;

      if (yyidenticalOptions (yybest, yyp))
        {
          yymergeOptionSets (yybest, yyp);
          *yypp = yyp->yynext;
        }
      else
        {
          switch (yypreference (yybest, yyp))
            {
            case 0:
              yyresolveLocations (yys, 1, yystackp);
              return yyreportAmbiguity (yybest, yyp);
              break;
            case 1:
              yymerge = yytrue;
              break;
            case 2:
              break;
            case 3:
              yybest = yyp;
              yymerge = yyfalse;
              break;
            default:
              /* This cannot happen so it is not worth a YY_ASSERT (yyfalse),
                 but some compilers complain if the default case is
                 omitted.  */
              break;
            }
          yypp = &yyp->yynext;
        }
    }

  if (yymerge)
    {
      yySemanticOption* yyp;
      int yyprec = yydprec[yybest->yyrule];
      yyflag = yyresolveAction (yybest, yystackp, &yyval, yylocp);
      if (yyflag == yyok)
        for (yyp = yybest->yynext; yyp != YY_NULLPTR; yyp = yyp->yynext)
          {
            if (yyprec == yydprec[yyp->yyrule])
              {
                YYSTYPE yyval_other;
                YYLTYPE yydummy;
                yyflag = yyresolveAction (yyp, yystackp, &yyval_other, &yydummy);
                if (yyflag != yyok)
                  {
                    yydestruct ("Cleanup: discarding incompletely merged value for",
                                yy_accessing_symbol (yys->yylrState),
                                &yyval, yylocp);
                    break;
                  }
                yyuserMerge (yymerger[yyp->yyrule], &yyval, &yyval_other);
              }
          }
    }
  else
    yyflag = yyresolveAction (yybest, yystackp, &yyval, yylocp);

  if (yyflag == yyok)
    {
      yys->yyresolved = yytrue;
      yys->yysemantics.yyval = yyval;
    }
  else
    yys->yysemantics.yyfirstVal = YY_NULLPTR;
  return yyflag;
}

static YYRESULTTAG
yyresolveStack (yyGLRStack* yystackp)
{
  if (yystackp->yysplitPoint != YY_NULLPTR)
    {
      yyGLRState* yys;
      int yyn;

      for (yyn = 0, yys = yystackp->yytops.yystates[0];
           yys != yystackp->yysplitPoint;
           yys = yys->yypred, yyn += 1)
        continue;
      YYCHK (yyresolveStates (yystackp->yytops.yystates[0], yyn, yystackp
                             ));
    }
  return yyok;
}

/** Called when returning to deterministic operation to clean up the extra
 * stacks. */
static void
yycompressStack (yyGLRStack* yystackp)
{
  /* yyr is the state after the split point.  */
  yyGLRState *yyr;

  if (yystackp->yytops.yysize != 1 || yystackp->yysplitPoint == YY_NULLPTR)
    return;

  {
    yyGLRState *yyp, *yyq;
    for (yyp = yystackp->yytops.yystates[0], yyq = yyp->yypred, yyr = YY_NULLPTR;
         yyp != yystackp->yysplitPoint;
         yyr = yyp, yyp = yyq, yyq = yyp->yypred)
      yyp->yypred = yyr;
  }

  yystackp->yyspaceLeft += yystackp->yynextFree - yystackp->yyitems;
  yystackp->yynextFree = YY_REINTERPRET_CAST (yyGLRStackItem*, yystackp->yysplitPoint) + 1;
  yystackp->yyspaceLeft -= yystackp->yynextFree - yystackp->yyitems;
  yystackp->yysplitPoint = YY_NULLPTR;
  yystackp->yylastDeleted = YY_NULLPTR;

  while (yyr != YY_NULLPTR)
    {
      yystackp->yynextFree->yystate = *yyr;
      yyr = yyr->yypred;
      yystackp->yynextFree->yystate.yypred = &yystackp->yynextFree[-1].yystate;
      yystackp->yytops.yystates[0] = &yystackp->yynextFree->yystate;
      yystackp->yynextFree += 1;
      yystackp->yyspaceLeft -= 1;
    }
}

static YYRESULTTAG
yyprocessOneStack (yyGLRStack* yystackp, YYPTRDIFF_T yyk,
                   YYPTRDIFF_T yyposn)
{
  while (yystackp->yytops.yystates[yyk] != YY_NULLPTR)
    {
      yy_state_t yystate = yystackp->yytops.yystates[yyk]->yylrState;
      YY_DPRINTF ((stderr, "Stack %ld Entering state %d\n",
                   YY_CAST (long, yyk), yystate));

      YY_ASSERT (yystate != YYFINAL);

      if (yyisDefaultedState (yystate))
        {
          YYRESULTTAG yyflag;
          yyRuleNum yyrule = yydefaultAction (yystate);
          if (yyrule == 0)
            {
              YY_DPRINTF ((stderr, "Stack %ld dies.\n", YY_CAST (long, yyk)));
              yymarkStackDeleted (yystackp, yyk);
              return yyok;
            }
          yyflag = yyglrReduce (yystackp, yyk, yyrule, yyimmediate[yyrule]);
          if (yyflag == yyerr)
            {
              YY_DPRINTF ((stderr,
                           "Stack %ld dies "
                           "(predicate failure or explicit user error).\n",
                           YY_CAST (long, yyk)));
              yymarkStackDeleted (yystackp, yyk);
              return yyok;
            }
          if (yyflag != yyok)
            return yyflag;
        }
      else
        {
          yysymbol_kind_t yytoken = yygetToken (&yychar);
          const short* yyconflicts;
          const int yyaction = yygetLRActions (yystate, yytoken, &yyconflicts);
          yystackp->yytops.yylookaheadNeeds[yyk] = yytrue;

          for (/* nothing */; *yyconflicts; yyconflicts += 1)
            {
              YYRESULTTAG yyflag;
              YYPTRDIFF_T yynewStack = yysplitStack (yystackp, yyk);
              YY_DPRINTF ((stderr, "Splitting off stack %ld from %ld.\n",
                           YY_CAST (long, yynewStack), YY_CAST (long, yyk)));
              yyflag = yyglrReduce (yystackp, yynewStack,
                                    *yyconflicts,
                                    yyimmediate[*yyconflicts]);
              if (yyflag == yyok)
                YYCHK (yyprocessOneStack (yystackp, yynewStack,
                                          yyposn));
              else if (yyflag == yyerr)
                {
                  YY_DPRINTF ((stderr, "Stack %ld dies.\n", YY_CAST (long, yynewStack)));
                  yymarkStackDeleted (yystackp, yynewStack);
                }
              else
                return yyflag;
            }

          if (yyisShiftAction (yyaction))
            break;
          else if (yyisErrorAction (yyaction))
            {
              YY_DPRINTF ((stderr, "Stack %ld dies.\n", YY_CAST (long, yyk)));
              yymarkStackDeleted (yystackp, yyk);
              break;
            }
          else
            {
              YYRESULTTAG yyflag = yyglrReduce (yystackp, yyk, -yyaction,
                                                yyimmediate[-yyaction]);
              if (yyflag == yyerr)
                {
                  YY_DPRINTF ((stderr,
                               "Stack %ld dies "
                               "(predicate failure or explicit user error).\n",
                               YY_CAST (long, yyk)));
                  yymarkStackDeleted (yystackp, yyk);
                  break;
                }
              else if (yyflag != yyok)
                return yyflag;
            }
        }
    }
  return yyok;
}

/* Put in YYARG at most YYARGN of the expected tokens given the
   current YYSTACKP, and return the number of tokens stored in YYARG.  If
   YYARG is null, return the number of expected tokens (guaranteed to
   be less than YYNTOKENS).  */
static int
yypcontext_expected_tokens (const yyGLRStack* yystackp,
                            yysymbol_kind_t yyarg[], int yyargn)
{
  /* Actual size of YYARG. */
  int yycount = 0;
  int yyn = yypact[yystackp->yytops.yystates[0]->yylrState];
  if (!yypact_value_is_default (yyn))
    {
      /* Start YYX at -YYN if negative to avoid negative indexes in
         YYCHECK.  In other words, skip the first -YYN actions for
         this state because they are default actions.  */
      int yyxbegin = yyn < 0 ? -yyn : 0;
      /* Stay within bounds of both yycheck and yytname.  */
      int yychecklim = YYLAST - yyn + 1;
      int yyxend = yychecklim < YYNTOKENS ? yychecklim : YYNTOKENS;
      int yyx;
      for (yyx = yyxbegin; yyx < yyxend; ++yyx)
        if (yycheck[yyx + yyn] == yyx && yyx != YYSYMBOL_YYerror
            && !yytable_value_is_error (yytable[yyx + yyn]))
          {
            if (!yyarg)
              ++yycount;
            else if (yycount == yyargn)
              return 0;
            else
              yyarg[yycount++] = YY_CAST (yysymbol_kind_t, yyx);
          }
    }
  if (yyarg && yycount == 0 && 0 < yyargn)
    yyarg[0] = YYSYMBOL_YYEMPTY;
  return yycount;
}

static int
yy_syntax_error_arguments (const yyGLRStack* yystackp,
                           yysymbol_kind_t yyarg[], int yyargn)
{
  yysymbol_kind_t yytoken = yychar == YYEMPTY ? YYSYMBOL_YYEMPTY : YYTRANSLATE (yychar);
  /* Actual size of YYARG. */
  int yycount = 0;
  /* There are many possibilities here to consider:
     - If this state is a consistent state with a default action, then
       the only way this function was invoked is if the default action
       is an error action.  In that case, don't check for expected
       tokens because there are none.
     - The only way there can be no lookahead present (in yychar) is if
       this state is a consistent state with a default action.  Thus,
       detecting the absence of a lookahead is sufficient to determine
       that there is no unexpected or expected token to report.  In that
       case, just report a simple "syntax error".
     - Don't assume there isn't a lookahead just because this state is a
       consistent state with a default action.  There might have been a
       previous inconsistent state, consistent state with a non-default
       action, or user semantic action that manipulated yychar.
     - Of course, the expected token list depends on states to have
       correct lookahead information, and it depends on the parser not
       to perform extra reductions after fetching a lookahead from the
       scanner and before detecting a syntax error.  Thus, state merging
       (from LALR or IELR) and default reductions corrupt the expected
       token list.  However, the list is correct for canonical LR with
       one exception: it will still contain any token that will not be
       accepted due to an error action in a later state.
  */
  if (yytoken != YYSYMBOL_YYEMPTY)
    {
      int yyn;
      if (yyarg)
        yyarg[yycount] = yytoken;
      ++yycount;
      yyn = yypcontext_expected_tokens (yystackp,
                                        yyarg ? yyarg + 1 : yyarg, yyargn - 1);
      if (yyn == YYENOMEM)
        return YYENOMEM;
      else
        yycount += yyn;
    }
  return yycount;
}



static void
yyreportSyntaxError (yyGLRStack* yystackp)
{
  if (yystackp->yyerrState != 0)
    return;
  {
  yybool yysize_overflow = yyfalse;
  char* yymsg = YY_NULLPTR;
  enum { YYARGS_MAX = 5 };
  /* Internationalized format string. */
  const char *yyformat = YY_NULLPTR;
  /* Arguments of yyformat: reported tokens (one for the "unexpected",
     one per "expected"). */
  yysymbol_kind_t yyarg[YYARGS_MAX];
  /* Cumulated lengths of YYARG.  */
  YYPTRDIFF_T yysize = 0;

  /* Actual size of YYARG. */
  int yycount
    = yy_syntax_error_arguments (yystackp, yyarg, YYARGS_MAX);
  if (yycount == YYENOMEM)
    yyMemoryExhausted (yystackp);

  switch (yycount)
    {
#define YYCASE_(N, S)                   \
      case N:                           \
        yyformat = S;                   \
      break
    default: /* Avoid compiler warnings. */
      YYCASE_(0, YY_("syntax error"));
      YYCASE_(1, YY_("syntax error, unexpected %s"));
      YYCASE_(2, YY_("syntax error, unexpected %s, expecting %s"));
      YYCASE_(3, YY_("syntax error, unexpected %s, expecting %s or %s"));
      YYCASE_(4, YY_("syntax error, unexpected %s, expecting %s or %s or %s"));
      YYCASE_(5, YY_("syntax error, unexpected %s, expecting %s or %s or %s or %s"));
#undef YYCASE_
    }

  /* Compute error message size.  Don't count the "%s"s, but reserve
     room for the terminator.  */
  yysize = yystrlen (yyformat) - 2 * yycount + 1;
  {
    int yyi;
    for (yyi = 0; yyi < yycount; ++yyi)
      {
        YYPTRDIFF_T yysz
          = yytnamerr (YY_NULLPTR, yytname[yyarg[yyi]]);
        if (YYSIZE_MAXIMUM - yysize < yysz)
          yysize_overflow = yytrue;
        else
          yysize += yysz;
      }
  }

  if (!yysize_overflow)
    yymsg = YY_CAST (char *, YYMALLOC (YY_CAST (YYSIZE_T, yysize)));

  if (yymsg)
    {
      char *yyp = yymsg;
      int yyi = 0;
      while ((*yyp = *yyformat))
        {
          if (*yyp == '%' && yyformat[1] == 's' && yyi < yycount)
            {
              yyp += yytnamerr (yyp, yytname[yyarg[yyi++]]);
              yyformat += 2;
            }
          else
            {
              ++yyp;
              ++yyformat;
            }
        }
      yyerror (yymsg);
      YYFREE (yymsg);
    }
  else
    {
      yyerror (YY_("syntax error"));
      yyMemoryExhausted (yystackp);
    }
  }
  yynerrs += 1;
}

/* Recover from a syntax error on *YYSTACKP, assuming that *YYSTACKP->YYTOKENP,
   yylval, and yylloc are the syntactic category, semantic value, and location
   of the lookahead.  */
static void
yyrecoverSyntaxError (yyGLRStack* yystackp)
{
  if (yystackp->yyerrState == 3)
    /* We just shifted the error token and (perhaps) took some
       reductions.  Skip tokens until we can proceed.  */
    while (yytrue)
      {
        yysymbol_kind_t yytoken;
        int yyj;
        if (yychar == YYEOF)
          yyFail (yystackp, YY_NULLPTR);
        if (yychar != YYEMPTY)
          {
            /* We throw away the lookahead, but the error range
               of the shifted error token must take it into account.  */
            yyGLRState *yys = yystackp->yytops.yystates[0];
            yyGLRStackItem yyerror_range[3];
            yyerror_range[1].yystate.yyloc = yys->yyloc;
            yyerror_range[2].yystate.yyloc = yylloc;
            YYLLOC_DEFAULT ((yys->yyloc), yyerror_range, 2);
            yytoken = YYTRANSLATE (yychar);
            yydestruct ("Error: discarding",
                        yytoken, &yylval, &yylloc);
            yychar = YYEMPTY;
          }
        yytoken = yygetToken (&yychar);
        yyj = yypact[yystackp->yytops.yystates[0]->yylrState];
        if (yypact_value_is_default (yyj))
          return;
        yyj += yytoken;
        if (yyj < 0 || YYLAST < yyj || yycheck[yyj] != yytoken)
          {
            if (yydefact[yystackp->yytops.yystates[0]->yylrState] != 0)
              return;
          }
        else if (! yytable_value_is_error (yytable[yyj]))
          return;
      }

  /* Reduce to one stack.  */
  {
    YYPTRDIFF_T yyk;
    for (yyk = 0; yyk < yystackp->yytops.yysize; yyk += 1)
      if (yystackp->yytops.yystates[yyk] != YY_NULLPTR)
        break;
    if (yyk >= yystackp->yytops.yysize)
      yyFail (yystackp, YY_NULLPTR);
    for (yyk += 1; yyk < yystackp->yytops.yysize; yyk += 1)
      yymarkStackDeleted (yystackp, yyk);
    yyremoveDeletes (yystackp);
    yycompressStack (yystackp);
  }

  /* Pop stack until we find a state that shifts the error token.  */
  yystackp->yyerrState = 3;
  while (yystackp->yytops.yystates[0] != YY_NULLPTR)
    {
      yyGLRState *yys = yystackp->yytops.yystates[0];
      int yyj = yypact[yys->yylrState];
      if (! yypact_value_is_default (yyj))
        {
          yyj += YYSYMBOL_YYerror;
          if (0 <= yyj && yyj <= YYLAST && yycheck[yyj] == YYSYMBOL_YYerror
              && yyisShiftAction (yytable[yyj]))
            {
              /* Shift the error token.  */
              int yyaction = yytable[yyj];
              /* First adjust its location.*/
              YYLTYPE yyerrloc;
              yystackp->yyerror_range[2].yystate.yyloc = yylloc;
              YYLLOC_DEFAULT (yyerrloc, (yystackp->yyerror_range), 2);
              YY_SYMBOL_PRINT ("Shifting", yy_accessing_symbol (yyaction),
                               &yylval, &yyerrloc);
              yyglrShift (yystackp, 0, yyaction,
                          yys->yyposn, &yylval, &yyerrloc);
              yys = yystackp->yytops.yystates[0];
              break;
            }
        }
      yystackp->yyerror_range[1].yystate.yyloc = yys->yyloc;
      if (yys->yypred != YY_NULLPTR)
        yydestroyGLRState ("Error: popping", yys);
      yystackp->yytops.yystates[0] = yys->yypred;
      yystackp->yynextFree -= 1;
      yystackp->yyspaceLeft += 1;
    }
  if (yystackp->yytops.yystates[0] == YY_NULLPTR)
    yyFail (yystackp, YY_NULLPTR);
}

#define YYCHK1(YYE)                             \
  do {                                          \
    switch (YYE) {                              \
    case yyok:     break;                       \
    case yyabort:  goto yyabortlab;             \
    case yyaccept: goto yyacceptlab;            \
    case yyerr:    goto yyuser_error;           \
    case yynomem:  goto yyexhaustedlab;         \
    default:       goto yybuglab;               \
    }                                           \
  } while (0)

/*----------.
| yyparse.  |
`----------*/

int
yyparse (void)
{
  int yyresult;
  yyGLRStack yystack;
  yyGLRStack* const yystackp = &yystack;
  YYPTRDIFF_T yyposn;

  YY_DPRINTF ((stderr, "Starting parse\n"));

  yychar = YYEMPTY;
  yylval = yyval_default;
  yylloc = yyloc_default;

  if (! yyinitGLRStack (yystackp, YYINITDEPTH))
    goto yyexhaustedlab;
  switch (YYSETJMP (yystack.yyexception_buffer))
    {
    case 0: break;
    case 1: goto yyabortlab;
    case 2: goto yyexhaustedlab;
    default: goto yybuglab;
    }
  yyglrShift (&yystack, 0, 0, 0, &yylval, &yylloc);
  yyposn = 0;

  while (yytrue)
    {
      /* For efficiency, we have two loops, the first of which is
         specialized to deterministic operation (single stack, no
         potential ambiguity).  */
      /* Standard mode. */
      while (yytrue)
        {
          yy_state_t yystate = yystack.yytops.yystates[0]->yylrState;
          YY_DPRINTF ((stderr, "Entering state %d\n", yystate));
          if (yystate == YYFINAL)
            goto yyacceptlab;
          if (yyisDefaultedState (yystate))
            {
              yyRuleNum yyrule = yydefaultAction (yystate);
              if (yyrule == 0)
                {
                  yystack.yyerror_range[1].yystate.yyloc = yylloc;
                  yyreportSyntaxError (&yystack);
                  goto yyuser_error;
                }
              YYCHK1 (yyglrReduce (&yystack, 0, yyrule, yytrue));
            }
          else
            {
              yysymbol_kind_t yytoken = yygetToken (&yychar);
              const short* yyconflicts;
              int yyaction = yygetLRActions (yystate, yytoken, &yyconflicts);
              if (*yyconflicts)
                /* Enter nondeterministic mode.  */
                break;
              if (yyisShiftAction (yyaction))
                {
                  YY_SYMBOL_PRINT ("Shifting", yytoken, &yylval, &yylloc);
                  yychar = YYEMPTY;
                  yyposn += 1;
                  yyglrShift (&yystack, 0, yyaction, yyposn, &yylval, &yylloc);
                  if (0 < yystack.yyerrState)
                    yystack.yyerrState -= 1;
                }
              else if (yyisErrorAction (yyaction))
                {
                  yystack.yyerror_range[1].yystate.yyloc = yylloc;
                  /* Issue an error message unless the scanner already
                     did. */
                  if (yychar != YYerror)
                    yyreportSyntaxError (&yystack);
                  goto yyuser_error;
                }
              else
                YYCHK1 (yyglrReduce (&yystack, 0, -yyaction, yytrue));
            }
        }

      /* Nondeterministic mode. */
      while (yytrue)
        {
          yysymbol_kind_t yytoken_to_shift;
          YYPTRDIFF_T yys;

          for (yys = 0; yys < yystack.yytops.yysize; yys += 1)
            yystackp->yytops.yylookaheadNeeds[yys] = yychar != YYEMPTY;

          /* yyprocessOneStack returns one of three things:

              - An error flag.  If the caller is yyprocessOneStack, it
                immediately returns as well.  When the caller is finally
                yyparse, it jumps to an error label via YYCHK1.

              - yyok, but yyprocessOneStack has invoked yymarkStackDeleted
                (&yystack, yys), which sets the top state of yys to NULL.  Thus,
                yyparse's following invocation of yyremoveDeletes will remove
                the stack.

              - yyok, when ready to shift a token.

             Except in the first case, yyparse will invoke yyremoveDeletes and
             then shift the next token onto all remaining stacks.  This
             synchronization of the shift (that is, after all preceding
             reductions on all stacks) helps prevent double destructor calls
             on yylval in the event of memory exhaustion.  */

          for (yys = 0; yys < yystack.yytops.yysize; yys += 1)
            YYCHK1 (yyprocessOneStack (&yystack, yys, yyposn));
          yyremoveDeletes (&yystack);
          if (yystack.yytops.yysize == 0)
            {
              yyundeleteLastStack (&yystack);
              if (yystack.yytops.yysize == 0)
                yyFail (&yystack, YY_("syntax error"));
              YYCHK1 (yyresolveStack (&yystack));
              YY_DPRINTF ((stderr, "Returning to deterministic operation.\n"));
              yystack.yyerror_range[1].yystate.yyloc = yylloc;
              yyreportSyntaxError (&yystack);
              goto yyuser_error;
            }

          /* If any yyglrShift call fails, it will fail after shifting.  Thus,
             a copy of yylval will already be on stack 0 in the event of a
             failure in the following loop.  Thus, yychar is set to YYEMPTY
             before the loop to make sure the user destructor for yylval isn't
             called twice.  */
          yytoken_to_shift = YYTRANSLATE (yychar);
          yychar = YYEMPTY;
          yyposn += 1;
          for (yys = 0; yys < yystack.yytops.yysize; yys += 1)
            {
              yy_state_t yystate = yystack.yytops.yystates[yys]->yylrState;
              const short* yyconflicts;
              int yyaction = yygetLRActions (yystate, yytoken_to_shift,
                              &yyconflicts);
              /* Note that yyconflicts were handled by yyprocessOneStack.  */
              YY_DPRINTF ((stderr, "On stack %ld, ", YY_CAST (long, yys)));
              YY_SYMBOL_PRINT ("shifting", yytoken_to_shift, &yylval, &yylloc);
              yyglrShift (&yystack, yys, yyaction, yyposn,
                          &yylval, &yylloc);
              YY_DPRINTF ((stderr, "Stack %ld now in state %d\n",
                           YY_CAST (long, yys),
                           yystack.yytops.yystates[yys]->yylrState));
            }

          if (yystack.yytops.yysize == 1)
            {
              YYCHK1 (yyresolveStack (&yystack));
              YY_DPRINTF ((stderr, "Returning to deterministic operation.\n"));
              yycompressStack (&yystack);
              break;
            }
        }
      continue;
    yyuser_error:
      yyrecoverSyntaxError (&yystack);
      yyposn = yystack.yytops.yystates[0]->yyposn;
    }

 yyacceptlab:
  yyresult = 0;
  goto yyreturnlab;

 yybuglab:
  YY_ASSERT (yyfalse);
  goto yyabortlab;

 yyabortlab:
  yyresult = 1;
  goto yyreturnlab;

 yyexhaustedlab:
  yyerror (YY_("memory exhausted"));
  yyresult = 2;
  goto yyreturnlab;

 yyreturnlab:
  if (yychar != YYEMPTY)
    yydestruct ("Cleanup: discarding lookahead",
                YYTRANSLATE (yychar), &yylval, &yylloc);

  /* If the stack is well-formed, pop the stack until it is empty,
     destroying its entries as we go.  But free the stack regardless
     of whether it is well-formed.  */
  if (yystack.yyitems)
    {
      yyGLRState** yystates = yystack.yytops.yystates;
      if (yystates)
        {
          YYPTRDIFF_T yysize = yystack.yytops.yysize;
          YYPTRDIFF_T yyk;
          for (yyk = 0; yyk < yysize; yyk += 1)
            if (yystates[yyk])
              {
                while (yystates[yyk])
                  {
                    yyGLRState *yys = yystates[yyk];
                    yystack.yyerror_range[1].yystate.yyloc = yys->yyloc;
                    if (yys->yypred != YY_NULLPTR)
                      yydestroyGLRState ("Cleanup: popping", yys);
                    yystates[yyk] = yys->yypred;
                    yystack.yynextFree -= 1;
                    yystack.yyspaceLeft += 1;
                  }
                break;
              }
        }
      yyfreeGLRStack (&yystack);
    }

  return yyresult;
}

/* DEBUGGING ONLY */
#if YYDEBUG
/* Print *YYS and its predecessors. */
static void
yy_yypstack (yyGLRState* yys)
{
  if (yys->yypred)
    {
      yy_yypstack (yys->yypred);
      YY_FPRINTF ((stderr, " -> "));
    }
  YY_FPRINTF ((stderr, "%d@%ld", yys->yylrState, YY_CAST (long, yys->yyposn)));
}

/* Print YYS (possibly NULL) and its predecessors. */
static void
yypstates (yyGLRState* yys)
{
  if (yys == YY_NULLPTR)
    YY_FPRINTF ((stderr, "<null>"));
  else
    yy_yypstack (yys);
  YY_FPRINTF ((stderr, "\n"));
}

/* Print the stack #YYK.  */
static void
yypstack (yyGLRStack* yystackp, YYPTRDIFF_T yyk)
{
  yypstates (yystackp->yytops.yystates[yyk]);
}

/* Print all the stacks.  */
static void
yypdumpstack (yyGLRStack* yystackp)
{
#define YYINDEX(YYX)                                                    \
  YY_CAST (long,                                                        \
           ((YYX)                                                       \
            ? YY_REINTERPRET_CAST (yyGLRStackItem*, (YYX)) - yystackp->yyitems \
            : -1))

  yyGLRStackItem* yyp;
  for (yyp = yystackp->yyitems; yyp < yystackp->yynextFree; yyp += 1)
    {
      YY_FPRINTF ((stderr, "%3ld. ",
                   YY_CAST (long, yyp - yystackp->yyitems)));
      if (*YY_REINTERPRET_CAST (yybool *, yyp))
        {
          YY_ASSERT (yyp->yystate.yyisState);
          YY_ASSERT (yyp->yyoption.yyisState);
          YY_FPRINTF ((stderr, "Res: %d, LR State: %d, posn: %ld, pred: %ld",
                       yyp->yystate.yyresolved, yyp->yystate.yylrState,
                       YY_CAST (long, yyp->yystate.yyposn),
                       YYINDEX (yyp->yystate.yypred)));
          if (! yyp->yystate.yyresolved)
            YY_FPRINTF ((stderr, ", firstVal: %ld",
                         YYINDEX (yyp->yystate.yysemantics.yyfirstVal)));
        }
      else
        {
          YY_ASSERT (!yyp->yystate.yyisState);
          YY_ASSERT (!yyp->yyoption.yyisState);
          YY_FPRINTF ((stderr, "Option. rule: %d, state: %ld, next: %ld",
                       yyp->yyoption.yyrule - 1,
                       YYINDEX (yyp->yyoption.yystate),
                       YYINDEX (yyp->yyoption.yynext)));
        }
      YY_FPRINTF ((stderr, "\n"));
    }

  YY_FPRINTF ((stderr, "Tops:"));
  {
    YYPTRDIFF_T yyi;
    for (yyi = 0; yyi < yystackp->yytops.yysize; yyi += 1)
      YY_FPRINTF ((stderr, "%ld: %ld; ", YY_CAST (long, yyi),
                   YYINDEX (yystackp->yytops.yystates[yyi])));
    YY_FPRINTF ((stderr, "\n"));
  }
#undef YYINDEX
}
#endif

#undef yylval
#undef yychar
#undef yynerrs
#undef yylloc




#line 1206 "src/frontend/evil.y"


void yyerror(const char *s) {
    cvil_span_t span = { NULL, yylineno, yycolumn, yylineno, yycolumn };
    cvil_diag_error(span, "CVIL0100", "%s near '%s'", s, yytext ? yytext : "");
}
