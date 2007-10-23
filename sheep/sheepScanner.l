%{
	#include <stdio.h>
	#include <string.h>
	#include <stdlib.h>
	#include "sheepParser.hpp"
	
	int currentLine = 1;
%}


%%


[\n] { currentLine++; }
[ \t]+	{ }


"int"		return INTSYM;
"float"		return FLOATSYM;
"string"	return STRINGSYM;

"code"		return CODE;
"symbols"	return SYMBOLS;
"return" 	return RETURN;
"if"		return IF;
"else"		return ELSE;

"snippet"	return SNIP;

";"		return SEMICOLON;
","		return COMMA;
"$"		return DOLLAR;
"("		return LPAREN;
")"		return RPAREN;
"{"		return LBRACE;
"}"		return RBRACE;

"\""		return QUOTE;

"=="		return EQUALS;
"!="		return NOTEQUAL;
"="		return BECOMES;

"+"		return PLUS;
"-"		return MINUS;
"*"		return TIMES;
"/"		return DIVIDE;

">"		return GREATERTHAN;
"<"		return LESSTHAN;

"&&"	return AND;
"||"	return OR;

([0-9])+\.([0-9])+	{ return FLOAT; }
([0-9])+		{ return INTEGER; }
\"[^\"]*\"	{ return STRING; }

[a-zA-Z][a-zA-Z0-9]*[$] { return LOCALIDENTIFIER; }
[a-zA-Z][a-zA-Z0-9]* { return IDENTIFIER; }


%%

int yywrap()
{
	return 1;
}
