#include <iostream>
#include <algorithm>
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepException.h"
#include "sheepCodeBuffer.h"

void IntermediateOutput::Print()
{
	std::cout << "------------" << std::endl;

	for (std::vector<SheepSymbol>::iterator itr = Symbols.begin(); itr != Symbols.end(); itr++)
		std::cout << "Symbol: " << SheepSymbolTypeNames[(*itr).Type] << " " << (*itr).Name << std::endl;

	for (std::vector<SheepFunction>::iterator itr = Functions.begin(); itr != Functions.end(); itr++)
		std::cout << "Function: " << (*itr).Name << std::endl;

	for (std::vector<CompilerOutput>::iterator itr = Errors.begin(); itr != Errors.end(); itr++)
		std::cout << "Error at line " << (*itr).LineNumber << ": " << (*itr).Output << std::endl;

	std::cout << "------------" << std::endl;
}

SheepCodeGenerator::SheepCodeGenerator(SheepCodeTree* tree, SheepImportTable* imports)
{
	assert(tree != NULL);
	assert(imports != NULL);

	m_tree = tree;
	m_imports = imports;
}

IntermediateOutput* SheepCodeGenerator::BuildIntermediateOutput()
{
	std::auto_ptr<IntermediateOutput> output(new IntermediateOutput());

	std::map<std::string, SheepImport> usedImports;

	try
	{
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
			if (section->GetSectionType() == SECTIONTYPE_CODE ||
				section->GetSectionType() == SECTIONTYPE_SNIPPET)
				determineExpressionTypes(section->GetChild(0));

			// iterate over each function/snippet and output a SheepFunction object
			if (section->GetSectionType() == SECTIONTYPE_CODE)
			{
				SheepCodeTreeDeclarationNode* function = 
					static_cast<SheepCodeTreeDeclarationNode*>(section->GetChild(0));

				while(function != NULL)
				{
					SheepFunction func = writeFunction(function);
					output->Functions.push_back(func);

					// copy any new imports to the list of imports
					for (std::vector<std::string>::iterator itr = func.ImportList.begin();
						itr != func.ImportList.end(); itr++)
					{
						SheepImport import;
						m_imports->TryFindImport(*itr, import);
						usedImports.insert(std::pair<std::string, SheepImport>(import.Name, import));
					}

					function = static_cast<SheepCodeTreeDeclarationNode*>(function->GetNextSibling());
				}
			}
			else if (section->GetSectionType() == SECTIONTYPE_SNIPPET)
			{
				// TODO!
			}

			section = static_cast<SheepCodeTreeSectionNode*>(section->GetNextSibling());
		}

		// copy the symbols into the output
		for (SymbolMap::iterator itr = m_symbolMap.begin(); itr != m_symbolMap.end(); itr++)
		{
			SheepSymbol symbol = (*itr).second;

			if (symbol.Type == SYM_INT || symbol.Type == SYM_FLOAT || symbol.Type == SYM_STRING)
				output->Symbols.push_back((*itr).second);
		}

		// copy the imports into the output
		for (std::map<std::string, SheepImport>::iterator itr = usedImports.begin();
			itr != usedImports.end(); itr++)
		{
			output->Imports.push_back((*itr).second);
		}

	}
	catch(SheepCompilerException& ex)
	{
		CompilerOutput error;
		error.LineNumber = ex.GetLineNumber();
		error.Output = ex.GetMessage();

		output->Errors.push_back(error);
	}

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
		assert(node->GetType() == NODETYPE_SECTION);

		SheepCodeTreeSectionNode* section = static_cast<SheepCodeTreeSectionNode*>(node);

		if (section->GetSectionType() == SECTIONTYPE_SYMBOLS ||
			section->GetSectionType() == SECTIONTYPE_CODE)
		{
			// section is full of yummy declaration nodes!
			SheepCodeTreeDeclarationNode* declaration = 
				static_cast<SheepCodeTreeDeclarationNode*>(section->GetChild(0));

			while(declaration != NULL)
			{
				SheepSymbol symbol;
				symbol.Name = declaration->GetDeclarationName();
				symbol.Type = convertToSymbolType(declaration->GetDeclarationType());

				SheepCodeTreeConstantNode* constant =
						static_cast<SheepCodeTreeConstantNode*>(declaration->GetChild(1));

				if (constant != NULL)
				{
					if (symbol.Type == SYM_INT)
						symbol.InitialIntValue = constant->GetIntValue();
					else if (symbol.Type == SYM_FLOAT)
						symbol.InitialFloatValue = constant->GetFloatValue();
					else if (symbol.Type == SYM_STRING)
						symbol.InitialStringValue = constant->GetStringValue();
					else if (symbol.Type == SYM_LOCALFUNCTION)
					{
						// don't do anything
					}
					else
					{
						throw SheepCompilerException(declaration->GetLineNumber(),
							"Symbols must be 'int', 'float', or 'string'");
					}
				}

				if (m_symbolMap.insert(SymbolMap::value_type(symbol.Name, symbol)).second == false)
					throw SheepCompilerException(declaration->GetLineNumber(), "Symbol already defined");

				// should we add this as a variable?
				if (section->GetSectionType() == SECTIONTYPE_SYMBOLS)
				{
					if (symbol.Type == SYM_INT || symbol.Type == SYM_FLOAT || symbol.Type == SYM_STRING)
						m_variables.push_back(symbol);
				}

				declaration = static_cast<SheepCodeTreeDeclarationNode*>(declaration->GetNextSibling());
			}
		}

		node = node->GetNextSibling();
	}
}

