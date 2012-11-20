
/* A Bison parser, made by GNU Bison 2.4.1.  */

/* Skeleton implementation for Bison's Yacc-like parsers in C
   
      Copyright (C) 1984, 1989, 1990, 2000, 2001, 2002, 2003, 2004, 2005, 2006
   Free Software Foundation, Inc.
   
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
#define YYBISON_VERSION "2.4.1"

/* Skeleton name.  */
#define YYSKELETON_NAME "yacc.c"

/* Pure parsers.  */
#define YYPURE 0

/* Push parsers.  */
#define YYPUSH 0

/* Pull parsers.  */
#define YYPULL 1

/* Using locations.  */
#define YYLSP_NEEDED 0



/* Copy the first part of user declarations.  */

/* Line 189 of yacc.c  */
#line 1 "sheepParser.y"


#include <stdio.h>
#include <string.h>
#include "symbols.h"
#include "sheepCodeTree.h"
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



/* Line 189 of yacc.c  */
#line 110 "sheepParser.cpp"

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


/* Copy the second part of user declarations.  */


/* Line 264 of yacc.c  */
#line 190 "sheepParser.cpp"

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

/* YYFINAL -- State number of the termination state.  */
#define YYFINAL  8
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   286

/* YYNTOKENS -- Number of terminals.  */
#define YYNTOKENS  39
/* YYNNTS -- Number of nonterminals.  */
#define YYNNTS  23
/* YYNRULES -- Number of rules.  */
#define YYNRULES  70
/* YYNRULES -- Number of states.  */
#define YYNSTATES  125

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
static const yytype_uint8 yyprhs[] =
{
       0,     0,     3,     4,     7,     9,    11,    15,    20,    22,
      25,    27,    29,    31,    35,    37,    41,    45,    51,    55,
      60,    62,    65,    71,    78,    80,    83,    85,    88,    92,
      95,    98,   100,   104,   107,   111,   117,   119,   121,   127,
     135,   137,   141,   149,   151,   153,   155,   157,   159,   163,
     168,   170,   174,   176,   180,   182,   184,   186,   190,   193,
     197,   201,   205,   209,   213,   217,   222,   227,   231,   235,
     239
};

/* YYRHS -- A `-1'-separated list of the rules' RHS.  */
static const yytype_int8 yyrhs[] =
{
      40,     0,    -1,    -1,    41,    46,    -1,    41,    -1,    46,
      -1,    12,    23,    24,    -1,    12,    23,    42,    24,    -1,
      44,    -1,    42,    44,    -1,     8,    -1,     9,    -1,    10,
      -1,    43,    45,    19,    -1,    55,    -1,    55,    29,    57,
      -1,    45,    26,    55,    -1,    45,    26,    55,    29,    57,
      -1,    11,    23,    24,    -1,    11,    23,    47,    24,    -1,
      48,    -1,    47,    48,    -1,    55,    21,    22,    23,    24,
      -1,    55,    21,    22,    23,    49,    24,    -1,    52,    -1,
      49,    52,    -1,    19,    -1,    55,    18,    -1,    17,    55,
      19,    -1,    61,    19,    -1,    14,    19,    -1,    51,    -1,
      55,    29,    61,    -1,    13,    19,    -1,    13,    58,    19,
      -1,    13,    23,    59,    19,    24,    -1,    53,    -1,    54,
      -1,    15,    21,    61,    22,    52,    -1,    15,    21,    61,
      22,    54,    16,    53,    -1,    50,    -1,    23,    49,    24,
      -1,    15,    21,    61,    22,    54,    16,    54,    -1,     4,
      -1,     3,    -1,     5,    -1,     6,    -1,     7,    -1,    56,
      21,    22,    -1,    56,    21,    60,    22,    -1,    58,    -1,
      59,    19,    58,    -1,    61,    -1,    60,    26,    61,    -1,
      57,    -1,    58,    -1,    55,    -1,    21,    61,    22,    -1,
      36,    61,    -1,    61,    30,    61,    -1,    61,    31,    61,
      -1,    61,    32,    61,    -1,    61,    33,    61,    -1,    61,
      34,    61,    -1,    61,    35,    61,    -1,    61,    34,    29,
      61,    -1,    61,    35,    29,    61,    -1,    61,    27,    61,
      -1,    61,    28,    61,    -1,    61,    37,    61,    -1,    61,
      38,    61,    -1
};

