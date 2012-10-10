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

    public struct BspVertex
    {
        public float X, Y, Z;
        public float U, V;
        public float LU, LV;
    }

    [Flags]
    public enum BspSurfaceFlags
    {
        HighQuality = 0x0001,
        LowQuality =  0x0002,
        DontCastShadows = 0x0004,
        DontReceiveShadows = 0x0008,
        Translucent = 0x0010,
        Unknown = 0x0020,
        ShadowTexture = 0x0040
    }

    public class BspSurface
    {
        public int VertexIndex;
        public int VertexCount;

        public uint modelIndex;
        public string texture;
        public float uCoord, vCoord;
        public float uScale, vScale;
        public ushort size1;
        public ushort size2;
        public BspSurfaceFlags flags;

        public List<BspPolygon> polygons;

        // used as an "extension"
        public ushort[] indices;

        // not loaded from the file, just used for color-coding surfaces (if needed)
        public float r, g, b;
        public uint index;
        public uint numTriangles;
        public TextureResource textureResource;

        public Math.Vector4 boundingSphere;
        public bool Hidden;
    }

    struct BspModel
    {
        public string name;
        public List<BspSurface> surfaces;
    }

    struct BspNode
    {
        public short Left;
        public short Right;
        public short PlaneIndex;
        public short PolygonStartIndex;
        public short NumPolygons;
    }

    #endregion

    public class BspResource : Resource.Resource
    {
        public static void Init(Resource.ResourceManager content)
        {
            _basicTexturedEffect = content.Load<Effect>("basic_textured.fx");
            _lightmapEffect = content.Load<Effect>("basic_lightmapped.fx");
            _lightmapNoTextureEffect = content.Load<Effect>("basic_lightmapped_notexture.fx");

            if (_vertexDeclaration == null)
                _vertexDeclaration = new VertexElementSet(new VertexElement[] {
                    new VertexElement(0, VertexElementFormat.Float3, VertexElementUsage.Position, 0),
                    new VertexElement(sizeof(float) * 3, VertexElementFormat.Float2, VertexElementUsage.TexCoord, 0),
                    new VertexElement(sizeof(float) * 5, VertexElementFormat.Float2, VertexElementUsage.TexCoord, 1)
                });
        }

        public BspResource(string name, System.IO.Stream stream, Resource.ResourceManager content)
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
            _modelsNames = new string[header.numModels];
            for (uint i = 0; i < header.numModels; i++)
            {
                models[i] = new BspModel();

                models[i].name = Gk3Main.Utils.ConvertAsciiToString(reader.ReadBytes(32));
                _modelsNames[i] = models[i].name;
            }

            // read the surfaces
            Random randomGenerator = new Random();
            _surfaces = new BspSurface[header.numSurfaces];
            for (uint i = 0; i < header.numSurfaces; i++)
            {
                if (i == 134)
                    i = i;

                _surfaces[i] = new BspSurface();

                _surfaces[i].modelIndex = reader.ReadUInt32();
                _surfaces[i].texture = Gk3Main.Utils.ConvertAsciiToString(reader.ReadBytes(32));
                _surfaces[i].uCoord = reader.ReadSingle();
                _surfaces[i].vCoord = reader.ReadSingle();
                _surfaces[i].uScale = reader.ReadSingle();
                _surfaces[i].vScale = reader.ReadSingle();
                _surfaces[i].size1 = reader.ReadUInt16();
                _surfaces[i].size2 = reader.ReadUInt16();
                _surfaces[i].flags = (BspSurfaceFlags)reader.ReadUInt32();

                _surfaces[i].r = (float)randomGenerator.NextDouble();
                _surfaces[i].g = (float)randomGenerator.NextDouble();
                _surfaces[i].b = (float)randomGenerator.NextDouble();
                _surfaces[i].index = i;
            }

            // read the BSP nodes (for now throw them away)
            _nodes = new BspNode[header.numNodes];
            for (uint i = 0; i < header.numNodes; i++)
            {
                _nodes[i].Left = reader.ReadInt16();
                _nodes[i].Right = reader.ReadInt16();
                _nodes[i].PlaneIndex = reader.ReadInt16();
                _nodes[i].PolygonStartIndex = reader.ReadInt16();
                reader.ReadInt16();
                _nodes[i].NumPolygons = reader.ReadInt16();

                uint i3 = reader.ReadUInt16();
                uint i4 = reader.ReadUInt16();
            }

            // TEMP: validate the BSP
            foreach (BspNode node in _nodes)
            {
                if (node.Left >= _nodes.Length ||
                    node.Right >= _nodes.Length)
                    throw new Exception("OH NO!");

                
            }

            int parent = findParent(_nodes, 0);

            // read all the polygons
            _polygons = new BspPolygon[header.numPolygons];
            for (uint i = 0; i < header.numPolygons; i++)
            {
                _polygons[i] = new BspPolygon();

                _polygons[i].vertexIndex = reader.ReadUInt16();
                _polygons[i].flags = reader.ReadUInt16();
                _polygons[i].numVertices = reader.ReadUInt16();
                _polygons[i].surfaceIndex = reader.ReadUInt16();

                if (_polygons[i].surfaceIndex == 134)
                    i = i;
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
                _vertices[i * 3 + 0] = reader.ReadSingle();
                _vertices[i * 3 + 1] = reader.ReadSingle();
                _vertices[i * 3 + 2] = reader.ReadSingle();
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

            // read the bounding spheres
            _boundingSpheres = new Math.Vector4[header.numNodes];
            for (uint i = 0; i < header.numNodes; i++)
            {
                _boundingSpheres[i].Z = reader.ReadSingle();
                _boundingSpheres[i].Y = reader.ReadSingle();
                _boundingSpheres[i].X = reader.ReadSingle();
                _boundingSpheres[i].W = reader.ReadSingle();
            }

            // load the "thingies", whatever that means
            _bspVertices = new List<BspVertex>();
            for (int i = 0; i < header.numSurfaces; i++)
            {
                // throw junk away
                _surfaces[i].boundingSphere.X = reader.ReadSingle();
                _surfaces[i].boundingSphere.Y = reader.ReadSingle();
                _surfaces[i].boundingSphere.Z = reader.ReadSingle();
                _surfaces[i].boundingSphere.W = reader.ReadSingle();

                reader.ReadBytes(12);

                uint numIndices = reader.ReadUInt32();
                _surfaces[i].numTriangles = reader.ReadUInt32();

                UInt16[] myindices = new UInt16[numIndices];
                for (uint j = 0; j < numIndices; j++)
                {
                    myindices[j] = reader.ReadUInt16();
                }

                _surfaces[i].VertexIndex = _bspVertices.Count;
                _surfaces[i].indices = new ushort[_surfaces[i].numTriangles * 3];
                for (uint j = 0; j < _surfaces[i].numTriangles; j++)
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
                    BspVertex vertex = new BspVertex();
                    vertex.X = _vertices[myindices[x] * 3 + 0];
                    vertex.Y = _vertices[myindices[x] * 3 + 1];
                    vertex.Z = _vertices[myindices[x] * 3 + 2];
                    vertex.U = _texcoords[myindices[x] * 2 + 0];
                    vertex.V = _texcoords[myindices[x] * 2 + 1];
                    _bspVertices.Add(vertex);

                    // vertex 2
                    vertex.X = _vertices[myindices[y] * 3 + 0];
                    vertex.Y = _vertices[myindices[y] * 3 + 1];
                    vertex.Z = _vertices[myindices[y] * 3 + 2];
                    vertex.U = _texcoords[myindices[y] * 2 + 0];
                    vertex.V = _texcoords[myindices[y] * 2 + 1];
                    _bspVertices.Add(vertex);

                    // vertex 3
                    vertex.X = _vertices[myindices[z] * 3 + 0];
                    vertex.Y = _vertices[myindices[z] * 3 + 1];
                    vertex.Z = _vertices[myindices[z] * 3 + 2];
                    vertex.U = _texcoords[myindices[z] * 2 + 0];
                    vertex.V = _texcoords[myindices[z] * 2 + 1];
                    _bspVertices.Add(vertex);
                }
                _surfaces[i].VertexCount = _bspVertices.Count - _surfaces[i].VertexIndex;
            }

            reader.Close();

            loadTextures(content);
        }

        public void FinalizeVertices(LightmapResource lightmap, bool regen)
        {
            _lightmapcoords = new float[_bspVertices.Count * 2];

            for (int i = 0; i < _surfaces.Length; i++)
            {
                if (i == 45)
                    i = i;

                for (int j = 0; j < _surfaces[i].indices.Length; j++)
                {
                    float u = (_surfaces[i].uCoord + _texcoords[_surfaces[i].indices[j] * 2 + 0]) * _surfaces[i].uScale;
                    float v = (_surfaces[i].vCoord + _texcoords[_surfaces[i].indices[j] * 2 + 1]) * _surfaces[i].vScale;

                    //_lightmapcoords[_surfaces[i].indices[j] * 2 + 0] = u;
                    //_lightmapcoords[_surfaces[i].indices[j] * 2 + 1] = v;

                    _lightmapcoords[(_surfaces[i].VertexIndex + j) * 2 + 0] = u;
                    _lightmapcoords[(_surfaces[i].VertexIndex + j) * 2 + 1] = v;

                    if (lightmap != null)
                    {
                        Rect lightmapRect = lightmap.PackedLightmaps.GetPackedTextureRect(i);

                        //_lightmapcoords[_surfaces[i].indices[j] * 2 + 0] += 1.0f / lightmapRect.Width * 0.5f;
                        //_lightmapcoords[_surfaces[i].indices[j] * 2 + 1] += 1.0f / lightmapRect.Height * 0.5f;
                     
                        float lu = (lightmapRect.X + 0.5f) / lightmap.PackedLightmapTexture.Width;
                        float lv = (lightmapRect.Y + 0.5f) / lightmap.PackedLightmapTexture.Height;
                        float lw = (lightmapRect.Width - 1.0f) / lightmap.PackedLightmapTexture.Width;
                        float lh = (lightmapRect.Height - 1.0f) / lightmap.PackedLightmapTexture.Height;

                        BspVertex vertex = _bspVertices[_surfaces[i].VertexIndex + j];
                        vertex.LU = lu + u * lw;
                        vertex.LV = lv + v * lh;
                        _bspVertices[_surfaces[i].VertexIndex + j] = vertex;
                    }
                    else
                    {
                        BspVertex vertex = _bspVertices[_surfaces[i].VertexIndex + j];
                        vertex.LU = u;
                        vertex.LV = v;
                        _bspVertices[_surfaces[i].VertexIndex + j] = vertex;
                    }
                }
            }

            _allVertices = RendererManager.CurrentRenderer.CreateVertexBuffer(VertexBufferUsage.Static, _bspVertices.ToArray(), _bspVertices.Count, _vertexDeclaration);
        }

        public void Render(Camera camera, LightmapResource lightmaps, bool calculatingRadiosity)
        {
            Effect currentEffect;
            bool lightmappingEnabled = false;
            if (SceneManager.LightmapsEnabled && lightmaps != null)
            {
                if (SceneManager.CurrentShadeMode == ShadeMode.Flat)
                {
                    currentEffect = _lightmapNoTextureEffect;
                    lightmappingEnabled = true;
                }
                else
                {
                    currentEffect = _lightmapEffect;
                    lightmappingEnabled = true;
                }
            }
            else
            {
                if (SceneManager.CurrentShadeMode == ShadeMode.Textured)
                    currentEffect = _basicTexturedEffect;
                else
                    return; // nothing to render
            }

            if (SceneManager.CurrentFilterMode == TextureFilterMode.None)
            {
                RendererManager.CurrentRenderer.SamplerStates[0] = SamplerState.PointWrap;
                RendererManager.CurrentRenderer.SamplerStates[1] = SamplerState.PointWrap;
            }
            else
            {
                RendererManager.CurrentRenderer.SamplerStates[0] = SamplerState.LinearWrap;
                RendererManager.CurrentRenderer.SamplerStates[1] = SamplerState.LinearWrap;
            }


            float lightmapMultiplier;
            if (SceneManager.DoubleLightmapValues)
                lightmapMultiplier = 2.0f;
            else
                lightmapMultiplier = 1.0f;

            RendererManager.CurrentRenderer.SetVertexBuffer(_allVertices);
            currentEffect.Bind();

            if (lightmappingEnabled)
            {
                currentEffect.SetParameter("LightmapMultiplier", lightmapMultiplier);
                TextureResource lightmap = lightmaps.PackedLightmapTexture;
                if (lightmap != null)
                    currentEffect.SetParameter("Lightmap", lightmap, 1);
            }

            currentEffect.SetParameter("ModelViewProjection", camera.ViewProjection);
            currentEffect.Begin();
            for (int i = 0; i < _surfaces.Length; i++)
            {
                if (_surfaces[i].Hidden == false)
                {
                    if (!calculatingRadiosity || (_surfaces[i].flags & BspSurfaceFlags.DontCastShadows) == 0)
                        drawSurface(_surfaces[i], currentEffect, camera);
                }
            }
            currentEffect.End();
        }

        public bool CollideRayWithSurfaces(Math.Vector3 origin, Math.Vector3 direction, float length, out BspSurface surface)
        {
            if (_surfaces == null)
            {
                surface = null;
                return false;
            }

            surface = null;
            float distance = float.MaxValue, minDistance = float.MaxValue;
            foreach (BspSurface isurface in _surfaces)
            {
                if (collideRayWithSurface(isurface, origin, direction, length, out distance) == true)
                {
                    if (distance < minDistance && distance > 0)
                    {
                        minDistance = distance;
                        surface = isurface;
                    }
                }
            }

            if (surface == null)
                return false;

            
            return true;
        }

        public override void Dispose()
        {
            // nothing
        }

        public string GetModelName(uint index) { return _modelsNames[index]; }

        public List<string> GetAllModels()
        {
            return new List<string>(_modelsNames);
        }

        public BspSurface[] Surfaces
        {
            get { return _surfaces; }
        }

        public void SetSurfaceVisibility(string name, bool visible)
        {
            foreach (BspSurface surface in _surfaces)
            {
                if (_modelsNames[surface.modelIndex].Equals(name, StringComparison.OrdinalIgnoreCase))
                    surface.Hidden = !visible;
            }
        }

        public void SetSurfaceVisibility(int index, bool visible)
        {
            _surfaces[index].Hidden = !visible;
        }

        public void SetAllSurfacesVisibility(bool visible)
        {
            foreach (BspSurface surface in _surfaces)
            {
                surface.Hidden = !visible;
            }
        }

        public Game.RadiosityMaps GenerateMemoryTextures(LightmapResource lightmaps)
        {
            if (IntPtr.Size != 4)
                throw new InvalidOperationException("Cannot generate memory textures unless running in 32-bit mode");

            const int minSize = 4;
            Game.RadiosityMaps maps = new Game.RadiosityMaps(lightmaps, minSize);

            foreach (BspSurface surface in _surfaces)
            {
                BitmapSurface originalLightmap = lightmaps.Maps[surface.index];

                int width = System.Math.Max(originalLightmap.Width, minSize);
                int height = System.Math.Max(originalLightmap.Height, minSize);

                //int width = originalLightmap.Width;
                //int height = originalLightmap.Height;

                maps.Maps[surface.index].MemoryTexture = Game.Radiosity.GenerateMemoryTexture(width, height, 0, 0, 0);
            }

            return maps;
        }

        public void CalcRadiosityPass(Graphics.LightmapResource original, Game.RadiosityMaps radiosity)
        {
            const int batchSize = 256;
            Game.Radiosity.LightmapTexel[] texels = new Game.Radiosity.LightmapTexel[batchSize];

            System.Runtime.InteropServices.GCHandle[] h = new System.Runtime.InteropServices.GCHandle[_surfaces.Length];

            int count = 0;


            int currentBatchIndex = 0;
            foreach (BspSurface surface in _surfaces)
            {

                if (surface.index == 50)
                    count++;

                count++;
               //if (count >950)break;

               h[surface.index] = System.Runtime.InteropServices.GCHandle.Alloc(radiosity.Maps[surface.index].Map, System.Runtime.InteropServices.GCHandleType.Pinned);

                //TextureResource memTex = surface.textureResource;
               TextureResource memTex = radiosity.Maps[surface.index].MemoryTexture;
                Rect lightmapRect = original.PackedLightmaps.GetPackedTextureRect((int)surface.index);

                // iterate over each triangle
                for (int tri = 0; tri < surface.VertexCount / 3; tri++)
                {
                    Math.Vector2 a, b, c;

                    /*a.X = _lightmapcoords[surface.indices[tri * 3 + 0] * 2 + 0];
                    a.Y = _lightmapcoords[surface.indices[tri * 3 + 0] * 2 + 1];

                    b.X = _lightmapcoords[surface.indices[tri * 3 + 1] * 2 + 0];
                    b.Y = _lightmapcoords[surface.indices[tri * 3 + 1] * 2 + 1];

                    c.X = _lightmapcoords[surface.indices[tri * 3 + 2] * 2 + 0];
                    c.Y = _lightmapcoords[surface.indices[tri * 3 + 2] * 2 + 1];*/

                   // (_surfaces[i].VertexIndex + j) * 2 + 0

                    a.X = _lightmapcoords[(surface.VertexIndex + tri * 3 + 0) * 2 + 0];
                    a.Y = _lightmapcoords[(surface.VertexIndex + tri * 3 + 0) * 2 + 1];

                    b.X = _lightmapcoords[(surface.VertexIndex + tri * 3 + 1) * 2 + 0];
                    b.Y = _lightmapcoords[(surface.VertexIndex + tri * 3 + 1) * 2 + 1];

                    c.X = _lightmapcoords[(surface.VertexIndex + tri * 3 + 2) * 2 + 0];
                    c.Y = _lightmapcoords[(surface.VertexIndex + tri * 3 + 2) * 2 + 1];
                    
                    /*BspVertex vertex = _bspVertices[surface.VertexIndex + tri * 3 + 0];
                    a.X = (vertex.LU - lightmapRect.X) / (lightmapRect.Width + 1.0f);
                    a.Y = (vertex.LV - lightmapRect.Y) / (lightmapRect.Height + 1.0f);

                    vertex = _bspVertices[surface.VertexIndex + tri * 3 + 1];
                    b.X = (vertex.LU - lightmapRect.X) / (lightmapRect.Width + 1.0f);
                    b.Y = (vertex.LV - lightmapRect.Y) / (lightmapRect.Height + 1.0f);

                    vertex = _bspVertices[surface.VertexIndex + tri * 3 + 2];
                    c.X = (vertex.LU - lightmapRect.X) / (lightmapRect.Width + 1.0f);
                    c.Y = (vertex.LV - lightmapRect.Y) / (lightmapRect.Height + 1.0f);*/
                    
                    /*a.X = _lightmapcoords[surface.indices[tri * 3 + 0] * 2 + 0];
                    a.Y = _lightmapcoords[surface.indices[tri * 3 + 0] * 2 + 1];

                    b.X = _lightmapcoords[surface.indices[tri * 3 + 1] * 2 + 0];
                    b.Y = _lightmapcoords[surface.indices[tri * 3 + 1] * 2 + 1];

                    c.X = _lightmapcoords[surface.indices[tri * 3 + 2] * 2 + 0];
                    c.Y = _lightmapcoords[surface.indices[tri * 3 + 2] * 2 + 1];*/

                    
                    /*a.X = _bspVertices[surface.VertexIndex + tri + 0].LU;
                    a.Y = _bspVertices[surface.VertexIndex + tri + 0].LV;

                    b.X = _bspVertices[surface.VertexIndex + tri + 1].LU;
                    b.Y = _bspVertices[surface.VertexIndex + tri + 1].LV;

                    c.X = _bspVertices[surface.VertexIndex + tri + 2].LU;
                    c.Y = _bspVertices[surface.VertexIndex + tri + 2].LV;*/

                    Math.Vector3 pa, pb, pc;
                    pa.X = _bspVertices[surface.VertexIndex + tri * 3 + 0].X;
                    pa.Y = _bspVertices[surface.VertexIndex + tri * 3 + 0].Y;
                    pa.Z = _bspVertices[surface.VertexIndex + tri * 3 + 0].Z;

                    pb.X = _bspVertices[surface.VertexIndex + tri * 3 + 1].X;
                    pb.Y = _bspVertices[surface.VertexIndex + tri * 3 + 1].Y;
                    pb.Z = _bspVertices[surface.VertexIndex + tri * 3 + 1].Z;

                    pc.X = _bspVertices[surface.VertexIndex + tri * 3 + 2].X;
                    pc.Y = _bspVertices[surface.VertexIndex + tri * 3 + 2].Y;
                    pc.Z = _bspVertices[surface.VertexIndex + tri * 3 + 2].Z;

                    Math.Vector3 up = (pa - pb).Normalize();
                    Math.Vector3 n = up.Cross((pa - pc).Normalize()).Normalize();

                    Math.Vector2 minUV, maxUV;
                    minUV.X = System.Math.Min(System.Math.Min(a.X, b.X), c.X);
                    minUV.Y = System.Math.Min(System.Math.Min(a.Y, b.Y), c.Y);
                    maxUV.X = System.Math.Max(System.Math.Max(a.X, b.X), c.X);
                    maxUV.Y = System.Math.Max(System.Math.Max(a.Y, b.Y), c.Y);

                    // map the triangle to the texture
                    int minTX = System.Math.Max((int)(memTex.Width * minUV.X) - 1, 0);
                    int minTY = System.Math.Max((int)(memTex.Height * minUV.Y) - 1, 0);
                    int maxTX = System.Math.Min((int)(System.Math.Ceiling(memTex.Width * maxUV.X + 1)) , memTex.Width);
                    int maxTY = System.Math.Min((int)(System.Math.Ceiling(memTex.Height * maxUV.Y + 1)) , memTex.Height);

                    float halfPixelU = 1.0f / memTex.Width * 0.5f;
                    float halfPixelV = 1.0f / memTex.Height * 0.5f;

                    // go through each texel and do the radiosity stuff
                    for (int y = minTY; y < maxTY; y++)
                    {
                        for (int x = minTX; x < maxTX; x++)
                        {
                           /* if (x < 0 || y < 0 ||
                                   x >= radiosity.Maps[surface.index].Width ||
                                   y >= radiosity.Maps[surface.index].Height)
                                continue;*/

                            Math.Vector2 texelUV;

                            texelUV.X = (float)x / memTex.Width + halfPixelU;
                            texelUV.Y = (float)y / memTex.Height + halfPixelV;

                            /*if (memTex.Width > 1)
                                texelUV.X = (float)x / memTex.Width + halfPixelU;
                            else
                                texelUV.X = (float)x / memTex.Width;

                            if (memTex.Height > 1)
                                texelUV.Y = (float)y / memTex.Height + halfPixelV;
                            else
                                texelUV.Y = (float)y / memTex.Height;
                            */

                           // radiosity.Maps[surface.index].Map[(y * radiosity.Maps[surface.index].Width + x) * 3 + 0] = 1000.0f;
                            //radiosity.Maps[surface.index].Map[(y * radiosity.Maps[surface.index].Width + x) * 3 + 1] = 0;
                           // radiosity.Maps[surface.index].Map[(y * radiosity.Maps[surface.index].Width + x) * 3 + 2] = 0;
                            //radiosity.Maps[surface.index].SetColor(x, y, 1000.0f, 0, 0);

                            // are we on the triangle?
                            Math.Vector2 projectedUV;
                            Math.Vector2 texelMin, texelMax;
                            texelMin.X = (float)x / memTex.Width;
                            texelMin.Y = (float)y / memTex.Height;
                            texelMax.X = (float)(x + 1) / memTex.Width;
                            texelMax.Y = (float)(y + 1) / memTex.Height;
                            if (Gk3Main.Utils.TestTriangleBox(a, b, c, texelMin, texelMax))
                            {

                                Gk3Main.Utils.IsPointInTriangle(texelUV, a, b, c, out projectedUV);

                                // calc the world coordinates
                                Math.Vector3 p3 = pa + (pb - pa) * projectedUV.Y + (pc - pa) * projectedUV.X;
                                //Math.Vector3 p3(p.X, p.Y, p.Z);

                                //Math.Vector4 p4 = worldView * p3;



                               

                                texels[currentBatchIndex].Tag = Gk3Main.Utils.IncrementIntPtr(h[surface.index].AddrOfPinnedObject(), (y * radiosity.Maps[surface.index].Width + x) * 3 * 4);

                                //texels[currentBatchIndex].Tag = (IntPtr)(h[surface.index].AddrOfPinnedObject() + (IntPtr)((y * radiosity.Maps[surface.index].Width + x) * 3 * 4);
                                texels[currentBatchIndex].PosX = p3.X;
                                texels[currentBatchIndex].PosY = p3.Y;
                                texels[currentBatchIndex].PosZ = p3.Z;
                                texels[currentBatchIndex].NormalX = n.X;
                                texels[currentBatchIndex].NormalY = n.Y;
                                texels[currentBatchIndex].NormalZ = n.Z;
                                texels[currentBatchIndex].UpX = up.X;
                                texels[currentBatchIndex].UpY = up.Y;
                                texels[currentBatchIndex].UpZ = up.Z;
                                texels[currentBatchIndex].Red = 0;
                                texels[currentBatchIndex].Green = 0;
                                texels[currentBatchIndex].Blue = 0;

                                //rad_CalcPass(p4.getX(), p4.getY(), p4.getZ(),
                                //    n.X, n.Y, n.Z, 
                                //    &results[(y * texture->GetWidth() + x) * 3 + 0],
                                //    &results[(y * texture->GetWidth() + x) * 3 + 1],
                                //    &results[(y * texture->GetWidth() + x) * 3 + 2]);

                                //results[(y * texture->GetWidth() + x) * 3 + 0] *= 0.1f;
                                //results[(y * texture->GetWidth() + x) * 3 + 1] *= 0.1f;
                                //results[(y * texture->GetWidth() + x) * 3 + 2] *= 0.1f;

                                currentBatchIndex++;

                                if (currentBatchIndex >= batchSize)
                                {
                                    Game.Radiosity.CalcPass(texels, currentBatchIndex);

                                    for (int i = 0; i < currentBatchIndex; i++)
                                    {
                                        //int index = texels[i].Tag.ToInt32() * 3;

                                        unsafe
                                        {
                                            float* f = (float*)texels[i].Tag.ToPointer();
                                            /*radiosity.Maps[surface.index].Map[index + 0] = texels[i].Red;
                                            radiosity.Maps[surface.index].Map[index + 1] = texels[i].Green;
                                            radiosity.Maps[surface.index].Map[index + 2] = texels[i].Blue;*/


                                            f[0] = System.Math.Max(texels[i].Red, f[0]);
                                            f[1] = System.Math.Max(texels[i].Green, f[1]);
                                            f[2] = System.Math.Max(texels[i].Blue, f[2]);
                                        }

                                        //texels[i].r
                                    }

                                    currentBatchIndex = 0;
                                }
                            }
                            else
                            {
                                currentBatchIndex = currentBatchIndex;
                            }
                        }
                    }
                }
            }

            if (currentBatchIndex > 0)
            {
                Game.Radiosity.CalcPass(texels, currentBatchIndex);

                for (int i = 0; i < currentBatchIndex; i++)
                {
                    unsafe
                    {
                        float* f = (float*)texels[i].Tag.ToPointer();
                        /*radiosity.Maps[surface.index].Map[index + 0] = texels[i].Red;
                        radiosity.Maps[surface.index].Map[index + 1] = texels[i].Green;
                        radiosity.Maps[surface.index].Map[index + 2] = texels[i].Blue;*/

                        f[0] = System.Math.Max(texels[i].Red, f[0]);
                        f[1] = System.Math.Max(texels[i].Green, f[1]);
                        f[2] = System.Math.Max(texels[i].Blue, f[2]);
                        
                    }

                    /*int index = texels[i].Tag.ToInt32() * 3;
                    radiosity.Maps[surface.index].Map[index + 0] = texels[i].Red;
                    radiosity.Maps[surface.index].Map[index + 1] = texels[i].Green;
                    radiosity.Maps[surface.index].Map[index + 2] = texels[i].Blue;

                    */
                    //texels[i].r
                }
            }
        }

        private void loadTextures(Resource.ResourceManager content)
        {
            foreach (BspSurface surface in _surfaces)
            {
                surface.textureResource = content.Load<TextureResource>(surface.texture);
            }
        }

        private bool collideRayWithSurface(BspSurface surface, 
            Math.Vector3 origin, Math.Vector3 direction, float length, out float distance)
        {
            Math.Vector3? collisionPoint;

            for (int i = 0; i < surface.indices.Length / 3; i++)
            {
                Math.Vector3 v1 = new Math.Vector3(
                    _vertices[surface.indices[i * 3 + 0] * 3 + 0],
                    _vertices[surface.indices[i * 3 + 0] * 3 + 1],
                    _vertices[surface.indices[i * 3 + 0] * 3 + 2]);

                Math.Vector3 v2 = new Math.Vector3(
                    _vertices[surface.indices[i * 3 + 1] * 3 + 0],
                    _vertices[surface.indices[i * 3 + 1] * 3 + 1],
                    _vertices[surface.indices[i * 3 + 1] * 3 + 2]);

                Math.Vector3 v3 = new Math.Vector3(
                    _vertices[surface.indices[i * 3 + 2] * 3 + 0],
                    _vertices[surface.indices[i * 3 + 2] * 3 + 1],
                    _vertices[surface.indices[i * 3 + 2] * 3 + 2]);

                if (Gk3Main.Utils.TestRayTriangleCollision(origin, direction,
                    v1, v2, v3, out distance, out collisionPoint) == true)
                    return true;
            }

            distance = float.NaN;
            return false;
        }

        /// <summary>
        /// Recursively draws the BSP tree, beginning at the specified node. SLOW!!
        /// </summary>
        private void drawBspNode(int nodeIndex, Effect effect, LightmapResource lightmaps, Camera camera)
        {
            if (_boundingSpheres[nodeIndex].W > 100.0f &&
                camera.Frustum.IsSphereOutside(_boundingSpheres[nodeIndex]))
                return;

            BspNode node = _nodes[nodeIndex];
            if (node.Left < 0 && node.Right < 0)
            {
                // draw the polygon
                for (int i = node.PolygonStartIndex; i < node.PolygonStartIndex + node.NumPolygons; i++)
                {
                    //drawSurface(_surfaces[_polygons[i].surfaceIndex], lightmaps[_polygons[i].surfaceIndex], effect, camera);
                }
            }
            else
            {
                if (node.Left >= 0)
                    drawBspNode(node.Left, effect, lightmaps, camera);
                if (node.Right >= 0)
                    drawBspNode(node.Right, effect, lightmaps, camera);

                // draw the polygon
                for (int i = node.PolygonStartIndex; i < node.PolygonStartIndex + node.NumPolygons; i++)
                {
                    //drawSurface(_surfaces[_polygons[i].surfaceIndex], lightmaps[_polygons[i].surfaceIndex], effect, camera);
                }
            }
        }

        private void drawSurface(BspSurface surface, Effect effect, Camera camera)
        {
            if (camera.Frustum.IsSphereOutside(surface.boundingSphere))
                return;

            effect.SetParameter("Diffuse", surface.textureResource, 0);

            effect.CommitParams();

            RendererManager.CurrentRenderer.RenderPrimitives(surface.VertexIndex, surface.VertexCount);
        }

        private int findParent(BspNode[] nodes, int index)
        {
            int currentIndex = 0;
            bool foundNode = false;

            do
            {
                foundNode = false;

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].Left == index ||
                        nodes[i].Right == index)
                    {
                        currentIndex = i;
                        foundNode = true;
                        break;
                    }
                }
            } while (foundNode);

            return currentIndex;
        }

        private float[] _vertices;
        private float[] _texcoords;
        private float[] _lightmapcoords;
        private List<BspVertex> _bspVertices;
        private VertexBuffer _allVertices;
        private BspSurface[] _surfaces;
        private BspNode[] _nodes;
        private BspPolygon[] _polygons;
        private Math.Vector4[] _boundingSpheres;
        private string[] _modelsNames;

        private static Effect _basicTexturedEffect;
        private static Effect _lightmapEffect;
        private static Effect _lightmapNoTextureEffect;
        private static VertexElementSet _vertexDeclaration;
    }

    public class BspResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            if (name.IndexOf('.') < 0)
                name += ".BSP";

            System.IO.Stream stream = FileSystem.Open(name);

            BspResource resource = new BspResource(name, stream, content);

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
