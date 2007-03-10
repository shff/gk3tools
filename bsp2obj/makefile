COMPILER = g++
LINKER = g++
COMPILER_FLAGS = -Wall
LINKER_FLAGS = 
EXECUTABLE = bsp2obj

SOURCES = bsp2obj.cpp
OBJECTS = $(SOURCES:.cpp=.o)

$(EXECUTABLE): $(OBJECTS)
	$(LINKER) $(LINKER_FLAGS) $(OBJECTS) -o $@
	
.cpp.o:
	$(COMPILER) $(COMPILER_FLAGS) -c $< -o $@
	
clean:
	rm $(OBJECTS)

