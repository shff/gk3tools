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
	~SheepFileWriter();

	void Write(const std::string& filename);
    ResizableBuffer* GetBuffer() { return m_buffer; }
	
private:

	void writeToBuffer();

	void writeFileHeader(int sectionCount);
	void writeSectionHeader(const std::string& label, int dataOffset, int dataCount);
	void writeVariablesSection();
	void writeImportsSection();
	void writeConstantsSection();
	void writeFunctionsSection();
	void writeCodeSection();

	template<typename T, typename Adder>
	void writeSection(const std::string& label, std::vector<T> collection, Adder adder);

	static const int DataSectionHeaderSize = 28;

	ResizableBuffer* m_buffer;
	IntermediateOutput* m_intermediateOutput;
};

#endif // SHEEPFILEWRITER_H
