#ifndef MAINWINDOW_H
#define MAINWINDOW_H

class MainWindow
{
	bool m_active;
	HWND m_hwnd;
	HWND m_label;
	HWND m_modeList;
	HWND m_btnGo;

public:
	MainWindow(HINSTANCE instance, int cmdShow);
	~MainWindow();

	void ProcessMessages();

	void AddDisplayMode(int width, int height);

	bool IsActive() { return m_active; }

	void Go();

private:

	void createChildControls(HINSTANCE instance);

	static HWND createLabel(HINSTANCE instance, HWND parent, const char* text);
	static LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
};

HWND CreateListbox(HINSTANCE instance, HWND parent, int x, int y, int width, int height);
HWND CreateButton(HINSTANCE instance, HWND parent, int x, int y, const char* text);
HWND CreateCheckbox(HINSTANCE instance, HWND parent, int x, int y, const char* text, bool checked);

#endif // MAINWINDOW_H
