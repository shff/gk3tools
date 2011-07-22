COMPILER = g++
LINKER = g++
COMPILER_FLAGS = -g -fPIC -Iinclude -I/usr/include/lzo -Wall
LINKER_FLAGS = -shared -llzo2 -lz
EXECUTABLE = libbarn.so

SOURCES = src/barn.cpp src/extractbuffer.cpp
OBJECTS = $(SOURCES:.cpp=.o)

$(EXECUTABLE): $(OBJECTS)
	$(LINKER) $(LINKER_FLAGS) $(OBJECTS) -o $@
	
.cpp.o:
	$(COMPILER) $(COMPILER_FLAGS) -c $< -o $@
	
clean:
	rm $(OBJECTS)

test:
	$(COMPILER) -Iinclude test.cpp -o test -L. -lbarn
