#ifndef SHEEPFILEWRITER_H
#define SHEEPFILEWRITER_H

#include <string>
#include <vector>

class ResizableBuffer;
class IntermediateOutput;

class SheepFileWriter
{
public:

	SheepFileWriter(IntermediateOutput* output);

	void Write(const std::string& filename);
	
private:

	void writeSectionHeader(const std::string& label, size_t dataOffset, size_t dataCount);
	void writeVariablesSection();
	void writeImportsSection();
	void writeConstantsSection();
	void writeFunctionsSection();

	template<typename T, typename Adder>
	void writeSection(const std::string& label, std::vector<T> collection, Adder adder);

	static const int DataSectionHeaderSize = 28;

	ResizableBuffer* m_buffer;
	IntermediateOutput* m_intermediateOutput;
};

#endif // SHEEPFILEWRITER_H