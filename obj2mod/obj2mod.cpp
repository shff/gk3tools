#include <iostream>
#include <fstream>
#include <string>

#include "model.h"

using namespace std;

string inputFile, outputFile;

int parseArguments(int argc, char** argv);

int main(int argc, char** argv)
{
	if (parseArguments(argc, argv) != 0)
		return 1;
	
	Model* m = new Model();
	m->Load(inputFile);
	m->Save(outputFile);
	
	delete m;
}

int parseArguments(int argc, char** argv)
{
	if (argc < 2)
	{
		cout << "Usage: " << endl << "\tobj2mod input.obj [output.mod]" << endl << endl;
		return -1;
	}
	
	inputFile = argv[1];
	
	if (argc > 3)
		outputFile = argv[2];
	else
	{
		outputFile = "default.mod";
	}
	
	return 0;
}
