#include <iostream>
#include <algorithm>
#include <memory>
#include "sheepMemoryAllocator.h"
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepException.h"
#include "sheepCodeBuffer.h"

#ifdef _MSC_VER
#pragma warning(error:4267)
#endif

IntermediateOutput::~IntermediateOutput()
{
	for (std::vector<SheepFunction>::iterator itr = Functions.begin(); itr != Functions.end(); itr++)
	{
		if ((*itr).Code != NULL)
			SHEEP_DELETE((*itr).Code);
	}
}

void IntermediateOutput::Print()
{
	std::cout << "------------" << std::endl;

	for (std::vector<SheepSymbol>::iterator itr = Symbols.begin(); itr != Symbols.end(); itr++)
		std::cout << "Symbol: " << SheepSymbolTypeNames[(int)(*itr).Type] << " " << (*itr).Name << std::endl;

	for (std::vector<SheepFunction>::iterator itr = Functions.begin(); itr != Functions.end(); itr++)
		std::cout << "Function: " << (*itr).Name << std::endl;

	for (std::vector<CompilerOutput>::iterator itr = Errors.begin(); itr != Errors.end(); itr++)
		std::cout << "Error at line " << (*itr).LineNumber << ": " << (*itr).Output << std::endl;

	std::cout << "------------" << std::endl;
}

SheepCodeGenerator::SheepCodeGenerator(SheepCodeTree* tree, SheepImportTable* imports, Sheep::SheepLanguageVersion languageVersion)
{
	assert(tree != NULL);
	assert(imports != NULL);

	m_languageVersion = languageVersion;
	m_tree = tree;
	m_imports = imports;
}

IntermediateOutput* SheepCodeGenerator::BuildIntermediateOutput()
{
	shp_auto_ptr<IntermediateOutput> output(SHEEP_NEW IntermediateOutput(m_languageVersion));

	//std::map<std::string, SheepImport> usedImports;

	CompilerContext context;

	try
	{
		int functionCodeOffset = 0;

		// copy the string constants (which the parser should have gathered for us)
		// into the output
		loadStringConstants(output.get());

		SheepCodeTreeNode* root = m_tree->GetCodeTree();

		// collect all the symbols (including functions, but not labels)
		buildSymbolMap(root);

		// determine the types of all the expressions
		SheepCodeTreeSectionNode* section = static_cast<SheepCodeTreeSectionNode*>(root);
		while(section != NULL)
		{
			// iterate over each function/snippet and output a SheepFunction object
			if (section->GetSectionType() == CodeTreeSectionType::Code)
			{
				SheepCodeTreeFunctionListNode* functions = 
					static_cast<SheepCodeTreeFunctionListNode*>(section->GetChild(0));

				for (int i = 0; i < functions->Functions.size(); i++)
				{
					SheepFunction func = writeFunction(functions->Functions[i], functionCodeOffset, context);
					func.ParentCode = output.get();
					output->Functions.push_back(func);
					functionCodeOffset += (int)func.Code->GetSize();

					// copy any new imports to the list of imports
					for (std::vector<std::string>::iterator itr = func.ImportList.begin();
						itr != func.ImportList.end(); itr++)
					{
						SheepImport import;
						m_imports->TryFindImport(*itr, import);
						// TODO: I don't think this is even necessary...
					}
				}
			}

			section = static_cast<SheepCodeTreeSectionNode*>(section->GetNextSibling());
		}

		// copy the symbols into the output
		for (std::vector<SheepSymbol>::iterator itr = m_variables.begin(); itr != m_variables.end(); itr++)
		{
			SheepSymbol symbol = (*itr);

			if (symbol.Type == SheepSymbolType::Int || symbol.Type == SheepSymbolType::Float || symbol.Type == SheepSymbolType::String)
				output->Symbols.push_back((*itr));
		}

		// copy the imports into the output
		for (std::vector<SheepImport>::iterator itr = m_usedImports.begin();
			itr != m_usedImports.end(); itr++)
		{
			output->Imports.push_back(*itr);
		}

	}
	catch(SheepCompilerException& ex)
	{
		CompilerOutput error;
		error.LineNumber = ex.GetLineNumber();
		error.Output = ex.GetMessage();

		context.Output.push_back(error);
	}

	output->Errors = context.Output;

	return output.release();
}

struct ConstantOffsetComparer
{
	int operator()(const SheepStringConstant& c1, const SheepStringConstant& c2)
	{
		return c1.Offset < c2.Offset;
	}
};

void SheepCodeGenerator::loadStringConstants(IntermediateOutput *output)
{
	assert(output != NULL);

	for (std::map<std::string, SheepCodeTree::StringConst>::const_iterator itr = m_tree->GetFirstConstant();
		itr != m_tree->GetEndOfConstants(); itr++)
	{
		SheepStringConstant constant;

		constant.Value = (*itr).second.Value;
		constant.Offset = (*itr).second.Offset;

		output->Constants.push_back(constant);
	}

	std::sort(output->Constants.begin(), output->Constants.end(), ConstantOffsetComparer());
}

