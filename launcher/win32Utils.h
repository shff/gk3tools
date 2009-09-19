#ifndef WIN32UTILS_H
#define WIN32UTILS_H

HWND CreateListbox(HINSTANCE instance, HWND parent, int x, int y, int width, int height);
HWND CreateButton(HINSTANCE instance, HWND parent, int x, int y, const char* text);
HWND CreateCheckbox(HINSTANCE instance, HWND parent, int x, int y, const char* text, bool checked);
HWND CreateLabel(HINSTANCE instance, HWND parent, int x, int y, const char* text);

void CenterWindow(HWND window);

void GetErrorMessage(DWORD error, const char* prefix, char* buffer, int len);

#endif // WIN32UTILS_H
