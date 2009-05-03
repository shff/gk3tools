/* A Bison parser, made by GNU Bison 2.3.  */

/* Skeleton interface for Bison's Yacc-like parsers in C

   Copyright (C) 1984, 1989, 1990, 2000, 2001, 2002, 2003, 2004, 2005, 2006
   Free Software Foundation, Inc.

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2, or (at your option)
   any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 51 Franklin Street, Fifth Floor,
   Boston, MA 02110-1301, USA.  */

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

/* Tokens.  */
#ifndef YYTOKENTYPE
# define YYTOKENTYPE
   /* Put the tokens into the symbol table, so that GDB and other debuggers
      know about them.  */
   enum yytokentype {
     IDENTIFIER = 258,
     LOCALIDENTIFIER = 259,
     INTEGER = 260,
     FLOAT = 261,
     STRING = 262,
     INTSYM = 263,
     FLOATSYM = 264,
     STRINGSYM = 265,
     CODE = 266,
     SYMBOLS = 267,
     SNIP = 268,
     WAIT = 269,
     RETURN = 270,
     IF = 271,
     ELSE = 272,
     GOTO = 273,
     COLON = 274,
     SEMICOLON = 275,
     DOLLAR = 276,
     LPAREN = 277,
     RPAREN = 278,
     LBRACE = 279,
     RBRACE = 280,
     QUOTE = 281,
     COMMA = 282,
     EQUALS = 283,
     NOTEQUAL = 284,
     BECOMES = 285,
     PLUS = 286,
     MINUS = 287,
     TIMES = 288,
     DIVIDE = 289,
     LESSTHAN = 290,
     GREATERTHAN = 291,
     NOT = 292,
     OR = 293,
     AND = 294
   };
#endif
/* Tokens.  */
#define IDENTIFIER 258
#define LOCALIDENTIFIER 259
#define INTEGER 260
#define FLOAT 261
#define STRING 262
#define INTSYM 263
#define FLOATSYM 264
#define STRINGSYM 265
#define CODE 266
#define SYMBOLS 267
#define SNIP 268
#define WAIT 269
#define RETURN 270
#define IF 271
#define ELSE 272
#define GOTO 273
#define COLON 274
#define SEMICOLON 275
#define DOLLAR 276
#define LPAREN 277
#define RPAREN 278
#define LBRACE 279
#define RBRACE 280
#define QUOTE 281
#define COMMA 282
#define EQUALS 283
#define NOTEQUAL 284
#define BECOMES 285
#define PLUS 286
#define MINUS 287
#define TIMES 288
#define DIVIDE 289
#define LESSTHAN 290
#define GREATERTHAN 291
#define NOT 292
#define OR 293
#define AND 294




#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
typedef int YYSTYPE;
# define yystype YYSTYPE /* obsolescent; will be withdrawn */
# define YYSTYPE_IS_DECLARED 1
# define YYSTYPE_IS_TRIVIAL 1
#endif

extern YYSTYPE yylval;