void SheepCodeGenerator::buildSymbolMap(SheepCodeTreeNode *node)
{
	while (node != NULL)
	{
		assert(node->GetType() == CodeTreeNodeType::Section);

		SheepCodeTreeSectionNode* section = static_cast<SheepCodeTreeSectionNode*>(node);

		if (section->GetSectionType() == CodeTreeSectionType::Symbols)
		{
			// section is full of yummy declaration nodes!
			SheepCodeTreeVariableDeclarationNode* variableDecl = polymorphic_downcast<SheepCodeTreeVariableDeclarationNode*>(section->GetChild(0));

			while(variableDecl != NULL)
			{
				SheepSymbolType type;
				if (variableDecl->VariableType->GetRefType() == CodeTreeTypeReferenceType::Int)
					type = SheepSymbolType::Int;
				else if (variableDecl->VariableType->GetRefType() == CodeTreeTypeReferenceType::Float)
					type = SheepSymbolType::Float;
				else if (variableDecl->VariableType->GetRefType() == CodeTreeTypeReferenceType::String)
					type = SheepSymbolType::String;
				else
					assert(false && "Unknown symbol type! Probable compiler bug.");

				SheepCodeTreeVariableDeclarationNameAndValueNode* variable = variableDecl->FirstVariable;
				
				while(variable)
				{
					SheepSymbol symbol;
					symbol.Name = variable->VariableName->GetName();
					symbol.Type = type;

					if (variable->InitialValue != nullptr)
					{
						if (symbol.Type == SheepSymbolType::Int)
							symbol.InitialIntValue = variable->InitialValue->GetIntValue();
						else if (symbol.Type == SheepSymbolType::Float)
							symbol.InitialFloatValue = variable->InitialValue->GetFloatValue();
						else if (symbol.Type == SheepSymbolType::String)
							symbol.InitialStringValue = variable->InitialValue->GetStringValue();
					}
					
					if (m_symbolMap.insert(SymbolMap::value_type(symbol.Name, symbol)).second == false)
						throw SheepCompilerException(variable->GetLineNumber(), "Symbol already defined");

					m_variables.push_back(symbol);
					
					variable = polymorphic_downcast<SheepCodeTreeVariableDeclarationNameAndValueNode*>(variable->GetNextSibling());
				}

				variableDecl = polymorphic_downcast<SheepCodeTreeVariableDeclarationNode*>(variableDecl->GetNextSibling());
			}
		}
		else
		{
			// this is a Code section
			SheepCodeTreeFunctionListNode* functions = static_cast<SheepCodeTreeFunctionListNode*>(section->GetChild(0));

			for (int i = 0; i < functions->Functions.size(); i++)
			{
				SheepSymbol symbol;
				symbol.Type = SheepSymbolType::LocalFunction;
				symbol.Name = functions->Functions[i]->Name->GetName();

				if (m_symbolMap.insert(SymbolMap::value_type(symbol.Name, symbol)).second == false)
					throw SheepCompilerException(functions->Functions[i]->GetLineNumber(), "Function already defined");
				
				// dive in and gather a list of all labels in the function.
				std::pair<FunctionLabelMap::iterator, bool> result = m_labels.insert(FunctionLabelMap::value_type(functions->Functions[i], LabelMap()));
				assert(result.second == true);

				gatherFunctionLabels(m_labels[functions->Functions[i]], functions->Functions[i]);
			}
		}

		node = node->GetNextSibling();
	}
}

