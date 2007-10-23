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

char* removeQuotes(char* str)
{
	str[strlen(str)-1] = 0;
	return str+1;
}

%}

%token <id> IDENTIFIER <id> LOCALIDENTIFIER <intVal> INTEGER <floatVal> FLOAT <stringVal> STRING
%token INTSYM FLOATSYM STRINGSYM CODE SYMBOLS SNIP
%token RETURN IF ELSE
%token SEMICOLON DOLLAR LPAREN RPAREN LBRACE RBRACE QUOTE COMMA
%token EQUALS NOTEQUAL BECOMES PLUS MINUS TIMES DIVIDE LESSTHAN GREATERTHAN OR AND

%left EQUALS NOTEQUAL
%left LESSTHAN GREATERTHAN
%left PLUS MINUS
%left TIMES DIVIDE
%left AND
%left OR
%left BECOMES

%%

sheep:
	/* empty */
	| symbol_section code_section { g_codeTree = $1; if ($1 && $2) $1->AttachSibling($2); }
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
	| expr SEMICOLON { $$ = $1 }
	| local_identifier BECOMES expr { $$ = SheepCodeTreeNode::CreateOperation(OP_ASSIGN, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3); }
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
	| STRING { $$ = SheepCodeTreeNode::CreateStringConstant(removeQuotes(yytext), currentLine); }
	;

global_function_call:
	global_identifier LPAREN RPAREN { $$ = $1 }
	| global_identifier LPAREN parameter_list RPAREN { $$ = $1; $$->SetChild(0, $3); }
	;

parameter_list:
	expr { $$ = $1 }
	| parameter_list COMMA expr { $$->AttachSibling($3); }
	;

expr:
	constant { $$ = $1; }
	| global_function_call { $$ = $1 }
	| local_identifier { $$ = $1 }
	| LPAREN expr RPAREN { $$ = $2; }
	| expr PLUS expr { $$ = SheepCodeTreeNode::CreateOperation(OP_ADD, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3); }
	| expr MINUS expr { $$ = SheepCodeTreeNode::CreateOperation(OP_MINUS, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3); }
	| expr TIMES expr { $$ = SheepCodeTreeNode::CreateOperation(OP_TIMES, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3); }
	| expr DIVIDE expr { $$ = SheepCodeTreeNode::CreateOperation(OP_DIVIDE, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3);}
	| expr LESSTHAN expr { $$ = SheepCodeTreeNode::CreateOperation(OP_LT, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3);}
	| expr GREATERTHAN expr { $$ = SheepCodeTreeNode::CreateOperation(OP_GT, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3);}
	| expr LESSTHAN BECOMES expr { $$ = SheepCodeTreeNode::CreateOperation(OP_LTE, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $4);}
	| expr GREATERTHAN BECOMES expr { $$ = SheepCodeTreeNode::CreateOperation(OP_GTE, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $4);}
	| expr EQUALS expr { $$ = SheepCodeTreeNode::CreateOperation(OP_EQ, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3);}
	| expr NOTEQUAL expr { $$ = SheepCodeTreeNode::CreateOperation(OP_NE, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3);}
	;