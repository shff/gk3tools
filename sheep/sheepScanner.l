%{
	#include <stdio.h>
	#include <string.h>
	#include <stdlib.h>
	#include "sheepParser.hpp"
    
	int currentLine = 1;
%}

%option never-interactive

%s IN_COMMENT


%%


[\n] { currentLine++; }
[ \t]+	{ }
"//".* { }

<INITIAL>{
"int"		return INTSYM;
"float"		return FLOATSYM;
"string"	return STRINGSYM;

"code"		return CODE;
"symbols"	return SYMBOLS;
"return" 	return RETURN;
"wait"		return WAIT;
"if"		return IF;
"else"		return ELSE;
"goto"		return GOTO;

"snippet"	return SNIP;

":"		return COLON;
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

"!"		return NOT;
"&&"	return AND;
"||"	return OR;


([0-9])+\.([0-9])+	{ return FLOAT; }
([0-9])+		{ return INTEGER; }
\"([^\"\\]|\\\")*\"	{ return STRING; }
\|\<[^\>\|]*\>\|	{ return STRING; }
}

[a-zA-Z][a-zA-Z0-9_]*[$] { return LOCALIDENTIFIER; }
[a-zA-Z][a-zA-Z0-9_]* { return IDENTIFIER; }


<INITIAL>{
"/*"    BEGIN(IN_COMMENT);
}
<IN_COMMENT>{
"*/"    BEGIN(INITIAL);
[^*\n]+ // eat comment in chucks
"*"     // eat the lone star
}


%%

int yywrap()
{
	return 1;
}