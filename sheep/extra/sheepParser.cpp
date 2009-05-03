/* A Bison parser, made by GNU Bison 2.3.  */

/* Skeleton implementation for Bison's Yacc-like parsers in C

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

/* C LALR(1) parser skeleton written by Richard Stallman, by
   simplifying the original so-called "semantic" parser.  */

/* All symbols defined below should begin with yy or YY, to avoid
   infringing on user name space.  This should be done even for local
   variables, as they might otherwise be expanded by user macros.
   There are some unavoidable exceptions within include files to
   define necessary library symbols; they are noted "INFRINGES ON
   USER NAME SPACE" below.  */

/* Identify Bison output.  */
#define YYBISON 1

/* Bison version.  */
#define YYBISON_VERSION "2.3"

/* Skeleton name.  */
#define YYSKELETON_NAME "yacc.c"

/* Pure parsers.  */
#define YYPURE 0

/* Using locations.  */
#define YYLSP_NEEDED 0



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




/* Copy the first part of user declarations.  */
#line 1 "sheepParser.y"


#include <stdio.h>
#include <string.h>
#include "symbols.h"
#include "sheepCodeTree.h"
#define YYSTYPE SheepCodeTreeNode*

#define TRUE 1
#define FALSE 0

extern SheepCodeTreeNode* g_codeTreeRoot;
extern int currentLine;

void yyerror(const char* str)
{
	printf("error at line %d: %s\n", currentLine, str);
}

char* removeQuotes(char* str)
{
	if (str[0] == '"')
	{
		str[strlen(str)-1] = 0;
		return str+1;
	}
	else // assume "|< >|" strings
	{
		str[strlen(str)-2] = 0;
		return str+2;
	}
}



/* Enabling traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif

/* Enabling verbose error messages.  */
#ifdef YYERROR_VERBOSE
# undef YYERROR_VERBOSE
# define YYERROR_VERBOSE 1
#else
# define YYERROR_VERBOSE 0
#endif

/* Enabling the token table.  */
#ifndef YYTOKEN_TABLE
# define YYTOKEN_TABLE 0
#endif

#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
typedef int YYSTYPE;
# define yystype YYSTYPE /* obsolescent; will be withdrawn */
# define YYSTYPE_IS_DECLARED 1
# define YYSTYPE_IS_TRIVIAL 1
#endif



/* Copy the second part of user declarations.  */


/* Line 216 of yacc.c.  */
#line 219 "sheepParser.cpp"

#ifdef short
# undef short
#endif

#ifdef YYTYPE_UINT8
typedef YYTYPE_UINT8 yytype_uint8;
#else
typedef unsigned char yytype_uint8;
#endif

#ifdef YYTYPE_INT8
typedef YYTYPE_INT8 yytype_int8;
#elif (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
typedef signed char yytype_int8;
#else
typedef short int yytype_int8;
#endif

#ifdef YYTYPE_UINT16
typedef YYTYPE_UINT16 yytype_uint16;
#else
typedef unsigned short int yytype_uint16;
#endif

#ifdef YYTYPE_INT16
typedef YYTYPE_INT16 yytype_int16;
#else
typedef short int yytype_int16;
#endif

#ifndef YYSIZE_T
# ifdef __SIZE_TYPE__
#  define YYSIZE_T __SIZE_TYPE__
# elif defined size_t
#  define YYSIZE_T size_t
# elif ! defined YYSIZE_T && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
#  include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  define YYSIZE_T size_t
# else
#  define YYSIZE_T unsigned int
# endif
#endif

#define YYSIZE_MAXIMUM ((YYSIZE_T) -1)

#ifndef YY_
# if YYENABLE_NLS
#  if ENABLE_NLS
#   include <libintl.h> /* INFRINGES ON USER NAME SPACE */
#   define YY_(msgid) dgettext ("bison-runtime", msgid)
#  endif
# endif
# ifndef YY_
#  define YY_(msgid) msgid
# endif
#endif

/* Suppress unused-variable warnings by "using" E.  */
#if ! defined lint || defined __GNUC__
# define YYUSE(e) ((void) (e))
#else
# define YYUSE(e) /* empty */
#endif

/* Identity function, used to suppress warnings about constant conditions.  */
#ifndef lint
# define YYID(n) (n)
#else
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static int
YYID (int i)
#else
static int
YYID (i)
    int i;
#endif
{
  return i;
}
#endif

#if ! defined yyoverflow || YYERROR_VERBOSE

/* The parser invokes alloca or malloc; define the necessary symbols.  */

# ifdef YYSTACK_USE_ALLOCA
#  if YYSTACK_USE_ALLOCA
#   ifdef __GNUC__
#    define YYSTACK_ALLOC __builtin_alloca
#   elif defined __BUILTIN_VA_ARG_INCR
#    include <alloca.h> /* INFRINGES ON USER NAME SPACE */
#   elif defined _AIX
#    define YYSTACK_ALLOC __alloca
#   elif defined _MSC_VER
#    include <malloc.h> /* INFRINGES ON USER NAME SPACE */
#    define alloca _alloca
#   else
#    define YYSTACK_ALLOC alloca
#    if ! defined _ALLOCA_H && ! defined _STDLIB_H && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
#     include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#     ifndef _STDLIB_H
#      define _STDLIB_H 1
#     endif
#    endif
#   endif
#  endif
# endif

# ifdef YYSTACK_ALLOC
   /* Pacify GCC's `empty if-body' warning.  */
#  define YYSTACK_FREE(Ptr) do { /* empty */; } while (YYID (0))
#  ifndef YYSTACK_ALLOC_MAXIMUM
    /* The OS might guarantee only one guard page at the bottom of the stack,
       and a page size can be as small as 4096 bytes.  So we cannot safely
       invoke alloca (N) if N exceeds 4096.  Use a slightly smaller number
       to allow for a few compiler-allocated temporary stack slots.  */
#   define YYSTACK_ALLOC_MAXIMUM 4032 /* reasonable circa 2006 */
#  endif
# else
#  define YYSTACK_ALLOC YYMALLOC
#  define YYSTACK_FREE YYFREE
#  ifndef YYSTACK_ALLOC_MAXIMUM
#   define YYSTACK_ALLOC_MAXIMUM YYSIZE_MAXIMUM
#  endif
#  if (defined __cplusplus && ! defined _STDLIB_H \
       && ! ((defined YYMALLOC || defined malloc) \
	     && (defined YYFREE || defined free)))
#   include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#   ifndef _STDLIB_H
#    define _STDLIB_H 1
#   endif
#  endif
#  ifndef YYMALLOC
#   define YYMALLOC malloc
#   if ! defined malloc && ! defined _STDLIB_H && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
void *malloc (YYSIZE_T); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
#  ifndef YYFREE
#   define YYFREE free
#   if ! defined free && ! defined _STDLIB_H && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
void free (void *); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
# endif
#endif /* ! defined yyoverflow || YYERROR_VERBOSE */


#if (! defined yyoverflow \
     && (! defined __cplusplus \
	 || (defined YYSTYPE_IS_TRIVIAL && YYSTYPE_IS_TRIVIAL)))

/* A type that is properly aligned for any stack member.  */
union yyalloc
{
  yytype_int16 yyss;
  YYSTYPE yyvs;
  };

/* The size of the maximum gap between one aligned stack and the next.  */
# define YYSTACK_GAP_MAXIMUM (sizeof (union yyalloc) - 1)

/* The size of an array large to enough to hold all stacks, each with
   N elements.  */
# define YYSTACK_BYTES(N) \
     ((N) * (sizeof (yytype_int16) + sizeof (YYSTYPE)) \
      + YYSTACK_GAP_MAXIMUM)

/* Copy COUNT objects from FROM to TO.  The source and destination do
   not overlap.  */
