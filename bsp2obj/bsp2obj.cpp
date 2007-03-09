#define VERSION_MAJOR 0
#define VERSION_MINOR 1

#include <iostream>
#include <string>
#include <cstdio>
#include "bspfile.h"

#define READ4(f,b) fread(b, 4, 1, f)
#define READ2(f,b) fread(b, 2, 1, f)
#define READ1(f,b) fread(b, 1, 1, f)

bool verbose = false;

void displayUsage(const std::string& currentExecutableName);
bspfile extractInfo(FILE* fp);
void writeOBJFile(bspfile, const std::string& outputFilename);

int main(int argc, char** argv)
{
	if (argc < 2)
	{
		displayUsage(argv[0]);
		return 0;
	}

	std::string inputFile, outputFile = "default.obj";

	// parse the command arguments
	bool knowInputFile = false;
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
		std::cout << "Must specify an input file" << std::endl << std::endl;
		return 1;
	}

	// open the specified input file
	FILE* fp = fopen(inputFile.c_str(), "rb");

	if (!fp)
	{
		std::cout << "Unable to open the requested file" << std::endl << std::endl;
		return 1;
	}

	try
	{
		// extract information
		bspfile b = extractInfo(fp);

		// close the file
		fclose(fp);

		// write a .obj file
		writeOBJFile(b, outputFile);
	}
	catch(const char* ex)
	{
		std::cout << "Exception thrown: " << ex << std::endl << std::endl;
		return 1;
	}

	return 0;
}

void displayUsage(const std::string& currentExecutableName)
{
	std::cout << "GK3 BSP 2 OBJ Converter v" << VERSION_MAJOR << "." << VERSION_MINOR << std::endl;
	std::cout << "Usage: " << std::endl;
	std::cout << "\t" << currentExecutableName << " filename [output filename]" << std::endl;
	std::cout << "where 'filename' is a valid Gabriel Knight 3 BSP file" << std::endl << std::endl;
}

bspfile extractInfo(FILE* fp)
{
	bspfile bsp;

	// read the header
	READ4(fp, bsp.h.heading);
	READ2(fp, &bsp.h.minorVersion);
	READ2(fp, &bsp.h.majorVersion);
	READ4(fp, &bsp.h.dataSectionSize);
	READ4(fp, &bsp.h.rootIndex);
	READ4(fp, &bsp.h.numModels);
	READ4(fp, &bsp.h.numVertices);
	READ4(fp, &bsp.h.numTexCoords);
	READ4(fp, &bsp.h.numVertexIndices);
	READ4(fp, &bsp.h.numTexIndices);
	READ4(fp, &bsp.h.numSurfaces);
	READ4(fp, &bsp.h.numPlanes);
	READ4(fp, &bsp.h.numNodes);
	READ4(fp, &bsp.h.numPolygons);

	if (verbose)
	{
		std::cout << "BSP info:"<< std::endl;
		std::cout << "	Version: " << (int)bsp.h.majorVersion << "." << (int)bsp.h.minorVersion << std::endl;
		std::cout << "	Num Models: " << bsp.h.numModels << std::endl;
		std::cout << "	Num vertices: " << bsp.h.numVertices << std::endl;
		std::cout << "	Num indices: " << bsp.h.numVertexIndices << std::endl;
		std::cout << "	Models:" << std::endl;
	}

	// read the model names
	for (unsigned int i = 0; i < bsp.h.numModels; i++)
	{
		bspmodel model;

		fread(model.name, 32, 1, fp);

		bsp.models.push_back(model);
	}

	// read the surfaces
	for (unsigned int i = 0; i < bsp.h.numSurfaces; i++)
	{
		surface s;

		READ4(fp, &s.modelIndex);
		fread(s.texture, 32, 1, fp);
		READ4(fp, &s.uCoord);
		READ4(fp, &s.vCoord);
		READ4(fp, &s.uScale);
		READ4(fp, &s.vScale);
		READ4(fp, &s.size);
		READ4(fp, &s.flags);

		if (s.modelIndex >= bsp.models.size())
			throw "Invalid .bsp file! Surface model index out of range!";

		bsp.models[s.modelIndex].surfaces.push_back(s);
		bsp.surfaces.push_back(s);

	}

	if (verbose)
	{
		for (unsigned int i = 0; i < bsp.h.numModels; i++)
		{
			std::cout << "		" << bsp.models[i].name << "'s surfaces:" << std::endl;

			for (unsigned int j = 0; j < bsp.models[i].surfaces.size(); j++)
				std::cout << "			" << bsp.models[i].surfaces[j].texture << std::endl;
		}
	}

	// read the nodes (for now throw them away)
	for (unsigned int i = 0; i < bsp.h.numNodes; i++)
	{
		bspNode n;
		
		READ2(fp, &n.childIndex1);
		READ2(fp, &n.childIndex2);
		READ2(fp, &n.planeIndex);
		READ2(fp, &n.polygonStartIndex);
		READ2(fp, &n.unknown1);
		READ2(fp, &n.numPolygons);
		READ2(fp, &n.unknown2);
		READ2(fp, &n.unknown3);
	}

	// read all the polygons
	for (unsigned int i = 0; i < bsp.h.numPolygons; i++)
	{
		polygon p;

		READ2(fp, &p.vertexIndex);
		READ2(fp, &p.flags);
		READ2(fp, &p.numVertices);
		READ2(fp, &p.surfaceIndex);

		bsp.polygons.push_back(p);
		//bsp.models[surfaces[p.surfaceIndex].modelIndex].surfaces[p.surfaceIndex].polygons.push_back(p);
	}

	// read all the planes
	for (unsigned int i = 0; i < bsp.h.numPlanes; i++)
	{
		plane p;

		READ4(fp, &p.x);
		READ4(fp, &p.y);
		READ4(fp, &p.z);
		READ4(fp, &p.d);
	}

	// read all the vertices
	for (unsigned int i = 0; i < bsp.h.numVertices; i++)
	{
		vertex v;
		
		READ4(fp, &v.x);
		READ4(fp, &v.y);
		READ4(fp, &v.z);

		bsp.vertices.push_back(v);
	}

	// read all the texture vertices
	for (unsigned int i = 0; i < bsp.h.numTexCoords; i++)
	{
		texCoord t;
		
		READ4(fp, &t.u);
		READ4(fp, &t.v);

		bsp.textureCoords.push_back(t);
	}

	// read all the vertex indices
	for (unsigned int i = 0; i < bsp.h.numVertexIndices; i++)
	{
		unsigned short index;

		READ2(fp, &index);

		bsp.indices.push_back(index);
	}

	// read all the texture indices
	for (unsigned int i = 0; i < bsp.h.numTexIndices; i++)
	{
		unsigned short index;

		READ2(fp, &index);

		bsp.texindices.push_back(index);
	}

	// read the bounding spheres
	for (unsigned int i = 0; i < bsp.h.numNodes; i++)
	{
		boundingSphere s;

		READ4(fp, &s.x);
		READ4(fp, &s.y);
		READ4(fp, &s.z);
		READ4(fp, &s.radius);
	}

	// load the thingies
	for (unsigned int i = 0; i < bsp.h.numSurfaces; i++)
	{
		thingy t;
		
		READ4(fp, &t.boundingSphereX);
		READ4(fp, &t.boundingSphereY);
		READ4(fp, &t.boundingSphereZ);
		READ4(fp, &t.boundingSphereRadius);
		READ4(fp, &t.chromeValue);
		READ4(fp, &t.grazing);
		READ4(fp, &t.chromeColor);
		READ4(fp, &t.numIndices);
		READ4(fp, &t.numTriangles);

		if (verbose)
		{
			std::cout << "num triangles: " << t.numTriangles << " num indices: " << t.numIndices <<std::endl;
		}

		std::vector<unsigned short> indices;
		for (int j = 0; j < t.numIndices; j++)
		{
			unsigned short index;

			READ2(fp, &index);
			indices.push_back(index);
		}

		for (int j = 0; j < t.numTriangles; j++)
		{
			unsigned short x, y, z;

			READ2(fp, &x);
			READ2(fp, &y);
			READ2(fp, &z);

			bsp.surfaces[i].secondaryTriangles.push_back(indices[x]);
			bsp.surfaces[i].secondaryTriangles.push_back(indices[y]);
			bsp.surfaces[i].secondaryTriangles.push_back(indices[z]);
		}
	}



	std::cout << "final position: " << ftell(fp) << " counter: " << std::endl;

	return bsp;
}