void SheepCodeGenerator::determineExpressionTypes(SheepCodeTreeNode* node)
{
	while (node != NULL)
	{
		if (node->GetType() == NODETYPE_DECLARATION)
		{
			determineExpressionTypes(node->GetChild(1));
		}
		else if (node->GetType() == NODETYPE_EXPRESSION)
		{
			SheepCodeTreeExpressionNode* expr = static_cast<SheepCodeTreeExpressionNode*>(node);

			if (expr->GetExpressionType() == EXPRTYPE_OPERATION)
			{
				SheepCodeTreeOperationNode* operation = static_cast<SheepCodeTreeOperationNode*>(expr);

				SheepCodeTreeExpressionNode* child1 = static_cast<SheepCodeTreeExpressionNode*>(expr->GetChild(0));
				SheepCodeTreeExpressionNode* child2 = static_cast<SheepCodeTreeExpressionNode*>(expr->GetChild(1));

				if (child1 != NULL)	determineExpressionTypes(child1);
				if (child2 != NULL) determineExpressionTypes(child2);

				if (child1->GetValueType() == EXPRVAL_VOID ||
					child2->GetValueType() == EXPRVAL_VOID)
				{
					// can't use void with *any* operators!
					throw SheepCompilerException(operation->GetLineNumber(), "Cannot use void types with operator");
				}

				switch(operation->GetOperationType())
				{
					case OP_ADD:
					case OP_MINUS:
					case OP_TIMES:
					case OP_DIVIDE:
						if (child1->GetValueType() == EXPRVAL_STRING ||
							child2->GetValueType() == EXPRVAL_STRING)
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
							operation->SetValueType(EXPRVAL_FLOAT);
						}
						break;
					case OP_GT:
					case OP_LT:
					case OP_GTE:
					case OP_LTE:
					case OP_EQ:
					case OP_NE:
						if ((child1->GetValueType() == EXPRVAL_STRING ||
							child2->GetValueType() == EXPRVAL_STRING) &&
							child1->GetValueType() != child2->GetValueType())
						{
							throw SheepCompilerException(operation->GetLineNumber(), "Cannot compare string and non-string");
						}
						else
						{
							operation->SetValueType(EXPRVAL_INT);
						}
						break;
					default:
						throw SheepException("Unknown operation type");

				}
				
			}
			else if (expr->GetExpressionType() == EXPRTYPE_IDENTIFIER)
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
					determineExpressionTypes(identifier->GetChild(1));

					identifier->SetValueType(convertToExpressionValueType(import.ReturnType));
				}
				else
				{
					SheepSymbolType definedType = getSymbolType(identifier->GetLineNumber(), identifier->GetName());

					if (definedType == SYM_LOCALFUNCTION)
						throw SheepCompilerException(identifier->GetLineNumber(), "Function name used like a variable");
					else if (definedType == SYM_INT)
						identifier->SetValueType(EXPRVAL_INT);
					else if (definedType == SYM_FLOAT)
						identifier->SetValueType(EXPRVAL_FLOAT);
					else if (definedType == SYM_STRING)
						identifier->SetValueType(EXPRVAL_STRING);
					else
						throw SheepCompilerException(identifier->GetLineNumber(), "Expected variable");
				}
			}
			else if (expr->GetExpressionType() == EXPRTYPE_CONSTANT)
			{
				// nothing to do! type is already defined
			}
		}
		else if (node->GetType() == NODETYPE_STATEMENT)
		{
			SheepCodeTreeStatementNode* statement = static_cast<SheepCodeTreeStatementNode*>(node);

			if (statement->GetStatementType() == SMT_IF)
			{
				determineExpressionTypes(statement->GetChild(0));
				determineExpressionTypes(statement->GetChild(1));
				determineExpressionTypes(statement->GetChild(2));

				SheepCodeTreeExpressionNode* ifCondition = 
					static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0));

				if (ifCondition->GetValueType() != EXPRVAL_INT)
					throw SheepCompilerException(statement->GetLineNumber(), "Condition must evaluate to an int");
			}
			else if (statement->GetStatementType() == SMT_WAIT)
			{
				determineExpressionTypes(statement->GetChild(0));
			}
			else if (statement->GetStatementType() == SMT_ASSIGN)
			{
				determineExpressionTypes(statement->GetChild(0));
				determineExpressionTypes(statement->GetChild(1));
			}
			else if (statement->GetStatementType() == SMT_EXPR)
			{
				determineExpressionTypes(statement->GetChild(0));
			}
		}

		node = node->GetNextSibling();
	}
}


