CC=gcc
CPPC=g++
LINKER=g++
COMPILER_FLAGS= -g -std=c++11
LINKER_FLAGS=
EXECUTABLE=parser

SOURCES = sheepScanner.cpp sheepParser.cpp sheepCodeGenerator.cpp sheepCodeTree.cpp sheepContextTree.cpp sheepFileWriter.cpp sheepFileReader.cpp sheepMachine.cpp sheepMain.cpp sheepCompiler.cpp sheepDisassembler.cpp sheepMemoryAllocator.cpp sheepcpp.cpp Internal/compiler.cpp Internal/script.cpp
OBJECTS = $(SOURCES:.cpp=.o)

$(EXECUTABLE): $(OBJECTS)
	$(LINKER) $(LINKER_FLAGS) $(OBJECTS) -o $@
	
%.o: %.cpp
	$(CPPC) $(COMPILER_FLAGS) -c $< -o $@
	
sheepScanner.cpp: sheepScanner.l
	flex -i -o sheepScanner.cpp sheepScanner.l
	
sheepParser.cpp: sheepParser.y
	bison -d -o sheepParser.cpp sheepParser.y
	
clean:
	rm $(EXECUTABLE)
