#include <D3D9.h>
#include "Video.h"

std::vector<VideoDisplayMode> GetValidDisplayModes(int minimumHeight)
{
	// create a D3D9 object
	LPDIRECT3D9 d3d9 = Direct3DCreate9(D3D_SDK_VERSION);
	if(d3d9 == NULL)
		return std::vector<VideoDisplayMode>();

	UINT count = d3d9->GetAdapterModeCount(D3DADAPTER_DEFAULT, D3DFMT_R5G6B5);

	int previousWidth = 0, previousHeight = 0;
	std::vector<VideoDisplayMode> modes;
	for (UINT i = 0; i < count; i++)
	{
		D3DDISPLAYMODE mode;
		if (d3d9->EnumAdapterModes(D3DADAPTER_DEFAULT, D3DFMT_R5G6B5, i, &mode) != D3D_OK)
			return std::vector<VideoDisplayMode>();

		if (mode.Height < minimumHeight ||
			(mode.Width == previousWidth && mode.Height == previousHeight))
			continue;

		VideoDisplayMode vdm;
		vdm.Width = mode.Width;
		vdm.Height = mode.Height;

		previousWidth = mode.Width;
		previousHeight = mode.Height;

		modes.push_back(vdm);
	}

	d3d9->Release();

	return modes;
}