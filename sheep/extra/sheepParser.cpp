/* A Bison parser, made by GNU Bison 2.7.12-4996.  */

/* Bison implementation for Yacc-like parsers in C
   
      Copyright (C) 1984, 1989-1990, 2000-2013 Free Software Foundation, Inc.
   
   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.
   
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.
   
   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.  */

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
#define YYBISON_VERSION "2.7.12-4996"

/* Skeleton name.  */
#define YYSKELETON_NAME "yacc.c"

/* Pure parsers.  */
#define YYPURE 0

/* Push parsers.  */
#define YYPUSH 0

/* Pull parsers.  */
#define YYPULL 1




/* Copy the first part of user declarations.  */
/* Line 371 of yacc.c  */
#line 1 "sheepParser.y"


#include <stdio.h>
#include <string.h>
#include <vector>
#include "symbols.h"
#include "sheepCodeTree.h"
#include "sheepMemoryAllocator.h"
#define YYSTYPE SheepCodeTreeNode*

#define TRUE 1
#define FALSE 0

extern SheepCodeTreeNode* g_codeTreeRoot;
extern SheepCodeTree* g_codeTree;
extern int currentLine;

void yyerror(const char* str)
{
	g_codeTree->LogError(currentLine, str);
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


/* Line 371 of yacc.c  */
#line 106 "sheepParser.cpp"

# ifndef YY_NULL
#  if defined __cplusplus && 201103L <= __cplusplus
#   define YY_NULL nullptr
#  else
#   define YY_NULL 0
#  endif
# endif

/* Enabling verbose error messages.  */
#ifdef YYERROR_VERBOSE
# undef YYERROR_VERBOSE
# define YYERROR_VERBOSE 1
#else
# define YYERROR_VERBOSE 0
#endif

/* In a future release of Bison, this section will be replaced
   by #include "sheepParser.hpp".  */
#ifndef YY_YY_SHEEPPARSER_HPP_INCLUDED
# define YY_YY_SHEEPPARSER_HPP_INCLUDED
/* Enabling traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif
#if YYDEBUG
extern int yydebug;
#endif

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
     WAIT = 268,
     RETURN = 269,
     IF = 270,
     ELSE = 271,
     GOTO = 272,
     COLON = 273,
     SEMICOLON = 274,
     DOLLAR = 275,
     LPAREN = 276,
     RPAREN = 277,
     LBRACE = 278,
     RBRACE = 279,
     QUOTE = 280,
     COMMA = 281,
     EQUALS = 282,
     NOTEQUAL = 283,
     BECOMES = 284,
     PLUS = 285,
     MINUS = 286,
     TIMES = 287,
     DIVIDE = 288,
     LESSTHAN = 289,
     GREATERTHAN = 290,
     NOT = 291,
     OR = 292,
     AND = 293
   };
#endif


#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
typedef int YYSTYPE;
# define YYSTYPE_IS_TRIVIAL 1
# define yystype YYSTYPE /* obsolescent; will be withdrawn */
# define YYSTYPE_IS_DECLARED 1
#endif

extern YYSTYPE yylval;

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

#endif /* !YY_YY_SHEEPPARSER_HPP_INCLUDED  */

/* Copy the second part of user declarations.  */

/* Line 390 of yacc.c  */
#line 210 "sheepParser.cpp"

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

#ifndef __attribute__
/* This feature is available in gcc versions 2.5 and later.  */
# if (! defined __GNUC__ || __GNUC__ < 2 \
      || (__GNUC__ == 2 && __GNUC_MINOR__ < 5))
#  define __attribute__(Spec) /* empty */
# endif
#endif

/* Suppress unused-variable warnings by "using" E.  */
#if ! defined lint || defined __GNUC__
# define YYUSE(E) ((void) (E))
#else
# define YYUSE(E) /* empty */
#endif


/* Identity function, used to suppress warnings about constant conditions.  */
#ifndef lint
# define YYID(N) (N)
#else
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static int
YYID (int yyi)
#else
static int
YYID (yyi)
    int yyi;
#endif
{
  return yyi;
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
#    if ! defined _ALLOCA_H && ! defined EXIT_SUCCESS && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
#     include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
      /* Use EXIT_SUCCESS as a witness for stdlib.h.  */
#     ifndef EXIT_SUCCESS
#      define EXIT_SUCCESS 0
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
#  if (defined __cplusplus && ! defined EXIT_SUCCESS \
       && ! ((defined YYMALLOC || defined malloc) \
	     && (defined YYFREE || defined free)))
#   include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#   ifndef EXIT_SUCCESS
#    define EXIT_SUCCESS 0
#   endif
#  endif
#  ifndef YYMALLOC
#   define YYMALLOC malloc
#   if ! defined malloc && ! defined EXIT_SUCCESS && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
void *malloc (YYSIZE_T); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
#  ifndef YYFREE
#   define YYFREE free
#   if ! defined free && ! defined EXIT_SUCCESS && (defined __STDC__ || defined __C99__FUNC__ \
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
  yytype_int16 yyss_alloc;
  YYSTYPE yyvs_alloc;
};

/* The size of the maximum gap between one aligned stack and the next.  */
# define YYSTACK_GAP_MAXIMUM (sizeof (union yyalloc) - 1)

/* The size of an array large to enough to hold all stacks, each with
   N elements.  */
# define YYSTACK_BYTES(N) \
     ((N) * (sizeof (yytype_int16) + sizeof (YYSTYPE)) \
      + YYSTACK_GAP_MAXIMUM)

# define YYCOPY_NEEDED 1

/* Relocate STACK from its old location to the new one.  The
   local variables YYSIZE and YYSTACKSIZE give the old and new number of
   elements in the stack, and YYPTR gives the new location of the
   stack.  Advance YYPTR to a properly aligned location for the next
   stack.  */
# define YYSTACK_RELOCATE(Stack_alloc, Stack)				\
    do									\
      {									\
	YYSIZE_T yynewbytes;						\
	YYCOPY (&yyptr->Stack_alloc, Stack, yysize);			\
	Stack = &yyptr->Stack_alloc;					\
	yynewbytes = yystacksize * sizeof (*Stack) + YYSTACK_GAP_MAXIMUM; \
	yyptr += yynewbytes / sizeof (*yyptr);				\
      }									\
    while (YYID (0))

#endif

#if defined YYCOPY_NEEDED && YYCOPY_NEEDED
/* Copy COUNT objects from SRC to DST.  The source and destination do
   not overlap.  */
# ifndef YYCOPY
#  if defined __GNUC__ && 1 < __GNUC__
#   define YYCOPY(Dst, Src, Count) \
      __builtin_memcpy (Dst, Src, (Count) * sizeof (*(Src)))
