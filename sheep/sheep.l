%{
	#include <stdio.h>
	#include <string.h>
	#include <stdlib.h>
	#include "y.tab.h"

	char* removeQuotes(char* str)
	{
		str[strlen(str)-1] = 0;
		return str+1;
	}
%}

%%

[ \n\t]+	{ }


"int"		return INTSYM;
"float"		return FLOATSYM;
"string"	return STRINGSYM;

"code"		return CODE;
"symbols"	return SYMBOLS;
"return" 	return RETURN;
"if"		return IF;
"else"		return ELSE;

";"		return SEMICOLON;
","		return COMMA;
"$"		return DOLLAR;
"("		return LPAREN;
")"		return RPAREN;
"{"		return LBRACE;
"}"		return RBRACE;

"\""		return QUOTE;

"=="		return EQUALS;
"="		return BECOMES;

"+"		return PLUS;
"-"		return MINUS;
"*"		return MULTIPLY;
"/"		return DIVIDE;

">"		return GREATERTHAN;
"<"		return LESSTHAN;

[0-9]+.[0-9]+	{ yylval.floatVal = atof(yytext); return FLOAT; }
[0-9]+		{ yylval.intVal = atoi(yytext); return INTEGER; }

\"[^\"]*\"	{ yylval.stringVal = removeQuotes(yytext); return STRING; }

[a-zA-Z][a-zA-Z0-9]*[$] { yylval.id = (char*)strdup(yytext); return LOCALIDENTIFIER; }
[a-zA-Z][a-zA-Z0-9]* { yylval.id = (char*)strdup(yytext); return IDENTIFIER; }


%%
