#include <sstream>
#include <stdio.h>
#include "model.h"

#define WRITE4(f,p) fwrite(p, 4, 1, fp)
#define WRITE2(f,p) fwrite(p, 2, 1, fp)
#define WRITE1(f,p) fwrite(p, 1, 1, fp)

void Model::Load(const std::string& filename)
{
	// open the input file
	std::ifstream file(filename.c_str(), std::ios::in);
	
	if (file.good() == false)
		throw ModelException("Unable to open file");
	
	char line[256];
	group* currentGroup = NULL;
	while(file.good())
	{		
		file.getline(line, 255);
		std::stringstream ss(line);
		std::string command, param1, param2, param3;
		ss >> command >> param1 >> param2 >> param3;
		
		if (command == "mtllib")
		{
			loadMaterials(param1);
		}
		else if (command == "v")
		{
			vertex v;
			
			v.x = getFloat(param1);
			v.y = getFloat(param2);
			v.z = getFloat(param3);
			
			vertices.push_back(v);
		}
		else if (command == "vn")
		{
			normal n;
			
			n.x = getFloat(param1);
			n.y = getFloat(param2);
			n.z = getFloat(param3);
			
			normals.push_back(n);
		}
		else if (command == "vt")
		{
			texcoord t;
			
			t.u = getFloat(param1);
			t.v = getFloat(param2);

			texcoords.push_back(t);
		}
		else if (command == "g")
		{
			if (param1 != "")
			{
				if (groups.find(param1) == groups.end())
				{
					group newgroup;
					newgroup.name = param1;
					
					groups[param1] = newgroup;
				}
				
				std::cout << "Adding group" << std::endl;
				currentGroup = &groups[param1];
			}
		}
		else if (command == "f" && currentGroup != NULL)
		{
			std::cout << "face!" << std::endl;
			int v[3] = {getVertexIndex(param1), getVertexIndex(param2), getVertexIndex(param3)};
			int n[3] = {getNormalIndex(param1), getNormalIndex(param2), getNormalIndex(param3)};
			int t[3] = {getTexcoordIndex(param1), getTexcoordIndex(param2), getTexcoordIndex(param3)};
			
			unsigned int i;
			
			for (int index = 0; index < 3; index++)
			{
				for (i = 0; i < currentGroup->vertexindices.size(); i++)
				{
					if (currentGroup->vertexindices[i] == v[index] &&
						currentGroup->textureindices[i] == t[index] &&
						currentGroup->normalindices[i] == n[index])
					{
						std::cout << "equal" << std::endl;
						break;
					}
				}
					
				if (i == currentGroup->vertexindices.size())
				{
					std::cout << "Pushing " << v[index] << std::endl;
					currentGroup->vertexindices.push_back(v[index]);
					currentGroup->textureindices.push_back(t[index]);
					currentGroup->normalindices.push_back(n[index]);
				}
					
				currentGroup->indices.push_back(i);
			}
		}
		else if (command == "usemtl" && currentGroup != NULL)
		{
			std::map<std::string, std::string>::iterator itr = materials.find(param1);
			
			if (itr != materials.end())
			{
				std::cout << "Setting texture!" << std::endl;
				currentGroup->texture = (*itr).second;
			}
		}
	}
		
	file.close();
	
	// now go through all the groups and build the list of vertices, normals, and texcoords using
	// the handy index lists we've built
	for (groupmap::iterator itr = groups.begin(); itr != groups.end(); itr++)
	{
		group* currentgroup = &(*itr).second;
		std::cout << "Changing group" << std::endl;
			
		for (int j = 0; j < currentgroup->vertexindices.size(); j++)
		{
			// DON'T FORGET the difference in indexing (we start at 0, .OBJ's start at 1)!
			currentgroup->vertices.push_back(vertices[currentgroup->vertexindices[j] - 1]);
			currentgroup->normals.push_back(normals[currentgroup->normalindices[j]-1]);
			currentgroup->texcoords.push_back(texcoords[currentgroup->textureindices[j]-1]);
				
			std::cout << "Adding!" << std::endl;
		}
	}
}

