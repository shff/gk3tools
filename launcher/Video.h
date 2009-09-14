#ifndef VIDEO_H
#define VIDEO_H

#include <vector>

struct VideoDisplayMode
{
	int Width, Height;
};


std::vector<VideoDisplayMode> GetValidDisplayModes(int minimumHeight);


#endif // VIDEO_H
