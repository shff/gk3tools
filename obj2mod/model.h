#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <map>

struct vertex
{	
	float x, y, z;
};

struct normal
{
	float x, y, z;
};

struct texcoord
{
	float u, v;
};

struct BBox
{
	vertex vertex1, vertex2;
};

struct group
{
	std::string name;
	std::string texture;
	std::vector<vertex> vertices;
	std::vector<normal> normals;
	std::vector<texcoord> texcoords;
	std::vector<unsigned short> indices;
	
	// these are temporary, during the load process
	std::vector<unsigned short> vertexindices;
	std::vector<unsigned short> normalindices;
	std::vector<unsigned short> textureindices;
};

class Model
{
public:
	
	void Load(const std::string& filename);
	void Save(const std::string& filename);

private:

	float getFloat(const std::string& str);
	unsigned int getUInt(const std::string& str);

	// these parse the x/y/z format, returning -1 if the requested index wasn't found
	int getVertexIndex(const std::string& str);
	int getNormalIndex(const std::string& str);
	int getTexcoordIndex(const std::string& str);
	int getIndex(const std::string& str, int whichone);

	BBox calculateBBox(std::vector<vertex> vertices);

	unsigned int getDataSectionSize();

	void loadMaterials(const std::string& file);
	
	// global lists
	std::vector<vertex> vertices;
	std::vector<normal> normals;
	std::vector<texcoord> texcoords;
	std::map<std::string, std::string> materials;
	
	typedef std::map<std::string, group> groupmap;
	groupmap groups;
};

class ModelException
{
public:
	ModelException(const std::string& message)
	{
		this->message = message;
	}
	
	std::string message;
};
