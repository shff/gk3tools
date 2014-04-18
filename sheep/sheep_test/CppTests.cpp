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
	Sheep::IVirtualMachine* vm = CreateSheepVirtualMachine();
	ASSERT_NE(vm, nullptr);
	vm->Release();
}

TEST(CreateNewVMTestCpp, TestVMTags)
{
	Sheep::IVirtualMachine* vm = CreateSheepVirtualMachine();
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