#  else
#   define YYCOPY(Dst, Src, Count)              \
      do                                        \
        {                                       \
          YYSIZE_T yyi;                         \
          for (yyi = 0; yyi < (Count); yyi++)   \
            (Dst)[yyi] = (Src)[yyi];            \
        }                                       \
      while (YYID (0))
#  endif
# endif
#endif /* !YYCOPY_NEEDED */

/* YYFINAL -- State number of the termination state.  */
#define YYFINAL  8
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   411

/* YYNTOKENS -- Number of terminals.  */
#define YYNTOKENS  39
/* YYNNTS -- Number of nonterminals.  */
#define YYNNTS  25
/* YYNRULES -- Number of rules.  */
#define YYNRULES  79
/* YYNRULES -- Number of states.  */
#define YYNSTATES  146

/* YYTRANSLATE(YYLEX) -- Bison symbol number corresponding to YYLEX.  */
#define YYUNDEFTOK  2
#define YYMAXUTOK   293

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
      35,    36,    37,    38
};

#if YYDEBUG
/* YYPRHS[YYN] -- Index of the first RHS symbol of rule number YYN in
   YYRHS.  */
static const yytype_uint16 yyprhs[] =
{
       0,     0,     3,     4,     7,     9,    11,    15,    20,    22,
      25,    27,    29,    31,    35,    37,    41,    45,    51,    55,
      60,    62,    65,    72,    80,    88,    97,    98,   100,   103,
     108,   110,   113,   115,   118,   122,   125,   128,   132,   134,
     138,   141,   145,   151,   153,   155,   161,   169,   171,   174,
     178,   186,   188,   190,   192,   194,   196,   200,   205,   207,
     211,   213,   217,   219,   221,   223,   227,   230,   233,   237,
     241,   245,   249,   253,   257,   262,   267,   271,   275,   279
};

/* YYRHS -- A `-1'-separated list of the rules' RHS.  */
static const yytype_int8 yyrhs[] =
{
      40,     0,    -1,    -1,    41,    46,    -1,    41,    -1,    46,
      -1,    12,    23,    24,    -1,    12,    23,    42,    24,    -1,
      44,    -1,    42,    44,    -1,     8,    -1,     9,    -1,    10,
      -1,    43,    45,    19,    -1,    57,    -1,    57,    29,    59,
      -1,    45,    26,    57,    -1,    45,    26,    57,    29,    59,
      -1,    11,    23,    24,    -1,    11,    23,    47,    24,    -1,
      48,    -1,    47,    48,    -1,    57,    21,    49,    22,    23,
      24,    -1,    57,    21,    49,    22,    23,    51,    24,    -1,
      43,    57,    21,    49,    22,    23,    24,    -1,    43,    57,
      21,    49,    22,    23,    51,    24,    -1,    -1,    50,    -1,
      43,    57,    -1,    50,    26,    43,    57,    -1,    54,    -1,
      51,    54,    -1,    19,    -1,    57,    18,    -1,    17,    57,
      19,    -1,    63,    19,    -1,    14,    19,    -1,    14,    63,
      19,    -1,    53,    -1,    57,    29,    63,    -1,    13,    19,
      -1,    13,    60,    19,    -1,    13,    23,    61,    19,    24,
      -1,    55,    -1,    56,    -1,    15,    21,    63,    22,    54,
      -1,    15,    21,    63,    22,    56,    16,    55,    -1,    52,
      -1,    23,    24,    -1,    23,    51,    24,    -1,    15,    21,
      63,    22,    56,    16,    56,    -1,     4,    -1,     3,    -1,
       5,    -1,     6,    -1,     7,    -1,    58,    21,    22,    -1,
      58,    21,    62,    22,    -1,    60,    -1,    61,    19,    60,
      -1,    63,    -1,    62,    26,    63,    -1,    59,    -1,    60,
      -1,    57,    -1,    21,    63,    22,    -1,    36,    63,    -1,
      31,    63,    -1,    63,    30,    63,    -1,    63,    31,    63,
      -1,    63,    32,    63,    -1,    63,    33,    63,    -1,    63,
      34,    63,    -1,    63,    35,    63,    -1,    63,    34,    29,
      63,    -1,    63,    35,    29,    63,    -1,    63,    27,    63,
      -1,    63,    28,    63,    -1,    63,    37,    63,    -1,    63,
      38,    63,    -1
};

/* YYRLINE[YYN] -- source line where rule number YYN was defined.  */
static const yytype_uint16 yyrline[] =
{
       0,    56,    56,    58,    59,    60,    64,    65,    70,    71,
      75,    76,    77,    81,    92,    99,   107,   115,   127,   128,
     132,   139,   149,   158,   168,   178,   192,   193,   197,   206,
     218,   219,   223,   224,   225,   226,   227,   228,   229,   230,
     234,   235,   236,   242,   243,   247,   248,   252,   253,   254,
     255,   259,   268,   276,   277,   278,   282,   283,   287,   288,
     292,   293,   297,   298,   299,   300,   301,   302,   303,   304,
     305,   306,   307,   308,   309,   310,   311,   312,   313,   314
};
#endif

#if YYDEBUG || YYERROR_VERBOSE || 0
/* YYTNAME[SYMBOL-NUM] -- String name of the symbol SYMBOL-NUM.
   First, the terminals, then, starting at YYNTOKENS, nonterminals.  */
static const char *const yytname[] =
{
  "$end", "error", "$undefined", "IDENTIFIER", "LOCALIDENTIFIER",
  "INTEGER", "FLOAT", "STRING", "INTSYM", "FLOATSYM", "STRINGSYM", "CODE",
  "SYMBOLS", "WAIT", "RETURN", "IF", "ELSE", "GOTO", "COLON", "SEMICOLON",
  "DOLLAR", "LPAREN", "RPAREN", "LBRACE", "RBRACE", "QUOTE", "COMMA",
  "EQUALS", "NOTEQUAL", "BECOMES", "PLUS", "MINUS", "TIMES", "DIVIDE",
  "LESSTHAN", "GREATERTHAN", "NOT", "OR", "AND", "$accept", "sheep",
  "symbol_section", "symbol_list", "symbol_type", "symbol_declaration",
  "symbol_declaration_list", "code_section", "function_list", "function",
  "function_parameter_list_opt", "function_parameter_list",
  "statement_list", "simple_statement", "wait_statement", "statement",
  "open_statement", "closed_statement", "local_identifier",
  "global_identifier", "constant", "global_function_call",
  "global_function_call_list", "parameter_list", "expr", YY_NULL
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
     285,   286,   287,   288,   289,   290,   291,   292,   293
};
# endif

/* YYR1[YYN] -- Symbol number of symbol that rule YYN derives.  */
static const yytype_uint8 yyr1[] =
{
       0,    39,    40,    40,    40,    40,    41,    41,    42,    42,
      43,    43,    43,    44,    45,    45,    45,    45,    46,    46,
      47,    47,    48,    48,    48,    48,    49,    49,    50,    50,
      51,    51,    52,    52,    52,    52,    52,    52,    52,    52,
      53,    53,    53,    54,    54,    55,    55,    56,    56,    56,
      56,    57,    58,    59,    59,    59,    60,    60,    61,    61,
      62,    62,    63,    63,    63,    63,    63,    63,    63,    63,
      63,    63,    63,    63,    63,    63,    63,    63,    63,    63
};

