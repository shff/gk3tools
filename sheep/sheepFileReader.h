#ifndef SHEEPFILEREADER_H
#define SHEEPFILEREADER_H

#include "sheepc.h"
#include "sheepTypes.h"
#include <string>
#include <cstring>
#include <vector>

struct SheepHeader
{
	unsigned int Magic1;
	unsigned int Magic2;
	unsigned int Unknown;
	unsigned int ExtraOffset;
	unsigned int DataOffset;
	unsigned int DataSize;
	unsigned int DataCount;

	unsigned int* OffsetArray;

	static const unsigned int SheepHeaderSize = 28;
	static const unsigned int Magic1Value = 0x53334b47;
	static const unsigned int Magic2Value = 0x70656568;
};

struct SectionHeader
{
	SectionHeader() { memset(Label, 0, 12); OffsetArray = NULL; }

	char Label[12];
	unsigned int ExtraOffset;
	unsigned int DataOffset;
	unsigned int DataSize;
	unsigned int DataCount;

	unsigned int* OffsetArray;

	static const unsigned int SectionHeaderSize = 28;
};

class IntermediateOutput;
class SheepImportTable;
class SheepMachine;

class SheepFileReader
{
public:

	SheepFileReader(const std::string& filename);
	SheepFileReader(std::ifstream& file);
	SheepFileReader(const byte* data, int length);

	void WireImportCallbacks(SheepMachine* imports);

	IntermediateOutput* GetIntermediateOutput() { return m_intermediateOutput; }

private:

	void read(const byte* data, unsigned int length);

	static unsigned int getFileSize(std::ifstream& file);
	SectionHeader readSectionHeader(const byte* data, const std::string& name, int* bytesRead);
	static std::string readString(const byte* data, int* bytesRead);
	static std::string readString(const byte* data, unsigned int length, int* bytesRead);

	IntermediateOutput* m_intermediateOutput;
};


#endif // SHEEPFILEREADER_H
