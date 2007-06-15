%{

#include <stdio.h>
#include <string.h>
#include "symbols.h"

#define TRUE 1
#define FALSE 0

void yyerror(const char* str)
{
	printf("error: %s\n", str);
}

int yywrap()
{
	return 1;
}

%}

%union
{
	int intVal;
	float floatVal;
	char* stringVal;

	char* id;
}

%token <id> IDENTIFIER <id> LOCALIDENTIFIER <intVal> INTEGER <floatVal> FLOAT <stringVal> STRING
%token INTSYM FLOATSYM STRINGSYM CODE SYMBOLS
%token RETURN IF ELSE
%token SEMICOLON DOLLAR LPAREN RPAREN LBRACE RBRACE QUOTE COMMA
%token EQUALS BECOMES PLUS MINUS

%left PLUS
%left BECOMES

%%

sheep:
	/* empty */
	| symbs cde
	| symbs
	| cde
	;

symbs:
	SYMBOLS LBRACE symbol_list RBRACE
	;
	
symbol_list:
	/* empty */
	| symbol_list INTSYM LOCALIDENTIFIER SEMICOLON    { AddIntSymbol($3, 0); }
	| symbol_list FLOATSYM LOCALIDENTIFIER SEMICOLON  { AddFloatSymbol($3, 0); }
	| symbol_list STRINGSYM LOCALIDENTIFIER SEMICOLON { AddStringSymbol($3, NULL); }
	| symbol_list INTSYM LOCALIDENTIFIER BECOMES INTEGER SEMICOLON {AddIntSymbol($3, $5); }
	| symbol_list FLOATSYM LOCALIDENTIFIER BECOMES FLOAT SEMICOLON {AddFloatSymbol($3, $5); }	
	| symbol_list STRINGSYM LOCALIDENTIFIER BECOMES STRING SEMICOLON {AddStringSymbol($3, $5); }	
	;
	
cde:
	CODE LBRACE function_list RBRACE
	;
	
function_list:
	/* empty */
	| function_list LOCALIDENTIFIER LPAREN RPAREN LBRACE statement_list RBRACE
		{ AddLocalFunction($2, TRUE); }
	;
	
statement_list:
	/* empty */
	| statement_list statement
	;
	
statement:
	if_statement
	| LOCALIDENTIFIER BECOMES expr { AssignSymbolValue($1); }
	| expr SEMICOLON
	| RETURN SEMICOLON
	| block_statement
	;
	
if_statement:
	IF LPAREN expr RPAREN block_statement
	| IF LPAREN expr RPAREN block_statement ELSE block_statement
	| IF LPAREN expr RPAREN block_statement ELSE if_statement
	;

function_call:
	IDENTIFIER LPAREN expr_list RPAREN { AddFunctionCall($1); }
	;
	
block_statement:
	LBRACE statement_list RBRACE
	| SEMICOLON
	;
	
expr:
	INTEGER { AddIntegerToStack($1); }
	| STRING { AddStringToStack($1); }
	| function_call
	| LOCALIDENTIFIER { AddLocalValueToStack($1); }
	| expr PLUS expr
	;

expr_list:
	/* empty */
	| expr
	| expr_list COMMA expr
	;