/* YYR2[YYN] -- Number of symbols composing right hand side of rule YYN.  */
static const yytype_uint8 yyr2[] =
{
       0,     2,     0,     2,     1,     1,     3,     4,     1,     2,
       1,     1,     1,     3,     1,     3,     3,     5,     3,     4,
       1,     2,     6,     7,     7,     8,     0,     1,     2,     4,
       1,     2,     1,     2,     3,     2,     2,     3,     1,     3,
       2,     3,     5,     1,     1,     5,     7,     1,     2,     3,
       7,     1,     1,     1,     1,     1,     3,     4,     1,     3,
       1,     3,     1,     1,     1,     3,     2,     2,     3,     3,
       3,     3,     3,     3,     4,     4,     3,     3,     3,     3
};

/* YYDEFACT[STATE-NAME] -- Default reduction number in state STATE-NUM.
   Performed when YYTABLE doesn't specify something else to do.  Zero
   means the default is an error.  */
static const yytype_uint8 yydefact[] =
{
       2,     0,     0,     0,     4,     5,     0,     0,     1,     3,
      51,    10,    11,    12,    18,     0,     0,    20,     0,     6,
       0,     0,     8,     0,    19,    21,    26,     7,     9,     0,
      14,    26,     0,     0,    27,    13,     0,     0,     0,    28,
       0,     0,    16,    53,    54,    55,    15,     0,     0,     0,
       0,     0,    52,     0,     0,     0,     0,    32,     0,     0,
      22,     0,     0,     0,    47,    38,    30,    43,    44,    64,
       0,    62,    63,     0,    29,    17,    24,     0,    40,     0,
       0,    36,    64,     0,     0,     0,     0,    48,     0,    67,
      66,    23,    31,    33,     0,     0,    35,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    25,    58,     0,
      41,    37,     0,    34,    65,    49,    39,    56,     0,    60,
      76,    77,    68,    69,    70,    71,     0,    72,     0,    73,
      78,    79,     0,     0,    57,     0,    74,    75,    42,    59,
      45,    44,    61,     0,    46,    50
};

/* YYDEFGOTO[NTERM-NUM].  */
static const yytype_int8 yydefgoto[] =
{
      -1,     3,     4,    20,    15,    22,    29,     5,    16,    17,
      33,    34,    63,    64,    65,    66,    67,    68,    82,    70,
      71,    72,   109,   118,    73
};

/* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
   STATE-NUM.  */
#define YYPACT_NINF -123
static const yytype_int16 yypact[] =
{
     114,   -17,     2,    39,    37,  -123,   271,    84,  -123,  -123,
    -123,  -123,  -123,  -123,  -123,    48,   303,  -123,    34,  -123,
     330,    48,  -123,    41,  -123,  -123,   103,  -123,  -123,    28,
      29,   103,    48,    63,    65,  -123,    48,   144,    73,  -123,
      76,   103,    80,  -123,  -123,  -123,  -123,    82,    83,    48,
     144,   117,  -123,     5,    13,   112,    48,  -123,   242,   151,
    -123,   242,   242,   185,  -123,  -123,  -123,  -123,  -123,    22,
     118,  -123,  -123,   298,  -123,  -123,  -123,   219,  -123,   132,
     123,  -123,  -123,   315,   242,   133,   329,  -123,   253,    96,
    -123,  -123,  -123,  -123,   242,   140,  -123,   242,   242,   242,
     242,   242,   242,   174,   208,   242,   242,  -123,  -123,   141,
    -123,  -123,   343,  -123,  -123,  -123,   355,  -123,   -19,   355,
     376,   376,    96,    96,  -123,  -123,   242,    85,   242,    85,
     367,    33,    -1,   287,  -123,   242,   355,   355,  -123,  -123,
    -123,   143,   355,   287,  -123,  -123
};

/* YYPGOTO[NTERM-NUM].  */
static const yytype_int16 yypgoto[] =
{
    -123,  -123,  -123,  -123,    15,   147,  -123,   159,  -123,   153,
     142,  -123,   -47,  -123,  -123,   -50,    40,  -122,    -6,  -123,
     -36,   -48,  -123,  -123,   -25
};

/* YYTABLE[YYPACT[STATE-NUM]].  What to do in state STATE-NUM.  If
   positive, shift that token.  If negative, reduce the rule which
   number is the opposite.  If YYTABLE_NINF, syntax error.  */
#define YYTABLE_NINF -1
static const yytype_uint8 yytable[] =
{
      18,    46,    52,   134,    77,    80,     6,   135,    52,    23,
      18,   141,    88,    92,    75,    30,    52,    10,    43,    44,
      45,   145,    21,   138,    78,     7,    39,    92,    79,    83,
      42,   108,    81,    86,    58,    21,    89,    90,    92,     8,
      93,    32,    69,    74,    61,    69,    32,    35,     1,    62,
      85,    94,    10,    69,    36,    26,    49,    69,    37,   112,
      97,    98,    31,    99,   100,   101,   102,   103,   104,   116,
     119,    69,   120,   121,   122,   123,   124,   125,   127,   129,
     130,   131,    69,   140,   139,    40,    52,    10,    43,    44,
      45,    41,    11,    12,    13,    47,    53,    54,    55,    48,
      56,   136,    57,   137,    58,    51,    59,    60,    19,    50,
     142,    11,    12,    13,    61,    99,   100,   101,   102,    62,
      52,    10,    43,    44,    45,     1,     2,    69,   101,   102,
      53,    54,    55,    84,    56,    52,    57,    69,    58,    95,
      59,    76,   110,    52,    10,    43,    44,    45,    61,    43,
      44,    45,   113,    62,    52,    10,    43,    44,    45,   143,
     132,    58,   117,     9,    53,    54,    55,    28,    56,    25,
      57,    61,    58,    38,    59,    87,    62,    52,    10,    43,
      44,    45,    61,   144,     0,     0,     0,    62,    52,    10,
      43,    44,    45,     0,     0,    58,     0,     0,    53,    54,
      55,     0,    56,   126,    57,    61,    58,     0,    59,    91,
      62,    52,    10,    43,    44,    45,    61,     0,     0,     0,
       0,    62,    52,    10,    43,    44,    45,     0,     0,    58,
       0,     0,    53,    54,    55,     0,    56,   128,    57,    61,
      58,     0,    59,   107,    62,    52,    10,    43,    44,    45,
      61,     0,     0,     0,     0,    62,    52,    10,    43,    44,
      45,     0,     0,    58,     0,     0,    53,    54,    55,     0,
      56,     0,    57,    61,    58,    10,    59,   115,    62,    11,
      12,    13,     0,     0,    61,     0,     0,     0,     0,    62,
      52,    10,    43,    44,    45,    14,     0,     0,     0,     0,
      53,    54,    55,     0,    56,     0,    57,    10,    58,     0,
      59,    11,    12,    13,     0,     0,     0,    96,    61,     0,
       0,     0,     0,    62,     0,    97,    98,    24,    99,   100,
     101,   102,   103,   104,   111,   105,   106,     0,    11,    12,
      13,     0,    97,    98,     0,    99,   100,   101,   102,   103,
     104,   114,   105,   106,    27,     0,    97,    98,     0,    99,
     100,   101,   102,   103,   104,   133,   105,   106,     0,     0,
      97,    98,     0,    99,   100,   101,   102,   103,   104,     0,
     105,   106,    97,    98,     0,    99,   100,   101,   102,   103,
     104,     0,   105,   106,    97,    98,     0,    99,   100,   101,
     102,   103,   104,     0,     0,   106,    99,   100,   101,   102,
     103,   104
};

