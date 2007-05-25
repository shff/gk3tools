using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace gk3levelviewer.Graphics
{
    #region Bsp file structures

    struct BspHeader
    {
        public byte[] heading;
        public ushort minorVersion;
        public ushort majorVersion;
        public uint dataSectionSize;
        public uint rootIndex;
        public uint numModels;
        public uint numVertices;
        public uint numTexCoords;
        public uint numVertexIndices;
        public uint numTexIndices;
        public uint numSurfaces;
        public uint numPlanes;
        public uint numNodes;
        public uint numPolygons;
    }

    struct BspPolygon
    {
        public ushort vertexIndex;
        public ushort flags;
        public ushort numVertices;
        public ushort surfaceIndex;
    }

    struct BspSurface
    {
        public uint modelIndex;
        public string texture;
        public float uCoord, vCoord;
        public float uScale, vScale;
        public float size;
        public uint flags;

        public List<BspPolygon> polygons;

        // used as an "extension"
        public ushort[] indices;
    }

    struct BspModel
    {
        public string name;
        public List<BspSurface> surfaces;
    }

    #endregion

    class BspResource : Resource.Resource
    {
        public BspResource(string name, System.IO.Stream stream)
            : base(name)
        {
            System.IO.BinaryReader reader = 
                new System.IO.BinaryReader(stream);

            // read the header
            BspHeader header = new BspHeader();
            header.heading = reader.ReadBytes(4);
            header.minorVersion = reader.ReadUInt16();
            header.majorVersion = reader.ReadUInt16();
            header.dataSectionSize = reader.ReadUInt32();
            header.rootIndex = reader.ReadUInt32();
            header.numModels = reader.ReadUInt32();
            header.numVertices = reader.ReadUInt32();
            header.numTexCoords = reader.ReadUInt32();
            header.numVertexIndices = reader.ReadUInt32();
            header.numTexIndices = reader.ReadUInt32();
            header.numSurfaces = reader.ReadUInt32();
            header.numPlanes = reader.ReadUInt32();
            header.numNodes = reader.ReadUInt32();
            header.numPolygons = reader.ReadUInt32();

            // read the model names
            byte[] buffer32 = new byte[32];
            BspModel[] models = new BspModel[header.numModels];
            for (uint i = 0; i < header.numModels; i++)
            {
                models[i] = new BspModel();

                models[i].name = convertAsciiToString(reader.ReadBytes(32));
            }

            // read the surfaces
            _surfaces = new BspSurface[header.numSurfaces];
            for (uint i = 0; i < header.numSurfaces; i++)
            {
                _surfaces[i] = new BspSurface();

                _surfaces[i].modelIndex = reader.ReadUInt32();
                _surfaces[i].texture = convertAsciiToString(reader.ReadBytes(32));
                _surfaces[i].uCoord = reader.ReadSingle();
                _surfaces[i].vCoord = reader.ReadSingle();
                _surfaces[i].uScale = reader.ReadSingle();
                _surfaces[i].vScale = reader.ReadSingle();
                _surfaces[i].size = reader.ReadUInt32();
                _surfaces[i].flags = reader.ReadUInt32();
            }

            // read the BSP nodes (for now throw them away)
            for (uint i = 0; i < header.numNodes; i++)
            {
                reader.ReadBytes(16);
            }

            // read all the polygons
            BspPolygon[] polys = new BspPolygon[header.numPolygons];
            for (uint i = 0; i < header.numPolygons; i++)
            {
                polys[i] = new BspPolygon();

                polys[i].vertexIndex = reader.ReadUInt16();
                polys[i].flags = reader.ReadUInt16();
                polys[i].numVertices = reader.ReadUInt16();
                polys[i].surfaceIndex = reader.ReadUInt16();
            }

            // read all the planes (thow them away)
            for (uint i = 0; i < header.numPlanes; i++)
            {
                reader.ReadBytes(16);
            }

            // read the vertices
            _vertices = new float[header.numVertices * 3];
            for (uint i = 0; i < header.numVertices; i++)
            {
                _vertices[i * 3 + 2] = reader.ReadSingle();
                _vertices[i * 3 + 1] = reader.ReadSingle();
                _vertices[i * 3 + 0] = reader.ReadSingle();
            }

            // read the texture vertices
            _texcoords = new float[header.numTexCoords * 2];
            for (uint i = 0; i < header.numTexCoords; i++)
            {
                _texcoords[i * 2 + 0] = -reader.ReadSingle();
                _texcoords[i * 2 + 1] = reader.ReadSingle();
            }

            // read all the vertex indices
            ushort[] indices = new ushort[header.numVertexIndices];
            for (uint i = 0; i < header.numVertexIndices; i++)
            {
                indices[i] = reader.ReadUInt16();
            }

            // read all the texcoord indices
            ushort[] texindices = new ushort[header.numTexIndices];
            for (uint i = 0; i < header.numTexIndices; i++)
            {
                texindices[i] = reader.ReadUInt16();
            }

            // read the bounding spheres (throw them away)
            for (uint i = 0; i < header.numNodes; i++)
            {
                reader.ReadBytes(16);
            }

            // load the "thingies", whatever that means
            for (int i = 0; i < header.numSurfaces; i++)
            {
                // throw junk away
                reader.ReadBytes(28);

                uint numIndices = reader.ReadUInt32();
                uint numTriangles = reader.ReadUInt32();

                UInt16[] myindices = new UInt16[numIndices];
                for (uint j = 0; j < numIndices; j++)
                {
                    myindices[j] = reader.ReadUInt16();
                }

                _surfaces[i].indices = new ushort[numTriangles * 3];
                for (uint j = 0; j < numTriangles; j++)
                {
                    ushort x = reader.ReadUInt16();
                    ushort y = reader.ReadUInt16();
                    ushort z = reader.ReadUInt16();

                    _surfaces[i].indices[j * 3 + 0] = myindices[x];
                    _surfaces[i].indices[j * 3 + 1] = myindices[y];
                    _surfaces[i].indices[j * 3 + 2] = myindices[z];
                }
            }

            reader.Close();

            setupLightmapCoords();
            loadTextures();
        }

        private void setupLightmapCoords()
        {
            _lightmapcoords = new float[_texcoords.Length];

            for (int i = 0; i < _surfaces.Length; i++)
            {
                // get the maximum and minumum of each surface
                Math.Vector min = new Math.Vector(float.MaxValue, float.MaxValue, float.MaxValue);
                Math.Vector max = new Math.Vector(float.MinValue, float.MinValue, float.MinValue);

                for (int j = 0; j < _surfaces[i].indices.Length; j++)
                {
                    float x = _vertices[_surfaces[i].indices[j] * 3 + 0];
                    float y = _vertices[_surfaces[i].indices[j] * 3 + 1];
                    float z = _vertices[_surfaces[i].indices[j] * 3 + 2];

                    if (x < min.X) min.X = x;
                    if (y < min.Y) min.Y = y;
                    if (z < min.Z) min.Z = z;

                    if (x > max.X) max.X = x;
                    if (y > max.Y) max.Y = y;
                    if (z > max.Z) max.Z = z;
                }

                float diffX = max.X - min.X;
                float diffY = max.Y - min.Y;
                float diffZ = max.Z - min.Z;

                //float uScale = (float)System.Math.Sqrt(diffX * diffX + diffY * diffY);
                //float vScale = (float)System.Math.Sqrt(diffZ * diffZ + diffY * diffY);

                float uScale = diffX;
                float vScale = diffY;

                //float uScale = 1.0f / _surfaces[i].uScale;
                //float vScale = 1.0f / _surfaces[i].vScale;

                for (int j = 0; j < _surfaces[i].indices.Length; j++)
                {
                    _lightmapcoords[_surfaces[i].indices[j] * 2 + 0] =
                        (_vertices[_surfaces[i].indices[j] * 3 + 0] - min.X) * uScale;
                    _lightmapcoords[_surfaces[i].indices[j] * 2 + 1] = 
                        (_vertices[_surfaces[i].indices[j] * 3 + 1] - min.Y) * vScale;
                }
            }
        }

        public void Render(LightmapResource lightmaps)
        {
            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

            Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, _vertices);
            Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _texcoords);

            if (lightmaps != null)
            {
                Gl.glActiveTexture(Gl.GL_TEXTURE1);
                Gl.glClientActiveTexture(Gl.GL_TEXTURE1);
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _lightmapcoords);
                
                Gl.glActiveTexture(Gl.GL_TEXTURE0);
                Gl.glClientActiveTexture(Gl.GL_TEXTURE0);
            }

            for (int i = 0; i < _surfaces.Length; i++)
            {
                BspSurface surface = _surfaces[i];
                TextureResource lightmap = lightmaps[i];

                TextureResource texture =
                    Resource.ResourceManager.Get(surface.texture.ToUpper() + ".BMP") as TextureResource;

                /*if (texture != null)
                    texture.Bind();
                else
                    Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
                */
                if (lightmap != null)
                {
                    Gl.glActiveTexture(Gl.GL_TEXTURE1);
                    lightmap.Bind();
                    Gl.glActiveTexture(Gl.GL_TEXTURE0);
                }

                Gl.glDrawElements(Gl.GL_TRIANGLES, surface.indices.Length, Gl.GL_UNSIGNED_SHORT,
                    surface.indices);
            }

            if (lightmaps != null)
            {
                Gl.glActiveTexture(Gl.GL_TEXTURE1);
                Gl.glClientActiveTexture(Gl.GL_TEXTURE0);
                Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                Gl.glDisable(Gl.GL_TEXTURE_2D);
                Gl.glActiveTexture(Gl.GL_TEXTURE0);
                Gl.glClientActiveTexture(Gl.GL_TEXTURE0);
            }

            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
        }

        public override void Dispose()
        {
            // nothing
        }

        private void loadTextures()
        {
            foreach (BspSurface surface in _surfaces)
            {
                try
                {
                    Resource.ResourceManager.Load(surface.texture.ToUpper() + ".BMP");
                }
                catch (System.IO.FileNotFoundException)
                {
                }
            }
        }

        private static string convertAsciiToString(byte[] bytes)
        {
            string text = System.Text.Encoding.ASCII.GetString(bytes);

            return text.Trim((char)0);
        }

        private float[] _vertices;
        private float[] _texcoords;
        private float[] _lightmapcoords;
        private BspSurface[] _surfaces;
    }

    class BspResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            BspResource resource = new BspResource(name, stream);

            stream.Close();

            return resource;
        }

        public string[] SupportedExtensions
        {
            get { return new string[] { "BSP" }; }
        }
    }
}
