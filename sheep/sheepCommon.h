#ifndef SHEEPCOMMON_H
#define SHEEPCOMMON_H

/// @file

#ifdef _MSC_VER
#define SHP_DECLSPEC __declspec(dllexport)
#define SHP_LIB_CALL __cdecl
#define SHP_CALLBACK __stdcall
#define SHP_APIENTRY __stdcall
#else
#define SHP_DECLSPEC
#define SHP_LIB_CALL
#define SHP_CALLBACK __attribute__((stdcall))
#define SHP_APIENTRY __attribute__((stdcall))
#endif


/// Success, no errors
#define SHEEP_SUCCESS 0

/// Generic error
#define SHEEP_ERROR -1

/// No such function exists
#define SHEEP_ERR_NO_SUCH_FUNCTION -2

/// The specified file was not found
#define SHEEP_ERR_FILE_NOT_FOUND -10

/// The specified file was not in a valid format
#define SHEEP_ERR_INVALID_FILE_FORMAT -11

/// There was an unspecified compiler error
#define SHEEP_GENERIC_COMPILER_ERROR -100

/// There is not a running context
#define SHEEP_ERR_NO_CONTEXT_AVAILABLE -101

/// The stack cannot be popped because it is empty
#define SHEEP_ERR_EMPTY_STACK -102

/// The variable on the top of the stack was not the expected type
#define SHEEP_ERR_WRONG_TYPE_ON_STACK -103

/// An invalid argument was passed
#define SHEEP_ERR_INVALID_ARGUMENT -105

/// The specified variable does not exist
#define SHEEP_ERR_VARIABLE_NOT_FOUND -106

/// The specified variable was found, but was not the expected type
#define SHEEP_ERR_VARIABLE_INCORRECT_TYPE -107

/// The object is not in a state that can perform the operation
#define SHEEP_ERR_INVALID_OPERATION -108

/// An unspecified VM error
#define SHEEP_GENERIC_VM_ERROR -200

/// An unexpected error that most likely means there's a bug in Sheep
#define SHEEP_UNKNOWN_ERROR_PROBABLY_BUG -1000

/// The virtual machine has been suspended by a wait command
#define SHEEP_SUSPENDED 2


extern "C"
{
	struct SHP_Version
	{
		unsigned char Major, Minor, Revision;
	};

	SHP_DECLSPEC SHP_Version shp_GetVersion();
}

#endif // SHEEPCOMMON_H