# ifndef YYCOPY
#  if defined __GNUC__ && 1 < __GNUC__
#   define YYCOPY(To, From, Count) \
      __builtin_memcpy (To, From, (Count) * sizeof (*(From)))
#  else
#   define YYCOPY(To, From, Count)		\
      do					\
	{					\
	  YYSIZE_T yyi;				\
	  for (yyi = 0; yyi < (Count); yyi++)	\
	    (To)[yyi] = (From)[yyi];		\
	}					\
      while (YYID (0))
#  endif
# endif

/* Relocate STACK from its old location to the new one.  The
   local variables YYSIZE and YYSTACKSIZE give the old and new number of
   elements in the stack, and YYPTR gives the new location of the
   stack.  Advance YYPTR to a properly aligned location for the next
   stack.  */
# define YYSTACK_RELOCATE(Stack)					\
    do									\
      {									\
	YYSIZE_T yynewbytes;						\
	YYCOPY (&yyptr->Stack, Stack, yysize);				\
	Stack = &yyptr->Stack;						\
	yynewbytes = yystacksize * sizeof (*Stack) + YYSTACK_GAP_MAXIMUM; \
	yyptr += yynewbytes / sizeof (*yyptr);				\
      }									\
    while (YYID (0))

#endif

/* YYFINAL -- State number of the termination state.  */
#define YYFINAL  11
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   324

/* YYNTOKENS -- Number of terminals.  */
#define YYNTOKENS  40
/* YYNNTS -- Number of nonterminals.  */
#define YYNNTS  22
/* YYNRULES -- Number of rules.  */
#define YYNRULES  71
/* YYNRULES -- Number of states.  */
#define YYNSTATES  136

/* YYTRANSLATE(YYLEX) -- Bison symbol number corresponding to YYLEX.  */
#define YYUNDEFTOK  2
#define YYMAXUTOK   294

#define YYTRANSLATE(YYX)						\
  ((unsigned int) (YYX) <= YYMAXUTOK ? yytranslate[YYX] : YYUNDEFTOK)

/* YYTRANSLATE[YYLEX] -- Bison symbol number corresponding to YYLEX.  */
static const yytype_uint8 yytranslate[] =
{
       0,     2,     2,     2,     2,     2,     2,     2,     2,     2,
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
      35,    36,    37,    38,    39
};

#if YYDEBUG
/* YYPRHS[YYN] -- Index of the first RHS symbol of rule number YYN in
   YYRHS.  */
static const yytype_uint8 yyprhs[] =
{
       0,     0,     3,     4,     7,     9,    11,    13,    17,    22,
      24,    27,    31,    37,    41,    47,    51,    57,    61,    66,
      70,    75,    77,    80,    86,    93,    95,    98,   100,   103,
     107,   110,   113,   115,   119,   122,   126,   132,   134,   136,
     142,   150,   152,   156,   164,   166,   168,   170,   172,   174,
     178,   183,   185,   189,   191,   195,   197,   199,   201,   205,
     208,   212,   216,   220,   224,   228,   232,   237,   242,   246,
     250,   254
};

/* YYRHS -- A `-1'-separated list of the rules' RHS.  */
static const yytype_int8 yyrhs[] =
{
      41,     0,    -1,    -1,    42,    46,    -1,    42,    -1,    46,
      -1,    45,    -1,    12,    24,    25,    -1,    12,    24,    43,
      25,    -1,    44,    -1,    43,    44,    -1,     8,    55,    20,
      -1,     8,    55,    30,    57,    20,    -1,     9,    55,    20,
      -1,     9,    55,    30,    57,    20,    -1,    10,    55,    20,
      -1,    10,    55,    30,    57,    20,    -1,    13,    24,    25,
      -1,    13,    24,    61,    25,    -1,    11,    24,    25,    -1,
      11,    24,    47,    25,    -1,    48,    -1,    47,    48,    -1,
      55,    22,    23,    24,    25,    -1,    55,    22,    23,    24,
      49,    25,    -1,    52,    -1,    49,    52,    -1,    20,    -1,
      55,    19,    -1,    18,    55,    20,    -1,    61,    20,    -1,
      15,    20,    -1,    51,    -1,    55,    30,    61,    -1,    14,
      20,    -1,    14,    58,    20,    -1,    14,    24,    59,    20,
      25,    -1,    53,    -1,    54,    -1,    16,    22,    61,    23,
      52,    -1,    16,    22,    61,    23,    54,    17,    53,    -1,
      50,    -1,    24,    49,    25,    -1,    16,    22,    61,    23,
      54,    17,    54,    -1,     4,    -1,     3,    -1,     5,    -1,
       6,    -1,     7,    -1,    56,    22,    23,    -1,    56,    22,
      60,    23,    -1,    58,    -1,    59,    20,    58,    -1,    61,
      -1,    60,    27,    61,    -1,    57,    -1,    58,    -1,    55,
      -1,    22,    61,    23,    -1,    37,    61,    -1,    61,    31,
      61,    -1,    61,    32,    61,    -1,    61,    33,    61,    -1,
      61,    34,    61,    -1,    61,    35,    61,    -1,    61,    36,
      61,    -1,    61,    35,    30,    61,    -1,    61,    36,    30,
      61,    -1,    61,    28,    61,    -1,    61,    29,    61,    -1,
      61,    38,    61,    -1,    61,    39,    61,    -1
};

/* YYRLINE[YYN] -- source line where rule number YYN was defined.  */
static const yytype_uint8 yyrline[] =
{
       0,    53,    53,    55,    56,    57,    58,    62,    63,    68,
      69,    73,    74,    75,    76,    77,    78,    82,    83,    86,
      87,    91,    92,    96,    97,   102,   103,   107,   108,   109,
     110,   111,   112,   113,   117,   118,   119,   125,   126,   130,
     131,   135,   136,   137,   141,   145,   149,   150,   151,   155,
     156,   160,   161,   165,   166,   170,   171,   172,   173,   174,
     175,   176,   177,   178,   179,   180,   181,   182,   183,   184,
     185,   186
};
#endif

#if YYDEBUG || YYERROR_VERBOSE || YYTOKEN_TABLE
/* YYTNAME[SYMBOL-NUM] -- String name of the symbol SYMBOL-NUM.
   First, the terminals, then, starting at YYNTOKENS, nonterminals.  */
static const char *const yytname[] =
{
  "$end", "error", "$undefined", "IDENTIFIER", "LOCALIDENTIFIER",
  "INTEGER", "FLOAT", "STRING", "INTSYM", "FLOATSYM", "STRINGSYM", "CODE",
  "SYMBOLS", "SNIP", "WAIT", "RETURN", "IF", "ELSE", "GOTO", "COLON",
  "SEMICOLON", "DOLLAR", "LPAREN", "RPAREN", "LBRACE", "RBRACE", "QUOTE",
  "COMMA", "EQUALS", "NOTEQUAL", "BECOMES", "PLUS", "MINUS", "TIMES",
  "DIVIDE", "LESSTHAN", "GREATERTHAN", "NOT", "OR", "AND", "$accept",
  "sheep", "symbol_section", "symbol_list", "symbol_declaration",
  "snippet_section", "code_section", "function_list", "function",
  "statement_list", "simple_statement", "wait_statement", "statement",
  "open_statement", "closed_statement", "local_identifier",
  "global_identifier", "constant", "global_function_call",
  "global_function_call_list", "parameter_list", "expr", 0
};
#endif

# ifdef YYPRINT
/* YYTOKNUM[YYLEX-NUM] -- Internal token number corresponding to
   token YYLEX-NUM.  */
