// Handy Dandy GK3 .MOD to .OBJ convertor!
// Copyright 2006 Brad Farris
// Licensed under the GNU GPL

#define VERSION_MAJOR 0
#define VERSION_MINOR 1

#include <algorithm>
#include <iostream>
#include <string>
#include <vector>
#include <stdio.h>
#include <cassert>

#include "modfile.h"

#define READ4(f,b) fread(b, 4, 1, f)
#define READ2(f,b) fread(b, 2, 1, f)
#define READ1(f,b) fread(b, 1, 1, f)

bool verbose = false;

void displayUsage();
modfile extractInfo(FILE* fp);
void writeOBJFile(modfile, const std::string& outputFilename);

void MatrixVectorMul(float* m, float* v)
{
	assert(m);
	assert(v);
	
	float newv[4];
	
	newv[0] = m[0] * v[0] + m[3] * v[1] + m[6] * v[2] + m[9];
	newv[1] = m[1] * v[0] + m[4] * v[1] + m[7] * v[2] + m[10];
	newv[2] = m[2] * v[0] + m[5] * v[1] + m[8] * v[2] + m[11];
	//newv[3] = m[3] * v[0] + m[7] * v[1] + m[11] * v[2];
	
	v[0] = newv[0];
	v[1] = newv[1];
	v[2] = newv[2];
}

int main(int argc, char** argv)
{
	if (argc < 2)
	{
		displayUsage();
		return 0;
	}
	
	bool knowInputFile = false;
	std::string inputFile;
	std::string outputFile = "default.obj";
	
	// parse the command arguments
	for (int i = 1; i < argc; i++)
	{
		if (std::string(argv[i]) == "-v")
			verbose = true;
		else if (knowInputFile == false)
		{
			inputFile = argv[i];
			knowInputFile = true;
		}
		else
			outputFile = argv[i];
	}
	
	if (!knowInputFile)
	{
		std::cout << "Must specify an input file" << std::endl;
		return 1;
	}
	
	// open the given file
	FILE* fp = fopen(inputFile.c_str(), "rb");
	
	if (!fp)
	{
		std::cout << "Unable to open the requested file" << std::endl;
		return 1;
	}
	
	try {
	
		// extract information
		modfile m = extractInfo(fp);
		
		// close the file
		fclose(fp);
		
		// write a .obj file
		writeOBJFile(m, outputFile);
	}
	catch(const char* ex)
	{
		std::cout << "Exception thrown: " << ex << std::endl;
		return 1;
	}
	
	return 0;
}

void displayUsage()
{
	std::cout << "GK3 MOD 2 OBJ Convertor v" << VERSION_MAJOR << "." << VERSION_MINOR << std::endl;
	std::cout << "Usage:" << std::endl;
	std::cout << "   gk3mod2obj filename [output filename]" << std::endl;
	std::cout << "where [filename] is a valid Gabriel Knight 3 model file" << std::endl;
}

