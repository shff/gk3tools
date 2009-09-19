#ifndef MAINWINDOW_H
#define MAINWINDOW_H

class MainWindow
{
	bool m_active;
	HWND m_hwnd;
	HWND m_label;
	HWND m_modeList;
	HWND m_chkFullscreen;
	HWND m_btnGo;

public:
	MainWindow(HINSTANCE instance, int cmdShow);
	~MainWindow();

	void ProcessMessages();

	void AddDisplayMode(int width, int height);

	bool IsActive() { return m_active; }

	void Go();

private:

	void launchGame(int screenWidth, int screenHeight, bool fullscreen);
	void createChildControls(HINSTANCE instance);

	static LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
};



#endif // MAINWINDOW_H