static const yytype_uint16 yytoknum[] =
{
       0,   256,   257,   258,   259,   260,   261,   262,   263,   264,
     265,   266,   267,   268,   269,   270,   271,   272,   273,   274,
     275,   276,   277,   278,   279,   280,   281,   282,   283,   284,
     285,   286,   287,   288,   289,   290,   291,   292,   293,   294
};
# endif

/* YYR1[YYN] -- Symbol number of symbol that rule YYN derives.  */
static const yytype_uint8 yyr1[] =
{
       0,    40,    41,    41,    41,    41,    41,    42,    42,    43,
      43,    44,    44,    44,    44,    44,    44,    45,    45,    46,
      46,    47,    47,    48,    48,    49,    49,    50,    50,    50,
      50,    50,    50,    50,    51,    51,    51,    52,    52,    53,
      53,    54,    54,    54,    55,    56,    57,    57,    57,    58,
      58,    59,    59,    60,    60,    61,    61,    61,    61,    61,
      61,    61,    61,    61,    61,    61,    61,    61,    61,    61,
      61,    61
};

/* YYR2[YYN] -- Number of symbols composing right hand side of rule YYN.  */
static const yytype_uint8 yyr2[] =
{
       0,     2,     0,     2,     1,     1,     1,     3,     4,     1,
       2,     3,     5,     3,     5,     3,     5,     3,     4,     3,
       4,     1,     2,     5,     6,     1,     2,     1,     2,     3,
       2,     2,     1,     3,     2,     3,     5,     1,     1,     5,
       7,     1,     3,     7,     1,     1,     1,     1,     1,     3,
       4,     1,     3,     1,     3,     1,     1,     1,     3,     2,
       3,     3,     3,     3,     3,     3,     4,     4,     3,     3,
       3,     3
};

/* YYDEFACT[STATE-NAME] -- Default rule to reduce with in state
   STATE-NUM when YYTABLE doesn't specify something else to do.  Zero
   means the default is an error.  */
static const yytype_uint8 yydefact[] =
{
       2,     0,     0,     0,     0,     4,     6,     5,     0,     0,
       0,     1,     3,    44,    19,     0,    21,     0,     0,     0,
       0,     7,     0,     9,    45,    46,    47,    48,     0,    17,
       0,    57,     0,    55,    56,     0,    20,    22,     0,     0,
       0,     0,     8,    10,     0,    59,     0,    18,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    11,
       0,    13,     0,    15,     0,    58,    49,     0,    53,    68,
      69,    60,    61,    62,    63,     0,    64,     0,    65,    70,
      71,     0,     0,     0,     0,    50,     0,    66,    67,     0,
       0,     0,     0,    27,     0,    23,     0,    41,    32,    25,
      37,    38,    57,     0,    12,    14,    16,    54,    34,     0,
       0,    31,     0,     0,     0,    24,    26,    28,     0,    30,
      51,     0,    35,     0,    29,    42,    33,     0,     0,    36,
      52,    39,    38,     0,    40,    43
};

/* YYDEFGOTO[NTERM-NUM].  */
static const yytype_int8 yydefgoto[] =
{
      -1,     4,     5,    22,    23,     6,     7,    15,    16,    96,
      97,    98,    99,   100,   101,    31,    32,    33,    34,   121,
      67,   103
};

/* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
   STATE-NUM.  */
#define YYPACT_NINF -102
static const yytype_int16 yypact[] =
{
      83,   -20,    11,    12,    38,    39,  -102,  -102,     4,     6,
      94,  -102,  -102,  -102,  -102,     5,  -102,    27,    54,    54,
      54,  -102,    44,  -102,  -102,  -102,  -102,  -102,   202,  -102,
     202,  -102,    34,  -102,  -102,   249,  -102,  -102,    41,    -2,
      35,    37,  -102,  -102,   223,  -102,   161,  -102,   202,   202,
     202,   202,   202,   202,   182,   196,   202,   202,    78,  -102,
     135,  -102,   135,  -102,   135,  -102,  -102,    52,   261,   288,
     288,   -10,   -10,  -102,  -102,   202,    58,   202,    58,   273,
     282,    56,    62,    84,    85,  -102,   202,   261,   261,     2,
      87,    86,    54,  -102,   156,  -102,   108,  -102,  -102,  -102,
    -102,  -102,   -13,   209,  -102,  -102,  -102,   261,  -102,   107,
      97,  -102,   202,    98,   132,  -102,  -102,  -102,   202,  -102,
    -102,   101,  -102,   237,  -102,  -102,   261,     0,   156,  -102,
    -102,  -102,   110,   156,  -102,  -102
};

/* YYPGOTO[NTERM-NUM].  */
static const yytype_int16 yypgoto[] =
{
    -102,  -102,  -102,  -102,   112,  -102,   124,  -102,   128,    50,
    -102,  -102,   -94,    16,  -101,    -8,  -102,    23,   -76,  -102,
    -102,    -9
};

/* YYTABLE[YYPACT[STATE-NUM]].  What to do in state STATE-NUM.  If
   positive, shift that token.  If negative, reduce the rule which
   number is the opposite.  If zero, do what YYDEFACT says.
   If YYTABLE_NINF, syntax error.  */
#define YYTABLE_NINF -1
static const yytype_uint8 yytable[] =
{
      17,    35,   116,    24,     8,    24,   117,    17,    13,    13,
      39,    40,    41,   110,    18,    19,    20,   118,    59,    44,
     116,    45,   108,    52,    53,   129,   109,   132,    60,    14,
      36,    21,   135,   120,   131,     9,    10,    68,    11,    69,
      70,    71,    72,    73,    74,    76,    78,    79,    80,    38,
       1,   130,    18,    19,    20,    61,    46,    63,    13,    24,
      13,    25,    26,    27,    58,    62,    87,    64,    88,    42,
      89,    90,    91,   102,    92,    85,    93,   107,    28,    86,
      94,    95,   104,    82,   113,    83,   102,    84,   102,    50,
      51,    52,    53,    30,     1,     2,     3,    24,    13,    25,
      26,    27,    81,   123,   105,   106,   102,   111,   112,   126,
      24,    24,    13,    25,    26,    27,    28,   122,   124,    29,
     102,   127,    89,    90,    91,   102,    92,   133,    93,    12,
      28,    30,    94,   115,    43,    24,    13,    25,    26,    27,
      25,    26,    27,    37,   114,    30,    89,    90,    91,   134,
      92,     0,    93,     0,    28,     0,    94,   125,     0,    24,
      13,    25,    26,    27,    24,    13,    25,    26,    27,    30,
      89,    90,    91,     0,    92,     0,    93,     0,    28,     0,
      94,     0,     0,    28,    66,    24,    13,    25,    26,    27,
       0,     0,     0,    30,     0,     0,     0,     0,    30,    24,
      13,    25,    26,    27,    28,    24,    13,    25,    26,    27,
       0,     0,    75,     0,     0,     0,     0,     0,    28,    30,
       0,     0,     0,     0,    28,     0,    77,     0,     0,   119,
       0,     0,     0,    30,     0,     0,     0,    48,    49,    30,
      50,    51,    52,    53,    54,    55,    65,    56,    57,     0,
       0,    48,    49,     0,    50,    51,    52,    53,    54,    55,
     128,    56,    57,     0,     0,    48,    49,     0,    50,    51,
      52,    53,    54,    55,    47,    56,    57,    48,    49,     0,
      50,    51,    52,    53,    54,    55,     0,    56,    57,    48,
      49,     0,    50,    51,    52,    53,    54,    55,     0,    56,
      57,    48,    49,     0,    50,    51,    52,    53,    54,    55,
      48,    49,    57,    50,    51,    52,    53,    54,    55,    50,
      51,    52,    53,    54,    55
};