modfile extractInfo(FILE* fp)
{
	modfile mod;
	
	// read the header
	READ4(fp, mod.h.heading);
	READ1(fp, &mod.h.minorVersion);
	READ1(fp, &mod.h.majorVersion);
	READ2(fp, &mod.h.unknown1);
	READ4(fp, &mod.h.numMeshes);
	READ4(fp, &mod.h.size);
	READ4(fp, &mod.h.unknown2);
	READ4(fp, &mod.h.unknown3);
	
	if (verbose)
	{
		std::cout << "Minor version: " << (int)mod.h.minorVersion << std::endl;
		std::cout << "Major version: " << (int)mod.h.majorVersion << std::endl;
		std::cout << "numMeshes: " << mod.h.numMeshes << std::endl;
		std::cout << "size: " << mod.h.size << std::endl;
	}

	
	if (mod.h.minorVersion == 9 && mod.h.majorVersion == 1)
	{
		READ4(fp, &mod.hext.unknown[0]);
		READ4(fp, &mod.hext.unknown[1]);
		READ4(fp, &mod.hext.unknown[2]);
		READ4(fp, &mod.hext.unknown[3]);
		READ4(fp, &mod.hext.unknown[4]);
		READ4(fp, &mod.hext.unknown[5]);
	}
	
	for (int i = 0; i < mod.h.numMeshes; i++)
	{
		mesh mh;
		
		// it's dirty hack time! I don't know the format of the LODK sections, so I can't
		// accurately calculate how to skip it, so I've got to basically do a search for
		// the next MESH section.
		
		while(!feof(fp))
		{
			READ4(fp, &mh.heading);
			
			if (mh.heading == 0x4D455348)
			{
				break;
			}
			
			// back up 3 bytes and continue
			fseek(fp, -3, SEEK_CUR);
		}
		
		// if we didn't find it then we're screwed
		if (mh.heading != 0x4D455348)
		{
			throw "Not a valid model file! Unable to find MESH section!";
		}
		
		READ4(fp, &mh.transform[0]);
		READ4(fp, &mh.transform[1]);
		READ4(fp, &mh.transform[2]);
		READ4(fp, &mh.transform[3]);
		READ4(fp, &mh.transform[4]);
		READ4(fp, &mh.transform[5]);
		READ4(fp, &mh.transform[6]);
		READ4(fp, &mh.transform[7]);
		READ4(fp, &mh.transform[8]);
		READ4(fp, &mh.transform[9]);
		READ4(fp, &mh.transform[10]);
		READ4(fp, &mh.transform[11]);
		
		READ4(fp, &mh.numSections);
		
		READ4(fp, &mh.bbox[0]);
		READ4(fp, &mh.bbox[1]);
		READ4(fp, &mh.bbox[2]);
		READ4(fp, &mh.bbox[3]);
		READ4(fp, &mh.bbox[4]);
		READ4(fp, &mh.bbox[5]);
		
		if (verbose)
		{
			// write out the transform matrix
			std::cout << "Transformation matrix: " << std::endl;
			std::cout << "[ " << mh.transform[0] << " " << mh.transform[1] << " " << mh.transform[2] << "]" << std::endl;
			std::cout << "[ " << mh.transform[3] << " " << mh.transform[4] << " " << mh.transform[5] << "]" << std::endl;
			std::cout << "[ " << mh.transform[6] << " " << mh.transform[7] << " " << mh.transform[8] << "]" << std::endl;
			std::cout << "[ " << mh.transform[9] << " " << mh.transform[10] << " " << mh.transform[11] << "]" << std::endl;
			
			std::cout << "Num mesh sections: " << mh.numSections << std::endl;
		}
		
		
		mod.meshes.push_back(mh);
		
		for (int j = 0; j < mh.numSections; j++)
		{
			meshsection m;
			
			while(!feof(fp))
			{
				READ4(fp, &m.heading);
				
				if (m.heading == 0x4D475250)
				{
					break;
				}
				
				// back up 3 bytes and continue
				fseek(fp, -3, SEEK_CUR);
			}
			
			// if we didn't find it then we're screwed
			if (m.heading != 0x4D475250)
			{
				throw "Not a valid model file! Unable to find valid mesh section header!";
			}
			
			fread(m.texturefile, 32, 1, fp);
			READ4(fp, &m.unknown1);
			READ4(fp, &m.numFaces);
			READ4(fp, &m.numVerts);
			READ4(fp, &m.numTriangles);
			READ4(fp, &m.numLODs);
			READ4(fp, &m.unknown2);
			
			if (verbose)
			{
				std::cout << "Num faces: " << m.numFaces << std::endl;
				std::cout << "Num verts:" << m.numVerts << std::endl;
				std::cout << "Num triangles: " << m.numTriangles << std::endl;
				std::cout << "Num LODs: " << m.numLODs << std::endl;
			}
			
			// read the vertices
			for (int k = 0; k < m.numVerts; k++)
			{
				float x, y, z;
				
				READ4(fp, &x);
				READ4(fp, &y);
				READ4(fp, &z);
				
				m.vertices.push_back(x);
				m.vertices.push_back(y);
				m.vertices.push_back(z);
			}
			
			// read the normals
			for (int k = 0; k < m.numVerts; k++)
			{
				float x, y, z;
				
				READ4(fp, &x);
				READ4(fp, &y);
				READ4(fp, &z);
				
				m.normals.push_back(x);
				m.normals.push_back(y);
				m.normals.push_back(z);
			}
			
			// read the texture coords
			for (int k = 0; k < m.numVerts; k++)
			{
				float u, v;
				
				READ4(fp, &u);
				READ4(fp, &v);
				
				m.texcoords.push_back(u);
				m.texcoords.push_back(v);
			}
			
			for (int k = 0; k < m.numTriangles; k++)
			{
				unsigned short index1, index2, index3, dummy;
				
				READ2(fp, &index1);
				READ2(fp, &index2);
				READ2(fp, &index3);
				READ2(fp, &dummy);
				
				m.indices.push_back(index1);
				m.indices.push_back(index2);
				m.indices.push_back(index3);
			}
			
			mod.meshes[i].sections.push_back(m);
		}
	}
	
	return mod;
}

