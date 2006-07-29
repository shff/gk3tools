COMPILER = g++
LINKER = g++
COMPILER_FLAGS = -fPIC -Iinclude
LINKER_FLAGS = -shared
EXECUTABLE = libbarn.so

SOURCES = src/barn.cpp
OBJECTS = $(SOURCES:.cpp=.o)

$(EXECUTABLE): $(OBJECTS)
	$(LINKER) $(LINKER_FLAGS) $(OBJECTS) -o $@
	
.cpp.o:
	$(COMPILER) $(COMPILER_FLAGS) -c $< -o $@
	
clean:
	rm $(OBJECTS)