static const yytype_int16 yycheck[] =
{
       8,    10,    96,     3,    24,     3,    19,    15,     4,     4,
      18,    19,    20,    89,     8,     9,    10,    30,    20,    28,
     114,    30,    20,    33,    34,    25,    24,   128,    30,    25,
      25,    25,   133,   109,   128,    24,    24,    46,     0,    48,
      49,    50,    51,    52,    53,    54,    55,    56,    57,    22,
      11,   127,     8,     9,    10,    20,    22,    20,     4,     3,
       4,     5,     6,     7,    23,    30,    75,    30,    77,    25,
      14,    15,    16,    81,    18,    23,    20,    86,    22,    27,
      24,    25,    20,    60,    92,    62,    94,    64,    96,    31,
      32,    33,    34,    37,    11,    12,    13,     3,     4,     5,
       6,     7,    24,   112,    20,    20,   114,    20,    22,   118,
       3,     3,     4,     5,     6,     7,    22,    20,    20,    25,
     128,    20,    14,    15,    16,   133,    18,    17,    20,     5,
      22,    37,    24,    25,    22,     3,     4,     5,     6,     7,
       5,     6,     7,    15,    94,    37,    14,    15,    16,   133,
      18,    -1,    20,    -1,    22,    -1,    24,    25,    -1,     3,
       4,     5,     6,     7,     3,     4,     5,     6,     7,    37,
      14,    15,    16,    -1,    18,    -1,    20,    -1,    22,    -1,
      24,    -1,    -1,    22,    23,     3,     4,     5,     6,     7,
      -1,    -1,    -1,    37,    -1,    -1,    -1,    -1,    37,     3,
       4,     5,     6,     7,    22,     3,     4,     5,     6,     7,
      -1,    -1,    30,    -1,    -1,    -1,    -1,    -1,    22,    37,
      -1,    -1,    -1,    -1,    22,    -1,    30,    -1,    -1,    20,
      -1,    -1,    -1,    37,    -1,    -1,    -1,    28,    29,    37,
      31,    32,    33,    34,    35,    36,    23,    38,    39,    -1,
      -1,    28,    29,    -1,    31,    32,    33,    34,    35,    36,
      23,    38,    39,    -1,    -1,    28,    29,    -1,    31,    32,
      33,    34,    35,    36,    25,    38,    39,    28,    29,    -1,
      31,    32,    33,    34,    35,    36,    -1,    38,    39,    28,
      29,    -1,    31,    32,    33,    34,    35,    36,    -1,    38,
      39,    28,    29,    -1,    31,    32,    33,    34,    35,    36,
      28,    29,    39,    31,    32,    33,    34,    35,    36,    31,
      32,    33,    34,    35,    36
};

/* YYSTOS[STATE-NUM] -- The (internal number of the) accessing
   symbol of state STATE-NUM.  */
static const yytype_uint8 yystos[] =
{
       0,    11,    12,    13,    41,    42,    45,    46,    24,    24,
      24,     0,    46,     4,    25,    47,    48,    55,     8,     9,
      10,    25,    43,    44,     3,     5,     6,     7,    22,    25,
      37,    55,    56,    57,    58,    61,    25,    48,    22,    55,
      55,    55,    25,    44,    61,    61,    22,    25,    28,    29,
      31,    32,    33,    34,    35,    36,    38,    39,    23,    20,
      30,    20,    30,    20,    30,    23,    23,    60,    61,    61,
      61,    61,    61,    61,    61,    30,    61,    30,    61,    61,
      61,    24,    57,    57,    57,    23,    27,    61,    61,    14,
      15,    16,    18,    20,    24,    25,    49,    50,    51,    52,
      53,    54,    55,    61,    20,    20,    20,    61,    20,    24,
      58,    20,    22,    55,    49,    25,    52,    19,    30,    20,
      58,    59,    20,    61,    20,    25,    61,    20,    23,    25,
      58,    52,    54,    17,    53,    54
};

#define yyerrok		(yyerrstatus = 0)
#define yyclearin	(yychar = YYEMPTY)
#define YYEMPTY		(-2)
#define YYEOF		0

#define YYACCEPT	goto yyacceptlab
#define YYABORT		goto yyabortlab
#define YYERROR		goto yyerrorlab


/* Like YYERROR except do call yyerror.  This remains here temporarily
   to ease the transition to the new meaning of YYERROR, for GCC.
   Once GCC version 2 has supplanted version 1, this can go.  */

#define YYFAIL		goto yyerrlab

#define YYRECOVERING()  (!!yyerrstatus)

#define YYBACKUP(Token, Value)					\
do								\
  if (yychar == YYEMPTY && yylen == 1)				\
    {								\
      yychar = (Token);						\
      yylval = (Value);						\
      yytoken = YYTRANSLATE (yychar);				\
      YYPOPSTACK (1);						\
      goto yybackup;						\
    }								\
  else								\
    {								\
      yyerror (YY_("syntax error: cannot back up")); \
      YYERROR;							\
    }								\
while (YYID (0))


#define YYTERROR	1
#define YYERRCODE	256


/* YYLLOC_DEFAULT -- Set CURRENT to span from RHS[1] to RHS[N].
   If N is 0, then set CURRENT to the empty location which ends
   the previous symbol: RHS[0] (always defined).  */

#define YYRHSLOC(Rhs, K) ((Rhs)[K])
#ifndef YYLLOC_DEFAULT
# define YYLLOC_DEFAULT(Current, Rhs, N)				\
    do									\
      if (YYID (N))                                                    \
	{								\
	  (Current).first_line   = YYRHSLOC (Rhs, 1).first_line;	\
	  (Current).first_column = YYRHSLOC (Rhs, 1).first_column;	\
	  (Current).last_line    = YYRHSLOC (Rhs, N).last_line;		\
	  (Current).last_column  = YYRHSLOC (Rhs, N).last_column;	\
	}								\
      else								\
	{								\
	  (Current).first_line   = (Current).last_line   =		\
	    YYRHSLOC (Rhs, 0).last_line;				\
	  (Current).first_column = (Current).last_column =		\
	    YYRHSLOC (Rhs, 0).last_column;				\
	}								\
    while (YYID (0))
#endif


/* YY_LOCATION_PRINT -- Print the location on the stream.
   This macro was not mandated originally: define only if we know
   we won't break user code: when these are the locations we know.  */

#ifndef YY_LOCATION_PRINT
# if YYLTYPE_IS_TRIVIAL
#  define YY_LOCATION_PRINT(File, Loc)			\
     fprintf (File, "%d.%d-%d.%d",			\
	      (Loc).first_line, (Loc).first_column,	\
	      (Loc).last_line,  (Loc).last_column)
# else
#  define YY_LOCATION_PRINT(File, Loc) ((void) 0)
# endif
#endif


/* YYLEX -- calling `yylex' with the right arguments.  */

#ifdef YYLEX_PARAM
# define YYLEX yylex (YYLEX_PARAM)
#else
# define YYLEX yylex ()
#endif

/* Enable debugging if requested.  */
#if YYDEBUG

# ifndef YYFPRINTF
#  include <stdio.h> /* INFRINGES ON USER NAME SPACE */
#  define YYFPRINTF fprintf
# endif

# define YYDPRINTF(Args)			\
do {						\
  if (yydebug)					\
    YYFPRINTF Args;				\
} while (YYID (0))

# define YY_SYMBOL_PRINT(Title, Type, Value, Location)			  \
do {									  \
  if (yydebug)								  \
    {									  \
      YYFPRINTF (stderr, "%s ", Title);					  \
      yy_symbol_print (stderr,						  \
		  Type, Value); \
      YYFPRINTF (stderr, "\n");						  \
    }									  \
} while (YYID (0))


/*--------------------------------.
| Print this symbol on YYOUTPUT.  |
`--------------------------------*/