void SheepCodeGenerator::determineExpressionTypes(SheepFunction& function, SheepCodeTreeNode* node)
{
	while (node != NULL)
	{
		if (node->GetType() == CodeTreeNodeType::Declaration)
		{
			SheepCodeTreeDeclarationNode* decl = static_cast<SheepCodeTreeDeclarationNode*>(node);
			if (decl->GetDeclarationType() == CodeTreeDeclarationNodeType::Function)
				determineExpressionTypes(function, node->GetChild(3));
			else
				determineExpressionTypes(function, node->GetChild(1));
		}
		else if (node->GetType() == CodeTreeNodeType::Expression)
		{
			SheepCodeTreeExpressionNode* expr = static_cast<SheepCodeTreeExpressionNode*>(node);

			if (expr->GetExpressionType() == CodeTreeExpressionType::Operation)
			{
				SheepCodeTreeOperationNode* operation = static_cast<SheepCodeTreeOperationNode*>(expr);

				SheepCodeTreeExpressionNode* child1 = static_cast<SheepCodeTreeExpressionNode*>(expr->GetChild(0));
				SheepCodeTreeExpressionNode* child2 = static_cast<SheepCodeTreeExpressionNode*>(expr->GetChild(1));

				if (child1 != NULL)	determineExpressionTypes(function, child1);
				if (child2 != NULL) determineExpressionTypes(function, child2);

				if (child1->GetValueType() == CodeTreeExpressionValueType::Void ||
					(child2 && child2->GetValueType() == CodeTreeExpressionValueType::Void))
				{
					// can't use void with *any* operators!
					throw SheepCompilerException(operation->GetLineNumber(), "Cannot use void types with operator");
				}

				switch(operation->GetOperationType())
				{
					case CodeTreeOperationType::Add:
					case CodeTreeOperationType::Minus:
					case CodeTreeOperationType::Times:
					case CodeTreeOperationType::Divide:
						if (child1->GetValueType() == CodeTreeExpressionValueType::String ||
							child2->GetValueType() == CodeTreeExpressionValueType::String)
						{
							// strings cannot be added, multiplied, etc
							throw SheepCompilerException(operation->GetLineNumber(), "Cannot use this operator with strings");
						}
						else if (child1->GetValueType() == child2->GetValueType())
						{
							operation->SetValueType(child1->GetValueType());
						}
						else
						{
							// one must be an int and one is a float, so set the type to float
							operation->SetValueType(CodeTreeExpressionValueType::Float);
						}
						break;
					case CodeTreeOperationType::GreaterThan:
					case CodeTreeOperationType::LessThan:
					case CodeTreeOperationType::GreaterThanEqual:
					case CodeTreeOperationType::LessThanEqual:
					case CodeTreeOperationType::Equal:
					case CodeTreeOperationType::NotEqual:
						if ((child1->GetValueType() == CodeTreeExpressionValueType::String ||
							child2->GetValueType() == CodeTreeExpressionValueType::String) &&
							child1->GetValueType() != child2->GetValueType())
						{
							throw SheepCompilerException(operation->GetLineNumber(), "Cannot compare string and non-string");
						}
						else
						{
							operation->SetValueType(CodeTreeExpressionValueType::Int);
						}
						break;
					case CodeTreeOperationType::Not:
						if (child1->GetValueType() != CodeTreeExpressionValueType::Int)
							throw SheepCompilerException(operation->GetLineNumber(), "Cannot apply '!' operator to a non-integer");
						else
							operation->SetValueType(CodeTreeExpressionValueType::Int);
						break;
					case CodeTreeOperationType::Negate:
						if (child1->GetValueType() != CodeTreeExpressionValueType::Float &&
							child1->GetValueType() != CodeTreeExpressionValueType::Int)
							throw SheepCompilerException(operation->GetLineNumber(), "Can only negate ints and floats");
						else
							operation->SetValueType(child1->GetValueType());
						break;
					case CodeTreeOperationType::And:
					case CodeTreeOperationType::Or:
						if (child1->GetValueType() != CodeTreeExpressionValueType::Int ||
							child2->GetValueType() != CodeTreeExpressionValueType::Int)
							throw SheepCompilerException(operation->GetLineNumber(), "Cannot apply '&&' and '||' operators to non-integers");
						else
							operation->SetValueType(CodeTreeExpressionValueType::Int);
						break;
					default:
						throw SheepException("Unknown operation type", SHEEP_UNKNOWN_ERROR_PROBABLY_BUG);

				}
				
			}
			else if (expr->GetExpressionType() == CodeTreeExpressionType::Identifier)
			{
				SheepCodeTreeIdentifierReferenceNode* identifier = 
					static_cast<SheepCodeTreeIdentifierReferenceNode*>(expr);

				if (identifier->IsGlobal())
				{
					// this is a global function call
					SheepImport import;
					if (m_imports->TryFindImport(identifier->GetName(), import) == false)
						throw SheepCompilerException(identifier->GetLineNumber(), "Unrecognized import function");

					// check the parameters
					determineExpressionTypes(function, identifier->GetChild(0));

					identifier->SetValueType(convertToExpressionValueType(import.ReturnType));
				}
				else
				{
					SheepSymbolType definedType = SheepSymbolType::Void;
					SymbolMap::iterator itr = m_symbolMap.find(identifier->GetName());
					if (itr != m_symbolMap.end())
					{
						definedType = (*itr).second.Type;
					}
					else
					{
						for (int i = 0; i < function.Parameters.size(); i++)
						{
							if (CIEqual(function.Parameters[i].Name, identifier->GetName()))
							{
								definedType = function.Parameters[i].Type;
								break;
							}
						}
					}

					if (definedType == SheepSymbolType::Void)
						throw SheepCompilerException(identifier->GetLineNumber(), "Use of undefined symbol");

					if (definedType == SheepSymbolType::LocalFunction)
						throw SheepCompilerException(identifier->GetLineNumber(), "Function name used like a variable");
					else if (definedType == SheepSymbolType::Int)
						identifier->SetValueType(CodeTreeExpressionValueType::Int);
					else if (definedType == SheepSymbolType::Float)
						identifier->SetValueType(CodeTreeExpressionValueType::Float);
					else if (definedType == SheepSymbolType::String)
						identifier->SetValueType(CodeTreeExpressionValueType::String);
					else
						throw SheepCompilerException(identifier->GetLineNumber(), "Expected variable");
				}
			}
			else if (expr->GetExpressionType() == CodeTreeExpressionType::Constant)
			{
				// nothing to do! type is already defined
			}
		}
		else if (node->GetType() == CodeTreeNodeType::Statement)
		{
			SheepCodeTreeStatementNode* statement = static_cast<SheepCodeTreeStatementNode*>(node);

			if (statement->GetStatementType() == CodeTreeKeywordStatementType::If)
			{
				determineExpressionTypes(function, statement->GetChild(0));
				determineExpressionTypes(function, statement->GetChild(1));
				determineExpressionTypes(function, statement->GetChild(2));

				SheepCodeTreeExpressionNode* ifCondition = 
					static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0));

				if (ifCondition->GetValueType() != CodeTreeExpressionValueType::Int)
					throw SheepCompilerException(statement->GetLineNumber(), "Condition must evaluate to an int");
			}
			else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Wait)
			{
				determineExpressionTypes(function, statement->GetChild(0));
			}
			else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Assignment)
			{
				determineExpressionTypes(function, statement->GetChild(0));
				determineExpressionTypes(function, statement->GetChild(1));
			}
			else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Expression)
			{
				determineExpressionTypes(function, statement->GetChild(0));
			}
			else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Return)
			{
				determineExpressionTypes(function, statement->GetChild(0));
			}
		}

		node = node->GetNextSibling();
	}
}