/* YYRLINE[YYN] -- source line where rule number YYN was defined.  */
static const yytype_uint8 yyrline[] =
{
       0,    54,    54,    56,    57,    58,    62,    63,    68,    69,
      73,    74,    75,    79,    88,    89,    90,    91,    95,    96,
     100,   101,   105,   106,   111,   112,   116,   117,   118,   119,
     120,   121,   122,   126,   127,   128,   134,   135,   139,   140,
     144,   145,   146,   150,   159,   167,   168,   169,   173,   174,
     178,   179,   183,   184,   188,   189,   190,   191,   192,   193,
     194,   195,   196,   197,   198,   199,   200,   201,   202,   203,
     204
};
#endif

#if YYDEBUG || YYERROR_VERBOSE || YYTOKEN_TABLE
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
     285,   286,   287,   288,   289,   290,   291,   292,   293
};
# endif

/* YYR1[YYN] -- Symbol number of symbol that rule YYN derives.  */
static const yytype_uint8 yyr1[] =
{
       0,    39,    40,    40,    40,    40,    41,    41,    42,    42,
      43,    43,    43,    44,    45,    45,    45,    45,    46,    46,
      47,    47,    48,    48,    49,    49,    50,    50,    50,    50,
      50,    50,    50,    51,    51,    51,    52,    52,    53,    53,
      54,    54,    54,    55,    56,    57,    57,    57,    58,    58,
      59,    59,    60,    60,    61,    61,    61,    61,    61,    61,
      61,    61,    61,    61,    61,    61,    61,    61,    61,    61,
      61
};

/* YYR2[YYN] -- Number of symbols composing right hand side of rule YYN.  */
static const yytype_uint8 yyr2[] =
{
       0,     2,     0,     2,     1,     1,     3,     4,     1,     2,
       1,     1,     1,     3,     1,     3,     3,     5,     3,     4,
       1,     2,     5,     6,     1,     2,     1,     2,     3,     2,
       2,     1,     3,     2,     3,     5,     1,     1,     5,     7,
       1,     3,     7,     1,     1,     1,     1,     1,     3,     4,
       1,     3,     1,     3,     1,     1,     1,     3,     2,     3,
       3,     3,     3,     3,     3,     4,     4,     3,     3,     3,
       3
};

/* YYDEFACT[STATE-NAME] -- Default rule to reduce with in state
   STATE-NUM when YYTABLE doesn't specify something else to do.  Zero
   means the default is an error.  */
static const yytype_uint8 yydefact[] =
{
       2,     0,     0,     0,     4,     5,     0,     0,     1,     3,
      43,    18,     0,    20,     0,    10,    11,    12,     6,     0,
       0,     8,    19,    21,     0,     7,     9,     0,    14,     0,
      13,     0,     0,     0,    16,    45,    46,    47,    15,    44,
       0,     0,     0,     0,    26,     0,     0,    22,     0,     0,
      40,    31,    24,    36,    37,    56,     0,    54,    55,     0,
       0,    33,     0,     0,    30,     0,     0,    56,     0,     0,
      58,    23,    25,    27,     0,     0,    29,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,    17,    50,     0,
      34,     0,    28,    57,    41,    32,    48,     0,    52,    67,
      68,    59,    60,    61,    62,     0,    63,     0,    64,    69,
      70,     0,     0,    49,     0,    65,    66,    35,    51,    38,
      37,    53,     0,    39,    42
};

/* YYDEFGOTO[NTERM-NUM].  */
static const yytype_int8 yydefgoto[] =
{
      -1,     3,     4,    19,    20,    21,    27,     5,    12,    13,
      49,    50,    51,    52,    53,    54,    67,    56,    57,    58,
      89,    97,    59
};

/* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
   STATE-NUM.  */
#define YYPACT_NINF -106
static const yytype_int16 yypact[] =
{
       9,   -19,   -10,    66,    62,  -106,    12,    99,  -106,  -106,
    -106,  -106,    15,  -106,    17,  -106,  -106,  -106,  -106,   116,
      70,  -106,  -106,  -106,    61,  -106,  -106,    38,    68,    82,
    -106,    70,   107,    75,    81,  -106,  -106,  -106,  -106,  -106,
      39,    74,    94,    70,  -106,    25,   162,  -106,    25,   114,
    -106,  -106,  -106,  -106,  -106,    58,   101,  -106,  -106,   190,
     107,  -106,   127,   113,  -106,    25,   115,  -106,   204,   138,
    -106,  -106,  -106,  -106,    25,    64,  -106,    25,    25,    25,
      25,    25,    25,     5,   183,    25,    25,  -106,  -106,   117,
    -106,   218,  -106,  -106,  -106,   230,  -106,    69,   230,   161,
     161,    27,    27,  -106,  -106,    25,    71,    25,    71,   242,
     251,     0,   162,  -106,    25,   230,   230,  -106,  -106,  -106,
     123,   230,   162,  -106,  -106
};

