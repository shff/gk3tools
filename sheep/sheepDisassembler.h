#ifndef DISASSEMBLER_H
#define DISASSEMBLER_H

#include <string>
#include <fstream>
#include <vector>
#include <memory.h>
#include "sheepc.h"
#include "sheepFileReader.h"
//#include "sheepfile.h"

namespace Sheep
{
	struct Import
	{
		unsigned short LengthOfName;
		std::string Name;
		byte NumReturns;
		byte NumParameters;

		byte* ParametersTypes;
	};

	struct StringConstant
	{
		std::string String;
		unsigned int Offset;
	};

	struct LocalFunction
	{
		std::string Name;
		unsigned int Offset;
	};

    class Disassembly : public Sheep::IDisassembly
    {
        std::string m_text;

	public:

		Disassembly(const char* text)
		{
			m_text = text;
		}

		void Release() override
		{
			// don't bother with ref counting.
			delete this;
		}

		const char* GetDisassemblyText() override
		{
			return m_text.c_str();
		}
    };

	class Disassembler
	{
	public:
		static Disassembly* GetDisassembly(IntermediateOutput* intermediateOutput);
	
	private:

		static unsigned int printNextInstruction(const byte* code, std::ostream& output, std::vector<SheepImport>& imports, std::vector<SheepStringConstant>& constants);
		
		static void printDisassembly(std::ostream& output, int op, const std::string& name);
		static void printDisassembly(std::ostream& output, int op,
			int byte1, int byte2, int byte3, int byte4,
			const std::string& name, unsigned int param);
		static void printDisassembly(std::ostream& output, int op,
			int byte1, int byte2, int byte3, int byte4,
			const std::string& name, const std::string param);
		static void printDisassembly(std::ostream& output, int op,
			int byte1, int byte2, int byte3, int byte4,
			const std::string& name);
		
		static unsigned int convertBytesToInt(int byte1, int byte2, int byte3, int byte4)
		{
			return (byte4 << 24) | (byte3 << 16) | (byte2 << 8) | byte1;
		}
	};
}

#endif // DISASSEMBLER