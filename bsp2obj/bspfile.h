#ifndef BSPFILE_H
#define BSPFILE_H

#include <vector>

struct header
{
	char heading[4];
	unsigned short minorVersion;
	unsigned short majorVersion;
	unsigned int dataSectionSize;
	unsigned int rootIndex;
	unsigned int numModels;
	unsigned int numVertices;
	unsigned int numTexCoords;
	unsigned int numVertexIndices;
	unsigned int numTexIndices;
	unsigned int numSurfaces;
	unsigned int numPlanes;
	unsigned int numNodes;
	unsigned int numPolygons;
};

struct polygon
{
	unsigned short vertexIndex;
	unsigned short flags;
	unsigned short numVertices;
	unsigned short surfaceIndex;
};

struct surface
{
	unsigned int modelIndex;
	char texture[32];
	float uCoord, vCoord;
	float uScale, vScale;
	float size;
	unsigned int flags;

	std::vector<polygon> polygons;

	std::vector<unsigned short> secondaryTriangles;
};

struct bspNode
{
	unsigned short childIndex1, childIndex2;
	unsigned short planeIndex;
	unsigned short polygonStartIndex;
	unsigned short unknown1;
	unsigned short numPolygons;
	unsigned short unknown2;
	unsigned short unknown3;
};

struct plane
{
	float x, y, z, d;
};

struct vertex
{
	float x, y, z;

	vertex() { x = 0; y = 0; z = 0; }
	vertex(float x, float y, float z) { this->x = x; this->y = y; this->z = z; }
	vertex operator-(const vertex& v) { return vertex(x - v.x, y - v.y, z - v.z); }
};

struct texCoord
{
	float u, v;
};

struct boundingSphere
{
	float x, y, z, radius;
};

struct thingy
{
	float boundingSphereX, boundingSphereY, boundingSphereZ;
	float boundingSphereRadius;

	float chromeValue;
	float grazing;
	unsigned int chromeColor;
	unsigned int numIndices;
	unsigned int numTriangles;
};

struct bspmodel
{
	char name[32];
	
	std::vector<surface> surfaces;
};

struct bspfile
{
	header h;

	std::vector<bspmodel> models;
	std::vector<polygon> polygons;
	std::vector<surface> surfaces;
	std::vector<vertex> vertices;
	std::vector<texCoord> textureCoords;
	std::vector<unsigned short> indices;
	std::vector<unsigned short> texindices;
};

#endif // BSPFILE_H