/* YYPGOTO[NTERM-NUM].  */
static const yytype_int16 yypgoto[] =
{
    -106,  -106,  -106,  -106,  -106,   128,  -106,   142,  -106,   136,
     103,  -106,  -106,   -47,    32,  -105,    -6,  -106,   -27,   -39,
    -106,  -106,   -30
};

/* YYTABLE[YYPACT[STATE-NUM]].  What to do in state STATE-NUM.  If
   positive, shift that token.  If negative, reduce the rule which
   number is the opposite.  If zero, do what YYDEFACT says.
   If YYTABLE_NINF, syntax error.  */
#define YYTABLE_NINF -1
static const yytype_uint8 yytable[] =
{
      14,    63,    72,    39,     6,    38,    14,   120,    39,    10,
      35,    36,    37,     7,    28,    68,    10,   124,    70,    10,
       1,     2,    72,    88,   117,    34,    45,    55,    39,    10,
      35,    36,    37,    87,   105,    91,    11,    66,    24,    22,
      55,    48,    39,    55,    95,    98,    45,    99,   100,   101,
     102,   103,   104,   106,   108,   109,   110,    30,    61,    81,
      82,    48,    62,    55,    31,   119,     8,    39,    10,    35,
      36,    37,   118,     1,    10,   115,    73,   116,    39,    10,
      35,    36,    37,    29,   121,    45,    96,    74,    40,    41,
      42,   113,    43,    64,    44,   114,    45,    32,    46,    47,
      48,    79,    80,    81,    82,    33,    55,    15,    16,    17,
      60,    48,    35,    36,    37,    65,    55,    39,    10,    35,
      36,    37,    75,    18,    15,    16,    17,    40,    41,    42,
      39,    43,    90,    44,    92,    45,   111,    46,    71,   122,
      25,    39,    10,    35,    36,    37,     9,    26,    23,    69,
      48,    40,    41,    42,   123,    43,     0,    44,     0,    45,
       0,    46,    94,     0,     0,    39,    10,    35,    36,    37,
       0,     0,     0,     0,    48,    40,    41,    42,     0,    43,
       0,    44,     0,    45,     0,    46,    39,    10,    35,    36,
      37,    79,    80,    81,    82,    83,    84,     0,    48,     0,
       0,     0,     0,     0,    45,     0,     0,     0,     0,    76,
       0,     0,   107,     0,     0,     0,     0,    77,    78,    48,
      79,    80,    81,    82,    83,    84,    93,    85,    86,     0,
       0,    77,    78,     0,    79,    80,    81,    82,    83,    84,
     112,    85,    86,     0,     0,    77,    78,     0,    79,    80,
      81,    82,    83,    84,     0,    85,    86,    77,    78,     0,
      79,    80,    81,    82,    83,    84,     0,    85,    86,    77,
      78,     0,    79,    80,    81,    82,    83,    84,    77,    78,
      86,    79,    80,    81,    82,    83,    84
};

