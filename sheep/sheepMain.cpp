#include <iostream>
#include <fstream>
#include <sstream>
#include <functional>
#include <memory>
#include "sheepcpp.h"
/*#include "sheepMachine.h"
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepFileWriter.h"
#include "sheepImportTable.h"
#include "sheepTypes.h"
#include "sheepDisassembler.h"*/


void SHP_CALLBACK s_printString(Sheep::IExecutionContext* context)
{
	const char* result;
	context->PopStringFromStack(&result);
	std::cout << result << std::endl;
}

void SHP_CALLBACK s_printFloat(Sheep::IExecutionContext* context)
{
	float result;
	context->PopFloatFromStack(&result);
	std::cout << result << std::endl;
}

void SHP_CALLBACK s_printInt(Sheep::IExecutionContext* context)
{
	int result;
	context->PopIntFromStack(&result);
	std::cout << result << std::endl;
}

void SHP_CALLBACK s_isCurrentTime(Sheep::IExecutionContext* context)
{
	context->PopStringFromStack(nullptr);
	context->PushIntOntoStack(0);
}

enum class CompilerMode
{
	Compiler,
	Interpreter,
	Disassembler
};

Sheep::IScript* GenerateScript(std::ifstream& file, bool enhancementsEnabled)
{
	Sheep::IScript* result = nullptr;

	char magic;
	file.read(&magic, 1);
	if (magic == 'G')
	{
		// treat this as a compiled script
		file.seekg(0, std::ios::end);
		unsigned int length = file.tellg();
		file.seekg(0);

		char* buffer = new char[length];

		file.read(buffer, length);

		CreateScriptFromBytecode(buffer, length, &result);

		delete[] buffer;
	}
	else
	{
		file.seekg(0);

		std::string line;
		std::stringstream ss;
		while(std::getline(file, line))
		{
			ss << line << std::endl;
		}

		Sheep::ICompiler* compiler = CreateSheepCompiler(enhancementsEnabled ? Sheep::SheepLanguageVersion::V200 : Sheep::SheepLanguageVersion::V100);

		Sheep::SymbolType printStringParams[] = { Sheep::SymbolType::String };
		Sheep::SymbolType printFloatParams[] = { Sheep::SymbolType::Float };
		Sheep::SymbolType printIntParams[] = { Sheep::SymbolType::Int };
		compiler->DefineImportFunction("PrintString", Sheep::SymbolType::Void, printStringParams, 1);
		compiler->DefineImportFunction("PrintFloat", Sheep::SymbolType::Void, printFloatParams, 1);
		compiler->DefineImportFunction("PrintInt", Sheep::SymbolType::Void, printIntParams, 1);

		result = compiler->CompileScript(ss.str().c_str());

		compiler->Release();
	}
	file.close();

	return result;
}

template<typename T>
struct SheepReleaser
{
	void operator()(T* t) const
	{
		t->Release();
	}
};

