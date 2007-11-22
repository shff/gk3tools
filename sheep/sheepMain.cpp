#include <iostream>
#include <fstream>
#include <sstream>
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepFileWriter.h"
#include "sheepImportTable.h"
#include "sheepTypes.h"

int main(int argc, char** argv)
{
	if (argc < 2)
	{
		std::cout << "Sorry, need something to compile" << std::endl;
		return -1;
	}
	
	SheepCodeTree tree;
	
	std::ifstream file(argv[1]);
	if (file.good() == false)
	{
		std::cout << "Unable to open " << argv[1] << std::endl;
		return -1;
	}

	std::stringstream ss;
	std::string line;
	while(std::getline(file, line))
	{
		ss << line << std::endl;
	}

	file.close();

	std::cout << "Parsing:" << std::endl << ss.str() << std::endl;
	
	tree.Lock(ss.str(), NULL);
	tree.Print();

	SheepImportTable imports;
	imports.TryAddImport("PrintString", SYM_VOID, SYM_STRING);
	imports.TryAddImport("PrintFloat", SYM_VOID, SYM_FLOAT);

	SheepCodeGenerator generator(&tree, &imports);
	IntermediateOutput* output = generator.BuildIntermediateOutput();
	output->Print();

	SheepFileWriter writer(output);
	writer.Write("output.shp");

	//generator.WriteOutputToFile("output.shp", output);
	delete output;

	tree.Unlock();
}
