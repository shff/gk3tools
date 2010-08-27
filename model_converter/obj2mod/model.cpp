#include <sstream>
#include <cassert>
#include <cfloat>
#include <stdio.h>
#include "model.h"

#define WRITE4(f,p) fwrite(p, 4, 1, fp)
#define WRITE2(f,p) fwrite(p, 2, 1, fp)
#define WRITE1(f,p) fwrite(p, 1, 1, fp)

#define TO_COLORREF(c, r,g,b) (c = (int)(b * 255) << 16 | (int)(g * 255) << 8 | (int)(r * 255))

void getPathFromFilename(const std::string& filenameAndPath, std::string& path, std::string& filename)
{
    std::string::size_type slashIndex = filenameAndPath.find_last_of("\\/");

    if (slashIndex != std::string::npos)
    {
        filename = filenameAndPath.substr(slashIndex + 1);
        path = filenameAndPath.substr(0, slashIndex + 1);
    }
    else
    {
        filename = filenameAndPath;
        path = "";
    }
}

void Model::Load(const std::string& filename)
{
    std::vector<vertex> vertices;
    std::vector<normal> normals;
	std::vector<texcoord> texcoords;
    std::map<std::string, group> groups;

	// open the input file
	std::ifstream file(filename.c_str(), std::ios::in);
    std::string path, filenameWithoutPath;
    getPathFromFilename(filename, path, filenameWithoutPath);
	
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
			loadMaterials(path + param1);
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
                    newgroup.meshIndex = -1;
                    newgroup.sectionIndex = -1;
					
					groups[param1] = newgroup;
				}

				currentGroup = &groups[param1];
			}
		}
		else if (command == "f" && currentGroup != NULL)
		{
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
						break;
					}
				}
					
				if (i == currentGroup->vertexindices.size())
				{
					currentGroup->vertexindices.push_back(v[index]);
					currentGroup->textureindices.push_back(t[index]);
					currentGroup->normalindices.push_back(n[index]);
				}
					
				currentGroup->indices.push_back(i);
			}
		}
		else if (command == "usemtl" && currentGroup != NULL)
		{
			std::map<std::string, Material>::iterator itr = m_materials.find(param1);
			
			if (itr != m_materials.end())
			{
				currentGroup->material = (*itr).second;
			}
		}
	}
		
	file.close();
	
	// now go through all the groups and build the list of vertices, normals, and texcoords using
	// the handy index lists we've built
    for (std::map<std::string, group>::iterator itr = groups.begin(); itr != groups.end(); itr++)
	{
		group* currentgroup = &(*itr).second;
			
		for (int j = 0; j < currentgroup->vertexindices.size(); j++)
		{
			// DON'T FORGET the difference in indexing (we start at 0, .OBJ's start at 1)!
			currentgroup->vertices.push_back(vertices[currentgroup->vertexindices[j] - 1]);
			currentgroup->normals.push_back(normals[currentgroup->normalindices[j]-1]);
			currentgroup->texcoords.push_back(texcoords[currentgroup->textureindices[j]-1]);
		}
	}

    // now see if there's a meta file that specifies what mesh and section each group should go in
    std::string filenameWithoutExtension = filenameWithoutPath.substr(0, filenameWithoutPath.find_last_of("."));
    std::string metaFilename = path + filenameWithoutExtension + ".txt";
    loadMeta(metaFilename, groups);
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
	
	unsigned int numMeshes = m_meshes.size();
	unsigned int totalSizeOffset;
	
	WRITE4(fp, &header);
	WRITE1(fp, &version_minor);
	WRITE1(fp, &version_major);
	WRITE2(fp, &unknownShort);
	WRITE4(fp, &numMeshes);
    totalSizeOffset = ftell(fp);
	WRITE4(fp, &unknownInt);
	WRITE4(fp, &unknownInt);
	WRITE4(fp, &unknownInt);
    unsigned int endOfHeader = ftell(fp);

	// write the MESH sections
    for (std::vector<ModelMesh>::iterator itr = m_meshes.begin();
        itr != m_meshes.end(); itr++)
    {
        saveMesh(fp, (*itr));
    }
	
    // go back and write the total size of the data section
    unsigned int totalSize = ftell(fp) - endOfHeader;
    fseek(fp, totalSizeOffset, SEEK_SET);
    WRITE4(fp, &totalSize);

	fclose(fp);
}

