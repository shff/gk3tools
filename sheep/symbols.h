#ifndef SYMBOLS_H
#define SYMBOLS_H

#ifdef __cplusplus

extern "C"
{
#endif

int AddIntSymbol(char* name, int value);
int AddFloatSymbol(char* name, float value);
void AddStringSymbol(char* name, char* value);

void AssignSymbolValue(char* name);

void AddIntegerToStack(int i);
void AddFloatToStack(float f);
void AddStringToStack(char* string);

void AddLocalFunction(char* name, int makeCurrent);

void AddFunctionCall(char* functionName);

void AddLocalValueToStack(char* valueName);

void Addition();
void Subtraction();
void Multiplication();
void Division();

void GreaterThan();
void LessThan();

void And();
void Or();

void AddIf();
void AddElse();
void EndIf();

void CreateSnippet();

#ifdef __cplusplus
}
#endif

#endif // SYMBOLS_H
