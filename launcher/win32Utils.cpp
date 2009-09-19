#include <windows.h>
#include <cstdio>
#include "win32Utils.h"


#pragma comment(linker, \
    "\"/manifestdependency:type='Win32' "\
    "name='Microsoft.Windows.Common-Controls' "\
    "version='6.0.0.0' "\
    "processorArchitecture='*' "\
    "publicKeyToken='6595b64144ccf1df' "\
    "language='*'\"")


HWND CreateLabel(HINSTANCE instance, HWND parent, int x, int y, const char* text)
{
	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);

	HWND label = CreateWindowEx(0, "STATIC", text, WS_CHILD | WS_VISIBLE, x, y, 100, 13, parent, NULL, instance, NULL);

	SendMessage(label, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	return label;
}

HWND CreateListbox(HINSTANCE instance, HWND parent, int x, int y, int width, int height)
{
	HWND listbox = CreateWindowEx(WS_EX_CLIENTEDGE, "LISTBOX", NULL, WS_CHILD | WS_VISIBLE | WS_VSCROLL | WS_TABSTOP, x, y, width, height, parent, NULL, instance, NULL);

	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);
	SendMessage(listbox, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	SetWindowPos(listbox, HWND_TOP, x, y, width, height, 0);

	return listbox;
}

HWND CreateButton(HINSTANCE instance, HWND parent, int x, int y, const char* text)
{
	HWND button = CreateWindowEx(WS_EX_WINDOWEDGE, "Button", text, WS_CHILD | WS_VISIBLE | WS_TABSTOP, x, y, 75, 23, parent, NULL, instance, NULL);

	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);
	SendMessage(button, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	return button;
}

HWND CreateCheckbox(HINSTANCE instance, HWND parent, int x, int y, const char* text, bool checked)
{
	HWND checkbox = CreateWindowEx(0, "BUTTON", text, WS_CHILD | WS_VISIBLE | BS_AUTOCHECKBOX | WS_TABSTOP, x, y, 100, 17, parent, NULL, instance, NULL);

	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);
	SendMessage(checkbox, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	if (checked)
		SendMessage(checkbox, BM_SETCHECK, BST_CHECKED, 0);

	return checkbox;
}

void CenterWindow(HWND window)
{
	HWND parent;
	if ((parent = GetParent(window)) == NULL)
		parent = GetDesktopWindow();

	RECT parentRect;
	GetWindowRect(parent, &parentRect);

	RECT windowRect;
	GetWindowRect(window, &windowRect);

	OffsetRect(&windowRect, -windowRect.left, -windowRect.top);

	SetWindowPos(window, HWND_TOP, 
		parentRect.left + parentRect.right / 2 - windowRect.right / 2,
		parentRect.top + parentRect.bottom / 2 - windowRect.bottom / 2,
		windowRect.right, windowRect.bottom, 0);
}

void GetErrorMessage(DWORD error, const char* prefix, char* buffer, int len)
{
	char errorBuffer[256];
	FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, NULL, error, MAKELANGID(LANG_USER_DEFAULT, SUBLANG_DEFAULT), errorBuffer, 256, NULL);

	_snprintf(buffer, len, prefix, errorBuffer);
}