void Model::saveMesh(FILE* fp, const ModelMesh& mesh)
{
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
	
	unsigned int numSections = mesh.Groups.size();
	BBox bbox = calculateBBox(mesh);

	WRITE4(fp, &meshHeader);
    WRITE4(fp, &mesh.OriginalMatrix.M11);
    WRITE4(fp, &mesh.OriginalMatrix.M12);
    WRITE4(fp, &mesh.OriginalMatrix.M13);
    WRITE4(fp, &mesh.OriginalMatrix.M21);
    WRITE4(fp, &mesh.OriginalMatrix.M22);
    WRITE4(fp, &mesh.OriginalMatrix.M23);
    WRITE4(fp, &mesh.OriginalMatrix.M31);
    WRITE4(fp, &mesh.OriginalMatrix.M32);
    WRITE4(fp, &mesh.OriginalMatrix.M33);
    WRITE4(fp, &mesh.OriginalMatrix.M41);
    WRITE4(fp, &mesh.OriginalMatrix.M42);
    WRITE4(fp, &mesh.OriginalMatrix.M43);


	/*WRITE4(fp, &transform[0]);
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
	WRITE4(fp, &transform[11]);*/
	
	WRITE4(fp, &numSections);
	
	WRITE4(fp, &bbox.vertex1.x);
	WRITE4(fp, &bbox.vertex1.y);
	WRITE4(fp, &bbox.vertex1.z);
	WRITE4(fp, &bbox.vertex2.x);
	WRITE4(fp, &bbox.vertex2.y);
	WRITE4(fp, &bbox.vertex2.z);
	
	// write the subgroups
    for (std::vector<group>::const_iterator itr = mesh.Groups.begin(); itr != mesh.Groups.end(); itr++)
	{
		group currentgroup = (*itr);
		
		unsigned int groupHeader = 0x4D475250;
		char textureFile[32] = {0};
		strncpy(textureFile, currentgroup.material.Texture.c_str(), currentgroup.material.Texture.length());
		unsigned int numFaces = 1;
		unsigned int numVerts = currentgroup.vertices.size();
		unsigned int numTris = currentgroup.indices.size() / 3;
		unsigned int lodLevels = 0;
		
		std::cout << "Num verts: " << numVerts << std::endl;
		std::cout << "Num triangles: " << numTris << std::endl;
		
        unsigned int color;
        TO_COLORREF(color, currentgroup.material.R, currentgroup.material.G, currentgroup.material.B);
		WRITE4(fp, &groupHeader);
		fwrite(textureFile, 32, 1, fp);
		WRITE4(fp, &color);
		WRITE4(fp, &numFaces);
		WRITE4(fp, &numVerts);
		WRITE4(fp, &numTris);
		WRITE4(fp, &lodLevels);
        int zero = 0;
		WRITE4(fp, &zero);
		
		for (unsigned int i = 0; i < currentgroup.vertices.size(); i++)
		{
			// reverse z
			float z = -currentgroup.vertices[i].z;
			
			WRITE4(fp, &currentgroup.vertices[i].x);
			WRITE4(fp, &currentgroup.vertices[i].y);
			WRITE4(fp, &currentgroup.vertices[i].z);
			//WRITE4(fp, &z);
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
			WRITE4(fp, &currentgroup.texcoords[i].u);
			
			// reverse the v
			float v =  -currentgroup.texcoords[i].v;
			WRITE4(fp, &v);
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
}

void Model::loadMeta(const std::string& metaFilename, const std::map<std::string, group>& groups)
{
    std::ifstream metafile(metaFilename.c_str(), std::ios::in);
	
    if (metafile.good() == false)
    {
        std::cout << "No metadata found" << std::endl;

        // no metadata, so just toss all the groups into a single mesh
        ModelMesh mesh;
        for (std::map<std::string, group>::const_iterator itr = groups.begin();
            itr != groups.end(); itr++)
        {
            mesh.Groups.push_back((*itr).second);
        }

        return;
    }

    char buffer[256];
	while (metafile.good())
    {
        metafile.getline(buffer, 255);
        std::stringstream ss(buffer);
        std::string groupname;
        ss >> groupname;
        
        if (groupname == "transform")
        {
            int meshIndex;
            ss >> meshIndex;
            float transform[12];
            ss >> transform[0] >> transform[1] >> transform[2] >> transform[3]
                >> transform[4] >> transform[5] >> transform[6] >> transform[7]
                >> transform[8] >> transform[9] >> transform[10] >> transform[11];

            if (m_meshes.size() <= meshIndex)
                m_meshes.resize(meshIndex + 1);

            m_meshes[meshIndex].OriginalMatrix = Matrix(transform);
        }
        else
        {
            int meshIndex, sectionIndex;
            ss >> meshIndex >> sectionIndex;

            std::map<std::string, group>::const_iterator groupItr = groups.lower_bound(groupname);

            if (groupItr != groups.end())
            {
                if (m_meshes.size() <= meshIndex)
                    m_meshes.resize(meshIndex + 1);

                if (m_meshes[meshIndex].Groups.size() <= sectionIndex)
                    m_meshes[meshIndex].Groups.resize(sectionIndex + 1);

                m_meshes[meshIndex].Groups[sectionIndex] = (*groupItr).second;
            }
        }
    }

    // now we need to calculate the inverse of the original matrix
    // and undo the transformation that happened during the mod -> obj process
    for (std::vector<ModelMesh>::iterator itr = m_meshes.begin();
        itr != m_meshes.end(); itr++)
    {
        Matrix inverse;
        Matrix::Invert((*itr).OriginalMatrix, inverse);

        for (std::vector<group>::iterator itr2 = (*itr).Groups.begin();
            itr2 != (*itr).Groups.end(); itr2++)
        {
            for (std::vector<vertex>::iterator vitr = (*itr2).vertices.begin();
                vitr != (*itr2).vertices.end(); vitr++)
            {
                (*vitr) = inverse.Multiply(*vitr);
            }
        }
    }
    
}

unsigned int Model::getDataSectionSize(const std::map<std::string, group>& groups)
{
	const unsigned int MeshHeaderSize = 80;
	const unsigned int SectionHeaderSize = 70;
	
	unsigned int size = MeshHeaderSize;
	
    for (std::map<std::string, group>::const_iterator itr = groups.begin(); itr != groups.end(); itr++)
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

BBox Model::calculateBBox(const ModelMesh& mesh)
{
    BBox result;
    result.vertex1.x = FLT_MAX;
    result.vertex1.y = FLT_MAX;
    result.vertex1.z = FLT_MAX;
    result.vertex2.x = FLT_MIN;
    result.vertex2.y = FLT_MIN;
    result.vertex2.z = FLT_MIN;

    for (std::vector<group>::const_iterator itr = mesh.Groups.begin();
        itr != mesh.Groups.end(); itr++)
    {
        BBox bb = calculateBBox((*itr).vertices);
        
        if (bb.vertex1.x < result.vertex1.x)
            result.vertex1.x = bb.vertex1.x;
        if (bb.vertex1.y < result.vertex1.y)
            result.vertex1.y = bb.vertex1.y;
        if (bb.vertex1.z < result.vertex1.z)
            result.vertex1.z = bb.vertex1.z;

        if (bb.vertex2.x > result.vertex2.x)
            result.vertex2.x = bb.vertex2.x;
        if (bb.vertex2.y > result.vertex2.y)
            result.vertex2.y = bb.vertex2.y;
        if (bb.vertex2.z > result.vertex2.z)
            result.vertex2.z = bb.vertex2.z;
    }

    return result;
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
	std::map<std::string, Material>::iterator currentMaterial;
	while(file.good())
	{		
		file.getline(line, 255);
		std::stringstream str(line);
		
		std::string name, param;
		
		str >> param;
		
		if (param == "newmtl")
		{
			str >> name;
			
			currentMaterial = m_materials.insert(std::pair<std::string, Material>(name, Material())).first;
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
			
			(*currentMaterial).second.Texture = file;
		}
        else if (param == "Kd")
        {
            float r, g, b;

            str >> r >> g >> b;

            (*currentMaterial).second.R = r;
            (*currentMaterial).second.G = g;
            (*currentMaterial).second.B = b;
        }
	}
	
	
	file.close();
}
