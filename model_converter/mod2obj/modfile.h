#ifndef MODFILE_H
#define MODFILE_H

struct header
{
	char heading[4];
	unsigned char minorVersion;
	unsigned char majorVersion;
	unsigned short unknown1;
	unsigned int numMeshes;
	unsigned int size;
	float unknown2;
	unsigned int unknown3;
};

struct headerext
{
	unsigned int unknown[6];
};


struct meshsection
{
	unsigned int heading;
	char texturefile[32];
	
	unsigned int color;
	unsigned int smooth;
	unsigned int numVerts;
	unsigned int numTriangles;
	unsigned int numLODs;
	unsigned int unknown2;
	
	std::vector<float> vertices;
	std::vector<float> normals;
	std::vector<float> texcoords;
	std::vector<unsigned int> indices;
};

struct mesh
{
	unsigned int heading;
	float transform[12];
	
	unsigned int numSections;
	
	float bbox[6];
	
	std::vector<meshsection> sections;
};

struct modfile
{
	header h;
	headerext hext;
	
	std::vector<mesh> meshes;
};

#define COLORREF_R(c) (c & 0x000000ff)
#define COLORREF_G(c) ((c & 0x0000ff00) >> 8)
#define COLORREF_B(c) ((c & 0x00ff0000) >> 16)

#endif // MODFILE_H
