CC=gcc
CPPC=g++
EXECUTABLE=parser

$(EXECUTABLE):
	flex -i sheep.l
	bison -y -d sheep.y
	$(CC) -c lex.yy.c -o lex.yy.o
	$(CC) -c y.tab.c -o y.tab.o
	$(CPPC) symbols.cpp main.cpp compiler.cpp lex.yy.o y.tab.o -o $(EXECUTABLE)
	
clean:
	rm $(EXECUTABLE)