void writeOBJFile(bspfile bsp, const std::string& outputFileName)
{
	FILE* fp = fopen(outputFileName.c_str(), "w");

	fprintf(fp, "# This is a converted version of a GK3 .BSP file!\n\n");
	
	// write all the vertices
	for (int i = 0; i < bsp.h.numVertices; i++)
	{
		fprintf(fp, "v %f %f %f\n", bsp.vertices[i].x, bsp.vertices[i].y, bsp.vertices[i].z);
	}

	// write all the texture coords
	fprintf(fp, "\n");
	for (int i = 0; i < bsp.h.numTexCoords; i++)
	{
		fprintf(fp, "vt %f %f\n", bsp.textureCoords[i].u, bsp.textureCoords[i].v);
	}

	// write all the polygons
	/*for (int i = 0; i < bsp.h.numPolygons; i++)
	{
		polygon p = bsp.polygons[i];


		if (p.numVertices == 3)
			fprintf(fp, "f %u %u %u\n", bsp.indices[p.vertexIndex + 0] + 1, bsp.indices[p.vertexIndex + 1] + 1, bsp.indices[p.vertexIndex + 2] + 1);
		else if (p.numVertices == 4 || p.numVertices == 5)
			fprintf(fp, "f %u %u %u %u\n", bsp.indices[p.vertexIndex + 0] + 1, bsp.indices[p.vertexIndex + 1] + 1, bsp.indices[p.vertexIndex + 2] + 1 , bsp.indices[p.vertexIndex + 3] + 1);
		else if (p.numVertices == 5)
		{
			fprintf(fp, "f %u %u %u\n", bsp.indices[p.vertexIndex + 0] + 1, bsp.indices[p.vertexIndex + 3] + 1, bsp.indices[p.vertexIndex + 4] + 1);
		}
		
		else
			std::cout << "Skipping polygon with " << p.numVertices << " vertices" << std::endl;
	}*/

	for (int i = 0; i < bsp.h.numSurfaces; i++)
	{
		fprintf(fp, "\ng surface%u\n", i);
		fprintf(fp, "usemtl %s\n", bsp.surfaces[i].texture);
		for (int j = 0; j < bsp.surfaces[i].secondaryTriangles.size() / 3; j++)
		{
			fprintf(fp, "f %u/%u %u/%u %u/%u\n", bsp.surfaces[i].secondaryTriangles[j * 3 + 0] + 1,
				bsp.surfaces[i].secondaryTriangles[j * 3 + 0] + 1,
				bsp.surfaces[i].secondaryTriangles[j * 3 + 1] + 1,
				bsp.surfaces[i].secondaryTriangles[j * 3 + 1] + 1,
				bsp.surfaces[i].secondaryTriangles[j * 3 + 2] + 1,
				bsp.surfaces[i].secondaryTriangles[j * 3 + 2] + 1);
		}
	}

	fclose(fp);
}