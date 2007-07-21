#include "sheepc.h"
#include "compiler.h"
#include "rbuffer.h"

SheepCode* shp_Compile(const char* script)
{
	if (script == NULL)
		return NULL;

	SheepCompiler::Compiler::Init(false);

	SheepCompiler::Compiler::CompileScript(script);

	ResizableBuffer* buffer = SheepCompiler::Compiler::WriteCompiledSheep();

	SheepCode* code = new SheepCode;
	code->code = new char[buffer->GetSize()];
	code->size = buffer->GetSize();
	memcpy(code, buffer->GetData(), buffer->GetSize());

	delete buffer;

	return code;
}

SheepCode* shp_CompileSnippet(const char* script)
{
	if (script == NULL) return NULL;

	SheepCompiler::Compiler::Init(true);

	SheepCompiler::Compiler::CompileScript(script);

	ResizableBuffer* buffer = SheepCompiler::Compiler::WriteCompiledSheep();

	if (buffer != NULL)
	{
		SheepCode* code = new SheepCode;
		code->code = new char[buffer->GetSize()];
		code->size = buffer->GetSize();
		memcpy(code->code, buffer->GetData(), buffer->GetSize());

		delete buffer;

		return code;
	}

	return NULL;
}

void shp_DestroySheep(SheepCode* sheep)
{
	if (sheep != NULL)
	{
		if (sheep->code != NULL)
			delete[] sheep->code;
		
		delete sheep;
	}
}