/*ARGSUSED*/
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_symbol_value_print (FILE *yyoutput, int yytype, YYSTYPE const * const yyvaluep)
#else
static void
yy_symbol_value_print (yyoutput, yytype, yyvaluep)
    FILE *yyoutput;
    int yytype;
    YYSTYPE const * const yyvaluep;
#endif
{
  if (!yyvaluep)
    return;
# ifdef YYPRINT
  if (yytype < YYNTOKENS)
    YYPRINT (yyoutput, yytoknum[yytype], *yyvaluep);
# else
  YYUSE (yyoutput);
# endif
  switch (yytype)
    {
      default:
	break;
    }
}


/*--------------------------------.
| Print this symbol on YYOUTPUT.  |
`--------------------------------*/

#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_symbol_print (FILE *yyoutput, int yytype, YYSTYPE const * const yyvaluep)
#else
static void
yy_symbol_print (yyoutput, yytype, yyvaluep)
    FILE *yyoutput;
    int yytype;
    YYSTYPE const * const yyvaluep;
#endif
{
  if (yytype < YYNTOKENS)
    YYFPRINTF (yyoutput, "token %s (", yytname[yytype]);
  else
    YYFPRINTF (yyoutput, "nterm %s (", yytname[yytype]);

  yy_symbol_value_print (yyoutput, yytype, yyvaluep);
  YYFPRINTF (yyoutput, ")");
}

/*------------------------------------------------------------------.
| yy_stack_print -- Print the state stack from its BOTTOM up to its |
| TOP (included).                                                   |
`------------------------------------------------------------------*/

#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_stack_print (yytype_int16 *bottom, yytype_int16 *top)
#else
static void
yy_stack_print (bottom, top)
    yytype_int16 *bottom;
    yytype_int16 *top;
#endif
{
  YYFPRINTF (stderr, "Stack now");
  for (; bottom <= top; ++bottom)
    YYFPRINTF (stderr, " %d", *bottom);
  YYFPRINTF (stderr, "\n");
}

# define YY_STACK_PRINT(Bottom, Top)				\
do {								\
  if (yydebug)							\
    yy_stack_print ((Bottom), (Top));				\
} while (YYID (0))


/*------------------------------------------------.
| Report that the YYRULE is going to be reduced.  |
`------------------------------------------------*/

#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_reduce_print (YYSTYPE *yyvsp, int yyrule)
#else
static void
yy_reduce_print (yyvsp, yyrule)
    YYSTYPE *yyvsp;
    int yyrule;
#endif
{
  int yynrhs = yyr2[yyrule];
  int yyi;
  unsigned long int yylno = yyrline[yyrule];
  YYFPRINTF (stderr, "Reducing stack by rule %d (line %lu):\n",
	     yyrule - 1, yylno);
  /* The symbols being reduced.  */
  for (yyi = 0; yyi < yynrhs; yyi++)
    {
      fprintf (stderr, "   $%d = ", yyi + 1);
      yy_symbol_print (stderr, yyrhs[yyprhs[yyrule] + yyi],
		       &(yyvsp[(yyi + 1) - (yynrhs)])
		       		       );
      fprintf (stderr, "\n");
    }
}

# define YY_REDUCE_PRINT(Rule)		\
do {					\
  if (yydebug)				\
    yy_reduce_print (yyvsp, Rule); \
} while (YYID (0))

/* Nonzero means print parse trace.  It is left uninitialized so that
   multiple parsers can coexist.  */
int yydebug;
#else /* !YYDEBUG */
# define YYDPRINTF(Args)
# define YY_SYMBOL_PRINT(Title, Type, Value, Location)
# define YY_STACK_PRINT(Bottom, Top)
# define YY_REDUCE_PRINT(Rule)
#endif /* !YYDEBUG */


/* YYINITDEPTH -- initial size of the parser's stacks.  */
#ifndef	YYINITDEPTH
# define YYINITDEPTH 200
#endif

/* YYMAXDEPTH -- maximum size the stacks can grow to (effective only
   if the built-in stack extension method is used).

   Do not make this value too large; the results are undefined if
   YYSTACK_ALLOC_MAXIMUM < YYSTACK_BYTES (YYMAXDEPTH)
   evaluated with infinite-precision integer arithmetic.  */

#ifndef YYMAXDEPTH
# define YYMAXDEPTH 10000
#endif



#if YYERROR_VERBOSE

# ifndef yystrlen
#  if defined __GLIBC__ && defined _STRING_H
#   define yystrlen strlen
#  else
/* Return the length of YYSTR.  */
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static YYSIZE_T
yystrlen (const char *yystr)
#else
static YYSIZE_T
yystrlen (yystr)
    const char *yystr;
#endif
{
  YYSIZE_T yylen;
  for (yylen = 0; yystr[yylen]; yylen++)
    continue;
  return yylen;
}
#  endif
# endif

# ifndef yystpcpy
#  if defined __GLIBC__ && defined _STRING_H && defined _GNU_SOURCE
#   define yystpcpy stpcpy
#  else
/* Copy YYSRC to YYDEST, returning the address of the terminating '\0' in
   YYDEST.  */
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static char *
yystpcpy (char *yydest, const char *yysrc)
#else
static char *
yystpcpy (yydest, yysrc)
    char *yydest;
    const char *yysrc;
#endif
{
  char *yyd = yydest;
  const char *yys = yysrc;

  while ((*yyd++ = *yys++) != '\0')
    continue;

  return yyd - 1;
}
#  endif
# endif

# ifndef yytnamerr
/* Copy to YYRES the contents of YYSTR after stripping away unnecessary
   quotes and backslashes, so that it's suitable for yyerror.  The
   heuristic is that double-quoting is unnecessary unless the string
   contains an apostrophe, a comma, or backslash (other than
   backslash-backslash).  YYSTR is taken from yytname.  If YYRES is
   null, do not copy; instead, return the length of what the result
   would have been.  */