void SheepCodeGenerator::gatherFunctionLabels(LabelMap& labels, SheepCodeTreeNode* node)
{
	while(node != NULL)
	{
		if (node->GetType() == CodeTreeNodeType::Declaration)
		{
			SheepCodeTreeDeclarationNode* decl = static_cast<SheepCodeTreeDeclarationNode*>(node);

			// if this is a label declaration then add it to the list of labels
			if (decl->GetDeclarationType() == CodeTreeDeclarationNodeType::Label)
			{
				SheepCodeTreeNode* child = decl->GetChild(0);
				assert(child != NULL);
				assert(child->GetType() == CodeTreeNodeType::Expression);
				
				SheepCodeTreeExpressionNode* childExpr = static_cast<SheepCodeTreeExpressionNode*>(child);
				assert(childExpr->GetExpressionType() == CodeTreeExpressionType::Identifier);

				SheepCodeTreeIdentifierReferenceNode* childID = static_cast<SheepCodeTreeIdentifierReferenceNode*>(childExpr);

				// insert the label. We'll have to set its offset later when we generate bytecode.
				if (labels.insert(LabelMap::value_type(childID->GetName(), 0)).second == false)
					throw SheepCompilerException(decl->GetLineNumber(), "The label has already been declared in another location.");
			}
			else if (decl->GetDeclarationType() == CodeTreeDeclarationNodeType::Function)
			{
				SheepCodeTreeFunctionDeclarationNode* function = static_cast<SheepCodeTreeFunctionDeclarationNode*>(decl);

				SheepCodeTreeStatementNode* statement = function->FirstStatement;

				while(statement != nullptr)
				{
					gatherFunctionLabels(labels, statement);
					statement = static_cast<SheepCodeTreeStatementNode*>(statement->GetNextSibling());
				}

				return;
			}
		}
		
		// go through all the children
		gatherFunctionLabels(labels, node->GetChild(0));

		// move to the next sibling
		node = node->GetNextSibling();
	}
}

SheepFunction SheepCodeGenerator::writeFunction(SheepCodeTreeFunctionDeclarationNode* function, int codeOffset, CompilerContext& context)
{
	assert(function->GetDeclarationType() == CodeTreeDeclarationNodeType::Function);
	
	bool declarationErrorsFound = false;
	SheepCodeTreeSymbolTypeNode* type = function->ReturnType;
	SheepCodeTreeIdentifierReferenceNode* ref = function->Name;
	SheepCodeTreeVariableListNode* params = function->Parameters;

	if (type != nullptr && m_languageVersion < Sheep::SheepLanguageVersion::V200)
	{
		declarationErrorsFound = true;
		context.Output.push_back(CompilerOutput(type->GetLineNumber(), "Function return types are not allowed."));
	}
	if (params != nullptr && m_languageVersion < Sheep::SheepLanguageVersion::V200)
	{
		declarationErrorsFound = true;
		context.Output.push_back(CompilerOutput(params->GetLineNumber(), "Function parameters are not allowed."));
	}

	SheepFunction func(function);
	func.Name = ref->GetName();
	func.Code = SHEEP_NEW SheepCodeBuffer();
	func.CodeOffset = codeOffset;

	if (type != NULL)
	{
		if (type->GetRefType() == CodeTreeTypeReferenceType::Int)
			func.ReturnType = SheepSymbolType::Int;
		else if (type->GetRefType() == CodeTreeTypeReferenceType::Float)
			func.ReturnType = SheepSymbolType::Float;
		else if (type->GetRefType() == CodeTreeTypeReferenceType::String)
			func.ReturnType = SheepSymbolType::String;
		else
		{
			declarationErrorsFound = true;
			context.Output.push_back(CompilerOutput(type->GetLineNumber(), "Invalid function return type. Valid types are int, float, and string."));
		}
	}

	if (params != nullptr)
	{
		for (int i = 0; i < params->ParameterTypes.size(); i++)
		{
			SheepCodeTreeSymbolTypeNode* paramType = params->ParameterTypes[i];
			SheepCodeTreeIdentifierReferenceNode* paramID = params->ParameterNames[i];

			SheepSymbol param;
			param.Name = paramID->GetName();

			// make sure the parameter name doesn't conflict with a global symbol
			if (m_symbolMap.find(param.Name) != m_symbolMap.end())
			{
				declarationErrorsFound = true;
				context.Output.push_back(CompilerOutput(paramID->GetLineNumber(), "Parameter identifier conflicts with an existing symbol"));
			}

			if (paramType->GetRefType() == CodeTreeTypeReferenceType::Int)
				param.Type = SheepSymbolType::Int;
			else if (paramType->GetRefType() == CodeTreeTypeReferenceType::Float)
				param.Type = SheepSymbolType::Float;
			else if (paramType->GetRefType() == CodeTreeTypeReferenceType::String)
				param.Type = SheepSymbolType::String;
			else
			{
				declarationErrorsFound = true;
				context.Output.push_back(CompilerOutput(paramID->GetLineNumber(), "Unknown parameter type (possible compiler bug)"));
			}

			func.Parameters.push_back(param);
		}
	}

	if (declarationErrorsFound)
		return func;

	SheepCodeTreeNode* child = function->FirstStatement;

	determineExpressionTypes(func, child);

	if (type != nullptr)
	{
		// make sure the function is returning something
		SheepCodeTreeStatementNode* smt = static_cast<SheepCodeTreeStatementNode*>(child);
		bool validReturnFound = false;
		
		while(smt != nullptr)
		{
			if (smt->GetStatementType() == CodeTreeKeywordStatementType::Return)
			{
				validReturnFound = true;
				break;
			}

			smt = static_cast<SheepCodeTreeStatementNode*>(smt->GetNextSibling());
		}

		if (validReturnFound == false)
			throw SheepCompilerException(function->GetLineNumber(), "Not all paths return a value.");
	}

	// write "store" instructions for the parameters
	for (int i = (int)func.Parameters.size() - 1; i >= 0; i--)
	{
		if (func.Parameters[i].Type == SheepSymbolType::Int)
			func.Code->WriteSheepInstruction(StoreArgI);
		else if (func.Parameters[i].Type == SheepSymbolType::Float)
			func.Code->WriteSheepInstruction(StoreArgF);
		else if (func.Parameters[i].Type == SheepSymbolType::String)
			func.Code->WriteSheepInstruction(StoreArgS);

		func.Code->WriteInt(i);
	}

	writeCode(func, child);

	if (type == nullptr)
	{
		// add one last bit (the GK3 compiler seems to always do this)
		func.Code->WriteSheepInstruction(ReturnV);
		func.Code->WriteSheepInstruction(SitnSpin);
		func.Code->WriteSheepInstruction(SitnSpin);
		func.Code->WriteSheepInstruction(SitnSpin);
		func.Code->WriteSheepInstruction(SitnSpin);
	}

	// now we have to go back and update all the GOTOs
	for (int i = 0; i < func.Gotos.size(); i++)
	{
		func.Code->WriteIntAt((int)func.Gotos[i].second + func.CodeOffset, func.Gotos[i].first);
	}

	return func;
}