#define yypact_value_is_default(Yystate) \
  (!!((Yystate) == (-123)))

#define yytable_value_is_error(Yytable_value) \
  YYID (0)

static const yytype_int16 yycheck[] =
{
       6,    37,     3,    22,    51,    53,    23,    26,     3,    15,
      16,   133,    59,    63,    50,    21,     3,     4,     5,     6,
       7,   143,     7,    24,    19,    23,    32,    77,    23,    54,
      36,    79,    19,    58,    21,    20,    61,    62,    88,     0,
      18,    26,    48,    49,    31,    51,    31,    19,    11,    36,
      56,    29,     4,    59,    26,    21,    41,    63,    29,    84,
      27,    28,    21,    30,    31,    32,    33,    34,    35,    94,
      95,    77,    97,    98,    99,   100,   101,   102,   103,   104,
     105,   106,    88,   133,   132,    22,     3,     4,     5,     6,
       7,    26,     8,     9,    10,    22,    13,    14,    15,    23,
      17,   126,    19,   128,    21,    23,    23,    24,    24,    29,
     135,     8,     9,    10,    31,    30,    31,    32,    33,    36,
       3,     4,     5,     6,     7,    11,    12,   133,    32,    33,
      13,    14,    15,    21,    17,     3,    19,   143,    21,    21,
      23,    24,    19,     3,     4,     5,     6,     7,    31,     5,
       6,     7,    19,    36,     3,     4,     5,     6,     7,    16,
      19,    21,    22,     4,    13,    14,    15,    20,    17,    16,
      19,    31,    21,    31,    23,    24,    36,     3,     4,     5,
       6,     7,    31,   143,    -1,    -1,    -1,    36,     3,     4,
       5,     6,     7,    -1,    -1,    21,    -1,    -1,    13,    14,
      15,    -1,    17,    29,    19,    31,    21,    -1,    23,    24,
      36,     3,     4,     5,     6,     7,    31,    -1,    -1,    -1,
      -1,    36,     3,     4,     5,     6,     7,    -1,    -1,    21,
      -1,    -1,    13,    14,    15,    -1,    17,    29,    19,    31,
      21,    -1,    23,    24,    36,     3,     4,     5,     6,     7,
      31,    -1,    -1,    -1,    -1,    36,     3,     4,     5,     6,
       7,    -1,    -1,    21,    -1,    -1,    13,    14,    15,    -1,
      17,    -1,    19,    31,    21,     4,    23,    24,    36,     8,
       9,    10,    -1,    -1,    31,    -1,    -1,    -1,    -1,    36,
       3,     4,     5,     6,     7,    24,    -1,    -1,    -1,    -1,
      13,    14,    15,    -1,    17,    -1,    19,     4,    21,    -1,
      23,     8,     9,    10,    -1,    -1,    -1,    19,    31,    -1,
      -1,    -1,    -1,    36,    -1,    27,    28,    24,    30,    31,
      32,    33,    34,    35,    19,    37,    38,    -1,     8,     9,
      10,    -1,    27,    28,    -1,    30,    31,    32,    33,    34,
      35,    22,    37,    38,    24,    -1,    27,    28,    -1,    30,
      31,    32,    33,    34,    35,    22,    37,    38,    -1,    -1,
      27,    28,    -1,    30,    31,    32,    33,    34,    35,    -1,
      37,    38,    27,    28,    -1,    30,    31,    32,    33,    34,
      35,    -1,    37,    38,    27,    28,    -1,    30,    31,    32,
      33,    34,    35,    -1,    -1,    38,    30,    31,    32,    33,
      34,    35
};

/* YYSTOS[STATE-NUM] -- The (internal number of the) accessing
   symbol of state STATE-NUM.  */
