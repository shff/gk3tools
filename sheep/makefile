CC=gcc
CPPC=g++
FLAGS= -g
EXECUTABLE=parser

$(EXECUTABLE):
	flex -i sheep.l
	bison -y -d sheep.y
	$(CC) $(FLAGS) -c lex.yy.c -o lex.yy.o
	$(CC) $(FLAGS) -c y.tab.c -o y.tab.o
	$(CPPC) $(FLAGS) symbols.cpp main.cpp compiler.cpp disassembler.cpp sheepwriter.cpp lex.yy.o y.tab.o -o $(EXECUTABLE)
	
clean:
	rm $(EXECUTABLE)