void Model::Save(const std::string& filename)
{
	FILE* fp = fopen(filename.c_str(), "wb");
	
	if (!fp)
		throw ModelException("Unable to open file for writing");
	
	// write the header
	unsigned int header = 0x4D4F444C;
	char version_minor = 6;
	char version_major = 1;
	unsigned short unknownShort = 0;
	unsigned int unknownInt = 0;
	
	unsigned int numMeshes = 1;
	unsigned int totalSize = getDataSectionSize();
	
	WRITE4(fp, &header);
	WRITE1(fp, &version_minor);
	WRITE1(fp, &version_major);
	WRITE2(fp, &unknownShort);
	WRITE4(fp, &numMeshes);
	WRITE4(fp, &totalSize);
	WRITE4(fp, &unknownInt);
	WRITE4(fp, &unknownInt);
	
	// write the MESH section
	unsigned int meshHeader = 0x4D455348;
	float transform[12];
	transform[0] = 1.0f;
	transform[1] = 0;
	transform[2] = 0;
	transform[3] = 0;
	transform[4] = 1.0f;
	transform[5] = 0;
	transform[6] = 0;
	transform[7] = 0;
	transform[8] = 1.0f;
	transform[9] = 0;
	transform[10] = 0;
	transform[11] = 0;
	
	unsigned int numSections = 1;
	BBox bbox = calculateBBox(vertices);

	WRITE4(fp, &meshHeader);
	WRITE4(fp, &transform[0]);
	WRITE4(fp, &transform[1]);
	WRITE4(fp, &transform[2]);
	WRITE4(fp, &transform[3]);
	WRITE4(fp, &transform[4]);
	WRITE4(fp, &transform[5]);
	WRITE4(fp, &transform[6]);
	WRITE4(fp, &transform[7]);
	WRITE4(fp, &transform[8]);
	WRITE4(fp, &transform[9]);
	WRITE4(fp, &transform[10]);
	WRITE4(fp, &transform[11]);
	
	WRITE4(fp, &numSections);
	
	WRITE4(fp, &bbox.vertex1.x);
	WRITE4(fp, &bbox.vertex1.y);
	WRITE4(fp, &bbox.vertex1.z);
	WRITE4(fp, &bbox.vertex2.x);
	WRITE4(fp, &bbox.vertex2.y);
	WRITE4(fp, &bbox.vertex2.z);
	
	// write the subgroups
	for (groupmap::iterator itr = groups.begin(); itr != groups.end(); itr++)
	{
		group currentgroup = (*itr).second;
		
		unsigned int groupHeader = 0x4D475250;
		char textureFile[32] = {0};
		strncpy(textureFile, currentgroup.texture.c_str(), currentgroup.texture.length());
		unsigned int numFaces = 1;
		unsigned int numVerts = currentgroup.vertices.size();
		unsigned int numTris = currentgroup.indices.size() / 3;
		unsigned int lodLevels = 0;
		
		std::cout << "Num verts: " << numVerts << std::endl;
		std::cout << "Num triangles: " << numTris << std::endl;
		
		WRITE4(fp, &groupHeader);
		fwrite(textureFile, 32, 1, fp);
		WRITE4(fp, &unknownInt);
		WRITE4(fp, &numFaces);
		WRITE4(fp, &numVerts);
		WRITE4(fp, &numTris);
		WRITE4(fp, &lodLevels);
		WRITE4(fp, &unknownInt);
		
		for (unsigned int i = 0; i < currentgroup.vertices.size(); i++)
		{
			// reverse x
			float x = -currentgroup.vertices[i].x;
			
			WRITE4(fp, &currentgroup.vertices[i].x);
			WRITE4(fp, &currentgroup.vertices[i].y);
			WRITE4(fp, &currentgroup.vertices[i].z);
		}
		
		for (unsigned int i = 0; i < currentgroup.vertices.size(); i++)
		{
			WRITE4(fp, &currentgroup.normals[i].x);
			WRITE4(fp, &currentgroup.normals[i].y);
			WRITE4(fp, &currentgroup.normals[i].z);
		}
		
		for (unsigned int i = 0; i < currentgroup.vertices.size(); i++)
		{
			// adjust u
			float u = 1.0f - currentgroup.texcoords[i].u;
			WRITE4(fp, &u);
			
			// reverse the v
			float v = -currentgroup.texcoords[i].v;
			WRITE4(fp, &v);
			
			std::cout << "t: " << currentgroup.texcoords[i].u << " " << currentgroup.texcoords[i].v << std::endl;
		}
		
		for (unsigned int i = 0; i < currentgroup.indices.size() / 3; i++)
		{
			unsigned short zero = 0;
			WRITE2(fp, &currentgroup.indices[i * 3 + 0]);
			WRITE2(fp, &currentgroup.indices[i * 3 + 1]);
			WRITE2(fp, &currentgroup.indices[i * 3 + 2]);
			WRITE2(fp, &zero);
		}
	}
	
	fclose(fp);
}

