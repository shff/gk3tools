COMPILER = g++
LINKER = g++
COMPILER_FLAGS = -c
LINKER_FLAGS = 
EXECUTABLE = mod2obj

SOURCES = mod2obj.cpp
OBJECTS = $(SOURCES:.cpp=.o)

$(EXECUTABLE): $(OBJECTS)
	$(LINKER) $(LINKER_FLAGS) $(OBJECTS) -o $@
	
.cpp.o:
	$(COMPILER) $(COMPILER_FLAGS) -c $< -o $@
	
clean:
	rm $(OBJECTS)