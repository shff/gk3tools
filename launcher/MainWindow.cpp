#include <sstream>
#include <windows.h>
#include "MainWindow.h"

const char g_szClassName[] = "gk3LauncherWindowClass";


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
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 240, 320,
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

}

void MainWindow::createChildControls(HINSTANCE instance)
{
	m_label = createLabel(instance, m_hwnd, "Screen resolution:");

	m_modeList = CreateListbox(instance, m_hwnd, 0, 20, 200, 150);

	CreateCheckbox(instance, m_hwnd, 0, 170, "Fullscreen", true);

	m_btnGo = CreateButton(instance, m_hwnd, 0, 200, "Go!");
}

HWND MainWindow::createLabel(HINSTANCE instance, HWND parent, const char* text)
{
	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);

	HWND label = CreateWindowEx(WS_EX_CLIENTEDGE, "STATIC", text, WS_CHILD | WS_VISIBLE, 0, 0, 100, 20, parent, NULL, instance, NULL);

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
	HWND button = CreateWindowEx(WS_EX_WINDOWEDGE, "BUTTON", text, WS_CHILD | WS_VISIBLE, x, y, 100, 32, parent, NULL, instance, NULL);

	HGDIOBJ hfDefault = GetStockObject(DEFAULT_GUI_FONT);
	SendMessage(button, WM_SETFONT, (WPARAM)hfDefault, MAKELPARAM(FALSE, 0));

	return button;
}

HWND CreateCheckbox(HINSTANCE instance, HWND parent, int x, int y, const char* text, bool checked)
{
	HWND checkbox = CreateWindowEx(0, "BUTTON", text, WS_CHILD | WS_VISIBLE | BS_AUTOCHECKBOX, x, y, 100, 24, parent, NULL, instance, NULL);

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
