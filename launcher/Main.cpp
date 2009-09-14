#include <sstream>
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
		std::stringstream ss;
		ss << "width: " << modes[i].Width << " height: " << modes[i].Height;
		//MessageBox(NULL, ss.str().c_str(), "Mode", 0);

		window.AddDisplayMode(modes[i].Width, modes[i].Height);
	}

	window.ProcessMessages();


    return 0;
}
