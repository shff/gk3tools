%{

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

%}

%token IDENTIFIER LOCALIDENTIFIER INTEGER FLOAT STRING
%token INTSYM FLOATSYM STRINGSYM CODE SYMBOLS WAIT
%token RETURN IF ELSE GOTO
%token COLON SEMICOLON DOLLAR LPAREN RPAREN LBRACE RBRACE QUOTE COMMA
%token EQUALS NOTEQUAL BECOMES PLUS MINUS TIMES DIVIDE LESSTHAN GREATERTHAN NOT OR AND

%left BECOMES
%left OR
%left AND
%left EQUALS NOTEQUAL
%left LESSTHAN GREATERTHAN
%left PLUS MINUS
%left TIMES DIVIDE
%left NOT

%%

sheep:
	/* empty */
	| symbol_section code_section { g_codeTreeRoot = $1; if ($1 && $2) $1->AttachSibling($2); }
	| symbol_section { g_codeTreeRoot = $1; }
	| code_section { g_codeTreeRoot = $1; }
	;

symbol_section:
	SYMBOLS LBRACE RBRACE { $$ = SheepCodeTreeNode::CreateSymbolSection(currentLine); }
	| SYMBOLS LBRACE symbol_list RBRACE { $$ = SheepCodeTreeNode::CreateSymbolSection(currentLine); $$->SetChild(0, $3); }
	;
	

symbol_list:
	symbol_declaration { $$ = $1; }
	| symbol_list symbol_declaration { $1->AttachSibling($2); $$ = $1; }
	;
	
symbol_type:
	INTSYM { $$ = SheepCodeTreeNode::CreateTypeReference(TYPE_INT, currentLine); }
	| FLOATSYM { $$ = SheepCodeTreeNode::CreateTypeReference(TYPE_FLOAT, currentLine); }
	| STRINGSYM { $$ = SheepCodeTreeNode::CreateTypeReference(TYPE_STRING, currentLine); }
	;
	
symbol_declaration:
	symbol_type symbol_declaration_list SEMICOLON
	{
		$$ = SheepCodeTreeNode::CreateVariableDeclaration(currentLine);
		$$->SetChild(0, $2);
		$$->SetChild(1, $1);
	}
	;
	
symbol_declaration_list:
	local_identifier { $$ = $1; }
	| local_identifier BECOMES constant { $$ = $1;  $$->SetChild(0, $3); }
	| symbol_declaration_list COMMA local_identifier { $$ = $1; $$->AttachSibling($3); }
	| symbol_declaration_list COMMA local_identifier BECOMES constant { $$ = $1; $$->AttachSibling($3); $3->SetChild(0, $5); }
	;
	
code_section:
	CODE LBRACE RBRACE { $$ = SheepCodeTreeNode::CreateCodeSection(currentLine); }
	| CODE LBRACE function_list RBRACE { $$ = SheepCodeTreeNode::CreateCodeSection(currentLine); $$->SetChild(0, $3); }
	;
	
function_list:
	function { $$ = $1 }
	| function_list function { $1->AttachSibling($2); $$ = $1; }
	;

function:
	local_identifier LPAREN RPAREN LBRACE RBRACE { $$ = SheepCodeTreeNode::CreateFunctionDeclaration(currentLine); $$->SetChild(0, $1); }
	| local_identifier LPAREN RPAREN LBRACE statement_list RBRACE { $$ = SheepCodeTreeNode::CreateFunctionDeclaration(currentLine); $$->SetChild(0, $1); $$->SetChild(1, $5); }
	;
	
	
statement_list:
	statement { $$ = $1 }
	| statement_list statement { $1->AttachSibling($2); $$ = $1; }
	;
	
simple_statement:
	SEMICOLON
	| local_identifier COLON { $$ = SheepCodeTreeNode::CreateLabelDeclaration(currentLine); $$->SetChild(0, $1); }
	| GOTO local_identifier SEMICOLON { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_GOTO, currentLine); $$->SetChild(0, $2); }
	| expr SEMICOLON { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_EXPR, currentLine); $$->SetChild(0, $1); }
	| RETURN SEMICOLON { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_RETURN, currentLine); }
	| wait_statement { $$ = $1 }
	| local_identifier BECOMES expr { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_ASSIGN, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3); }
	;

wait_statement:
	WAIT SEMICOLON { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); }
	| WAIT global_function_call SEMICOLON { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); $$->SetChild(0, $2); }
	| WAIT LBRACE global_function_call_list SEMICOLON RBRACE { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_WAIT, currentLine); $$->SetChild(0, $3); }
	;

/* this "open" and "closed" stuff can be found here:
http://www.parsifalsoft.com/ifelse.html */
statement:
	open_statement { $$ = $1 }
	| closed_statement { $$ = $1 }
	;
	
open_statement:
	IF LPAREN expr RPAREN statement { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); $$->SetChild(0, $3); $$->SetChild(1, $5); }
	| IF LPAREN expr RPAREN closed_statement ELSE open_statement { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); $$->SetChild(0, $3); $$->SetChild(1, $5); $$->SetChild(2, $7); }
	;
	
closed_statement:
	simple_statement { $$ = $1 }
	| LBRACE statement_list RBRACE { $$ = $2 }
	| IF LPAREN expr RPAREN closed_statement ELSE closed_statement { $$ = SheepCodeTreeNode::CreateKeywordStatement(SMT_IF, currentLine); $$->SetChild(0, $3); $$->SetChild(1, $5); $$->SetChild(2, $7); }
	;
	
local_identifier:
	LOCALIDENTIFIER 
	{
		char errorBuffer[256];
		$$ = SheepCodeTreeNode::CreateIdentifierReference(yytext, false, currentLine, errorBuffer, 256); 
		if ($$ == NULL) { yyerror(errorBuffer); YYERROR; } 
	}
	;
	
global_identifier:
	IDENTIFIER 
	{
		char errorBuffer[256];
		$$ = SheepCodeTreeNode::CreateIdentifierReference(yytext, true, currentLine, errorBuffer, 256); 
	}
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
	
global_function_call_list:
	global_function_call { $$ = $1 }
	| global_function_call_list SEMICOLON global_function_call { $$ = $1; $$->AttachSibling($3); }
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
	| NOT expr { $$ = SheepCodeTreeNode::CreateOperation(OP_NOT, currentLine); $$->SetChild(0, $2); }
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
	| expr OR expr { $$ = SheepCodeTreeNode::CreateOperation(OP_OR, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3); }
	| expr AND expr { $$ = SheepCodeTreeNode::CreateOperation(OP_AND, currentLine); $$->SetChild(0, $1); $$->SetChild(1, $3);  }
	;