static const yytype_int8 yycheck[] =
{
       6,    40,    49,     3,    23,    32,    12,   112,     3,     4,
       5,     6,     7,    23,    20,    45,     4,   122,    48,     4,
      11,    12,    69,    62,    24,    31,    21,    33,     3,     4,
       5,     6,     7,    60,    29,    65,    24,    43,    21,    24,
      46,    36,     3,    49,    74,    75,    21,    77,    78,    79,
      80,    81,    82,    83,    84,    85,    86,    19,    19,    32,
      33,    36,    23,    69,    26,   112,     0,     3,     4,     5,
       6,     7,   111,    11,     4,   105,    18,   107,     3,     4,
       5,     6,     7,    22,   114,    21,    22,    29,    13,    14,
      15,    22,    17,    19,    19,    26,    21,    29,    23,    24,
      36,    30,    31,    32,    33,    23,   112,     8,     9,    10,
      29,    36,     5,     6,     7,    21,   122,     3,     4,     5,
       6,     7,    21,    24,     8,     9,    10,    13,    14,    15,
       3,    17,    19,    19,    19,    21,    19,    23,    24,    16,
      24,     3,     4,     5,     6,     7,     4,    19,    12,    46,
      36,    13,    14,    15,   122,    17,    -1,    19,    -1,    21,
      -1,    23,    24,    -1,    -1,     3,     4,     5,     6,     7,
      -1,    -1,    -1,    -1,    36,    13,    14,    15,    -1,    17,
      -1,    19,    -1,    21,    -1,    23,     3,     4,     5,     6,
       7,    30,    31,    32,    33,    34,    35,    -1,    36,    -1,
      -1,    -1,    -1,    -1,    21,    -1,    -1,    -1,    -1,    19,
      -1,    -1,    29,    -1,    -1,    -1,    -1,    27,    28,    36,
      30,    31,    32,    33,    34,    35,    22,    37,    38,    -1,
      -1,    27,    28,    -1,    30,    31,    32,    33,    34,    35,
      22,    37,    38,    -1,    -1,    27,    28,    -1,    30,    31,
      32,    33,    34,    35,    -1,    37,    38,    27,    28,    -1,
      30,    31,    32,    33,    34,    35,    -1,    37,    38,    27,
      28,    -1,    30,    31,    32,    33,    34,    35,    27,    28,
      38,    30,    31,    32,    33,    34,    35
};

/* YYSTOS[STATE-NUM] -- The (internal number of the) accessing
   symbol of state STATE-NUM.  */