SheepFunction SheepCodeGenerator::writeFunction(SheepCodeTreeDeclarationNode* function)
{
	assert(function->GetDeclarationType() == DECLARATIONTYPE_FUNCTION);

	SheepFunction func;
	func.Name = function->GetDeclarationName();
	func.Code = new SheepCodeBuffer();

	SheepCodeTreeNode* child = function->GetChild(1);

	writeCode(func, child);

	// add one last bit (the GK3 compiler seems to always do this)
	func.Code->WriteSheepInstruction(ReturnV);
	func.Code->WriteSheepInstruction(SitnSpin);
	func.Code->WriteSheepInstruction(SitnSpin);
	func.Code->WriteSheepInstruction(SitnSpin);
	func.Code->WriteSheepInstruction(SitnSpin);

	return func;
}

void SheepCodeGenerator::writeCode(SheepFunction& function, SheepCodeTreeNode* node)
{
	while(node != NULL)
	{
		if (node->GetType() == NODETYPE_STATEMENT)
			writeStatement(function, static_cast<SheepCodeTreeStatementNode*>(node));
		else if (node->GetType() == NODETYPE_EXPRESSION)
			writeExpression(function, static_cast<SheepCodeTreeExpressionNode*>(node));
		else if (node->GetType() == NODETYPE_DECLARATION)
		{
			// TODO: this must be a label, which can be ignored since 'goto' isn't supported yet
		}

		node = node->GetNextSibling();
	}
}

