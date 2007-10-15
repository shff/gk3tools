%{

#include <stdio.h>
#include <string.h>
#include "symbols.h"
#include "sheepCodeTree.h"
#define YYSTYPE SheepCodeTreeNode*

#define TRUE 1
#define FALSE 0

extern SheepCodeTreeNode* g_codeTree;
extern int currentLine;

void yyerror(const char* str)
{
	printf("error at line %d: %s\n", currentLine, str);
}

%}

%token <id> IDENTIFIER <id> LOCALIDENTIFIER <intVal> INTEGER <floatVal> FLOAT <stringVal> STRING
%token INTSYM FLOATSYM STRINGSYM CODE SYMBOLS SNIP
%token RETURN IF ELSE
%token SEMICOLON DOLLAR LPAREN RPAREN LBRACE RBRACE QUOTE COMMA
%token EQUALS BECOMES PLUS MINUS MULTIPLY DIVIDE LESSTHAN GREATERTHAN OR AND

%left LESSTHAN GREATERTHAN
%left PLUS MINUS
%left MULTIPLY DIVIDE
%left AND
%left OR
%left BECOMES

%%

sheep:
	/* empty */
	| symbol_section code_section { g_codeTree = $1; $1->AttachSibling($2); }
	| symbol_section { g_codeTree = $1; }
	| code_section { g_codeTree = $1; }
	;

symbol_section:
	SYMBOLS LBRACE RBRACE
	| SYMBOLS LBRACE symbol_list RBRACE { $$ = $3; }
	;
	

symbol_list:
	symbol_declaration { $$ = $1; }
	| symbol_list symbol_declaration { $1->AttachSibling($2); $$ = $1; }
	;
	
symbol_declaration:
	INTSYM local_identifier SEMICOLON { $$ = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_INT, currentLine); $$->SetChild(0, $2); }
	| INTSYM local_identifier BECOMES constant SEMICOLON{ $$ = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_INT, currentLine); $$->SetChild(0, $2); $$->SetChild(1, $4); }
	| FLOATSYM local_identifier SEMICOLON { $$ = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FLOAT, currentLine); $$->SetChild(0, $2); }
	| FLOATSYM local_identifier BECOMES constant SEMICOLON { $$ = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FLOAT, currentLine); $$->SetChild(0, $2); $$->SetChild(1, $4); }
	;
	
code_section:
	CODE LBRACE RBRACE
	| CODE LBRACE function_list RBRACE { $$ = $3 }
	;
	
function_list:
	function { $$ = $1 }
	| function_list function { $1->AttachSibling($2); $$ = $1; }
	;

function:
	local_identifier LPAREN RPAREN LBRACE RBRACE { $$ = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FUNCTION, currentLine); $$->SetChild(0, $1); }
	| local_identifier LPAREN RPAREN LBRACE statement_list RBRACE { $$ = SheepCodeTreeNode::CreateDeclaration(DECLARATIONTYPE_FUNCTION, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $5); }
	;
	
statement_list:
	statement { $$ = $1 }
	| statement_list statement { $1->AttachSibling($2); $$ = $1; }
	;
	
statement:
	SEMICOLON
	| global_identifier LPAREN RPAREN SEMICOLON { $$ = $1 }
	;
	
local_identifier:
	LOCALIDENTIFIER { $$ = SheepCodeTreeNode::CreateIdentifierReference(yytext, false, currentLine); }
	;
	
global_identifier:
	IDENTIFIER { $$ = SheepCodeTreeNode::CreateIdentifierReference(yytext, true, currentLine); }
	;
	
constant:
	INTEGER { $$ = SheepCodeTreeNode::CreateIntegerConstant(atoi(yytext), currentLine); }
	| FLOAT { $$ = SheepCodeTreeNode::CreateFloatConstant(atof(yytext), currentLine); }
	;