static const yytype_uint8 yystos[] =
{
       0,    11,    12,    40,    41,    46,    23,    23,     0,    46,
       4,    24,    47,    48,    55,     8,     9,    10,    24,    42,
      43,    44,    24,    48,    21,    24,    44,    45,    55,    22,
      19,    26,    29,    23,    55,     5,     6,     7,    57,     3,
      13,    14,    15,    17,    19,    21,    23,    24,    36,    49,
      50,    51,    52,    53,    54,    55,    56,    57,    58,    61,
      29,    19,    23,    58,    19,    21,    55,    55,    61,    49,
      61,    24,    52,    18,    29,    21,    19,    27,    28,    30,
      31,    32,    33,    34,    35,    37,    38,    57,    58,    59,
      19,    61,    19,    22,    24,    61,    22,    60,    61,    61,
      61,    61,    61,    61,    61,    29,    61,    29,    61,    61,
      61,    19,    22,    22,    26,    61,    61,    24,    58,    52,
      54,    61,    16,    53,    54
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


/* The lookahead symbol.  */
int yychar;

/* The semantic value of the lookahead symbol.  */
YYSTYPE yylval;

/* Number of syntax errors so far.  */
int yynerrs;



/*-------------------------.
| yyparse or yypush_parse.  |
`-------------------------*/

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

       Refer to the stacks thru separate pointers, to allow yyoverflow
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
  int yytoken;
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

  yytoken = 0;
  yyss = yyssa;
  yyvs = yyvsa;
  yystacksize = YYINITDEPTH;

  YYDPRINTF ((stderr, "Starting parse\n"));

  yystate = 0;
  yyerrstatus = 0;
  yynerrs = 0;
  yychar = YYEMPTY; /* Cause a token to be read.  */

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
  if (yyn == YYPACT_NINF)
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
      if (yyn == 0 || yyn == YYTABLE_NINF)
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

/* Line 1455 of yacc.c  */
#line 56 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (2)]); if ((yyvsp[(1) - (2)]) && (yyvsp[(2) - (2)])) (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); ;}
    break;

  case 4:

/* Line 1455 of yacc.c  */
#line 57 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (1)]); ;}
    break;

  case 5:

/* Line 1455 of yacc.c  */
#line 58 "sheepParser.y"
    { g_codeTreeRoot = (yyvsp[(1) - (1)]); ;}
    break;

  case 6:

/* Line 1455 of yacc.c  */
#line 62 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSymbolSection(currentLine); ;}
    break;

  case 7:

/* Line 1455 of yacc.c  */
#line 63 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateSymbolSection(currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); ;}
    break;

  case 8:

/* Line 1455 of yacc.c  */
#line 68 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); ;}
    break;

  case 9:

/* Line 1455 of yacc.c  */
#line 69 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); ;}
    break;

  case 10:

/* Line 1455 of yacc.c  */
#line 73 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateTypeReference(TYPE_INT, currentLine); ;}
    break;

  case 11:

/* Line 1455 of yacc.c  */
#line 74 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateTypeReference(TYPE_FLOAT, currentLine); ;}
    break;

  case 12:

/* Line 1455 of yacc.c  */
#line 75 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateTypeReference(TYPE_STRING, currentLine); ;}
    break;

  case 13:

/* Line 1455 of yacc.c  */
#line 80 "sheepParser.y"
    {
		(yyval) = SheepCodeTreeNode::CreateVariableDeclaration(currentLine);
		(yyval)->SetChild(0, (yyvsp[(2) - (3)]));
		(yyval)->SetChild(1, (yyvsp[(1) - (3)]));
	;}
    break;

  case 14:

/* Line 1455 of yacc.c  */
#line 88 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); ;}
    break;

  case 15:

/* Line 1455 of yacc.c  */
#line 89 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]);  (yyval)->SetChild(0, (yyvsp[(3) - (3)])); ;}
    break;

  case 16:

/* Line 1455 of yacc.c  */
#line 90 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]); (yyval)->AttachSibling((yyvsp[(3) - (3)])); ;}
    break;

  case 17:

/* Line 1455 of yacc.c  */
#line 91 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (5)]); (yyval)->AttachSibling((yyvsp[(3) - (5)])); (yyvsp[(3) - (5)])->SetChild(0, (yyvsp[(5) - (5)])); ;}
    break;

  case 18:

/* Line 1455 of yacc.c  */
#line 95 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateCodeSection(currentLine); ;}
    break;

  case 19:

/* Line 1455 of yacc.c  */
#line 96 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateCodeSection(currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); ;}
    break;

  case 20:

/* Line 1455 of yacc.c  */
#line 100 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 21:

/* Line 1455 of yacc.c  */
#line 101 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); ;}
    break;

  case 22:

/* Line 1455 of yacc.c  */
#line 105 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateFunctionDeclaration(currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (5)])); ;}
    break;

  case 23:

/* Line 1455 of yacc.c  */
#line 106 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateFunctionDeclaration(currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (6)])); (yyval)->SetChild(1, (yyvsp[(5) - (6)])); ;}
    break;

  case 24:

/* Line 1455 of yacc.c  */
#line 111 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 25:

/* Line 1455 of yacc.c  */
#line 112 "sheepParser.y"
    { (yyvsp[(1) - (2)])->AttachSibling((yyvsp[(2) - (2)])); (yyval) = (yyvsp[(1) - (2)]); ;}
    break;

  case 27:

/* Line 1455 of yacc.c  */
#line 117 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateLabelDeclaration(currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (2)])); ;}
    break;

  case 28:

/* Line 1455 of yacc.c  */
#line 118 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_GOTO, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); ;}
    break;

  case 29:

/* Line 1455 of yacc.c  */
#line 119 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_EXPR, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (2)])); ;}
    break;

  case 30:

/* Line 1455 of yacc.c  */
#line 120 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_RETURN, currentLine); ;}
    break;

  case 31:

/* Line 1455 of yacc.c  */
#line 121 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 32:

/* Line 1455 of yacc.c  */
#line 122 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_ASSIGN, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 33:

/* Line 1455 of yacc.c  */
#line 126 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); ;}
    break;

  case 34:

/* Line 1455 of yacc.c  */
#line 127 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (3)])); ;}
    break;

  case 35:

/* Line 1455 of yacc.c  */
#line 128 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (5)])); ;}
    break;

  case 36:

/* Line 1455 of yacc.c  */
#line 134 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 37:

/* Line 1455 of yacc.c  */
#line 135 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 38:

/* Line 1455 of yacc.c  */
#line 139 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (5)])); (yyval)->SetChild(1, (yyvsp[(5) - (5)])); ;}
    break;

  case 39:

/* Line 1455 of yacc.c  */
#line 140 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (7)])); (yyval)->SetChild(1, (yyvsp[(5) - (7)])); (yyval)->SetChild(2, (yyvsp[(7) - (7)])); ;}
    break;

  case 40:

/* Line 1455 of yacc.c  */
#line 144 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 41:

/* Line 1455 of yacc.c  */
#line 145 "sheepParser.y"
    { (yyval) = (yyvsp[(2) - (3)]) ;}
    break;

  case 42:

/* Line 1455 of yacc.c  */
#line 146 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); (yyval)->SetChild(0, (yyvsp[(3) - (7)])); (yyval)->SetChild(1, (yyvsp[(5) - (7)])); (yyval)->SetChild(2, (yyvsp[(7) - (7)])); ;}
    break;

  case 43:

/* Line 1455 of yacc.c  */
#line 151 "sheepParser.y"
    {
		char errorBuffer[256];
		(yyval) = SheepCodeTreeNode::CreateIdentifierReference(yytext, false, currentLine, errorBuffer, 256); 
		if ((yyval) == NULL) { yyerror(errorBuffer); YYERROR; } 
	;}
    break;

  case 44:

/* Line 1455 of yacc.c  */
#line 160 "sheepParser.y"
    {
		char errorBuffer[256];
		(yyval) = SheepCodeTreeNode::CreateIdentifierReference(yytext, true, currentLine, errorBuffer, 256); 
	;}
    break;

  case 45:

/* Line 1455 of yacc.c  */
#line 167 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateIntegerConstant(atoi(yytext), currentLine); ;}
    break;

  case 46:

/* Line 1455 of yacc.c  */
#line 168 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateFloatConstant(atof(yytext), currentLine); ;}
    break;

  case 47:

/* Line 1455 of yacc.c  */
#line 169 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateStringConstant(removeQuotes(yytext), currentLine); ;}
    break;

  case 48:

/* Line 1455 of yacc.c  */
#line 173 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]) ;}
    break;

  case 49:

/* Line 1455 of yacc.c  */
#line 174 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (4)]); (yyval)->SetChild(0, (yyvsp[(3) - (4)])); ;}
    break;

  case 50:

/* Line 1455 of yacc.c  */
#line 178 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 51:

/* Line 1455 of yacc.c  */
#line 179 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (3)]); (yyval)->AttachSibling((yyvsp[(3) - (3)])); ;}
    break;

  case 52:

/* Line 1455 of yacc.c  */
#line 183 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 53:

/* Line 1455 of yacc.c  */
#line 184 "sheepParser.y"
    { (yyval)->AttachSibling((yyvsp[(3) - (3)])); ;}
    break;

  case 54:

/* Line 1455 of yacc.c  */
#line 188 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]); ;}
    break;

  case 55:

/* Line 1455 of yacc.c  */
#line 189 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 56:

/* Line 1455 of yacc.c  */
#line 190 "sheepParser.y"
    { (yyval) = (yyvsp[(1) - (1)]) ;}
    break;

  case 57:

/* Line 1455 of yacc.c  */
#line 191 "sheepParser.y"
    { (yyval) = (yyvsp[(2) - (3)]); ;}
    break;

  case 58:

/* Line 1455 of yacc.c  */
#line 192 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_NOT, currentLine); (yyval)->SetChild(0, (yyvsp[(2) - (2)])); ;}
    break;

  case 59:

/* Line 1455 of yacc.c  */
#line 193 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_ADD, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 60:

/* Line 1455 of yacc.c  */
#line 194 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_MINUS, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 61:

/* Line 1455 of yacc.c  */
#line 195 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_TIMES, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 62:

/* Line 1455 of yacc.c  */
#line 196 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_DIVIDE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 63:

/* Line 1455 of yacc.c  */
#line 197 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_LT, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 64:

/* Line 1455 of yacc.c  */
#line 198 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_GT, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 65:

/* Line 1455 of yacc.c  */
#line 199 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_LTE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (4)])); (yyval)->SetChild(1, (yyvsp[(4) - (4)]));;}
    break;

  case 66:

/* Line 1455 of yacc.c  */
#line 200 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_GTE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (4)])); (yyval)->SetChild(1, (yyvsp[(4) - (4)]));;}
    break;

  case 67:

/* Line 1455 of yacc.c  */
#line 201 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_EQ, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 68:

/* Line 1455 of yacc.c  */
#line 202 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_NE, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));;}
    break;

  case 69:

/* Line 1455 of yacc.c  */
#line 203 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_OR, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)])); ;}
    break;

  case 70:

/* Line 1455 of yacc.c  */
#line 204 "sheepParser.y"
    { (yyval) = SheepCodeTreeNode::CreateOperation(OP_AND, currentLine); (yyval)->SetChild(0, (yyvsp[(1) - (3)])); (yyval)->SetChild(1, (yyvsp[(3) - (3)]));  ;}
    break;



/* Line 1455 of yacc.c  */
#line 2012 "sheepParser.cpp"
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

#if !defined(yyoverflow) || YYERROR_VERBOSE
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



