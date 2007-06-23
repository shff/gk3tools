// Copyright (c) 2007 Brad Farris
// This file is part of the GK3 Scene Viewer.

// The GK3 Scene Viewer is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// The GK3 Scene Viewer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace Gk3Main.Graphics
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

    public struct BspPolygon
    {
        public ushort vertexIndex;
        public ushort flags;
        public ushort numVertices;
        public ushort surfaceIndex;
    }

    public class BspSurface
    {
        public uint modelIndex;
        public string texture;
        public float uCoord, vCoord;
        public float uScale, vScale;
        public ushort size1;
        public ushort size2;
        public uint flags;

        public List<BspPolygon> polygons;

        // used as an "extension"
        public ushort[] indices;

        // not loaded from the file, just used for color-coding surfaces (if needed)
        public float r, g, b;
        public uint index;
        public float[] vertices;
        public float[] lightmapCoords;
        public float[] textureCoords;
        public TextureResource textureResource;
    }

    struct BspModel
    {
        public string name;
        public List<BspSurface> surfaces;
    }

    #endregion

    public class BspResource : Resource.Resource
    {
        public BspResource(string name, System.IO.Stream stream)
            : base(name, true)
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

                models[i].name = Utils.ConvertAsciiToString(reader.ReadBytes(32));
            }

            // read the surfaces
            Random randomGenerator = new Random();
            _surfaces = new BspSurface[header.numSurfaces];
            for (uint i = 0; i < header.numSurfaces; i++)
            {
                _surfaces[i] = new BspSurface();

                _surfaces[i].modelIndex = reader.ReadUInt32();
                _surfaces[i].texture = Utils.ConvertAsciiToString(reader.ReadBytes(32));
                _surfaces[i].uCoord = reader.ReadSingle();
                _surfaces[i].vCoord = reader.ReadSingle();
                _surfaces[i].uScale = reader.ReadSingle();
                _surfaces[i].vScale = reader.ReadSingle();
                _surfaces[i].size1 = reader.ReadUInt16();
                _surfaces[i].size2 = reader.ReadUInt16();
                _surfaces[i].flags = reader.ReadUInt32();

                _surfaces[i].r = (float)randomGenerator.NextDouble();
                _surfaces[i].g = (float)randomGenerator.NextDouble();
                _surfaces[i].b = (float)randomGenerator.NextDouble();
                _surfaces[i].index = i;
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
                _texcoords[i * 2 + 0] = reader.ReadSingle();
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
                _surfaces[i].vertices = new float[numTriangles * 3 * 3];
                _surfaces[i].textureCoords = new float[numTriangles * 3 * 2];
                for (uint j = 0; j < numTriangles; j++)
                {
                    ushort x = reader.ReadUInt16();
                    ushort y = reader.ReadUInt16();
                    ushort z = reader.ReadUInt16();

                    _surfaces[i].indices[j * 3 + 0] = myindices[x];
                    _surfaces[i].indices[j * 3 + 1] = myindices[y]; 
                    _surfaces[i].indices[j * 3 + 2] = myindices[z];

                    // TODO: since we aren't using indices the hardware can't cache vertices,
                    // so there's some performance loss. Figure out a good way to still use indices.

                    // vertex 1
                    _surfaces[i].vertices[(j * 3 + 0) * 3 + 0] = _vertices[myindices[x] * 3 + 0];
                    _surfaces[i].vertices[(j * 3 + 0) * 3 + 1] = _vertices[myindices[x] * 3 + 1];
                    _surfaces[i].vertices[(j * 3 + 0) * 3 + 2] = _vertices[myindices[x] * 3 + 2];

                    _surfaces[i].textureCoords[(j * 3 + 0) * 2 + 0] = _texcoords[myindices[x] * 2 + 0];
                    _surfaces[i].textureCoords[(j * 3 + 0) * 2 + 1] = _texcoords[myindices[x] * 2 + 1];

                    // vertex 2
                    _surfaces[i].vertices[(j * 3 + 1) * 3 + 0] = _vertices[myindices[y] * 3 + 0];
                    _surfaces[i].vertices[(j * 3 + 1) * 3 + 1] = _vertices[myindices[y] * 3 + 1];
                    _surfaces[i].vertices[(j * 3 + 1) * 3 + 2] = _vertices[myindices[y] * 3 + 2];

                    _surfaces[i].textureCoords[(j * 3 + 1) * 2 + 0] = _texcoords[myindices[y] * 2 + 0];
                    _surfaces[i].textureCoords[(j * 3 + 1) * 2 + 1] = _texcoords[myindices[y] * 2 + 1];

                    // vertex 3
                    _surfaces[i].vertices[(j * 3 + 2) * 3 + 0] = _vertices[myindices[z] * 3 + 0];
                    _surfaces[i].vertices[(j * 3 + 2) * 3 + 1] = _vertices[myindices[z] * 3 + 1];
                    _surfaces[i].vertices[(j * 3 + 2) * 3 + 2] = _vertices[myindices[z] * 3 + 2];

                    _surfaces[i].textureCoords[(j * 3 + 2) * 2 + 0] = _texcoords[myindices[z] * 2 + 0];
                    _surfaces[i].textureCoords[(j * 3 + 2) * 2 + 1] = _texcoords[myindices[z] * 2 + 1];
                }
            }

            reader.Close();

            setupLightmapCoords();
            loadTextures();
        }

        public void Render(LightmapResource lightmaps)
        {
            Gl.glColor3f(1.0f, 1.0f, 1.0f);
            Gl.glEnable(Gl.GL_TEXTURE_2D);

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

           // Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, _vertices);
            //Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _texcoords);

            if (SceneManager.LightmapsEnabled && lightmaps != null)
            {
                Gl.glActiveTextureARB(Gl.GL_TEXTURE1);
                Gl.glClientActiveTexture(Gl.GL_TEXTURE1);
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glEnable(Gl.GL_ALPHA_TEST);
                Gl.glAlphaFunc(Gl.GL_LESS, 0.1f);
                Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _lightmapcoords);
                
                Gl.glActiveTexture(Gl.GL_TEXTURE0);
                Gl.glClientActiveTexture(Gl.GL_TEXTURE0);
            }

            for (int i = 0; i < _surfaces.Length; i++)
            {
                Gl.glPushMatrix();
                Gl.glPushAttrib(Gl.GL_ENABLE_BIT);

                BspSurface surface = _surfaces[i];
                TextureResource lightmap = lightmaps[i];

                Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, surface.vertices);

                if (SceneManager.CurrentShadeMode == ShadeMode.Textured)
               //     && collidedSurface.HasValue && collidedSurface.Value.index != _surfaces[i].index)
                {
                    Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, surface.textureCoords);

                    Gl.glEnable(Gl.GL_TEXTURE_2D);
                    Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);
                    surface.textureResource.Bind();
                }
                else
                {
                    Gl.glDisable(Gl.GL_ALPHA_TEST);
                    Gl.glDisable(Gl.GL_TEXTURE_2D);
                   
                    if (SceneManager.CurrentShadeMode == ShadeMode.Colored)
                    {
                        Gl.glColor3f(_surfaces[i].r, _surfaces[i].g, _surfaces[i].b);
                    }
                    else
                    {
                        Gl.glColor3f(1.0f, 1.0f, 1.0f);
                    }
                }

                if (SceneManager.LightmapsEnabled && lightmap != null)
                {
                    Gl.glActiveTexture(Gl.GL_TEXTURE1);
                    Gl.glClientActiveTexture(Gl.GL_TEXTURE1);
                    Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, surface.lightmapCoords);
                    lightmap.Bind();
                    //Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB_EXT, Gl.GL_MODULATE);
                    Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_COMBINE);
                    Gl.glClientActiveTexture(Gl.GL_TEXTURE0);
                    Gl.glActiveTexture(Gl.GL_TEXTURE0);
                }

                Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, surface.vertices.Length / 3);
                //Gl.glDrawElements(Gl.GL_TRIANGLES, surface.indices.Length, Gl.GL_UNSIGNED_SHORT,
                //    surface.indices);

                /*lightmap.Bind(); 
                Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, surface.vertices);
                Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, surface.lightmapCoords );

                Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, surface.vertices.Length /3 );
                */

                Gl.glPopAttrib();
                Gl.glPopMatrix();

                int err = Gl.glGetError();

                if (err != Gl.GL_NO_ERROR)
                    Console.CurrentConsole.Write("error: " + err);
            }

            if (SceneManager.LightmapsEnabled && lightmaps != null)
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

        public bool CollideRayWithSurfaces(Math.Vector origin, Math.Vector direction, float length, out BspSurface surface)
        {
            if (_surfaces == null)
            {
                surface = null;
                return false;
            }

            foreach (BspSurface isurface in _surfaces)
            {
                if (collideRayWithSurface(isurface, origin, direction, length) == true)
                {
                    surface = isurface;
                    return true;
                }
            }

            surface = null;
            return false;
        }

        public override void Dispose()
        {
            // nothing
        }

        private void loadTextures()
        {
            foreach (BspSurface surface in _surfaces)
            {
                surface.textureResource = (TextureResource)Resource.ResourceManager.Load(surface.texture.ToUpper() + ".BMP");
            }
        }

        private void setupLightmapCoords()
        {
            _lightmapcoords = new float[_texcoords.Length];

            for (int i = 0; i < _surfaces.Length; i++)
            {
                _surfaces[i].lightmapCoords = new float[_surfaces[i].indices.Length * 2];
                for (int j = 0; j < _surfaces[i].indices.Length; j++)
                {
                    float u = (_surfaces[i].uCoord + _texcoords[_surfaces[i].indices[j] * 2 + 0]) * _surfaces[i].uScale;
                    float v = (_surfaces[i].vCoord + _texcoords[_surfaces[i].indices[j] * 2 + 1]) * _surfaces[i].vScale;

                    _lightmapcoords[_surfaces[i].indices[j] * 2 + 0] = u;
                    _lightmapcoords[_surfaces[i].indices[j] * 2 + 1] = v;

                    _surfaces[i].lightmapCoords[j * 2 + 0] = u;
                    _surfaces[i].lightmapCoords[j * 2 + 1] = v;
                }
            }
        }

        private bool collideRayWithSurface(BspSurface surface, 
            Math.Vector origin, Math.Vector direction, float length)
        {
            float distance;
            Math.Vector collisionPoint;

            for (int i = 0; i < surface.indices.Length / 3; i++)
            {
                Math.Vector v1 = new Math.Vector(
                    _vertices[surface.indices[i * 3 + 0] * 3 + 0],
                    _vertices[surface.indices[i * 3 + 0] * 3 + 1],
                    _vertices[surface.indices[i * 3 + 0] * 3 + 2]);

                Math.Vector v2 = new Math.Vector(
                    _vertices[surface.indices[i * 3 + 1] * 3 + 0],
                    _vertices[surface.indices[i * 3 + 1] * 3 + 1],
                    _vertices[surface.indices[i * 3 + 1] * 3 + 2]);

                Math.Vector v3 = new Math.Vector(
                    _vertices[surface.indices[i * 3 + 2] * 3 + 0],
                    _vertices[surface.indices[i * 3 + 2] * 3 + 1],
                    _vertices[surface.indices[i * 3 + 2] * 3 + 2]);

                if (Utils.TestRayTriangleCollision(origin, direction,
                    v1, v2, v3, out distance, out collisionPoint) == true)
                    return true;
            }

            return false;
        }

        private float[] _vertices;
        private float[] _texcoords;
        private float[] _lightmapcoords;
        private BspSurface[] _surfaces;
    }

    public class BspResourceLoader : Resource.IResourceLoader
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

        public bool EmptyResourceIfNotFound { get { return false; } }
    }
}