unsigned int Model::getDataSectionSize()
{
	const unsigned int MeshHeaderSize = 80;
	const unsigned int SectionHeaderSize = 70;
	
	unsigned int size = MeshHeaderSize;
	
	for (groupmap::iterator itr = groups.begin(); itr != groups.end(); itr++)
	{
		size += SectionHeaderSize                        // add the header size
			+ (*itr).second.vertices.size() * 3 * 4  // add the size of the vertices
			+ (*itr).second.vertices.size() * 3 * 4  // add the size of the normals
			+ (*itr).second.vertices.size() * 2 * 4  // add the size of the texture coords
			+ (*itr).second.indices.size() * 2       // add the size of the indices
			+ ((*itr).second.indices.size() / 3) * 2; // add the size of the blank 4th index
	}
	
	return size;
}

BBox Model::calculateBBox(std::vector<vertex> vertices)
{	
	vertex min = vertices[0];
	vertex max = vertices[0];
	
	for (unsigned int i = 0; i < vertices.size(); i++)
	{
		if (vertices[i].x < min.x)
			min.x = vertices[i].x;
		if (vertices[i].y < min.y)
			min.y = vertices[i].y;
		if (vertices[i].z < min.z)
			min.z = vertices[i].z;
		
		if (vertices[i].x > max.x)
			max.x = vertices[i].x;
		if (vertices[i].y > max.y)
			max.y = vertices[i].y;
		if (vertices[i].z > max.z)
			max.z = vertices[i].z;
	}
	
	BBox bbox;
	bbox.vertex1 = min;
	bbox.vertex1.x -= 0.001;
	bbox.vertex1.y -= 0.001;
	bbox.vertex1.z -= 0.001;
	
	bbox.vertex2 = max;
	bbox.vertex2.x += 0.001;
	bbox.vertex2.y += 0.001;
	bbox.vertex2.z += 0.001;
	
	return bbox;
}

float Model::getFloat(const std::string& str)
{
	std::stringstream ss(str);
	
	float f = 0;
	ss >> f;
	
	return f;
}

unsigned int Model::getUInt(const std::string& str)
{
	std::stringstream ss(str);
	
	unsigned int i = 0;
	ss >> i;
	
	return i;
}

int Model::getIndex(const std::string& str, int whichone)
{
	assert(whichone >= 0);
	assert(whichone < 3);
	
	char v[3][16];
	
	std::stringstream ss(str);
	ss.getline(v[0], 16, '/');
	ss.getline(v[1], 16, '/');
	ss.getline(v[2], 16, '/');
	
	float i = -1;
	
	std::stringstream(v[whichone]) >> i;
	
	return i;
}


int Model::getVertexIndex(const std::string& str)
{
	return getIndex(str, 0);
}

int Model::getNormalIndex(const std::string& str)
{
	return getIndex(str, 2);
}

int Model::getTexcoordIndex(const std::string& str)
{
	return getIndex(str, 1);
}

void Model::loadMaterials(const std::string& filename)
{
	std::ifstream file(filename.c_str(), std::ios::in);
	
	if (file.good() == false)
		throw ModelException("Unable to open material file");
	
	char line[256];
	std::map<std::string, std::string>::iterator currentMaterial;
	while(file.good())
	{		
		file.getline(line, 255);
		std::stringstream str(line);
		
		std::string name, param;
		
		str >> param;
		
		if (param == "newmtl")
		{
			str >> name;
			
			currentMaterial = materials.insert(std::pair<std::string, std::string>(name, "")).first;
		}
		else if (param == "map_Kd")
		{
			std::string file;
			
			str >> file;
			
			// remove the slashes
			std::string::size_type pos = file.rfind("\\");
			
			if (pos != std::string::npos)
				file = file.substr(pos + 1);
			
			// remove the extension
			std::string::size_type dot = file.find(".");
			
			if (dot != std::string::npos)
				file = file.substr(0, dot);
			
			(*currentMaterial).second = file;
		}
	}
	
	
	file.close();
}
