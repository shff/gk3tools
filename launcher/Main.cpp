#include <windows.h>
#include "MainWindow.h"
#include "Video.h"

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, 
    LPSTR lpCmdLine, int nCmdShow)
{
    MainWindow window(hInstance, nCmdShow);

	std::vector<VideoDisplayMode> modes = GetValidDisplayModes(480);

	for (int i =0 ; i < modes.size(); i++)
	{
		window.AddDisplayMode(modes[i].Width, modes[i].Height);
	}

	window.ProcessMessages();


    return 0;
}
