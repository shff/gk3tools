COMPILER = g++
LINKER = g++
COMPILER_FLAGS = -c
LINKER_FLAGS = 
EXECUTABLE = obj2mod

SOURCES = obj2mod.cpp model.cpp
OBJECTS = $(SOURCES:.cpp=.o)

$(EXECUTABLE): $(OBJECTS)
	$(LINKER) $(LINKER_FLAGS) $(OBJECTS) -o $@
	
.cpp.o:
	$(COMPILER) $(COMPILER_FLAGS) -c $< -o $@
	
clean:
	rm $(OBJECTS)