void SheepCodeGenerator::writeStatement(SheepFunction& function, SheepCodeTreeStatementNode* statement)
{
	assert(statement != NULL);

	if (statement->GetStatementType() == SMT_EXPR)
	{
		int itemsOnStack = writeExpression(function, static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0)));

		assert(itemsOnStack >= 0);
		for (int i = 0; i < itemsOnStack; i++)
			function.Code->WriteSheepInstruction(Pop);
	}
	else if (statement->GetStatementType() == SMT_RETURN)
	{
		function.Code->WriteSheepInstruction(ReturnV);
	}
	else if (statement->GetStatementType() == SMT_WAIT)
	{
		function.Code->WriteSheepInstruction(BeginWait);

		writeCode(function, statement->GetChild(0));

		function.Code->WriteSheepInstruction(EndWait);
	}
	else if (statement->GetStatementType() == SMT_GOTO)
	{
		throw SheepCompilerException(statement->GetLineNumber(), "Sorry, 'goto' isn't supported yet");
	}
	else if (statement->GetStatementType() == SMT_IF)
	{
		SheepCodeTreeExpressionNode* condition =
			static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0));
		writeExpression(function, condition);

		function.Code->WriteSheepInstruction(BranchIfZero);
		size_t ifBranchOffset = function.Code->Tell();
		function.Code->WriteInt(0xdddddddd);

		// write the "happy path"
		writeCode(function, statement->GetChild(1));

		// if there's an else clause...
		if (statement->GetChild(2) != NULL)
		{
			function.Code->WriteSheepInstruction(Branch);
			size_t elseBranchOffset = function.Code->Tell();
			function.Code->WriteInt(0xdddddddd);

			function.Code->WriteIntAt(function.Code->Tell(), ifBranchOffset);

			writeCode(function, statement->GetChild(2));

			function.Code->WriteIntAt(function.Code->Tell(), elseBranchOffset);
		}
		else
		{
			// no else? just set the earlier branch to this offset
			function.Code->WriteIntAt(function.Code->Tell(), ifBranchOffset);
		}
	}
	else if (statement->GetStatementType() == SMT_ASSIGN)
	{
		SheepCodeTreeExpressionNode* child1 = static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(0));
		SheepCodeTreeExpressionNode* child2 = static_cast<SheepCodeTreeExpressionNode*>(statement->GetChild(1));

		assert(child2 != NULL);
		writeExpression(function, child2);
		if (child1->GetValueType() != child2->GetValueType())
		{
			assert(child1->GetValueType() != SYM_STRING);

			// should only be assigning a float to int or int to float at this point!
			if (child1->GetValueType() == SYM_INT &&
				child2->GetValueType() == SYM_FLOAT)
				function.Code->WriteSheepInstruction(FToI);
			else
				function.Code->WriteSheepInstruction(IToF);
		}

		assert(child1 != NULL);
		assert(child1->GetExpressionType() == EXPRTYPE_IDENTIFIER);

		SheepCodeTreeIdentifierReferenceNode* reference =
			static_cast<SheepCodeTreeIdentifierReferenceNode*>(child1);

		assert(reference->IsGlobal() == false);
		SheepSymbol variable = (*m_symbolMap.find(reference->GetName())).second;
		size_t index = getIndexOfVariable(variable);
		
		if (child1->GetValueType() == EXPRVAL_INT)
		{
			function.Code->WriteSheepInstruction(StoreI);
			function.Code->WriteInt(index);
		}
		else if (child1->GetValueType() == EXPRVAL_FLOAT)
		{
			function.Code->WriteSheepInstruction(StoreF);
			function.Code->WriteInt(index);
		}
		else
		{
			assert(child1->GetValueType() == EXPRVAL_STRING);

			function.Code->WriteSheepInstruction(StoreS);
			function.Code->WriteInt(index);
		}
	}
}