static YYSIZE_T
yytnamerr (char *yyres, const char *yystr)
{
  if (*yystr == '"')
    {
      YYSIZE_T yyn = 0;
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
	    /* Fall through.  */
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

  if (! yyres)
    return yystrlen (yystr);

  return yystpcpy (yyres, yystr) - yyres;
}
# endif

/* Copy into YYRESULT an error message about the unexpected token
   YYCHAR while in state YYSTATE.  Return the number of bytes copied,
   including the terminating null byte.  If YYRESULT is null, do not
   copy anything; just return the number of bytes that would be
   copied.  As a special case, return 0 if an ordinary "syntax error"
   message will do.  Return YYSIZE_MAXIMUM if overflow occurs during
   size calculation.  */
static YYSIZE_T
yysyntax_error (char *yyresult, int yystate, int yychar)
{
  int yyn = yypact[yystate];

  if (! (YYPACT_NINF < yyn && yyn <= YYLAST))
    return 0;
  else
    {
      int yytype = YYTRANSLATE (yychar);
      YYSIZE_T yysize0 = yytnamerr (0, yytname[yytype]);
      YYSIZE_T yysize = yysize0;
      YYSIZE_T yysize1;
      int yysize_overflow = 0;
      enum { YYERROR_VERBOSE_ARGS_MAXIMUM = 5 };
      char const *yyarg[YYERROR_VERBOSE_ARGS_MAXIMUM];
      int yyx;

# if 0
      /* This is so xgettext sees the translatable formats that are
	 constructed on the fly.  */
      YY_("syntax error, unexpected %s");
      YY_("syntax error, unexpected %s, expecting %s");
      YY_("syntax error, unexpected %s, expecting %s or %s");
      YY_("syntax error, unexpected %s, expecting %s or %s or %s");
      YY_("syntax error, unexpected %s, expecting %s or %s or %s or %s");
# endif
      char *yyfmt;
      char const *yyf;
      static char const yyunexpected[] = "syntax error, unexpected %s";
      static char const yyexpecting[] = ", expecting %s";
      static char const yyor[] = " or %s";
      char yyformat[sizeof yyunexpected
		    + sizeof yyexpecting - 1
		    + ((YYERROR_VERBOSE_ARGS_MAXIMUM - 2)
		       * (sizeof yyor - 1))];
      char const *yyprefix = yyexpecting;

      /* Start YYX at -YYN if negative to avoid negative indexes in
	 YYCHECK.  */
      int yyxbegin = yyn < 0 ? -yyn : 0;

      /* Stay within bounds of both yycheck and yytname.  */
      int yychecklim = YYLAST - yyn + 1;
      int yyxend = yychecklim < YYNTOKENS ? yychecklim : YYNTOKENS;
      int yycount = 1;

      yyarg[0] = yytname[yytype];
      yyfmt = yystpcpy (yyformat, yyunexpected);

      for (yyx = yyxbegin; yyx < yyxend; ++yyx)
	if (yycheck[yyx + yyn] == yyx && yyx != YYTERROR)
	  {
	    if (yycount == YYERROR_VERBOSE_ARGS_MAXIMUM)
	      {
		yycount = 1;
		yysize = yysize0;
		yyformat[sizeof yyunexpected - 1] = '\0';
		break;
	      }
	    yyarg[yycount++] = yytname[yyx];
	    yysize1 = yysize + yytnamerr (0, yytname[yyx]);
	    yysize_overflow |= (yysize1 < yysize);
	    yysize = yysize1;
	    yyfmt = yystpcpy (yyfmt, yyprefix);
	    yyprefix = yyor;
	  }

      yyf = YY_(yyformat);
      yysize1 = yysize + yystrlen (yyf);
      yysize_overflow |= (yysize1 < yysize);
      yysize = yysize1;

      if (yysize_overflow)
	return YYSIZE_MAXIMUM;

      if (yyresult)
	{
	  /* Avoid sprintf, as that infringes on the user's name space.
	     Don't have undefined behavior even if the translation
	     produced a string with the wrong number of "%s"s.  */
	  char *yyp = yyresult;
	  int yyi = 0;
	  while ((*yyp = *yyf) != '\0')
	    {
	      if (*yyp == '%' && yyf[1] == 's' && yyi < yycount)
		{
		  yyp += yytnamerr (yyp, yyarg[yyi++]);
		  yyf += 2;
		}
	      else
		{
		  yyp++;
		  yyf++;
		}
	    }
	}
      return yysize;
    }
}
#endif /* YYERROR_VERBOSE */


/*-----------------------------------------------.
| Release the memory associated to this symbol.  |
`-----------------------------------------------*/

/*ARGSUSED*/
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yydestruct (const char *yymsg, int yytype, YYSTYPE *yyvaluep)
#else
static void
yydestruct (yymsg, yytype, yyvaluep)
    const char *yymsg;
    int yytype;
    YYSTYPE *yyvaluep;
#endif
{
  YYUSE (yyvaluep);

  if (!yymsg)
    yymsg = "Deleting";
  YY_SYMBOL_PRINT (yymsg, yytype, yyvaluep, yylocationp);

  switch (yytype)
    {

      default:
	break;
    }
}


/* Prevent warnings from -Wmissing-prototypes.  */

#ifdef YYPARSE_PARAM
#if defined __STDC__ || defined __cplusplus
int yyparse (void *YYPARSE_PARAM);
#else
int yyparse ();
#endif
#else /* ! YYPARSE_PARAM */
#if defined __STDC__ || defined __cplusplus
int yyparse (void);
#else
int yyparse ();
#endif
#endif /* ! YYPARSE_PARAM */



/* The look-ahead symbol.  */
int yychar;

/* The semantic value of the look-ahead symbol.  */
YYSTYPE yylval;

/* Number of syntax errors so far.  */
int yynerrs;



/*----------.
| yyparse.  |
`----------*/

#ifdef YYPARSE_PARAM
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
int
yyparse (void *YYPARSE_PARAM)
#else
int
yyparse (YYPARSE_PARAM)
    void *YYPARSE_PARAM;
#endif
#else /* ! YYPARSE_PARAM */
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
int
yyparse (void)
#else
int
yyparse ()

#endif
#endif
{
  
  int yystate;
  int yyn;
  int yyresult;
  /* Number of tokens to shift before error messages enabled.  */
  int yyerrstatus;
  /* Look-ahead token as an internal (translated) token number.  */
  int yytoken = 0;
#if YYERROR_VERBOSE
  /* Buffer for error messages, and its allocated size.  */
  char yymsgbuf[128];
  char *yymsg = yymsgbuf;
  YYSIZE_T yymsg_alloc = sizeof yymsgbuf;
#endif

  /* Three stacks and their tools:
     `yyss': related to states,
     `yyvs': related to semantic values,
     `yyls': related to locations.

     Refer to the stacks thru separate pointers, to allow yyoverflow
     to reallocate them elsewhere.  */

  /* The state stack.  */
  yytype_int16 yyssa[YYINITDEPTH];
  yytype_int16 *yyss = yyssa;
  yytype_int16 *yyssp;

  /* The semantic value stack.  */
  YYSTYPE yyvsa[YYINITDEPTH];
  YYSTYPE *yyvs = yyvsa;
  YYSTYPE *yyvsp;



#define YYPOPSTACK(N)   (yyvsp -= (N), yyssp -= (N))

  YYSIZE_T yystacksize = YYINITDEPTH;

  /* The variables used to return semantic value and location from the
     action routines.  */
  YYSTYPE yyval;


  /* The number of symbols on the RHS of the reduced rule.
     Keep to zero when no symbol should be popped.  */
  int yylen = 0;

  YYDPRINTF ((stderr, "Starting parse\n"));

  yystate = 0;
  yyerrstatus = 0;
  yynerrs = 0;
  yychar = YYEMPTY;		/* Cause a token to be read.  */

  /* Initialize stack pointers.
     Waste one element of value and location stack
     so that they stay on the same level as the state stack.
     The wasted elements are never initialized.  */

  yyssp = yyss;
  yyvsp = yyvs;

  goto yysetstate;

/*------------------------------------------------------------.
| yynewstate -- Push a new state, which is found in yystate.  |
`------------------------------------------------------------*/
 yynewstate:
  /* In all cases, when you get here, the value and location stacks
     have just been pushed.  So pushing a state here evens the stacks.  */
  yyssp++;

 yysetstate:
  *yyssp = yystate;

  if (yyss + yystacksize - 1 <= yyssp)
    {
      /* Get the current used size of the three stacks, in elements.  */
      YYSIZE_T yysize = yyssp - yyss + 1;

#ifdef yyoverflow
      {
	/* Give user a chance to reallocate the stack.  Use copies of
	   these so that the &'s don't force the real ones into
	   memory.  */
	YYSTYPE *yyvs1 = yyvs;
	yytype_int16 *yyss1 = yyss;


	/* Each stack pointer address is followed by the size of the
	   data in use in that stack, in bytes.  This used to be a
	   conditional around just the two extra args, but that might
	   be undefined if yyoverflow is a macro.  */
	yyoverflow (YY_("memory exhausted"),
		    &yyss1, yysize * sizeof (*yyssp),
		    &yyvs1, yysize * sizeof (*yyvsp),

		    &yystacksize);

	yyss = yyss1;
	yyvs = yyvs1;
      }
#else /* no yyoverflow */
# ifndef YYSTACK_RELOCATE
      goto yyexhaustedlab;
# else
      /* Extend the stack our own way.  */
      if (YYMAXDEPTH <= yystacksize)
	goto yyexhaustedlab;
      yystacksize *= 2;
      if (YYMAXDEPTH < yystacksize)
	yystacksize = YYMAXDEPTH;

      {
	yytype_int16 *yyss1 = yyss;
	union yyalloc *yyptr =
	  (union yyalloc *) YYSTACK_ALLOC (YYSTACK_BYTES (yystacksize));
	if (! yyptr)
	  goto yyexhaustedlab;
	YYSTACK_RELOCATE (yyss);
	YYSTACK_RELOCATE (yyvs);

#  undef YYSTACK_RELOCATE
	if (yyss1 != yyssa)
	  YYSTACK_FREE (yyss1);
      }
# endif
#endif /* no yyoverflow */

      yyssp = yyss + yysize - 1;
      yyvsp = yyvs + yysize - 1;


      YYDPRINTF ((stderr, "Stack size increased to %lu\n",
		  (unsigned long int) yystacksize));

      if (yyss + yystacksize - 1 <= yyssp)
	YYABORT;
    }

  YYDPRINTF ((stderr, "Entering state %d\n", yystate));

  goto yybackup;

/*-----------.
| yybackup.  |
`-----------*/
yybackup:

  /* Do appropriate processing given the current state.  Read a
     look-ahead token if we need one and don't already have one.  */

  /* First try to decide what to do without reference to look-ahead token.  */
  yyn = yypact[yystate];
  if (yyn == YYPACT_NINF)
    goto yydefault;

  /* Not known => get a look-ahead token if don't already have one.  */

  /* YYCHAR is either YYEMPTY or YYEOF or a valid look-ahead symbol.  */
  if (yychar == YYEMPTY)
    {
      YYDPRINTF ((stderr, "Reading a token: "));
      yychar = YYLEX;
    }

  if (yychar <= YYEOF)
    {
      yychar = yytoken = YYEOF;
      YYDPRINTF ((stderr, "Now at end of input.\n"));
    }
  else
    {
      yytoken = YYTRANSLATE (yychar);
      YY_SYMBOL_PRINT ("Next token is", yytoken, &yylval, &yylloc);
    }

  /* If the proper action on seeing token YYTOKEN is to reduce or to
     detect an error, take that action.  */
  yyn += yytoken;
  if (yyn < 0 || YYLAST < yyn || yycheck[yyn] != yytoken)
    goto yydefault;
  yyn = yytable[yyn];
  if (yyn <= 0)
    {
      if (yyn == 0 || yyn == YYTABLE_NINF)
	goto yyerrlab;
      yyn = -yyn;
      goto yyreduce;
    }

  if (yyn == YYFINAL)
    YYACCEPT;

  /* Count tokens shifted since error; after three, turn off error
     status.  */
  if (yyerrstatus)
    yyerrstatus--;

  /* Shift the look-ahead token.  */
  YY_SYMBOL_PRINT ("Shifting", yytoken, &yylval, &yylloc);

  /* Discard the shifted token unless it is eof.  */
  if (yychar != YYEOF)
    yychar = YYEMPTY;

  yystate = yyn;
  *++yyvsp = yylval;

  goto yynewstate;


/*-----------------------------------------------------------.
| yydefault -- do the default action for the current state.  |
`-----------------------------------------------------------*/
yydefault:
  yyn = yydefact[yystate];
  if (yyn == 0)
    goto yyerrlab;
  goto yyreduce;


/*-----------------------------.
| yyreduce -- Do a reduction.  |
`-----------------------------*/
yyreduce:
  /* yyn is the number of a rule to reduce with.  */
  yylen = yyr2[yyn];

  /* If YYLEN is nonzero, implement the default value of the action:
     `$$ = $1'.

     Otherwise, the following line sets YYVAL to garbage.
     This behavior is undocumented and Bison
     users should not rely upon it.  Assigning to YYVAL
     unconditionally makes the parser a bit smaller, and it avoids a
     GCC warning that YYVAL may be used uninitialized.  */
  yyval = yyvsp[1-yylen];


  YY_REDUCE_PRINT (yyn);
  switch (yyn)
    {
        case 3:
#line 55 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (2)]); if ((yyvsp[(1) - (2)]) && (yyvsp[(2) - (2)])) (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); ;}
    break;

  case 4:
#line 56 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (1)]); ;}
    break;

  case 5:
#line 57 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (1)]); ;}
    break;

  case 6:
#line 58 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (1)]); ;}
    break;

  case 7:
#line 62 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSymbolSection(currentLine); ;}
    break;

  case 8:
#line 63 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSymbolSection(currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); ;}
    break;

  case 9:
#line 68 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); ;}
    break;

  case 10:
#line 69 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); ;}
    break;

  case 11:
#line 73 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_INT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); ;}
    break;

  case 12:
#line 74 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_INT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (5)])); (yyval)->SetChild(1, (yyvsp[(4) - (5)])); ;}
    break;

  case 13:
#line 75 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FLOAT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); ;}
    break;

  case 14:
#line 76 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FLOAT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (5)])); (yyval)->SetChild(1, (yyvsp[(4) - (5)])); ;}
    break;

  case 15:
#line 77 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_STRING, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); ;}
    break;

  case 16:
#line 78 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_STRING, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (5)])); (yyval)->SetChild(1, (yyvsp[(4) - (5)])); ;}
    break;

  case 17:
#line 82 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSnippet(currentLine); ;}
    break;

  case 18:
#line 83 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSnippet(currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); ;}
    break;

  case 19:
#line 86 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateCodeSection(currentLine); ;}
    break;

  case 20:
#line 87 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateCodeSection(currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); ;}
    break;

  case 21:
#line 91 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 22:
#line 92 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); ;}
    break;

  case 23:
#line 96 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FUNCTION, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (5)])); ;}
    break;

  case 24:
#line 97 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FUNCTION, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (6)])); (yyval)->SetChild(1, (yyvsp[(5) - (6)])); ;}
    break;

  case 25:
#line 102 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 26:
#line 103 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); ;}
    break;

  case 28:
#line 108 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_LABEL, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (2)])); ;}
    break;

  case 29:
#line 109 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_GOTO, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); ;}
    break;

  case 30:
#line 110 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_EXPR, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (2)])); ;}
    break;

  case 31:
#line 111 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_RETURN, currentLine); ;}
    break;

  case 32:
#line 112 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 33:
#line 113 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_ASSIGN, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 34:
#line 117 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); ;}
    break;

  case 35:
#line 118 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); ;}
    break;

  case 36:
#line 119 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (5)])); ;}
    break;

  case 37:
#line 125 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 38:
#line 126 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 39:
#line 130 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (5)])); (yyval)->SetChild(1, (yyvsp[(5) - (5)])); ;}
    break;

  case 40:
#line 131 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (7)])); (yyval)->SetChild(1, (yyvsp[(5) - (7)])); (yyval)->SetChild(2, (yyvsp[(7) - (7)])); ;}
    break;

  case 41:
#line 135 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 42:
#line 136 "sheepParser.y"
    { (yyval) = (yyvsp[(2) - (3)]) ;}
    break;

  case 43:
#line 137 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (7)])); (yyval)->SetChild(1, (yyvsp[(5) - (7)])); (yyval)->SetChild(2, (yyvsp[(7) - (7)])); ;}
    break;

  case 44:
#line 141 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateIdentifierReference(yytext, false, currentLine); ;}
    break;

  case 45:
#line 145 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateIdentifierReference(yytext, true, currentLine); ;}
    break;

  case 46:
#line 149 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateIntegerConstant(atoi(yytext), currentLine); ;}
    break;

  case 47:
#line 150 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateFloatConstant(atof(yytext), currentLine); ;}
    break;

  case 48:
#line 151 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateStringConstant(removeQuotes(yytext), currentLine); ;}
    break;

  case 49:
#line 155 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]) ;}
    break;

  case 50:
#line 156 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (4)]); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); ;}
    break;

  case 51:
#line 160 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 52:
#line 161 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]); (yyval)->AttachSibling((yyvsp[(3) - (3)])); ;}
    break;

  case 53:
#line 165 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 54:
#line 166 "sheepParser.y"
    { (yyval)->AttachSibling((yyvsp[(3) - (3)])); ;}
    break;

  case 55:
#line 170 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); ;}
    break;

  case 56:
#line 171 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 57:
#line 172 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 58:
#line 173 "sheepParser.y"
    { (yyval) = (yyvsp[(2) - (3)]); ;}
    break;

  case 59:
#line 174 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_NOT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (2)])); ;}
    break;

  case 60:
#line 175 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_ADD, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 61:
#line 176 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_MINUS, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 62:
#line 177 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_TIMES, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 63:
#line 178 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_DIVIDE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 64:
#line 179 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_LT, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 65:
#line 180 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_GT, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 66:
#line 181 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_LTE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (4)])); (yyval)->SetChild(1, (yyvsp[(4) - (4)]));;}
    break;

  case 67:
#line 182 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_GTE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (4)])); (yyval)->SetChild(1, (yyvsp[(4) - (4)]));;}
    break;

  case 68:
#line 183 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_EQ, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 69:
#line 184 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_NE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 70:
#line 185 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_OR, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 71:
#line 186 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_AND, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));  ;}
    break;


/* Line 1267 of yacc.c.  */
#line 1915 "sheepParser.cpp"
      default: break;
    }
  YY_SYMBOL_PRINT ("-> $$ =", yyr1[yyn], &yyval, &yyloc);

  YYPOPSTACK (yylen);
  yylen = 0;
  YY_STACK_PRINT (yyss, yyssp);

  *++yyvsp = yyval;


  /* Now `shift' the result of the reduction.  Determine what state
     that goes to, based on the state we popped back to and the rule
     number reduced by.  */

  yyn = yyr1[yyn];

  yystate = yypgoto[yyn - YYNTOKENS] + *yyssp;
  if (0 <= yystate && yystate <= YYLAST && yycheck[yystate] == *yyssp)
    yystate = yytable[yystate];
  else
    yystate = yydefgoto[yyn - YYNTOKENS];

  goto yynewstate;


/*------------------------------------.
| yyerrlab -- here on detecting error |
`------------------------------------*/
yyerrlab:
  /* If not already recovering from an error, report this error.  */
  if (!yyerrstatus)
    {
      ++yynerrs;
#if ! YYERROR_VERBOSE
      yyerror (YY_("syntax error"));
#else
      {
	YYSIZE_T yysize = yysyntax_error (0, yystate, yychar);
	if (yymsg_alloc < yysize && yymsg_alloc < YYSTACK_ALLOC_MAXIMUM)
	  {
	    YYSIZE_T yyalloc = 2 * yysize;
	    if (! (yysize <= yyalloc && yyalloc <= YYSTACK_ALLOC_MAXIMUM))
	      yyalloc = YYSTACK_ALLOC_MAXIMUM;
	    if (yymsg != yymsgbuf)
	      YYSTACK_FREE (yymsg);
	    yymsg = (char *) YYSTACK_ALLOC (yyalloc);
	    if (yymsg)
	      yymsg_alloc = yyalloc;
	    else
	      {
		yymsg = yymsgbuf;
		yymsg_alloc = sizeof yymsgbuf;
	      }
	  }

	if (0 < yysize && yysize <= yymsg_alloc)
	  {
	    (void) yysyntax_error (yymsg, yystate, yychar);
	    yyerror (yymsg);
	  }
	else
	  {
	    yyerror (YY_("syntax error"));
	    if (yysize != 0)
	      goto yyexhaustedlab;
	  }
      }
#endif
    }



  if (yyerrstatus == 3)
    {
      /* If just tried and failed to reuse look-ahead token after an
	 error, discard it.  */

      if (yychar <= YYEOF)
	{
	  /* Return failure if at end of input.  */
	  if (yychar == YYEOF)
	    YYABORT;
	}
      else
	{
	  yydestruct ("Error: discarding",
		      yytoken, &yylval);
	  yychar = YYEMPTY;
	}
    }

  /* Else will try to reuse look-ahead token after shifting the error
     token.  */
  goto yyerrlab1;


/*---------------------------------------------------.
| yyerrorlab -- error raised explicitly by YYERROR.  |
`---------------------------------------------------*/
yyerrorlab:

  /* Pacify compilers like GCC when the user code never invokes
     YYERROR and the label yyerrorlab therefore never appears in user
     code.  */
  if (/*CONSTCOND*/ 0)
     goto yyerrorlab;

  /* Do not reclaim the symbols of the rule which action triggered
     this YYERROR.  */
  YYPOPSTACK (yylen);
  yylen = 0;
  YY_STACK_PRINT (yyss, yyssp);
  yystate = *yyssp;
  goto yyerrlab1;


/*-------------------------------------------------------------.
| yyerrlab1 -- common code for both syntax error and YYERROR.  |
`-------------------------------------------------------------*/
yyerrlab1:
  yyerrstatus = 3;	/* Each real token shifted decrements this.  */

  for (;;)
    {
      yyn = yypact[yystate];
      if (yyn != YYPACT_NINF)
	{
	  yyn += YYTERROR;
	  if (0 <= yyn && yyn <= YYLAST && yycheck[yyn] == YYTERROR)
	    {
	      yyn = yytable[yyn];
	      if (0 < yyn)
		break;
	    }
	}

      /* Pop the current state because it cannot handle the error token.  */
      if (yyssp == yyss)
	YYABORT;


      yydestruct ("Error: popping",
		  yystos[yystate], yyvsp);
      YYPOPSTACK (1);
      yystate = *yyssp;
      YY_STACK_PRINT (yyss, yyssp);
    }

  if (yyn == YYFINAL)
    YYACCEPT;

  *++yyvsp = yylval;


  /* Shift the error token.  */
  YY_SYMBOL_PRINT ("Shifting", yystos[yyn], yyvsp, yylsp);

  yystate = yyn;
  goto yynewstate;


/*-------------------------------------.
| yyacceptlab -- YYACCEPT comes here.  |
`-------------------------------------*/
yyacceptlab:
  yyresult = 0;
  goto yyreturn;

/*-----------------------------------.
| yyabortlab -- YYABORT comes here.  |
`-----------------------------------*/
yyabortlab:
  yyresult = 1;
  goto yyreturn;

#ifndef yyoverflow
/*-------------------------------------------------.
| yyexhaustedlab -- memory exhaustion comes here.  |
`-------------------------------------------------*/
yyexhaustedlab:
  yyerror (YY_("memory exhausted"));
  yyresult = 2;
  /* Fall through.  */
#endif

yyreturn:
  if (yychar != YYEOF && yychar != YYEMPTY)
     yydestruct ("Cleanup: discarding lookahead",
		 yytoken, &yylval);
  /* Do not reclaim the symbols of the rule which action triggered
     this YYABORT or YYACCEPT.  */
  YYPOPSTACK (yylen);
  YY_STACK_PRINT (yyss, yyssp);
  while (yyssp != yyss)
    {
      yydestruct ("Cleanup: popping",
		  yystos[*yyssp], yyvsp);
      YYPOPSTACK (1);
    }
#ifndef yyoverflow
  if (yyss != yyssa)
    YYSTACK_FREE (yyss);
#endif
#if YYERROR_VERBOSE
  if (yymsg != yymsgbuf)
    YYSTACK_FREE (yymsg);
#endif
  /* Make sure YYID is used.  */
  return YYID (yyresult);
}



