#include "gtest/gtest.h"
#include "sheepcpp.h"

class CreateNewVMTestCpp : public ::testing::Test
{
	
};

TEST(CreateNewVMTestCpp, CanCreateAndDestroyCompiler)
{
	Sheep::ICompiler* compiler = CreateSheepCompiler(Sheep::SheepLanguageVersion::V200);
	ASSERT_NE(compiler, nullptr);
	compiler->Release();
}

TEST(CreateNewVMTestCpp, CanCreateAndDestroyVM)
{
	Sheep::IVirtualMachine* vm = CreateSheepVirtualMachine(Sheep::SheepLanguageVersion::V200);
	ASSERT_NE(vm, nullptr);
	vm->Release();
}

TEST(CreateNewVMTestCpp, TestVMTags)
{
	Sheep::IVirtualMachine* vm = CreateSheepVirtualMachine(Sheep::SheepLanguageVersion::V200);
	ASSERT_NE(vm, nullptr);

	vm->SetTag((void*)12345);
	ASSERT_EQ((void*)12345, vm->GetTag());

	vm->SetTag(nullptr);
	ASSERT_EQ(nullptr, vm->GetTag());

	vm->Release();
}

TEST(CreateNewVMTestCpp, TestCompiler)
{
	Sheep::ICompiler* compiler = CreateSheepCompiler(Sheep::SheepLanguageVersion::V200);
	ASSERT_NE(compiler, nullptr);

	Sheep::SymbolType params[] = { Sheep::SymbolType::String };
	ASSERT_EQ(0, compiler->DefineImportFunction("PrintString", Sheep::SymbolType::Void, params, 1));
	ASSERT_EQ(SHEEP_ERR_INVALID_ARGUMENT, compiler->DefineImportFunction("PrintString", Sheep::SymbolType::Void, params, 1));

	Sheep::IScript* script = compiler->CompileScript("code { main$() { PrintString(\"hello\"); } }");
	ASSERT_NE(script, nullptr);
	ASSERT_EQ(Sheep::ScriptStatus::Success, script->GetStatus());

	ASSERT_EQ(nullptr, script->GetMessage(1000));

	script = compiler->CompileScript("code { main$() { call(\"foo$\"); } foo$() { } }");
	ASSERT_NE(script, nullptr);
	ASSERT_EQ(Sheep::ScriptStatus::Success, script->GetStatus());

	compiler->Release();
}

TEST(CreateNewVMTestCpp, TestVM)
{
	Sheep::ICompiler* compiler = CreateSheepCompiler(Sheep::SheepLanguageVersion::V200);
	ASSERT_NE(compiler, nullptr);

	Sheep::IScript* script = compiler->CompileScript("code { main$() { } }");
	ASSERT_NE(script, nullptr);
	ASSERT_EQ(Sheep::ScriptStatus::Success, script->GetStatus());

	Sheep::IVirtualMachine* vm = CreateSheepVirtualMachine(Sheep::SheepLanguageVersion::V200);
	ASSERT_NE(vm, nullptr);

	Sheep::IExecutionContext* context;
	ASSERT_EQ(SHEEP_SUCCESS, vm->PrepareScriptForExecution(script, "main$", &context));
	context->Release();

	vm->Release();
}

TEST(CreateNewVMTestCpp, TestFunctionParameters)
{
	Sheep::ICompiler* compilerV1 = CreateSheepCompiler(Sheep::SheepLanguageVersion::V100);
	Sheep::ICompiler* compilerV2 = CreateSheepCompiler(Sheep::SheepLanguageVersion::V200);
	ASSERT_NE(compilerV1, nullptr);
	ASSERT_NE(compilerV2, nullptr);

	const char* script = "symbols { int result$; } code { main$(int x$){ result$ = x$; } }";
	Sheep::IScript* script1 = compilerV1->CompileScript(script);
	Sheep::IScript* script2 = compilerV2->CompileScript(script);
	ASSERT_NE(script1, nullptr);
	ASSERT_EQ(Sheep::ScriptStatus::Error, script1->GetStatus());

	ASSERT_NE(script2, nullptr);
	ASSERT_EQ(Sheep::ScriptStatus::Success, script2->GetStatus());

	Sheep::IVirtualMachine* vm = CreateSheepVirtualMachine(Sheep::SheepLanguageVersion::V200);

	Sheep::IExecutionContext* context;
	ASSERT_EQ(SHEEP_SUCCESS, vm->PrepareScriptForExecution(script2, "main$", &context));
	ASSERT_EQ(SHEEP_SUCCESS, context->PushIntOntoStack(100));
	ASSERT_EQ(SHEEP_SUCCESS, context->Execute());
}