void SheepCodeGenerator::writeCode(SheepFunction& function, SheepCodeTreeNode* node)
{
	while(node != NULL)
	{
		if (node->GetType() == CodeTreeNodeType::Statement)
			writeStatement(function, static_cast<SheepCodeTreeStatementNode*>(node));
		else if (node->GetType() == CodeTreeNodeType::Expression)
			writeExpression(function, static_cast<SheepCodeTreeExpressionNode*>(node));
		else if (node->GetType() == CodeTreeNodeType::Declaration)
		{
			SheepCodeTreeDeclarationNode* decl = static_cast<SheepCodeTreeDeclarationNode*>(node);

			// if this is a label declaration then add it to the list of labels
			if (decl->GetDeclarationType() == CodeTreeDeclarationNodeType::Label)
			{
				SheepCodeTreeIdentifierReferenceNode* id = static_cast<SheepCodeTreeIdentifierReferenceNode*>(decl->GetChild(0));

				// go get the label and set its offset
				m_labels[function.Declaration][id->GetName()] = function.Code->Tell();
			}
		}

		node = node->GetNextSibling();
	}
}

void SheepCodeGenerator::writeStatement(SheepFunction& function, SheepCodeTreeStatementNode* statement)
{
	assert(statement != NULL);

	if (statement->GetStatementType() == CodeTreeKeywordStatementType::Expression)
	{
		int itemsOnStack = writeExpression(function, static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0)));

		assert(itemsOnStack >= 0);
		for (int i = 0; i < itemsOnStack; i++)
			function.Code->WriteSheepInstruction(Pop);
	}
	else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Return)
	{
		SheepCodeTreeExpressionNode* expr = static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0));
		SheepCodeTreeSymbolTypeNode* returnType = polymorphic_downcast<SheepCodeTreeFunctionDeclarationNode*>(function.Declaration)->ReturnType;

		if (expr == nullptr && returnType != nullptr)
			throw SheepCompilerException(statement->GetLineNumber(), "Non-void functions must return a value.");

		if (returnType == nullptr)
		{
			if (expr != nullptr)
				throw SheepCompilerException(expr->GetLineNumber(), "Unexpected return expression in a void function");

			function.Code->WriteSheepInstruction(ReturnV);
		}
		else
		{
			writeExpression(function, expr);

			CodeTreeExpressionValueType exprType = expr->GetValueType();
			if (returnType->GetRefType() == CodeTreeTypeReferenceType::Int)
			{
				if (exprType == CodeTreeExpressionValueType::Float)
					function.Code->WriteSheepInstruction(FToI);
				else if (exprType != CodeTreeExpressionValueType::Int)
					throw SheepCompilerException(expr->GetLineNumber(), "Expected an integer");

				function.Code->WriteSheepInstruction(ReturnI);
			}
			else if (returnType->GetRefType() == CodeTreeTypeReferenceType::Float)
			{
				if (exprType == CodeTreeExpressionValueType::Int)
					function.Code->WriteSheepInstruction(IToF);
				else if (exprType != CodeTreeExpressionValueType::Float)
					throw SheepCompilerException(expr->GetLineNumber(), "Expected a float");

				function.Code->WriteSheepInstruction(ReturnF);
			}
			else if (returnType->GetRefType() == CodeTreeTypeReferenceType::String)
			{
				if (exprType != CodeTreeExpressionValueType::String)
					throw SheepCompilerException(expr->GetLineNumber(), "Expected a string");

				function.Code->WriteSheepInstruction(ReturnS);
			}
		}
	}
	else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Wait)
	{
		function.Code->WriteSheepInstruction(BeginWait);

		writeCode(function, statement->GetChild(0));

		function.Code->WriteSheepInstruction(EndWait);
	}
	else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Goto)
	{
		// write the goto instruction
		function.Code->WriteSheepInstruction(BranchGoto);

		// go get the label to which this GOTO refers and remember it
		SheepCodeTreeIdentifierReferenceNode* label = static_cast<SheepCodeTreeIdentifierReferenceNode*>(statement->GetChild(0));
		if (m_labels[function.Declaration].find(label->GetName()) == m_labels[function.Declaration].end())
			throw SheepCompilerException(statement->GetLineNumber(), "Couldn't find the label to which this goto refers");

		function.Gotos.push_back(std::pair<size_t, size_t&>(function.Code->Tell(), m_labels[function.Declaration][label->GetName()]));

		// write the placeholder for the label offset
		function.Code->WriteInt(0xdddddddd);
	}
	else if (statement->GetStatementType() == CodeTreeKeywordStatementType::If)
	{
		SheepCodeTreeExpressionNode* condition =
			static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0));
		writeExpression(function, condition);

		function.Code->WriteSheepInstruction(BranchIfZero);
		int ifBranchOffset = (int)function.Code->Tell();
		function.Code->WriteInt(0xdddddddd);

		// write the "happy path"
		writeCode(function, statement->GetChild(1));

		// if there's an else clause...
		if (statement->GetChild(2) != NULL)
		{
			function.Code->WriteSheepInstruction(Branch);
			size_t elseBranchOffset = function.Code->Tell();
			function.Code->WriteInt(0xdddddddd);

			function.Code->WriteIntAt(function.CodeOffset + (int)function.Code->Tell(), ifBranchOffset);

			writeCode(function, statement->GetChild(2));

			function.Code->WriteIntAt(function.CodeOffset + (int)function.Code->Tell(), elseBranchOffset);
		}
		else
		{
			// no else? just set the earlier branch to this offset
			function.Code->WriteIntAt(function.CodeOffset + (int)function.Code->Tell(), ifBranchOffset);
		}
	}
	else if (statement->GetStatementType() == CodeTreeKeywordStatementType::Assignment)
	{
		SheepCodeTreeExpressionNode* child1 = static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0));
		SheepCodeTreeExpressionNode* child2 = static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(1));

		assert(child2 != NULL);
		writeExpression(function, child2);
		if (child1->GetValueType() != child2->GetValueType())
		{
			assert(child1->GetValueType() != CodeTreeExpressionValueType::String);

			// should only be assigning a float to int or int to float at this point!
			if (child1->GetValueType() == CodeTreeExpressionValueType::Int &&
				child2->GetValueType() == CodeTreeExpressionValueType::Float)
			{
				function.Code->WriteSheepInstruction(FToI);
				function.Code->WriteUInt(0);
			}
			else
			{
				function.Code->WriteSheepInstruction(IToF);
				function.Code->WriteUInt(0);
			}
		}

		assert(child1 != NULL);
		assert(child1->GetExpressionType() == CodeTreeExpressionType::Identifier);

		SheepCodeTreeIdentifierReferenceNode* reference =
			static_cast<SheepCodeTreeIdentifierReferenceNode*>(child1);

		assert(reference->IsGlobal() == false);
		SheepSymbol variable;
		int symbolIndex;
		bool isGlobalVariable = false;
		bool variableFound = false;

		SymbolMap::iterator symbol = m_symbolMap.find(reference->GetName());
		if (symbol != m_symbolMap.end())
		{
			variable = symbol->second;

			// expression is just a regular ol' identifier, so get its index
			symbolIndex = getIndexOfVariable(variable);
			isGlobalVariable = true;
			variableFound = true;
		}
		else
		{
			// the symbol is not a global symbol. Maybe it's a function parameter.
			for (int i = 0; function.Parameters.size(); i++)
			{
				if (CIEqual(reference->GetName(), function.Parameters[i].Name))
				{
					variable = function.Parameters[i];
					symbolIndex = i;
					isGlobalVariable = false;
					variableFound = true;
					break;
				}
			}
		}

		if (variableFound == false)
			throw SheepCompilerException(reference->GetLineNumber(), "Unrecognized variable");
		
		if (child1->GetValueType() == CodeTreeExpressionValueType::Int)
		{
			if (isGlobalVariable)
				function.Code->WriteSheepInstruction(StoreI);
			else
				function.Code->WriteSheepInstruction(StoreArgI);
			function.Code->WriteInt(symbolIndex);
		}
		else if (child1->GetValueType() == CodeTreeExpressionValueType::Float)
		{
			if (isGlobalVariable)
				function.Code->WriteSheepInstruction(StoreF);
			else
				function.Code->WriteSheepInstruction(StoreArgF);
			function.Code->WriteInt(symbolIndex);
		}
		else
		{
			assert(child1->GetValueType() == CodeTreeExpressionValueType::String);

			if (isGlobalVariable)
				function.Code->WriteSheepInstruction(StoreS);
			else
			{
				// TODO: support string parameters
				throw SheepCompilerException(reference->GetLineNumber(), "String parameters are not supported yet");
			}
			function.Code->WriteInt(symbolIndex);
		}
	}
}

