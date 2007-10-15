#include "sheepCodeTree.h"

int main(int argc, char** argv)
{
	if (argc < 2)
	{
		printf("Sorry, need something to compile\n");
		
		return -1;
	}
	
	SheepCodeTree tree;
	
	printf("Parsing \"%s\"\n\n", argv[1]);
	
	tree.Lock(argv[1], NULL);
	tree.Print();
	tree.Unlock();
}
