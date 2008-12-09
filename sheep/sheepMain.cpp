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

void CALLBACK s_printString(SheepVM* vm)
{
	std::cout << SHP_PopStringFromStack(vm) << std::endl;
}

void CALLBACK s_printFloat(SheepVM* vm)
{
	std::cout << SHP_PopFloatFromStack(vm) << std::endl;
}

void CALLBACK s_isCurrentTime(SheepVM* vm)
{
	SHP_PopStringFromStack(vm);

	SHP_PushIntOntoStack(vm, 0);
}

int main(int argc, char** argv)
{
	if (argc < 2)
	{
		std::cout << "Usage:\tsheep INPUTFILE [OUTPUTFILE]" << std::endl;
		//std::cout << "\t* compiles INPUTFILE and writes the\n\t  results to OUTPUTFILE (\"output.shp\" by default)" << std::endl;
		std::cout << "\tsheep -s INPUTFILE [FUNCTION]" << std::endl;
		//std::cout << "\t* compiles INPUTFILE and executes\n\t  FUNCTION (\"main$\" by default)" << std::endl;
		return -1;
	}
	
	bool interpreterMode = false;
	SheepCodeTree tree;

	int indexOfFile = 1;
	std::string outputFile = "output.shp";
	std::string functionToRun = "main$";

	if (std::string(argv[1]) == "-v")
	{
		SHP_Version v = SHP_GetVersion();

		std::cout << "Sheep Compiler and Virtual Machine " << (int)v.Major << "." << (int)v.Minor << "." << (int)v.Revision << std::endl;
		return 0;
	}

	if (argc >= 3)
	{
		if (std::string(argv[1]) == "-s")
		{
			interpreterMode = true;
			indexOfFile = 2;

			if (argc > 3)
			{
				functionToRun = argv[3];
			}
		}
		else
		{
			outputFile = argv[2];
		}
	}
	
	std::ifstream file(argv[indexOfFile]);
	if (file.good() == false)
	{
		std::cout << "Unable to open " << argv[indexOfFile] << std::endl;
		return -1;
	}

	std::stringstream ss;
	std::string line;
	while(std::getline(file, line))
	{
		ss << line << std::endl;
	}

	file.close();
	
	tree.Lock(ss.str(), NULL);

	SheepImportTable imports;
	imports.TryAddImport("PrintString", SYM_VOID, SYM_STRING, s_printString);
	imports.TryAddImport("PrintFloat", SYM_VOID, SYM_FLOAT, s_printFloat);
	imports.TryAddImport("IsCurrentTime", SYM_INT, SYM_STRING, s_isCurrentTime);

	SheepCodeGenerator generator(&tree, &imports);
	IntermediateOutput* output = generator.BuildIntermediateOutput();
	tree.Unlock();
	
	if (interpreterMode == false)
	{
		SheepFileWriter writer(output);
		writer.Write(outputFile);
		
		std::cout << "Num symbols: " << output->Symbols.size() << std::endl;
		std::cout << "Num functions: " << output->Functions.size() << std::endl;
	}
	else
	{
		try
		{
			SheepMachine machine;
			machine.Prepare(output);
			machine.Run(functionToRun);
		}
		catch(SheepException& ex)
		{
			std::cout << "Error: " << ex.GetMessage() << std::endl;
			return -1;
		}
	}

	return 0;
}