void writeOBJFile(modfile mod, const std::string& outputFilename)
{
	// this stores the vertex offset for each section
	std::vector<unsigned int> vertexOffsets;
	
	std::string materialFilename = outputFilename.substr(0, outputFilename.find_last_of(".")) + ".mtl";
	
	FILE* fp = fopen(outputFilename.c_str(), "w");
	FILE* mtlfp = fopen(materialFilename.c_str(), "w");
	
	fprintf(fp, "# This is a converted version of a GK3 .MOD file!\n\n");
	fprintf(fp, "mtllib %s\n\n", materialFilename.c_str());
	
	std::vector<std::string> materials;
	
	// write all the vertices
	unsigned int currentOffset = 1;
	for (int i = 0; i < mod.h.numMeshes; i++)
	{
		for (int j = 0; j < mod.meshes[i].numSections; j++)
		{
			for (int k = 0; k < mod.meshes[i].sections[j].numVerts; k++)
			{
				MatrixVectorMul(mod.meshes[i].transform, &mod.meshes[i].sections[j].vertices[k * 3]);
				
				fprintf(fp, "v %f %f %f\n", mod.meshes[i].sections[j].vertices[k * 3 + 0],
					mod.meshes[i].sections[j].vertices[k * 3 + 1],
					mod.meshes[i].sections[j].vertices[k * 3 + 2]);
			}
			
			vertexOffsets.push_back(currentOffset);
			currentOffset += mod.meshes[i].sections[j].numVerts;
		}
	}
	
	fprintf(fp, "\n");
	
	// write all the normals
	for (int i = 0; i < mod.h.numMeshes; i++)
	{
		for (int j = 0; j < mod.meshes[i].numSections; j++)
		{
			for (int k = 0; k < mod.meshes[i].sections[j].numVerts; k++)
			{
				fprintf(fp, "vn %f %f %f\n", mod.meshes[i].sections[j].normals[k * 3 + 0],
					mod.meshes[i].sections[j].normals[k * 3 + 1],
					mod.meshes[i].sections[j].normals[k * 3 + 2]);
			}
		}
	}
	
	fprintf(fp, "\n");
	
	// write the texture coordinates
	for (int i = 0; i < mod.h.numMeshes; i++)
	{
		for (int j = 0; j < mod.meshes[i].numSections; j++)
		{
			for (int k = 0; k < mod.meshes[i].sections[j].numVerts; k++)
			{
				fprintf(fp, "vt %f %f\n", mod.meshes[i].sections[j].texcoords[k * 2 + 0],
					-mod.meshes[i].sections[j].texcoords[k * 2 + 1]);
			}
		}
	}
	
	fprintf(fp, "\n");
	
	// write the indices
	unsigned int counter = 0;
	for (int i = 0; i < mod.h.numMeshes; i++)
	{
		for (int j = 0; j < mod.meshes[i].numSections; j++)
		{
			fprintf(fp, "g group%d\n", counter);
			
			if (strlen(mod.meshes[i].sections[j].texturefile) > 0)
			{
				fprintf(fp, "usemtl %s\n", mod.meshes[i].sections[j].texturefile);
				
				if (std::find(materials.begin(), materials.end(), mod.meshes[i].sections[j].texturefile)
					== materials.end())
				{
					// write the material to the material file
					fprintf(mtlfp, "newmtl %s\n", mod.meshes[i].sections[j].texturefile);
					fprintf(mtlfp, "map_Kd .\\%s.BMP\n\n", mod.meshes[i].sections[j].texturefile);
				}
			}
			
			unsigned int offset = vertexOffsets[counter];
			
			for (int k = 0; k < mod.meshes[i].sections[j].numTriangles; k++)
			{
				fprintf(fp, "f %u/%u/%u %u/%u/%u %u/%u/%u\n", 
					mod.meshes[i].sections[j].indices[k * 3 + 0] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 0] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 0] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 1] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 1] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 1] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 2] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 2] + offset,
					mod.meshes[i].sections[j].indices[k * 3 + 2] + offset);
			}
			
			counter++;
		}
		
		fprintf(fp, "\n");
	}
	
	fclose(fp);
	fclose(mtlfp);
}


