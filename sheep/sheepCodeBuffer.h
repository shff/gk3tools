#ifndef SHEEPCODEBUFFER_H
#define SHEEPCODEBUFFER_H

#include "rbuffer.h"

enum SheepInstruction
{
	SitnSpin             = 0x00,
	Yield                = 0x01,
	CallSysFunctionV     = 0x02,
	CallSysFunctionI     = 0x03,
	CallSysFunctionF     = 0x04,
	CallSysFunctionS     = 0x05,
	Branch               = 0x06,
	BranchGoto           = 0x07,
	BranchIfZero         = 0x08,
	
	BeginWait            = 0x09,
	EndWait              = 0x0A,
	ReturnV              = 0x0B,

	Fatal                = 0x0C,

	StoreI               = 0x0D,
	StoreF               = 0x0E,
	StoreS               = 0x0F,
	LoadI                = 0x10,
	LoadF                = 0x11,
	LoadS                = 0x12,
	PushI                = 0x13,
	PushF                = 0x14,
	PushS                = 0x15,
	Pop                  = 0x16,

	AddI                 = 0x17,
	AddF                 = 0x18,
	SubtractI            = 0x19,
	SubtractF            = 0x1A,
	MultiplyI            = 0x1B,
	MultiplyF            = 0x1C,
	DivideI              = 0x1D,
	DivideF              = 0x1E,
	NegateI              = 0x1F,
	NegateF              = 0x20,

	IsEqualI             = 0x21,
	IsEqualF             = 0x22,
	NotEqualI            = 0x23,
	NotEqualF            = 0x24,
	IsGreaterI           = 0x25,
	IsGreaterF           = 0x26,
	IsLessI              = 0x27,
	IsLessF              = 0x28,
	IsGreaterEqualI      = 0x29,
	IsGreaterEqualF      = 0x2A,
	IsLessEqualI         = 0x2B,
	IsLessEqualF         = 0x2C,

	IToF                 = 0x2D,
	FToI                 = 0x2E,
	Modulo               = 0x2F,
	And                  = 0x30,
	Or                   = 0x31,
	Not                  = 0x32,
	GetString            = 0x33,
	DebugBreakpoint      = 0x34,


	// the following are not part of GK3. They
	// are part of our own "enhanced" instruction set.
	ReturnI              = 0xA0,
	ReturnF              = 0xA1,
	ReturnS              = 0xA2,

	// pops the value on top of the stack into the specified local variable
	StoreLocalI          = 0xA3, 
	StoreLocalF          = 0xA4,
	StoreLocalS          = 0xA5,

	// pushes the value in the specified local variable onto the stack
	LoadLocalI           = 0xA6,
	LoadLocalF           = 0xA7,
	LoadLocalS           = 0xA8,
	
	// pops the value on the top of the stack into the specified function parameter
	StoreArgI            = 0xA9,
	StoreArgF            = 0xAA,
	StoreArgS            = 0xAB,

	// pushes the value in the specified function parameter onto the stack
	LoadArgI             = 0xAC,
	LoadArgF             = 0xAD,
	LoadArgS             = 0xAE
};

class SheepCodeBuffer : public ResizableBuffer
{
public:

	SheepCodeBuffer(int size)
		: ResizableBuffer(size)
	{
	}

	SheepCodeBuffer()
		: ResizableBuffer()
	{
	}

	virtual ~SheepCodeBuffer() { }

	void WriteSheepInstruction(SheepInstruction instruction)
	{
		char op = (char)instruction;
		Write(&op, 1);
	}

	void WriteIntAt(int value, size_t offset)
	{
		WriteAt((char*)&value, 4, offset);
	}
};

#endif // SHEEPCODEBUFFER_H