int SheepCodeGenerator::writeExpression(SheepFunction& function, SheepCodeTreeExpressionNode* expression)
{
	assert(expression != NULL);

	int itemsOnStack = 0;
	if (expression->GetExpressionType() == CodeTreeExpressionType::Constant)
	{
		SheepCodeTreeConstantNode* constant = static_cast<SheepCodeTreeConstantNode*>(expression);

		itemsOnStack++;
		if (constant->GetValueType() == CodeTreeExpressionValueType::Int)
		{
			function.Code->WriteSheepInstruction(PushI);
			function.Code->WriteInt(constant->GetIntValue());
		}
		else if (constant->GetValueType() == CodeTreeExpressionValueType::Float)
		{
			function.Code->WriteSheepInstruction(PushF);
			function.Code->WriteFloat(constant->GetFloatValue());
		}
		else if (constant->GetValueType() == CodeTreeExpressionValueType::String)
		{
			function.Code->WriteSheepInstruction(PushS);
			function.Code->WriteInt(constant->GetStringValue());
			function.Code->WriteSheepInstruction(GetString);
		}
		else
		{
			throw SheepCompilerException(constant->GetLineNumber(), "Unknown constant type");
		}
	}
	else if (expression->GetExpressionType() == CodeTreeExpressionType::Identifier)
	{
		SheepCodeTreeIdentifierReferenceNode* identifier =
			static_cast<SheepCodeTreeIdentifierReferenceNode*>(expression);

		if (identifier->IsGlobal())
		{
			// call to an import function, so check the parameters
			SheepImport import;
			if (m_imports->TryFindImport(identifier->GetName(), import) == false)
				throw SheepCompilerException(identifier->GetLineNumber(), "Unknown import function");
	

			std::vector<CodeTreeExpressionValueType> params;
			SheepCodeTreeExpressionNode* param =
				static_cast<SheepCodeTreeExpressionNode*>(identifier->GetChild(0));

			while(param != NULL)
			{
				if (params.size() >= import.Parameters.size())
					throw SheepCompilerException(identifier->GetLineNumber(), "Too many parameters");

				if (param->GetValueType() == CodeTreeExpressionValueType::String &&
					convertToExpressionValueType(import.Parameters[params.size()]) != CodeTreeExpressionValueType::String)
				{
					throw SheepCompilerException(param->GetLineNumber(), "Cannot convert string to parameter type");
				}
				else if (param->GetValueType() != CodeTreeExpressionValueType::String &&
					convertToExpressionValueType(import.Parameters[params.size()]) == CodeTreeExpressionValueType::String)
				{
					throw SheepCompilerException(param->GetLineNumber(), "Cannot convert parameter to string");
				}

				writeExpression(function, param);
				params.push_back(param->GetValueType());

				param = static_cast<SheepCodeTreeExpressionNode*>(param->GetNextSibling());
			}

			if (params.size() != import.Parameters.size())
				throw SheepCompilerException(identifier->GetLineNumber(), "Not enough parameters");

			// convert the parameters if necessary
			for (int i = 0; i < (int)params.size(); i++)
			{
				if (params[i] == CodeTreeExpressionValueType::Int && import.Parameters[i] == SheepSymbolType::Float)
				{
					function.Code->WriteSheepInstruction(IToF);
					function.Code->WriteUInt((int)params.size() - 1 - i);
				}
				else if (params[i] == CodeTreeExpressionValueType::Float && import.Parameters[i] == SheepSymbolType::Int)
				{
					function.Code->WriteSheepInstruction(FToI);
					function.Code->WriteUInt((int)params.size() - 1 - i);
				}
			}

			// write the number of parameters
			function.Code->WriteSheepInstruction(PushI);
			function.Code->WriteInt((int)params.size());

			if (import.ReturnType == SheepSymbolType::Void)
				function.Code->WriteSheepInstruction(CallSysFunctionV);
			else if (import.ReturnType == SheepSymbolType::Int)
				function.Code->WriteSheepInstruction(CallSysFunctionI);
			else if (import.ReturnType == SheepSymbolType::Float)
				function.Code->WriteSheepInstruction(CallSysFunctionF);
			else if (import.ReturnType == SheepSymbolType::String)
				function.Code->WriteSheepInstruction(CallSysFunctionS);
			else
				throw SheepCompilerException(identifier->GetLineNumber(), "Unsupported import return type");
			
			function.Code->WriteInt(getIndexOfImport(import));

			itemsOnStack++;

			function.ImportList.push_back(import.Name);
		}
		else
		{
			SheepSymbol variable;
			int symbolIndex;
			bool isGlobalVariable;
			bool variableExists = false;

			SymbolMap::iterator symbol = m_symbolMap.find(identifier->GetName());
			if (symbol != m_symbolMap.end())
			{
				variable = symbol->second;

				// expression is just a regular ol' identifier, so get its index
				symbolIndex = getIndexOfVariable(variable);
				isGlobalVariable = true;
				variableExists = true;
			}
			else
			{
				// the symbol is not a global symbol. Maybe it's a function parameter.
				for (int i = 0; function.Parameters.size(); i++)
				{
					if (CIEqual(identifier->GetName(), function.Parameters[i].Name))
					{
						variable = function.Parameters[i];
						symbolIndex = i;
						isGlobalVariable = false;
						variableExists = true;
						break;
					}
				}
			}

			if (variableExists == false)
				throw SheepCompilerException(identifier->GetLineNumber(), "Unknown identifier");

			itemsOnStack++;
			if (variable.Type == SheepSymbolType::Int)
			{
				if (isGlobalVariable)
					function.Code->WriteSheepInstruction(LoadI);
				else
					function.Code->WriteSheepInstruction(LoadArgI);
				function.Code->WriteInt(symbolIndex);
			}
			else if (variable.Type == SheepSymbolType::Float)
			{
				if (isGlobalVariable)
					function.Code->WriteSheepInstruction(LoadF);
				else
					function.Code->WriteSheepInstruction(LoadArgF);
				function.Code->WriteInt(symbolIndex);
			}
			else
			{
				assert(variable.Type == SheepSymbolType::String);
				if (isGlobalVariable)
					function.Code->WriteSheepInstruction(LoadS);
				else
				{
					// TODO: support string parameters
					throw SheepCompilerException(identifier->GetLineNumber(), "String parameters are not supported yet");
				}
				function.Code->WriteInt(symbolIndex);
				function.Code->WriteSheepInstruction(GetString);
			}
		}
	}
	else if (expression->GetExpressionType() == CodeTreeExpressionType::Operation)
	{
		SheepCodeTreeOperationNode* operation = static_cast<SheepCodeTreeOperationNode*>(expression);

		SheepCodeTreeExpressionNode* child1 = static_cast<SheepCodeTreeExpressionNode*>(operation->GetChild(0));
		SheepCodeTreeExpressionNode* child2 = static_cast<SheepCodeTreeExpressionNode*>(operation->GetChild(1));

		if (operation->GetOperationType() == CodeTreeOperationType::Negate)
		{
			writeExpression(function, child1);

			if (operation->GetValueType() == CodeTreeExpressionValueType::Int)
			{
				function.Code->WriteSheepInstruction(NegateI);
			}
			else // assume float
			{
				assert(operation->GetValueType() == CodeTreeExpressionValueType::Float);
				function.Code->WriteSheepInstruction(NegateF);
			}
		}
		else
		{
			// just a regular ol' binary operator
			SheepInstruction intOp, floatOp;

			itemsOnStack--; // everything pops twice and pushes once
			switch(operation->GetOperationType())
			{
			case CodeTreeOperationType::Add:
				intOp = AddI;
				floatOp = AddF;
				break;
			case CodeTreeOperationType::Minus:
				intOp = SubtractI;
				floatOp = SubtractF;
				break;
			case CodeTreeOperationType::Times:
				intOp = MultiplyI;
				floatOp = MultiplyF;
				break;
			case CodeTreeOperationType::Divide:
				intOp = DivideI;
				floatOp = DivideF;
				break;
			case CodeTreeOperationType::GreaterThan:
				intOp = IsGreaterI;
				floatOp = IsGreaterF;
				break;
			case CodeTreeOperationType::LessThan:
				intOp = IsLessI;
				floatOp = IsLessF;
				break;
			case CodeTreeOperationType::GreaterThanEqual:
				intOp = IsGreaterEqualI;
				floatOp = IsGreaterEqualF;
				break;
			case CodeTreeOperationType::LessThanEqual:
				intOp = IsLessEqualI;
				floatOp = IsLessEqualF;
				break;
			case CodeTreeOperationType::Equal:
				intOp = IsEqualI;
				floatOp = IsEqualF;
				break;
			case CodeTreeOperationType::NotEqual:
				intOp = NotEqualI;
				floatOp = NotEqualF;
				break;
			case CodeTreeOperationType::Not:
				intOp = Not;
				floatOp = Not;
				break;
			case CodeTreeOperationType::And:
				intOp = And;
				floatOp = And;
				break;
			case CodeTreeOperationType::Or:
				intOp = Or;
				floatOp = Or;
				break;
			default:
				throw SheepException("Unknown operator type!", SHEEP_UNKNOWN_ERROR_PROBABLY_BUG);
			}

			if (operation->GetValueType() == CodeTreeExpressionValueType::String)
				throw SheepCompilerException(operation->GetLineNumber(), "Operator not supported with strings (yet?)");
			
			itemsOnStack += writeExpression(function, child1);

			if (child2)
			{
				itemsOnStack += writeExpression(function, child2);

				// TODO: shouldn't type be determined by the parent's type (the operator)?
				if (child1->GetValueType() == CodeTreeExpressionValueType::Int && child2->GetValueType() == CodeTreeExpressionValueType::Float)
				{
					function.Code->WriteSheepInstruction(IToF);
					function.Code->WriteUInt(1);
				}
				
				if (child1->GetValueType() == CodeTreeExpressionValueType::Float && child2->GetValueType() == CodeTreeExpressionValueType::Int)
				{
					function.Code->WriteSheepInstruction(IToF);
					function.Code->WriteUInt(0);
				}
			}

			if (child1->GetValueType() == CodeTreeExpressionValueType::Int && (child2 == NULL || child2->GetValueType() == CodeTreeExpressionValueType::Int))
				function.Code->WriteSheepInstruction(intOp);
			else
				function.Code->WriteSheepInstruction(floatOp);
		}
	}

	return itemsOnStack;
}

