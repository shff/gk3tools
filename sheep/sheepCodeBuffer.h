#ifndef SHEEPCODEBUFFER_H
#define SHEEPCODEBUFFER_H

#include "rbuffer.h"

enum SheepInstruction
{
	SitnSpin             = 0x00,
	CallSysFunctionV     = 0x02,
	CallSysFunctionI     = 0x03,
	Branch               = 0x06,
	BranchIfZero         = 0x08,
	
	BeginWait            = 0x09,
	EndWait              = 0x0A,
	ReturnV              = 0x0B,

	StoreI               = 0x0D,
	StoreF               = 0x0E,
	LoadI                = 0x10,
	LoadF				 = 0x11,
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

	IsEqualI             = 0x21,
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
	And                  = 0x30,
	Or                   = 0x31,
	Not                  = 0x32,
	GetString            = 0x33
};

class SheepCodeBuffer : public ResizableBuffer
{
public:

	void WriteSheepInstruction(SheepInstruction instruction)
	{
		char op = instruction;
		Write(&op, 1);
	}

	void WriteInt(int value)
	{
		// TODO: adjust for endianness!
		Write((char*)&value, 4);
	}

	void WriteIntAt(int value, size_t offset)
	{
		WriteAt((char*)&value, 4, offset);
	}

	void WriteFloat(float value)
	{
		// TODO: adjust for endianness!
		Write((char*)&value, 4);
	}
};

#endif // SHEEPCODEBUFFER_H