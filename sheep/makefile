CC=gcc
CPPC=g++
LINKER=g++
COMPILER_FLAGS= -g
LINKER_FLAGS=
EXECUTABLE=parser

SOURCES = sheepScanner.cpp sheepParser.cpp sheepCodeGenerator.cpp sheepCodeTree.cpp sheepFileWriter.cpp sheepFileReader.cpp sheepMachine.cpp sheepMain.cpp sheepCompiler.cpp sheepDisassembler.cpp sheepMemoryAllocator.cpp
OBJECTS = $(SOURCES:.cpp=.o)

$(EXECUTABLE): $(OBJECTS)
	$(LINKER) $(LINKER_FLAGS) $(OBJECTS) -o $@
	
sheepScanner.cpp:
	flex -i -o sheepScanner.cpp sheepScanner.l
	
sheepParser.cpp: sheepScanner.cpp
	bison -d -o sheepParser.cpp sheepParser.y
	
.cpp.o:
	$(CPPC) $(COMPILER_FLAGS) -c $< -o $@
	
clean:
	rm $(EXECUTABLE)