static const yytype_uint8 yystos[] =
{
       0,    11,    12,    40,    41,    46,    23,    23,     0,    46,
       4,     8,     9,    10,    24,    43,    47,    48,    57,    24,
      42,    43,    44,    57,    24,    48,    21,    24,    44,    45,
      57,    21,    43,    49,    50,    19,    26,    29,    49,    57,
      22,    26,    57,     5,     6,     7,    59,    22,    23,    43,
      29,    23,     3,    13,    14,    15,    17,    19,    21,    23,
      24,    31,    36,    51,    52,    53,    54,    55,    56,    57,
      58,    59,    60,    63,    57,    59,    24,    51,    19,    23,
      60,    19,    57,    63,    21,    57,    63,    24,    51,    63,
      63,    24,    54,    18,    29,    21,    19,    27,    28,    30,
      31,    32,    33,    34,    35,    37,    38,    24,    60,    61,
      19,    19,    63,    19,    22,    24,    63,    22,    62,    63,
      63,    63,    63,    63,    63,    63,    29,    63,    29,    63,
      63,    63,    19,    22,    22,    26,    63,    63,    24,    60,
      54,    56,    63,    16,    55,    56
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
   Once GCC version 2 has supplanted version 1, this can go.  However,
   YYFAIL appears to be in use.  Nevertheless, it is formally deprecated
   in Bison 2.4.2's NEWS entry, where a plan to phase it out is
   discussed.  */

#define YYFAIL		goto yyerrlab
#if defined YYFAIL
  /* This is here to suppress warnings from the GCC cpp's
     -Wunused-macros.  Normally we don't worry about that warning, but
     some users do, and we want to make it easy for users to remove
     YYFAIL uses, which will produce warnings from Bison 2.5.  */
#endif

#define YYRECOVERING()  (!!yyerrstatus)

#define YYBACKUP(Token, Value)                                  \
do                                                              \
  if (yychar == YYEMPTY)                                        \
    {                                                           \
      yychar = (Token);                                         \
      yylval = (Value);                                         \
      YYPOPSTACK (yylen);                                       \
      yystate = *yyssp;                                         \
      goto yybackup;                                            \
    }                                                           \
  else                                                          \
    {                                                           \
      yyerror (YY_("syntax error: cannot back up")); \
      YYERROR;							\
    }								\
while (YYID (0))

/* Error token number */
#define YYTERROR	1
#define YYERRCODE	256


/* This macro is provided for backward compatibility. */
#ifndef YY_LOCATION_PRINT
# define YY_LOCATION_PRINT(File, Loc) ((void) 0)
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
  FILE *yyo = yyoutput;
  YYUSE (yyo);
  if (!yyvaluep)
    return;
# ifdef YYPRINT
  if (yytype < YYNTOKENS)
    YYPRINT (yyoutput, yytoknum[yytype], *yyvaluep);
# else
  YYUSE (yyoutput);
# endif
  YYUSE (yytype);
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
yy_stack_print (yytype_int16 *yybottom, yytype_int16 *yytop)
#else
static void
yy_stack_print (yybottom, yytop)
    yytype_int16 *yybottom;
    yytype_int16 *yytop;
#endif
{
  YYFPRINTF (stderr, "Stack now");
  for (; yybottom <= yytop; yybottom++)
    {
      int yybot = *yybottom;
      YYFPRINTF (stderr, " %d", yybot);
    }
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
      YYFPRINTF (stderr, "   $%d = ", yyi + 1);
      yy_symbol_print (stderr, yyrhs[yyprhs[yyrule] + yyi],
		       &(yyvsp[(yyi + 1) - (yynrhs)])
		       		       );
      YYFPRINTF (stderr, "\n");
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

/* Copy into *YYMSG, which is of size *YYMSG_ALLOC, an error message
   about the unexpected token YYTOKEN for the state stack whose top is
   YYSSP.

   Return 0 if *YYMSG was successfully written.  Return 1 if *YYMSG is
   not large enough to hold the message.  In that case, also set
   *YYMSG_ALLOC to the required number of bytes.  Return 2 if the
   required number of bytes is too large to store.  */
static int
yysyntax_error (YYSIZE_T *yymsg_alloc, char **yymsg,
                yytype_int16 *yyssp, int yytoken)
{
  YYSIZE_T yysize0 = yytnamerr (YY_NULL, yytname[yytoken]);
  YYSIZE_T yysize = yysize0;
  enum { YYERROR_VERBOSE_ARGS_MAXIMUM = 5 };
  /* Internationalized format string. */
  const char *yyformat = YY_NULL;
  /* Arguments of yyformat. */
  char const *yyarg[YYERROR_VERBOSE_ARGS_MAXIMUM];
  /* Number of reported tokens (one for the "unexpected", one per
     "expected"). */
  int yycount = 0;

  /* There are many possibilities here to consider:
     - Assume YYFAIL is not used.  It's too flawed to consider.  See
       <http://lists.gnu.org/archive/html/bison-patches/2009-12/msg00024.html>
       for details.  YYERROR is fine as it does not invoke this
       function.
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
  if (yytoken != YYEMPTY)
    {
      int yyn = yypact[*yyssp];
      yyarg[yycount++] = yytname[yytoken];
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
            if (yycheck[yyx + yyn] == yyx && yyx != YYTERROR
                && !yytable_value_is_error (yytable[yyx + yyn]))
              {
                if (yycount == YYERROR_VERBOSE_ARGS_MAXIMUM)
                  {
                    yycount = 1;
                    yysize = yysize0;
                    break;
                  }
                yyarg[yycount++] = yytname[yyx];
                {
                  YYSIZE_T yysize1 = yysize + yytnamerr (YY_NULL, yytname[yyx]);
                  if (! (yysize <= yysize1
                         && yysize1 <= YYSTACK_ALLOC_MAXIMUM))
                    return 2;
                  yysize = yysize1;
                }
              }
        }
    }

  switch (yycount)
    {
# define YYCASE_(N, S)                      \
      case N:                               \
        yyformat = S;                       \
      break
      YYCASE_(0, YY_("syntax error"));
      YYCASE_(1, YY_("syntax error, unexpected %s"));
      YYCASE_(2, YY_("syntax error, unexpected %s, expecting %s"));
      YYCASE_(3, YY_("syntax error, unexpected %s, expecting %s or %s"));
      YYCASE_(4, YY_("syntax error, unexpected %s, expecting %s or %s or %s"));
      YYCASE_(5, YY_("syntax error, unexpected %s, expecting %s or %s or %s or %s"));
# undef YYCASE_
    }

  {
    YYSIZE_T yysize1 = yysize + yystrlen (yyformat);
    if (! (yysize <= yysize1 && yysize1 <= YYSTACK_ALLOC_MAXIMUM))
      return 2;
    yysize = yysize1;
  }

  if (*yymsg_alloc < yysize)
    {
      *yymsg_alloc = 2 * yysize;
      if (! (yysize <= *yymsg_alloc
             && *yymsg_alloc <= YYSTACK_ALLOC_MAXIMUM))
        *yymsg_alloc = YYSTACK_ALLOC_MAXIMUM;
      return 1;
    }

  /* Avoid sprintf, as that infringes on the user's name space.
     Don't have undefined behavior even if the translation
     produced a string with the wrong number of "%s"s.  */
  {
    char *yyp = *yymsg;
    int yyi = 0;
    while ((*yyp = *yyformat) != '\0')
      if (*yyp == '%' && yyformat[1] == 's' && yyi < yycount)
        {
          yyp += yytnamerr (yyp, yyarg[yyi++]);
          yyformat += 2;
        }
      else
        {
          yyp++;
          yyformat++;
        }
  }
  return 0;
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

  YYUSE (yytype);
}




/* The lookahead symbol.  */
int yychar;


#ifndef YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_END
#endif
#ifndef YY_INITIAL_VALUE
# define YY_INITIAL_VALUE(Value) /* Nothing. */
#endif

/* The semantic value of the lookahead symbol.  */
YYSTYPE yylval YY_INITIAL_VALUE(yyval_default);

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
    /* Number of tokens to shift before error messages enabled.  */
    int yyerrstatus;

    /* The stacks and their tools:
       `yyss': related to states.
       `yyvs': related to semantic values.

       Refer to the stacks through separate pointers, to allow yyoverflow
       to reallocate them elsewhere.  */

    /* The state stack.  */
    yytype_int16 yyssa[YYINITDEPTH];
    yytype_int16 *yyss;
    yytype_int16 *yyssp;

    /* The semantic value stack.  */
    YYSTYPE yyvsa[YYINITDEPTH];
    YYSTYPE *yyvs;
    YYSTYPE *yyvsp;

    YYSIZE_T yystacksize;

  int yyn;
  int yyresult;
  /* Lookahead token as an internal (translated) token number.  */
  int yytoken = 0;
  /* The variables used to return semantic value and location from the
     action routines.  */
  YYSTYPE yyval;

#if YYERROR_VERBOSE
  /* Buffer for error messages, and its allocated size.  */
  char yymsgbuf[128];
  char *yymsg = yymsgbuf;
  YYSIZE_T yymsg_alloc = sizeof yymsgbuf;
#endif

#define YYPOPSTACK(N)   (yyvsp -= (N), yyssp -= (N))

  /* The number of symbols on the RHS of the reduced rule.
     Keep to zero when no symbol should be popped.  */
  int yylen = 0;

  yyssp = yyss = yyssa;
  yyvsp = yyvs = yyvsa;
  yystacksize = YYINITDEPTH;

  YYDPRINTF ((stderr, "Starting parse\n"));

  yystate = 0;
  yyerrstatus = 0;
  yynerrs = 0;
  yychar = YYEMPTY; /* Cause a token to be read.  */
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
	YYSTACK_RELOCATE (yyss_alloc, yyss);
	YYSTACK_RELOCATE (yyvs_alloc, yyvs);
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

  if (yystate == YYFINAL)
    YYACCEPT;

  goto yybackup;

/*-----------.
| yybackup.  |
`-----------*/
yybackup:

  /* Do appropriate processing given the current state.  Read a
     lookahead token if we need one and don't already have one.  */

  /* First try to decide what to do without reference to lookahead token.  */
  yyn = yypact[yystate];
  if (yypact_value_is_default (yyn))
    goto yydefault;

  /* Not known => get a lookahead token if don't already have one.  */

  /* YYCHAR is either YYEMPTY or YYEOF or a valid lookahead symbol.  */
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
      if (yytable_value_is_error (yyn))
        goto yyerrlab;
      yyn = -yyn;
      goto yyreduce;
    }

  /* Count tokens shifted since error; after three, turn off error
     status.  */
  if (yyerrstatus)
    yyerrstatus--;

  /* Shift the lookahead token.  */
  YY_SYMBOL_PRINT ("Shifting", yytoken, &yylval, &yylloc);

  /* Discard the shifted token.  */
  yychar = YYEMPTY;

  yystate = yyn;
  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  *++yyvsp = yylval;
  YY_IGNORE_MAYBE_UNINITIALIZED_END

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
/* Line 1787 of yacc.c  */
#line 58 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (2)]); if ((yyvsp[(1) - (2)]) && (yyvsp[(2) - (2)])) (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); }
    break;

  case 4:
/* Line 1787 of yacc.c  */
#line 59 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (1)]); }
    break;

  case 5:
/* Line 1787 of yacc.c  */
#line 60 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (1)]); }
    break;

  case 6:
/* Line 1787 of yacc.c  */
#line 64 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSymbolSection(currentLine); }
    break;

  case 7:
/* Line 1787 of yacc.c  */
#line 65 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSymbolSection(currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); }
    break;

  case 8:
/* Line 1787 of yacc.c  */
#line 70 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); }
    break;

  case 9:
/* Line 1787 of yacc.c  */
#line 71 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); }
    break;

  case 10:
/* Line 1787 of yacc.c  */
#line 75 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateTypeReference(CodeTreeTypeReferenceType::Int, currentLine); }
    break;

  case 11:
/* Line 1787 of yacc.c  */
#line 76 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateTypeReference(CodeTreeTypeReferenceType::Float, currentLine); }
    break;

  case 12:
/* Line 1787 of yacc.c  */
#line 77 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateTypeReference(CodeTreeTypeReferenceType::String, currentLine); }
    break;

  case 13:
/* Line 1787 of yacc.c  */
#line 82 "sheepParser.y"
    {
		SheepCodeTreeVariableDeclarationNode* decl = new SheepCodeTreeVariableDeclarationNode(currentLine);
		decl->VariableType = polymorphic_downcast<SheepCodeTreeSymbolTypeNode*>((yyvsp[(1) - (3)]));
		decl->FirstVariable = polymorphic_downcast<SheepCodeTreeVariableDeclarationNameAndValueNode*>((yyvsp[(2) - (3)]));

		(yyval) = decl;
	}
    break;

  case 14:
/* Line 1787 of yacc.c  */
#line 93 "sheepParser.y"
    {
		SheepCodeTreeVariableDeclarationNameAndValueNode* decl = new SheepCodeTreeVariableDeclarationNameAndValueNode(currentLine);
		decl->VariableName = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(1) - (1)]));
		 
		(yyval) = decl; 
	}
    break;

  case 15:
/* Line 1787 of yacc.c  */
#line 100 "sheepParser.y"
    {
		SheepCodeTreeVariableDeclarationNameAndValueNode* decl = new SheepCodeTreeVariableDeclarationNameAndValueNode(currentLine);
		decl->VariableName = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(1) - (3)]));
		decl->InitialValue = polymorphic_downcast<SheepCodeTreeConstantNode*>((yyvsp[(3) - (3)]));

		(yyval) = decl;
	}
    break;

  case 16:
/* Line 1787 of yacc.c  */
#line 108 "sheepParser.y"
    {
		SheepCodeTreeVariableDeclarationNameAndValueNode* decl = new SheepCodeTreeVariableDeclarationNameAndValueNode(currentLine);
		decl->VariableName = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(3) - (3)]));

		(yyval) = (yyvsp[(1) - (3)]); 
		(yyval)->AttachSibling(decl);
	}
    break;

  case 17:
/* Line 1787 of yacc.c  */
#line 116 "sheepParser.y"
    {
		SheepCodeTreeVariableDeclarationNameAndValueNode* decl = new SheepCodeTreeVariableDeclarationNameAndValueNode(currentLine);
		decl->VariableName = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(3) - (5)]));
		decl->InitialValue = polymorphic_downcast<SheepCodeTreeConstantNode*>((yyvsp[(5) - (5)]));

		(yyval) = (yyvsp[(1) - (5)]); 
		(yyval)->AttachSibling(decl);
	}
    break;

  case 18:
/* Line 1787 of yacc.c  */
#line 127 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateCodeSection(currentLine); }
    break;

  case 19:
/* Line 1787 of yacc.c  */
#line 128 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateCodeSection(currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); }
    break;

  case 20:
/* Line 1787 of yacc.c  */
#line 133 "sheepParser.y"
    {
		SheepCodeTreeFunctionListNode* function = SHEEP_NEW SheepCodeTreeFunctionListNode(currentLine);
		function->Functions.push_back(static_cast<SheepCodeTreeFunctionDeclarationNode*>((yyvsp[(1) - (1)])));
		 
		(yyval) = function;
	}
    break;

  case 21:
/* Line 1787 of yacc.c  */
#line 140 "sheepParser.y"
    { 
		SheepCodeTreeFunctionListNode* function = static_cast<SheepCodeTreeFunctionListNode*>((yyvsp[(1) - (2)]));
		function->Functions.push_back(static_cast<SheepCodeTreeFunctionDeclarationNode*>((yyvsp[(2) - (2)])));
		 
		(yyval) = function;
	}
    break;

  case 22:
/* Line 1787 of yacc.c  */
#line 150 "sheepParser.y"
    {
			SheepCodeTreeFunctionDeclarationNode* functionDecl = SHEEP_NEW SheepCodeTreeFunctionDeclarationNode(currentLine);

			functionDecl->Name = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(1) - (6)]));
			functionDecl->Parameters = polymorphic_downcast<SheepCodeTreeVariableListNode*>((yyvsp[(3) - (6)]));

			(yyval) = functionDecl;
		}
    break;

  case 23:
/* Line 1787 of yacc.c  */
#line 159 "sheepParser.y"
    {
			SheepCodeTreeFunctionDeclarationNode* functionDecl = SHEEP_NEW SheepCodeTreeFunctionDeclarationNode(currentLine);

			functionDecl->Name = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(1) - (7)]));
			functionDecl->Parameters = polymorphic_downcast<SheepCodeTreeVariableListNode*>((yyvsp[(3) - (7)]));
			functionDecl->FirstStatement = polymorphic_downcast<SheepCodeTreeStatementNode*>((yyvsp[(6) - (7)]));

			(yyval) = functionDecl;
		}
    break;

  case 24:
/* Line 1787 of yacc.c  */
#line 169 "sheepParser.y"
    { 
			SheepCodeTreeFunctionDeclarationNode* functionDecl = SHEEP_NEW SheepCodeTreeFunctionDeclarationNode(currentLine);

			functionDecl->ReturnType = polymorphic_downcast<SheepCodeTreeSymbolTypeNode*>((yyvsp[(1) - (7)]));
			functionDecl->Name = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(1) - (7)]));
			functionDecl->Parameters = polymorphic_downcast<SheepCodeTreeVariableListNode*>((yyvsp[(4) - (7)]));

			(yyval) = functionDecl;
		}
    break;

  case 25:
/* Line 1787 of yacc.c  */
#line 179 "sheepParser.y"
    {
			SheepCodeTreeFunctionDeclarationNode* functionDecl = SHEEP_NEW SheepCodeTreeFunctionDeclarationNode(currentLine);

			functionDecl->ReturnType = polymorphic_downcast<SheepCodeTreeSymbolTypeNode*>((yyvsp[(1) - (8)]));
			functionDecl->Name = polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(2) - (8)]));
			functionDecl->Parameters = polymorphic_downcast<SheepCodeTreeVariableListNode*>((yyvsp[(4) - (8)]));
			functionDecl->FirstStatement = polymorphic_downcast<SheepCodeTreeStatementNode*>((yyvsp[(7) - (8)]));

			(yyval) = functionDecl;
		}
    break;

  case 26:
/* Line 1787 of yacc.c  */
#line 192 "sheepParser.y"
    { (yyval) = NULL; }
    break;

  case 27:
/* Line 1787 of yacc.c  */
#line 193 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); }
    break;

  case 28:
/* Line 1787 of yacc.c  */
#line 198 "sheepParser.y"
    {
		SheepCodeTreeVariableListNode* variableList = SHEEP_NEW SheepCodeTreeVariableListNode(currentLine);

		variableList->ParameterTypes.push_back(polymorphic_downcast<SheepCodeTreeSymbolTypeNode*>((yyvsp[(1) - (2)])));
		variableList->ParameterNames.push_back(polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(2) - (2)])));

		(yyval) = variableList;
	}
    break;

  case 29:
/* Line 1787 of yacc.c  */
#line 207 "sheepParser.y"
    {
		SheepCodeTreeVariableListNode* variableList = polymorphic_downcast<SheepCodeTreeVariableListNode*>((yyvsp[(1) - (4)]));

		variableList->ParameterTypes.push_back(polymorphic_downcast<SheepCodeTreeSymbolTypeNode*>((yyvsp[(3) - (4)])));
		variableList->ParameterNames.push_back(polymorphic_downcast<SheepCodeTreeIdentifierReferenceNode*>((yyvsp[(4) - (4)])));

		(yyval) = (yyvsp[(1) - (4)]);
	}
    break;

  case 30:
/* Line 1787 of yacc.c  */
#line 218 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 31:
/* Line 1787 of yacc.c  */
#line 219 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); }
    break;

  case 33:
/* Line 1787 of yacc.c  */
#line 224 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateLabelDeclaration(currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (2)])); }
    break;

  case 34:
/* Line 1787 of yacc.c  */
#line 225 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Goto, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); }
    break;

  case 35:
/* Line 1787 of yacc.c  */
#line 226 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Expression, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (2)])); }
    break;

  case 36:
/* Line 1787 of yacc.c  */
#line 227 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Return, currentLine); }
    break;

  case 37:
/* Line 1787 of yacc.c  */
#line 228 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Return, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); }
    break;

  case 38:
/* Line 1787 of yacc.c  */
#line 229 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 39:
/* Line 1787 of yacc.c  */
#line 230 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Assignment, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); }
    break;

  case 40:
/* Line 1787 of yacc.c  */
#line 234 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Wait, currentLine); }
    break;

  case 41:
/* Line 1787 of yacc.c  */
#line 235 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Wait, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); }
    break;

  case 42:
/* Line 1787 of yacc.c  */
#line 236 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::Wait, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (5)])); }
    break;

  case 43:
/* Line 1787 of yacc.c  */
#line 242 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 44:
/* Line 1787 of yacc.c  */
#line 243 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 45:
/* Line 1787 of yacc.c  */
#line 247 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::If, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (5)])); (yyval)->SetChild(1, (yyvsp[(5) - (5)])); }
    break;

  case 46:
/* Line 1787 of yacc.c  */
#line 248 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::If, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (7)])); (yyval)->SetChild(1, (yyvsp[(5) - (7)])); (yyval)->SetChild(2, (yyvsp[(7) - (7)])); }
    break;

  case 47:
/* Line 1787 of yacc.c  */
#line 252 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 48:
/* Line 1787 of yacc.c  */
#line 253 "sheepParser.y"
    { (yyval) = NULL; }
    break;

  case 49:
/* Line 1787 of yacc.c  */
#line 254 "sheepParser.y"
    { (yyval) = (yyvsp[(2) - (3)]) ;}
    break;

  case 50:
/* Line 1787 of yacc.c  */
#line 255 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType::If, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (7)])); (yyval)->SetChild(1, (yyvsp[(5) - (7)])); (yyval)->SetChild(2, (yyvsp[(7) - (7)])); }
    break;

  case 51:
/* Line 1787 of yacc.c  */
#line 260 "sheepParser.y"
    {
		char errorBuffer[256];
		(yyval) = SheepCodeTreeNode::CreateIdentifierReference(yytext, false, currentLine, errorBuffer, 256); 
		if ((yyval) == NULL) { yyerror(errorBuffer); YYERROR; } 
	}
    break;

  case 52:
/* Line 1787 of yacc.c  */
#line 269 "sheepParser.y"
    {
		char errorBuffer[256];
		(yyval) = SheepCodeTreeNode::CreateIdentifierReference(yytext, true, currentLine, errorBuffer, 256); 
	}
    break;

  case 53:
/* Line 1787 of yacc.c  */
#line 276 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateIntegerConstant(atoi(yytext), currentLine); }
    break;

  case 54:
/* Line 1787 of yacc.c  */
#line 277 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateFloatConstant(atof(yytext), currentLine); }
    break;

  case 55:
/* Line 1787 of yacc.c  */
#line 278 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateStringConstant(removeQuotes(yytext), currentLine); }
    break;

  case 56:
/* Line 1787 of yacc.c  */
#line 282 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]) ;}
    break;

  case 57:
/* Line 1787 of yacc.c  */
#line 283 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (4)]); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); }
    break;

  case 58:
/* Line 1787 of yacc.c  */
#line 287 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 59:
/* Line 1787 of yacc.c  */
#line 288 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]); (yyval)->AttachSibling((yyvsp[(3) - (3)])); }
    break;

  case 60:
/* Line 1787 of yacc.c  */
#line 292 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 61:
/* Line 1787 of yacc.c  */
#line 293 "sheepParser.y"
    { (yyval)->AttachSibling((yyvsp[(3) - (3)])); }
    break;

  case 62:
/* Line 1787 of yacc.c  */
#line 297 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); }
    break;

  case 63:
/* Line 1787 of yacc.c  */
#line 298 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 64:
/* Line 1787 of yacc.c  */
#line 299 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 65:
/* Line 1787 of yacc.c  */
#line 300 "sheepParser.y"
    { (yyval) = (yyvsp[(2) - (3)]); }
    break;

  case 66:
/* Line 1787 of yacc.c  */
#line 301 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Not, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (2)])); }
    break;

  case 67:
/* Line 1787 of yacc.c  */
#line 302 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Negate, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (2)])); }
    break;

  case 68:
/* Line 1787 of yacc.c  */
#line 303 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Add, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); }
    break;

  case 69:
/* Line 1787 of yacc.c  */
#line 304 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Minus, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); }
    break;

  case 70:
/* Line 1787 of yacc.c  */
#line 305 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Times, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); }
    break;

  case 71:
/* Line 1787 of yacc.c  */
#line 306 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Divide, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));}
    break;

  case 72:
/* Line 1787 of yacc.c  */
#line 307 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::LessThan, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));}
    break;

  case 73:
/* Line 1787 of yacc.c  */
#line 308 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::GreaterThan, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));}
    break;

  case 74:
/* Line 1787 of yacc.c  */
#line 309 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::LessThanEqual, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (4)])); (yyval)->SetChild(1, (yyvsp[(4) - (4)]));}
    break;

  case 75:
/* Line 1787 of yacc.c  */
#line 310 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::GreaterThanEqual, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (4)])); (yyval)->SetChild(1, (yyvsp[(4) - (4)]));}
    break;

  case 76:
/* Line 1787 of yacc.c  */
#line 311 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Equal, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));}
    break;

  case 77:
/* Line 1787 of yacc.c  */
#line 312 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::NotEqual, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));}
    break;

  case 78:
/* Line 1787 of yacc.c  */
#line 313 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::Or, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); }
    break;

  case 79:
/* Line 1787 of yacc.c  */
#line 314 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(CodeTreeOperationType::And, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));  }
    break;


/* Line 1787 of yacc.c  */
#line 2136 "sheepParser.cpp"
      default: break;
    }
  /* User semantic actions sometimes alter yychar, and that requires
     that yytoken be updated with the new translation.  We take the
     approach of translating immediately before every use of yytoken.
     One alternative is translating here after every semantic action,
     but that translation would be missed if the semantic action invokes
     YYABORT, YYACCEPT, or YYERROR immediately after altering yychar or
     if it invokes YYBACKUP.  In the case of YYABORT or YYACCEPT, an
     incorrect destructor might then be invoked immediately.  In the
     case of YYERROR or YYBACKUP, subsequent parser actions might lead
     to an incorrect destructor call or verbose syntax error message
     before the lookahead is translated.  */
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
  /* Make sure we have latest lookahead translation.  See comments at
     user semantic actions for why this is necessary.  */
  yytoken = yychar == YYEMPTY ? YYEMPTY : YYTRANSLATE (yychar);

  /* If not already recovering from an error, report this error.  */
  if (!yyerrstatus)
    {
      ++yynerrs;
#if ! YYERROR_VERBOSE
      yyerror (YY_("syntax error"));
#else
# define YYSYNTAX_ERROR yysyntax_error (&yymsg_alloc, &yymsg, \
                                        yyssp, yytoken)
      {
        char const *yymsgp = YY_("syntax error");
        int yysyntax_error_status;
        yysyntax_error_status = YYSYNTAX_ERROR;
        if (yysyntax_error_status == 0)
          yymsgp = yymsg;
        else if (yysyntax_error_status == 1)
          {
            if (yymsg != yymsgbuf)
              YYSTACK_FREE (yymsg);
            yymsg = (char *) YYSTACK_ALLOC (yymsg_alloc);
            if (!yymsg)
              {
                yymsg = yymsgbuf;
                yymsg_alloc = sizeof yymsgbuf;
                yysyntax_error_status = 2;
              }
            else
              {
                yysyntax_error_status = YYSYNTAX_ERROR;
                yymsgp = yymsg;
              }
          }
        yyerror (yymsgp);
        if (yysyntax_error_status == 2)
          goto yyexhaustedlab;
      }
# undef YYSYNTAX_ERROR
#endif
    }



  if (yyerrstatus == 3)
    {
      /* If just tried and failed to reuse lookahead token after an
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

  /* Else will try to reuse lookahead token after shifting the error
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
      if (!yypact_value_is_default (yyn))
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

  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  *++yyvsp = yylval;
  YY_IGNORE_MAYBE_UNINITIALIZED_END


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

#if !defined yyoverflow || YYERROR_VERBOSE
/*-------------------------------------------------.
| yyexhaustedlab -- memory exhaustion comes here.  |
`-------------------------------------------------*/
yyexhaustedlab:
  yyerror (YY_("memory exhausted"));
  yyresult = 2;
  /* Fall through.  */
#endif

yyreturn:
  if (yychar != YYEMPTY)
    {
      /* Make sure we have latest lookahead translation.  See comments at
         user semantic actions for why this is necessary.  */
      yytoken = YYTRANSLATE (yychar);
      yydestruct ("Cleanup: discarding lookahead",
                  yytoken, &yylval);
    }
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