int main(int argc, char** argv)
{
	if (argc < 2)
	{
		std::cout << "Usage:\tsheep INPUTFILE [OUTPUTFILE]" << std::endl;
		std::cout << "\t* compiles INPUTFILE and writes the results to OUTPUTFILE (\"output.shp\" by default)" << std::endl << std::endl;
		std::cout << "\tsheep -s INPUTFILE [FUNCTION]" << std::endl;
		std::cout << "\t* compiles INPUTFILE and executes FUNCTION (\"main$\" by default)" << std::endl << std::endl;
		std::cout << "\tsheep -d INPUTFILE" << std::endl;
		std::cout << "\t* Disassembles the compiled sheep script" << std::endl << std::endl;
		return -1;
	}
	
	CompilerMode mode = CompilerMode::Compiler;

	int indexOfFile = 1;
	std::string outputFile = "output.shp";
	std::string functionToRun = "main$";
	bool enhancementsEnabled = false;

	if (std::string(argv[1]) == "-v")
	{
		SHP_Version v = shp_GetVersion();

		std::cout << "Sheep Compiler and Virtual Machine " << (int)v.Major << "." << (int)v.Minor << "." << (int)v.Revision << std::endl;
		std::cout << "Built " << __DATE__ << " " << __TIME__ << std::endl;
		return 0;
	}

	if (argc >= 3)
	{
		if (std::string(argv[1]) == "-s")
		{
			mode = CompilerMode::Interpreter;
			indexOfFile = 2;

			if (argc > 3)
			{
				functionToRun = argv[3];
			}
		}
		else if (std::string(argv[1]) == "-d")
		{
			mode = CompilerMode::Disassembler;
			indexOfFile = 2;
		}
		else if (std::string(argv[1]) == "-e")
		{
			enhancementsEnabled  = true;
			indexOfFile = 2;

			if (argc > 3)
				outputFile = argv[3];
		}
		else
		{
			outputFile = argv[2];
		}
	}

	if (mode == CompilerMode::Disassembler)
	{
		std::ifstream file(argv[indexOfFile]);
		if (file.good() == false)
		{
			std::cout << "Unable to open " << argv[indexOfFile] << std::endl;
			return -1;
		}

		std::unique_ptr<Sheep::IScript, SheepReleaser<Sheep::IScript>> script(GenerateScript(file, enhancementsEnabled));
		if (script == nullptr || script->GetStatus() != Sheep::ScriptStatus::Success)
		{
			std::cout << "Unable to compile " << argv[indexOfFile] << std::endl;
			return -1;
		}

		std::unique_ptr<Sheep::IDisassembly, SheepReleaser<Sheep::IDisassembly>> disassembly(script->GenerateDisassembly());
		if (disassembly == nullptr)
		{
			std::cout << "Unable to disassemble " << argv[indexOfFile] << std::endl;
			return -1;
		}

		const char* text = disassembly->GetDisassemblyText();
		if (text == nullptr)
		{
			std::cout << "Unable to disassemble " << argv[indexOfFile] << std::endl;
			return -1;
		}
			
		std::cout << text << std::endl;
	}
	else
	{
		std::ifstream file(argv[indexOfFile]);
		if (file.good() == false)
		{
			std::cout << "Unable to open " << argv[indexOfFile] << std::endl;
			return -1;
		}

		std::unique_ptr<Sheep::IScript, SheepReleaser<Sheep::IScript>> script(GenerateScript(file, enhancementsEnabled));
		if (script == nullptr)
		{
			std::cout << "Unknown error while trying to compile " << argv[indexOfFile] << std::endl;
			return -1;
		}

		// write out the errors
		for (int i = 0; i < script->GetNumMessages(); i++)
		{
			std::cout << "Error: " << script->GetMessageLineNumber(i) << ": " << script->GetMessage(i) << std::endl;
		}

		if (script->GetStatus() == Sheep::ScriptStatus::Success)
		{
			if (mode == CompilerMode::Compiler)
			{
				// write the output file
				std::unique_ptr<Sheep::ICompiledScriptOutput, SheepReleaser<Sheep::ICompiledScriptOutput>> output(script->GenerateCompiledOutput());

				std::ofstream out(outputFile.c_str(), std::ofstream::binary);
		
				if (out.good() == false)
				{
					std::cout << "Unable to open output file for writing" << std::endl;
					return -1;
				}
		
				out.write(output->GetData(), (std::streamsize)output->GetSize());
		
				out.close();
			}
			else
			{
				std::unique_ptr<Sheep::IVirtualMachine, SheepReleaser<Sheep::IVirtualMachine>> vm(CreateSheepVirtualMachine(enhancementsEnabled ? Sheep::SheepLanguageVersion::V200 : Sheep::SheepLanguageVersion::V100));

				vm->SetImportCallback("PrintString", s_printString);
				vm->SetImportCallback("PrintFloat", s_printFloat);
				vm->SetImportCallback("PrintInt", s_printInt);

				Sheep::IExecutionContext* context;
				if (vm->PrepareScriptForExecution(script.get(), functionToRun.c_str(), &context) != SHEEP_SUCCESS)
				{
					std::cout << "Error while prepareing to execute script" << std::endl;
					return -1;
				}

				int result = context->Execute();
				if (result == SHEEP_SUCCESS)
					return 0;
				else if (result == SHEEP_SUSPENDED)
				{
					std::cout << "Execution suspended" << std::endl;
					return 0;
				}
				else
				{
					std::cout << "Error during execution" << std::endl;
					return -1;
				}
			}
		}
		else
		{
			std::cout << "There were one or more errors during compilation." << std::endl;
			return -1;
		}
	}

	return 0;
}
