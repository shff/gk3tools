#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <map>
#include "matrix.h"

typedef Vector3 vertex;

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

struct Material
{
    std::string Texture;
    float R, G, B;
};

struct group
{
	std::string name;
	Material material;
	std::vector<vertex> vertices;
	std::vector<normal> normals;
	std::vector<texcoord> texcoords;
	std::vector<unsigned short> indices;

    int meshIndex;
    int sectionIndex;
	
	// these are temporary, during the load process
	std::vector<unsigned short> vertexindices;
	std::vector<unsigned short> normalindices;
	std::vector<unsigned short> textureindices;
};

struct ModelMesh
{
    Matrix OriginalMatrix;
    std::vector<group> Groups;
};

class Model
{
public:
	
	void Load(const std::string& filename);
	void Save(const std::string& filename);

private:

    void loadMeta(const std::string& filename, const std::map<std::string, group>& groups);
    void saveMesh(FILE* fp, const ModelMesh& mesh);

	float getFloat(const std::string& str);
	unsigned int getUInt(const std::string& str);

	// these parse the x/y/z format, returning -1 if the requested index wasn't found
	int getVertexIndex(const std::string& str);
	int getNormalIndex(const std::string& str);
	int getTexcoordIndex(const std::string& str);
	int getIndex(const std::string& str, int whichone);

    BBox calculateBBox(const ModelMesh& mesh);
	BBox calculateBBox(std::vector<vertex> vertices);

	unsigned int getDataSectionSize(const std::map<std::string, group>& groups);

	void loadMaterials(const std::string& file);
	
	// global lists
	std::map<std::string, Material> m_materials;
    std::vector<ModelMesh> m_meshes;
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