int SheepCodeGenerator::writeExpression(SheepFunction& function, SheepCodeTreeExpressionNode* expression)
{
	assert(expression != NULL);

	int itemsOnStack = 0;
	if (expression->GetExpressionType() == EXPRTYPE_CONSTANT)
	{
		SheepCodeTreeConstantNode* constant = static_cast<SheepCodeTreeConstantNode*>(expression);

		itemsOnStack++;
		if (constant->GetValueType() == EXPRVAL_INT)
		{
			function.Code->WriteSheepInstruction(PushI);
			function.Code->WriteInt(constant->GetIntValue());
		}
		else if (constant->GetValueType() == EXPRVAL_FLOAT)
		{
			function.Code->WriteSheepInstruction(PushF);
			function.Code->WriteFloat(constant->GetFloatValue());
		}
		else if (constant->GetValueType() == EXPRVAL_STRING)
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
	else if (expression->GetExpressionType() == EXPRTYPE_IDENTIFIER)
	{
		SheepCodeTreeIdentifierReferenceNode* identifier =
			static_cast<SheepCodeTreeIdentifierReferenceNode*>(expression);

		if (identifier->IsGlobal())
		{
			// call to an import function, so check the parameters
			SheepImport import;
			m_imports->TryFindImport(identifier->GetName(), import);
	
			size_t counter = 0;
			SheepCodeTreeExpressionNode* param =
				static_cast<SheepCodeTreeExpressionNode*>(identifier->GetChild(0));

			while(param != NULL)
			{
				if (counter >= import.Parameters.size())
					throw SheepCompilerException(identifier->GetLineNumber(), "Too many parameters");

				if (param->GetValueType() != convertToExpressionValueType(import.Parameters[counter]))
				{
					throw SheepCompilerException(param->GetLineNumber(), "Invalid parameter(s)");
				}

				writeExpression(function, param);

				counter++;
				param = static_cast<SheepCodeTreeExpressionNode*>(param->GetNextSibling());
			}

			if (counter != import.Parameters.size())
				throw SheepCompilerException(identifier->GetLineNumber(), "Not enough parameters");

			// write the number of parameters
			function.Code->WriteSheepInstruction(PushI);
			function.Code->WriteInt(counter);

			if (import.ReturnType == SYM_VOID)
				function.Code->WriteSheepInstruction(CallSysFunctionV);
			else if (import.ReturnType == SYM_INT)
				function.Code->WriteSheepInstruction(CallSysFunctionI);
			else
				throw SheepCompilerException(identifier->GetLineNumber(), "Unsupported import return type");
			
			function.Code->WriteInt(getIndexOfImport(import));

			itemsOnStack++;

			function.ImportList.push_back(import.Name);
		}
		else
		{
			SheepSymbol variable = (*m_symbolMap.find(identifier->GetName())).second;

			// expression is just a regular ol' identifier, so get its index
			int index = getIndexOfVariable(variable);

			itemsOnStack++;
			if (variable.Type == SYM_INT)
			{
				function.Code->WriteSheepInstruction(LoadI);
				function.Code->WriteInt(index);
			}
			else if (variable.Type == SYM_FLOAT)
			{
				function.Code->WriteSheepInstruction(LoadF);
				function.Code->WriteInt(index);
			}
			else
			{
				assert(variable.Type == SYM_STRING);
				function.Code->WriteSheepInstruction(LoadS);
				function.Code->WriteInt(index);
				function.Code->WriteSheepInstruction(GetString);
			}
		}
	}
	else if (expression->GetExpressionType() == EXPRTYPE_OPERATION)
	{
		SheepCodeTreeOperationNode* operation = static_cast<SheepCodeTreeOperationNode*>(expression);

		SheepInstruction op;

		SheepCodeTreeExpressionNode* child1 = static_cast<SheepCodeTreeExpressionNode*>(operation->GetChild(0));
		SheepCodeTreeExpressionNode* child2 = static_cast<SheepCodeTreeExpressionNode*>(operation->GetChild(1));

		if (operation->GetOperationType() == OP_NEGATE)
		{
			if (operation->GetValueType() == SYM_INT)
			{
				function.Code->WriteSheepInstruction(NegateI);
			}
			else // assume float
			{
				assert(operation->GetValueType() == SYM_FLOAT);
				function.Code->WriteSheepInstruction(NegateF);
			}
		}
		else
		{
			// just a regular ol' binary operator
			SheepInstruction intOp, floatOp, stringOp;

			itemsOnStack--; // everything pops twice and pushes once
			switch(operation->GetOperationType())
			{
			case OP_ADD:
				intOp = AddI;
				floatOp = AddF;
				break;
			case OP_MINUS:
				intOp = SubtractI;
				floatOp = SubtractF;
				break;
			case OP_TIMES:
				intOp = MultiplyI;
				floatOp = MultiplyF;
				break;
			case OP_DIVIDE:
				intOp = DivideI;
				floatOp = DivideF;
				break;
			case OP_GT:
				intOp = IsGreaterI;
				floatOp = IsGreaterF;
				break;
			case OP_LT:
				intOp = IsLessI;
				floatOp = IsLessF;
				break;
			case OP_GTE:
				intOp = IsGreaterEqualI;
				floatOp = IsGreaterEqualF;
				break;
			case OP_LTE:
				intOp = IsLessEqualI;
				floatOp = IsLessEqualF;
				break;
			case OP_EQ:
				intOp = IsEqualI;
				floatOp = IsEqualF;
				break;
			case OP_NE:
				throw SheepCompilerException(operation->GetLineNumber(), "Sorry, != operator not supported yet");
			default:
				throw SheepException("Unknown operator type!");
			}

			if (operation->GetValueType() == SYM_STRING)
				throw SheepCompilerException(operation->GetLineNumber(), "Operator not supported with strings (yet?)");
			
			itemsOnStack += writeExpression(function, child1);
			if (operation->GetValueType() == SYM_INT && child1->GetValueType() == SYM_FLOAT)
				function.Code->WriteSheepInstruction(FToI);
			else if (operation->GetValueType() == SYM_FLOAT && child1->GetValueType() == SYM_INT)
				function.Code->WriteSheepInstruction(IToF);

			itemsOnStack += writeExpression(function, child2);
			if (operation->GetValueType() == SYM_INT && child2->GetValueType() == SYM_FLOAT)
				function.Code->WriteSheepInstruction(FToI);
			else if (operation->GetValueType() == SYM_FLOAT && child2->GetValueType() == SYM_INT)
				function.Code->WriteSheepInstruction(IToF);

			if (operation->GetValueType() == SYM_INT)
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

SheepSymbolType SheepCodeGenerator::convertToSymbolType(CodeTreeDeclarationNodeType type)
{
	if (type == DECLARATIONTYPE_INT)
		return SYM_INT;
	if (type == DECLARATIONTYPE_FLOAT)
		return SYM_FLOAT;
	if (type == DECLARATIONTYPE_STRING)
		return SYM_STRING;
	if (type == DECLARATIONTYPE_FUNCTION)
		return SYM_LOCALFUNCTION;
	if (type == DECLARATIONTYPE_LABEL)
		return SYM_LABEL;

	// we should never get here!
	throw SheepException("Unknown declaration type");
}

CodeTreeExpressionValueType SheepCodeGenerator::convertToExpressionValueType(SheepSymbolType type)
{
	if (type == SYM_VOID)
		return EXPRVAL_VOID;
	if (type == SYM_INT)
		return EXPRVAL_INT;
	if (type == SYM_FLOAT)
		return EXPRVAL_FLOAT;
	if (type == SYM_STRING)
		return EXPRVAL_STRING;
	
	return EXPRVAL_UNKNOWN;
}

size_t SheepCodeGenerator::getIndexOfImport(SheepImport &import)
{
	for (size_t i = 0; i < m_usedImports.size(); i++)
	{
		if (m_usedImports[i].Name == import.Name)
			return i;
	}

	// still here? it must not be added yet, so add it.
	m_usedImports.push_back(import);

	return m_usedImports.size() - 1;
}

size_t SheepCodeGenerator::getIndexOfVariable(SheepSymbol &symbol)
{
	for (size_t i = 0; i < m_variables.size(); i++)
	{
		if (m_variables[i].Name == symbol.Name)
			return i;
	}

	// still here? it must not be added yet, so add it.
	m_variables.push_back(symbol);

	return m_variables.size() - 1;
}