SheepSymbolType SheepCodeGenerator::getSymbolType(int lineNumber, const std::string& name)
{
	SymbolMap::iterator itr = m_symbolMap.find(name);

	if (itr == m_symbolMap.end())
		throw SheepCompilerException(lineNumber, "Use of undefined symbol");

	return (*itr).second.Type;
}

CodeTreeExpressionValueType SheepCodeGenerator::convertToExpressionValueType(SheepSymbolType type)
{
	if (type == SheepSymbolType::Void)
		return CodeTreeExpressionValueType::Void;
	if (type == SheepSymbolType::Int)
		return CodeTreeExpressionValueType::Int;
	if (type == SheepSymbolType::Float)
		return CodeTreeExpressionValueType::Float;
	if (type == SheepSymbolType::String)
		return CodeTreeExpressionValueType::String;
	
	return CodeTreeExpressionValueType::Unknown;
}

int SheepCodeGenerator::getIndexOfImport(SheepImport &import)
{
	for (int i = 0; i < (int)m_usedImports.size(); i++)
	{
		if (CIEqual(m_usedImports[i].Name, import.Name))
			return i;
	}

	// still here? it must not be added yet, so add it.
	m_usedImports.push_back(import);

	return (int)m_usedImports.size() - 1;
}

int SheepCodeGenerator::getIndexOfVariable(SheepSymbol &symbol)
{
	for (int i = 0; i < (int)m_variables.size(); i++)
	{
		if (CIEqual(m_variables[i].Name, symbol.Name))
			return i;
	}

	// still here? it must not be added yet, so add it.
	m_variables.push_back(symbol);

	return (int)m_variables.size() - 1;
}