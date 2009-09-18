#include <sstream>
#include <windows.h>
#include "MainWindow.h"

const char g_szClassName[] = "gk3LauncherWindowClass";
const int g_windowWidth = 250;
const int g_windowHeight = 228;

MainWindow::MainWindow(HINSTANCE instance, int cmdShow)
{
	m_active = false;

	// register the window class
	WNDCLASSEX wc;

    wc.cbSize        = sizeof(WNDCLASSEX);
    wc.style         = 0;
    wc.lpfnWndProc   = WndProc;
    wc.cbClsExtra    = 0;
    wc.cbWndExtra    = 0;
    wc.hInstance     = instance;
    wc.hIcon         = LoadIcon(NULL, IDI_APPLICATION);
    wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_3DFACE+1);
    wc.lpszMenuName  = NULL;
    wc.lpszClassName = g_szClassName;
    wc.hIconSm       = LoadIcon(NULL, IDI_APPLICATION);

    if(!RegisterClassEx(&wc))
    {
        MessageBox(NULL, "Window Registration Failed!", "Error!",
            MB_ICONEXCLAMATION | MB_OK);
		return;
    }

	m_hwnd = CreateWindowEx(
        WS_EX_WINDOWEDGE,
        g_szClassName,
        "GK3 Launcher",
        WS_POPUPWINDOW | WS_CAPTION,
        CW_USEDEFAULT, CW_USEDEFAULT, g_windowWidth, 320,
        NULL, NULL, instance, NULL);

    if(m_hwnd == NULL)
    {
        MessageBox(NULL, "Window Creation Failed!", "Error!",
            MB_ICONEXCLAMATION | MB_OK);
        return;
    }

	SetWindowLongPtr(m_hwnd, GWLP_USERDATA, (LONG_PTR)this);

	createChildControls(instance);

    ShowWindow(m_hwnd, cmdShow);
    UpdateWindow(m_hwnd);

	m_active = true;
}

MainWindow::~MainWindow()
{
	// nothing
}

void MainWindow::ProcessMessages()
{
	MSG msg;

	while(GetMessage(&msg, NULL, 0, 0) > 0)
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
}

void MainWindow::AddDisplayMode(int width, int height)
{
	std::stringstream ss;
	ss << width << "x" << height;
	int index = SendMessage(m_modeList, LB_ADDSTRING, 0, (LPARAM)ss.str().c_str());
}

void MainWindow::Go()
{
	// get the selected window size
	int count = SendMessage(m_modeList, LB_GETCOUNT, NULL, NULL);
	for (int i = 0; i < count; i++)
	{
		if (SendMessage(m_modeList, LB_GETSEL, (WPARAM)i, NULL) > 0)
		{
			int len = SendMessage(m_modeList, LB_GETTEXTLEN, (WPARAM)i, NULL);
			
			const char* buffer = new char[len + 1];
			SendMessage(m_modeList, LB_GETTEXT, (WPARAM)i, (LPARAM)buffer);

			int w, h;
			sscanf(buffer, "%dx%d", &w, &h);

			bool fullscreen = (SendMessage(m_chkFullscreen, BM_GETCHECK, 0, 0) == BST_CHECKED);

			delete[] buffer;

			launchGame(w, h, fullscreen);
		}
	}
}

void MainWindow::launchGame(int screenWidth, int screenHeight, bool fullscreen)
{
	char demoArguments[255];
	char retailArguments[255];

	if (fullscreen)
	{
		_snprintf(demoArguments, 255, "Gk3demo.exe -width %d -height %d", screenWidth, screenHeight);
		_snprintf(retailArguments, 255, "GK3.exe -width %d -height %d", screenWidth, screenHeight);
	}
	else
	{
		_snprintf(demoArguments, 255, "Gk3demo.exe -width %d -height %d -window", screenWidth, screenHeight);
		_snprintf(retailArguments, 255, "GK3.exe -width %d -height %d -window", screenWidth, screenHeight);
	}

	STARTUPINFO si;
	FillMemory(&si, sizeof(STARTUPINFO), 0);
	si.cb = sizeof(STARTUPINFO);

	PROCESS_INFORMATION pi;

	// try to start normal GK3
	if (CreateProcess(NULL, retailArguments, NULL, NULL, false, 0, NULL, NULL, &si, &pi) != 0)
		return;

	DWORD err1 = GetLastError();

	// that didn't work? try the demo
	if (CreateProcess(NULL, demoArguments, NULL, NULL, false, 0, NULL, NULL, &si, &pi) == 0)
	{
		char buffer[256];
		FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, NULL,err1, MAKELANGID(LANG_USER_DEFAULT, SUBLANG_DEFAULT), buffer, 256, NULL);

		char bigbuffer[512];
		_snprintf(bigbuffer, 512, "Unable to launch GK3: %s", buffer);
		MessageBox(m_hwnd, bigbuffer, "Error!", MB_OK | MB_ICONERROR);
	}
}

void MainWindow::createChildControls(HINSTANCE instance)
{
	m_label = createLabel(instance, m_hwnd, 12, 9, "Screen resolution:");

	m_modeList = CreateListbox(instance, m_hwnd, 12, 25, g_windowWidth - 40, 147);

	m_chkFullscreen = CreateCheckbox(instance, m_hwnd, 12, 178, "Fullscreen", true);

	m_btnGo = CreateButton(instance, m_hwnd, 147, 195, "Go!");
}

HWND MainWindow::createLabel(HINSTANCE instance, HWND parent, int x, int y, const char* text)
{
	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);

	HWND label = CreateWindowEx(0, "STATIC", text, WS_CHILD | WS_VISIBLE, x, y, 100, 13, parent, NULL, instance, NULL);

	SendMessage(label, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	return label;
}

HWND CreateListbox(HINSTANCE instance, HWND parent, int x, int y, int width, int height)
{
	HWND listbox = CreateWindowEx(WS_EX_CLIENTEDGE, "LISTBOX", NULL, WS_CHILD | WS_VISIBLE | WS_VSCROLL, x, y, width, height, parent, NULL, instance, NULL);

	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);
	SendMessage(listbox, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	return listbox;
}

HWND CreateButton(HINSTANCE instance, HWND parent, int x, int y, const char* text)
{
	HWND button = CreateWindowEx(WS_EX_WINDOWEDGE, "Button", text, WS_CHILD | WS_VISIBLE, x, y, 75, 23, parent, NULL, instance, NULL);

	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);
	SendMessage(button, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	return button;
}

HWND CreateCheckbox(HINSTANCE instance, HWND parent, int x, int y, const char* text, bool checked)
{
	HWND checkbox = CreateWindowEx(0, "BUTTON", text, WS_CHILD | WS_VISIBLE | BS_AUTOCHECKBOX, x, y, 100, 17, parent, NULL, instance, NULL);

	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);
	SendMessage(checkbox, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	if (checked)
		SendMessage(checkbox, BM_SETCHECK, BST_CHECKED, 0);

	return checkbox;
}

LRESULT CALLBACK MainWindow::WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch(msg)
    {
	case WM_COMMAND:
		{
		MainWindow* window = (MainWindow*)GetWindowLongPtr(hwnd, GWLP_USERDATA);
		if ((HWND)lParam == window->m_btnGo)
			window->Go();
		}
		break;
    case WM_CLOSE:
        DestroyWindow(hwnd);
		break;
    case WM_DESTROY:
        PostQuitMessage(0);
		break;
    default:
        return DefWindowProc(hwnd, msg, wParam, lParam);
    }
    return 0;
}
