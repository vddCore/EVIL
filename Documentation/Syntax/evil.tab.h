/* A Bison parser, made by GNU Bison 3.8.2.  */

/* Skeleton interface for Bison GLR parsers in C

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

#ifndef YY_YY_EVIL_TAB_H_INCLUDED
# define YY_YY_EVIL_TAB_H_INCLUDED
/* Debug traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif
#if YYDEBUG
extern int yydebug;
#endif

/* Token kinds.  */
#ifndef YYTOKENTYPE
# define YYTOKENTYPE
  enum yytokentype
  {
    YYEMPTY = -2,
    YYEOF = 0,                     /* "end of file"  */
    YYerror = 256,                 /* error  */
    YYUNDEF = 257,                 /* "invalid token"  */
    TOKEN_FN = 258,                /* "fn"  */
    TOKEN_LOC = 259,               /* "loc"  */
    TOKEN_FOR = 260,               /* "for"  */
    TOKEN_WHILE = 261,             /* "while"  */
    TOKEN_DO = 262,                /* "do"  */
    TOKEN_RW = 263,                /* "rw"  */
    TOKEN_VAL = 264,               /* "val"  */
    TOKEN_IN = 265,                /* "in"  */
    TOKEN_IF = 266,                /* "if"  */
    TOKEN_IS = 267,                /* "is"  */
    TOKEN_ELIF = 268,              /* "elif"  */
    TOKEN_ELSE = 269,              /* "else"  */
    TOKEN_BREAK = 270,             /* "break"  */
    TOKEN_SKIP = 271,              /* "skip"  */
    TOKEN_BY = 272,                /* "by"  */
    TOKEN_RET = 273,               /* "ret"  */
    TOKEN_TRUE = 274,              /* "true"  */
    TOKEN_FALSE = 275,             /* "false"  */
    TOKEN_NIL = 276,               /* "nil"  */
    TOKEN_TYPEOF = 277,            /* "typeof"  */
    TOKEN_TRY = 278,               /* "try"  */
    TOKEN_CATCH = 279,             /* "catch"  */
    TOKEN_THROW = 280,             /* "throw"  */
    TOKEN_YIELD = 281,             /* "yield"  */
    TOKEN_EACH = 282,              /* "each"  */
    TOKEN_ARRAY = 283,             /* "array"  */
    TOKEN_SELF = 284,              /* "self"  */
    TOKEN_ERROR = 285,             /* "error"  */
    TOKEN_RETRY = 286,             /* "retry"  */
    TOKEN_WITH = 287,              /* "with"  */
    TOKEN_NAN = 288,               /* "NaN"  */
    TOKEN_INFINITY = 289,          /* "Infinity"  */
    TYPE_NIL = 290,                /* "Nil"  */
    TYPE_NUMBER = 291,             /* "Number"  */
    TYPE_STRING = 292,             /* "String"  */
    TYPE_BOOLEAN = 293,            /* "Boolean"  */
    TYPE_TABLE = 294,              /* "Table"  */
    TYPE_ARRAY = 295,              /* "Array"  */
    TYPE_FIBER = 296,              /* "Fiber"  */
    TYPE_CHUNK = 297,              /* "Function"  */
    TYPE_ERROR = 298,              /* "Error"  */
    TYPE_TYPE = 299,               /* "Type"  */
    TYPE_NATIVE_FUNCTION = 300,    /* "NativeFunction"  */
    TYPE_NATIVE_OBJECT = 301,      /* "NativeObject"  */
    TOKEN_ASSIGN_ADD = 302,        /* "+="  */
    TOKEN_ASSIGN_SUBTRACT = 303,   /* "-="  */
    TOKEN_ASSIGN_MULTIPLY = 304,   /* "*="  */
    TOKEN_ASSIGN_DIVIDE = 305,     /* "/="  */
    TOKEN_ASSIGN_MODULO = 306,     /* "%="  */
    TOKEN_ASSIGN_BITWISE_AND = 307, /* "&="  */
    TOKEN_ASSIGN_BITWISE_OR = 308, /* "|="  */
    TOKEN_ASSIGN_BITWISE_XOR = 309, /* "^="  */
    TOKEN_ASSIGN_SHIFT_RIGHT = 310, /* ">>="  */
    TOKEN_ASSIGN_SHIFT_LEFT = 311, /* "<<="  */
    TOKEN_ASSIGN_COALESCE = 312,   /* "??="  */
    TOKEN_DEEP_EQUAL = 313,        /* "<==>"  */
    TOKEN_DEEP_NOT_EQUAL = 314,    /* "<!=>"  */
    TOKEN_EQUAL = 315,             /* "=="  */
    TOKEN_NOT_EQUAL = 316,         /* "!="  */
    TOKEN_LESS_EQUAL = 317,        /* "<="  */
    TOKEN_GREATER_EQUAL = 318,     /* ">="  */
    TOKEN_SHIFT_LEFT = 319,        /* "<<"  */
    TOKEN_SHIFT_RIGHT = 320,       /* ">>"  */
    TOKEN_LOGICAL_AND = 321,       /* "&&"  */
    TOKEN_LOGICAL_OR = 322,        /* "||"  */
    TOKEN_INCREMENT = 323,         /* "++"  */
    TOKEN_DECREMENT = 324,         /* "--"  */
    TOKEN_SAFE_SPREAD = 325,       /* "...?"  */
    TOKEN_ELLIPSIS = 326,          /* "..."  */
    TOKEN_RANGE = 327,             /* ".."  */
    TOKEN_ASSOCIATE = 328,         /* "=>"  */
    TOKEN_RIGHT_ARROW = 329,       /* "->"  */
    TOKEN_LEFT_ARROW = 330,        /* "<-"  */
    TOKEN_ELVIS = 331,             /* "?."  */
    TOKEN_ELVIS_ARRAY = 332,       /* "?["  */
    TOKEN_SAFE_COLON = 333,        /* "?:"  */
    TOKEN_ELVIS_INVOCATION = 334,  /* "?("  */
    TOKEN_DOUBLE_QUESTION = 335,   /* "??"  */
    TOKEN_DOUBLE_COLON = 336,      /* "::"  */
    TOKEN_NOT_IN = 337,            /* "!in"  */
    TOKEN_IS_NOT = 338,            /* "!is"  */
    TOKEN_ATTRIBUTE_LIST = 339,    /* "#["  */
    IDENTIFIER = 340,              /* "identifier"  */
    TOKEN_NUMBER = 341,            /* "number"  */
    TOKEN_HEX_INTEGER = 342,       /* "hex_integer"  */
    TOKEN_PLAIN_STRING = 343,      /* "plain_string"  */
    TOKEN_INTERPOLATED_STRING = 344, /* "interpolated_string"  */
    LOWER_THAN_ELSE = 345,         /* LOWER_THAN_ELSE  */
    LOWER_THAN_CATCH = 346,        /* LOWER_THAN_CATCH  */
    UNARY = 347,                   /* UNARY  */
    PREFIX = 348,                  /* PREFIX  */
    POSTFIX = 349                  /* POSTFIX  */
  };
  typedef enum yytokentype yytoken_kind_t;
#endif

/* Value type.  */
#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
typedef int YYSTYPE;
# define YYSTYPE_IS_TRIVIAL 1
# define YYSTYPE_IS_DECLARED 1
#endif

/* Location type.  */
#if ! defined YYLTYPE && ! defined YYLTYPE_IS_DECLARED
typedef struct YYLTYPE YYLTYPE;
struct YYLTYPE
{
  int first_line;
  int first_column;
  int last_line;
  int last_column;
};
# define YYLTYPE_IS_DECLARED 1
# define YYLTYPE_IS_TRIVIAL 1
#endif


extern YYSTYPE yylval;
extern YYLTYPE yylloc;
int yyparse (void);

#endif /* !YY_YY_EVIL_TAB_H_INCLUDED  */
