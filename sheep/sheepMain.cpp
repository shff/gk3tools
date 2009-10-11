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
#include "sheepDisassembler.h"


void CALLBACK s_printString(SheepVM* vm)
{
	std::cout << SHP_PopStringFromStack(vm) << std::endl;
}

void CALLBACK s_printFloat(SheepVM* vm)
{
	std::cout << SHP_PopFloatFromStack(vm) << std::endl;
}

void CALLBACK s_printInt(SheepVM* vm)
{
	std::cout << SHP_PopIntFromStack(vm) << std::endl;
}

void CALLBACK s_isCurrentTime(SheepVM* vm)
{
	SHP_PopStringFromStack(vm);

	SHP_PushIntOntoStack(vm, 0);
}

enum CompilerMode
{
	Compiler,
	Interpreter,
	Disassembler
};

void CALLBACK compiler_output(int lineNumber, const char* error)
{
	std::cout << "ERROR: " << lineNumber << ": " << error << std::endl;
}

int main(int argc, char** argv)
{
	if (argc < 2)
	{
		std::cout << "Usage:\tsheep INPUTFILE [OUTPUTFILE]" << std::endl;
		//std::cout << "\t* compiles INPUTFILE and writes the\n\t  results to OUTPUTFILE (\"output.shp\" by default)" << std::endl;
		std::cout << "\tsheep -s INPUTFILE [FUNCTION]" << std::endl;
		//std::cout << "\t* compiles INPUTFILE and executes\n\t  FUNCTION (\"main$\" by default)" << std::endl;
		std::cout << "\tsheep -d INPUTFILE" << std::endl;
		return -1;
	}
	
	CompilerMode mode = Compiler;
	SheepCodeTree tree;

	int indexOfFile = 1;
	std::string outputFile = "output.shp";
	std::string functionToRun = "main$";

	if (std::string(argv[1]) == "-v")
	{
		SHP_Version v = SHP_GetVersion();

		std::cout << "Sheep Compiler and Virtual Machine " << (int)v.Major << "." << (int)v.Minor << "." << (int)v.Revision << std::endl;
		std::cout << "Built " << __DATE__ << " " << __TIME__ << std::endl;
		return 0;
	}

	if (argc >= 3)
	{
		if (std::string(argv[1]) == "-s")
		{
			mode = Interpreter;
			indexOfFile = 2;

			if (argc > 3)
			{
				functionToRun = argv[3];
			}
		}
		else if (std::string(argv[1]) == "-d")
		{
			mode = Disassembler;
			indexOfFile = 2;
		}
		else
		{
			outputFile = argv[2];
		}
	}

	if (mode == Disassembler)
	{
		try
		{
			std::cout << SheepCompiler::Disassembler::GetDisassembly(argv[indexOfFile]) << std::endl;
			return 0;
		}
		catch(SheepException& ex)
		{
			std::cout << "Error: " << ex.GetMessage() << std::endl;
			return -1;
		}
	}
	
	std::ifstream file(argv[indexOfFile]);
	if (file.good() == false)
	{
		std::cout << "Unable to open " << argv[indexOfFile] << std::endl;
		return -1;
	}

	std::stringstream ss;
	SheepFileReader* reader = NULL;
	char magic;
	file.read(&magic, 1);
	if (magic == 'G')
	{
		// treat this as a compiled script
		file.seekg(0);

		reader = SHEEP_NEW SheepFileReader(file);
	}
	else
	{
		file.seekg(0);

		
		std::string line;
		while(std::getline(file, line))
		{
			ss << line << std::endl;
		}
	}
	file.close();
	
/*	tree.Lock(ss.str(), NULL);

	SheepImportTable imports;
	imports.TryAddImport("PrintString", SYM_VOID, SYM_STRING, s_printString);
	imports.TryAddImport("PrintFloat", SYM_VOID, SYM_FLOAT, s_printFloat);
	imports.TryAddImport("IsCurrentTime", SYM_INT, SYM_STRING, s_isCurrentTime);

	SheepCodeGenerator generator(&tree, &imports);
	IntermediateOutput* output = generator.BuildIntermediateOutput();
	tree.Unlock();

	if (output->Errors.empty() == false)
	{
		for (size_t i = 0; i < output->Errors.size(); i++)
		{
			std::cout << "OE: " << output->Errors[i].LineNumber << ": " << output->Errors[i].Output << std::endl;
		}
	}*/
	
	if (mode == Compiler)
	{
		SheepMachine m;

		m.GetImports().TryAddImport("PrintString", SYM_VOID, SYM_STRING, s_printString);
		m.GetImports().TryAddImport("PrintFloat", SYM_VOID, SYM_FLOAT, s_printFloat);
		m.GetImports().TryAddImport("PrintInt", SYM_VOID, SYM_INT, s_printInt);
		
		IntermediateOutput* output;
		
		if (reader)
			output = reader->GetIntermediateOutput();
		else
			output = m.Compile(ss.str());

		if (output->Errors.empty() == false)
		{
			for (size_t i = 0; i < output->Errors.size(); i++)
			{
				std::cout << "Error " << output->Errors[i].LineNumber << ": " << output->Errors[i].Output << std::endl;
			}
		}
		else
		{
			SheepFileWriter writer(output);
			writer.Write(outputFile);
			
			std::cout << "Num symbols: " << output->Symbols.size() << std::endl;
			std::cout << "Num functions: " << output->Functions.size() << std::endl;
		}

		SHEEP_DELETE(output);
	}
	else
	{
		try
		{
			SheepMachine machine;

			machine.GetImports().TryAddImport("PrintString", SYM_VOID, SYM_STRING, s_printString);
			machine.GetImports().TryAddImport("PrintFloat", SYM_VOID, SYM_FLOAT, s_printFloat);
			machine.GetImports().TryAddImport("PrintInt", SYM_VOID, SYM_INT, s_printInt);

			IntermediateOutput* output;
		
			if (reader)
				output = reader->GetIntermediateOutput();
			else
				output = machine.Compile(ss.str());

			machine.Run(output, functionToRun);
		}
		catch(SheepException& ex)
		{
			std::cout << "Error: " << ex.GetMessage() << std::endl;
			return -1;
		}
	}

	return 0;
}
