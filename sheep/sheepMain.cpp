#include <iostream>
#include <fstream>
#include <sstream>
#include "sheepc.h"
#include "sheepMachine.h"
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepFileWriter.h"
#include "sheepImportTable.h"
#include "sheepTypes.h"

void s_printString(SheepVM* vm)
{
	std::cout << SHP_PopStringFromStack(vm) << std::endl;
}

void s_printFloat(SheepVM* vm)
{
	std::cout << SHP_PopFloatFromStack(vm) << std::endl;
}

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
	imports.TryAddImport("PrintString", SYM_VOID, SYM_STRING, s_printString);
	imports.TryAddImport("PrintFloat", SYM_VOID, SYM_FLOAT, s_printFloat);

	SheepCodeGenerator generator(&tree, &imports);
	IntermediateOutput* output = generator.BuildIntermediateOutput();
	output->Print();

	SheepFileWriter writer(output);
	writer.Write("output.shp");
	
	std::cout << "Num symbols: " << output->Symbols.size() << std::endl;
	std::cout << "Num functions: " << output->Functions.size() << std::endl;

	SheepMachine machine;
	machine.Prepare(output);
	machine.Run("blah$");
	//std::cout << "result: " << machine.RunSnippet(ss.str()) << std::endl;

	//generator.WriteOutputToFile("output.shp", output);
	//delete output;

	tree.Unlock();
}
