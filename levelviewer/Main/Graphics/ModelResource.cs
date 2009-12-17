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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301 

using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;

namespace Gk3Main.Graphics
{
    #region Model file structure

    struct ModHeader
    {
        public uint heading;
        public byte minorVersion;
        public byte majorVersion;
        public ushort unknown1;
        public uint numMeshes;
        public uint size;
        public float lodDistance;
        public uint unknown3;
    }

    struct ModHeaderExtension
    {
        public bool isBillboard;
        public bool useCenterForBillboard;
        public float centerX;
        public float centerY;
        public float centerZ;
        public bool smooth;
    }

    struct ModMeshSection
    {
        public uint heading;
        public string texture;
        public TextureResource textureResource;

        public uint color;
        public bool smooth;
        public uint numVerts;
        public uint numTriangles;
        public uint numLODs;
        public bool axes;

        public float[] vertices;
        public float[] normals;
        public float[] texCoords;
        public int[] indices;
    }

    struct ModMeshLod
    {
        public uint Magic;
        public uint Heading;
        public uint Heading2;
        public uint Heading3;

        public ushort[] Section1;
        public ushort[] Section2;
        public ushort[] Section3;
    }

    struct ModMesh
    {
        public uint heading;
        public Math.Matrix TransformMatrix;
        public uint numSections;
        public float[] boundingBox;
        public float[] TransformedBoundingBox;

        public ModMeshSection[] sections;

        public Math.Vector3 CalcBoundingBoxCenter()
        {
            return new Math.Vector3(
                boundingBox[3] + boundingBox[0],
                boundingBox[4] + boundingBox[1],
                boundingBox[5] + boundingBox[2])
                * 0.5f;
        }
    }

    #endregion

    class ModelResource : Resource.Resource
    {
        static ModelResource()
        {
            _elements = new VertexElementSet(new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Float3, VertexElementUsage.Position, 0),
                new VertexElement(3 * sizeof(float), VertexElementFormat.Float2, VertexElementUsage.TexCoord, 0),
                new VertexElement(5 * sizeof(float), VertexElementFormat.Float3, VertexElementUsage.Normal, 0)
            });
        }

        public ModelResource(string name)
            : base(name, false)
        {
            _effect = (Effect)Resource.ResourceManager.Load("basic_textured.fx");
        }

        public ModelResource(string name, System.IO.Stream stream)
            : base(name, true)
        {
            int currentStreamPosition = (int)stream.Position;
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream, Encoding.ASCII);

            // read the header
            ModHeader header;
            header.heading = reader.ReadUInt32();
            header.minorVersion = reader.ReadByte();
            header.majorVersion = reader.ReadByte();
            header.unknown1 = reader.ReadUInt16();
            header.numMeshes = reader.ReadUInt32();
            header.size = reader.ReadUInt32();
            header.lodDistance = reader.ReadSingle();
            header.unknown3 = reader.ReadUInt32();

            if (header.minorVersion == 9 && header.majorVersion == 1)
            {
                ModHeaderExtension headerExtension;
                headerExtension.isBillboard = reader.ReadUInt32() != 0;
                headerExtension.useCenterForBillboard = reader.ReadUInt32() != 0;
                headerExtension.centerX = reader.ReadSingle();
                headerExtension.centerY = reader.ReadSingle();
                headerExtension.centerZ = reader.ReadSingle();
                headerExtension.smooth = reader.ReadUInt32() != 0;

                _isBillboard = headerExtension.isBillboard;
                _useBillboardCenter = headerExtension.useCenterForBillboard;
                _billboardCenter.X = headerExtension.centerX;
                _billboardCenter.Y = headerExtension.centerY;
                _billboardCenter.Z = headerExtension.centerZ;
            }

            // read the meshes
            _meshes = new ModMesh[header.numMeshes];
            for (uint i = 0; i < header.numMeshes; i++)
            {
                ModMesh mesh = new ModMesh();

                mesh.heading = reader.ReadUInt32();

                // if we didn't find it then we're screwed
                if (mesh.heading != 0x4D455348)
                {
                    throw new Resource.InvalidResourceFileFormat("Not a valid model file! Unable to find MESH section!");
                }

                float[] transform = new float[16];
                transform[0] = reader.ReadSingle();
                transform[1] = reader.ReadSingle();
                transform[2] = reader.ReadSingle();
                transform[3] = 0;

                transform[4] = reader.ReadSingle();
                transform[5] = reader.ReadSingle();
                transform[6] = reader.ReadSingle();
                transform[7] = 0;

                transform[8] = reader.ReadSingle();
                transform[9] = reader.ReadSingle();
                transform[10] = reader.ReadSingle();
                transform[11] = 0;
                 
                transform[12] = reader.ReadSingle();
                transform[13] = reader.ReadSingle();
                transform[14] = reader.ReadSingle();
                transform[15] = 1.0f;

                mesh.TransformMatrix = new Gk3Main.Math.Matrix(transform);

                mesh.numSections = reader.ReadUInt32();

                Math.Vector3 bbMin, bbMax;
                bbMin.X = reader.ReadSingle();
                bbMin.Y = reader.ReadSingle();
                bbMin.Z = reader.ReadSingle();
                bbMax.X = reader.ReadSingle();
                bbMax.Y = reader.ReadSingle();
                bbMax.Z = reader.ReadSingle();

                Math.Vector3 transformedBBMin = mesh.TransformMatrix * bbMin;
                Math.Vector3 transformedBBMax = mesh.TransformMatrix * bbMax; 

                // make sure the AABB is still min < max, since the transformation
                // may have changed stuff
                mesh.boundingBox = new float[]
                    {
                        System.Math.Min(bbMin.X, bbMax.X),
                        System.Math.Min(bbMin.Y, bbMax.Y),
                        System.Math.Min(bbMin.Z, bbMax.Z),
                        System.Math.Max(bbMin.X, bbMax.X),
                        System.Math.Max(bbMin.Y, bbMax.Y),
                        System.Math.Max(bbMin.Z, bbMax.Z)
                    };

                mesh.TransformedBoundingBox = new float[]
                {
                    System.Math.Min(transformedBBMin.X, transformedBBMax.X),
                    System.Math.Min(transformedBBMin.Y, transformedBBMax.Y),
                    System.Math.Min(transformedBBMin.Z, transformedBBMax.Z),
                    System.Math.Max(transformedBBMin.X, transformedBBMax.X),
                    System.Math.Max(transformedBBMin.Y, transformedBBMax.Y),
                    System.Math.Max(transformedBBMin.Z, transformedBBMax.Z)
                };


                mesh.sections = new ModMeshSection[mesh.numSections];
                for (int j = 0; j < mesh.numSections; j++)
		        {
                    ModMeshSection meshSection = new ModMeshSection();
 

                    meshSection.heading = reader.ReadUInt32();
			        // if we didn't find it then we're screwed
			        if (meshSection.heading != 0x4D475250)
			        {
				        throw new Resource.InvalidResourceFileFormat("Not a valid model file! Unable to find valid mesh section header!");
			        }

                    meshSection.texture = Gk3Main.Utils.ConvertAsciiToString(reader.ReadBytes(32));
                    meshSection.textureResource = (TextureResource)Resource.ResourceManager.Load(meshSection.texture + ".BMP");
                    meshSection.color = reader.ReadUInt32();
                    meshSection.smooth = reader.ReadUInt32() != 0;
                    meshSection.numVerts = reader.ReadUInt32();
                    meshSection.numTriangles = reader.ReadUInt32();
                    meshSection.numLODs = reader.ReadUInt32();
                    meshSection.axes = reader.ReadUInt32() != 0;

                    // read the vertices
                    const int vertexStride = 3 + 3 + 2;
                    meshSection.vertices = new float[meshSection.numVerts * vertexStride];
                    Math.Vector3 dummy = new Gk3Main.Math.Vector3();
                    for (uint k = 0; k < meshSection.numVerts; k++)
                    {
                        dummy.X = reader.ReadSingle();
                        dummy.Y = reader.ReadSingle();
                        dummy.Z = reader.ReadSingle();

                        meshSection.vertices[k * vertexStride + 0] = dummy.X;
                        meshSection.vertices[k * vertexStride + 1] = dummy.Y;
                        meshSection.vertices[k * vertexStride + 2] = dummy.Z;
                    }

                    // read the normals
                    meshSection.normals = new float[meshSection.numVerts * 3];
                    for (uint k = 0; k < meshSection.numVerts; k++)
                    {
                        meshSection.vertices[k * vertexStride + 5] = reader.ReadSingle();
                        meshSection.vertices[k * vertexStride + 6] = reader.ReadSingle();
                        meshSection.vertices[k * vertexStride + 7] = reader.ReadSingle();
                    }

                    // read the tex coords
                    meshSection.texCoords = new float[meshSection.numVerts * 2];
                    for (uint k = 0; k < meshSection.numVerts; k++)
                    {
                        meshSection.vertices[k * vertexStride + 3] = reader.ReadSingle();
                        meshSection.vertices[k * vertexStride + 4] = reader.ReadSingle();
                    }

                    // read the indices
                    meshSection.indices = new int[meshSection.numTriangles * 3];
                    for (uint k = 0; k < meshSection.numTriangles; k++)
                    {
                        meshSection.indices[k * 3 + 0] = reader.ReadUInt16();
                        meshSection.indices[k * 3 + 1] = reader.ReadUInt16();
                        meshSection.indices[k * 3 + 2] = reader.ReadUInt16();
                        reader.ReadUInt16();
                    }

                    mesh.sections[j] = meshSection;


                    // read the LODK sections
                    for (uint k = 0; k < meshSection.numLODs; k++)
                    {
                        ModMeshLod lod = new ModMeshLod();
                        lod.Magic = reader.ReadUInt32();
                        lod.Heading = reader.ReadUInt32();
                        lod.Heading2 = reader.ReadUInt32();
                        lod.Heading3 = reader.ReadUInt32();

                        if (lod.Magic != 0x4C4F444B)
                            throw new Resource.InvalidResourceFileFormat("Unable to find LODK section");

                        // read each section
                        lod.Section1 = new ushort[lod.Heading * 4];
                        lod.Section2 = new ushort[lod.Heading2 * 2];
                        lod.Section3 = new ushort[lod.Heading3];
                        for (uint l = 0; l < lod.Heading; l++)
                        {
                            lod.Section1[l * 4 + 0] = reader.ReadUInt16();
                            lod.Section1[l * 4 + 1] = reader.ReadUInt16();
                            lod.Section1[l * 4 + 2] = reader.ReadUInt16();
                            lod.Section1[l * 4 + 3] = reader.ReadUInt16();
                        }

                        for (uint l = 0; l < lod.Heading2; l++)
                        {
                            lod.Section2[l * 2 + 0] = reader.ReadUInt16();
                            lod.Section2[l * 2 + 1] = reader.ReadUInt16();
                        }

                        for (uint l = 0; l < lod.Heading3; l++)
                        {
                            lod.Section3[l] = reader.ReadUInt16();
                        }
                    }
                }

                _meshes[i] = mesh;
            }

            // read the MODX stuff
            uint modxMagic = reader.ReadUInt32();
            if (modxMagic == 0x4d4f4458)
            {
                for (int i = 0; i < _meshes.Length; i++)
                {
                    for (int j = 0; j < _meshes[i].numSections; j++)
                    {
                        uint grpxMagic = reader.ReadUInt32();
                        uint numVertices = reader.ReadUInt32();

                        if (grpxMagic == 0x47525058)
                        {
                            for (int k = 0; k < numVertices; k++)
                            {
                                byte b = reader.ReadByte();
                                
                                for (int l = 0; l < b; l++)
                                {
                                    byte meshIndex = reader.ReadByte();
                                    byte groupIndex = reader.ReadByte();
                                    ushort polyIndex = reader.ReadUInt16();
                                }
                            }
                        }
                    }
                }
            }

            _effect = (Effect)Resource.ResourceManager.Load("basic_textured.fx");
        }

        public void Render(Camera camera)
        {
            if (_loaded == true)
            {
                if (!_isBillboard)
                {
                    
                    Gl.glEnable(Gl.GL_TEXTURE_2D);

                    foreach (ModMesh mesh in _meshes)
                    {
                        _effect.SetParameter("ModelViewProjection", mesh.TransformMatrix * camera.ViewProjection);
                        _effect.Begin();
                        _effect.BeginPass(0);

                        foreach (ModMeshSection section in mesh.sections)
                        {
                            section.textureResource.Bind();

                            RendererManager.CurrentRenderer.RenderIndices(_elements, PrimitiveType.Triangles, 0, section.indices.Length, section.indices, section.vertices);
                        }

                        _effect.EndPass();
                        _effect.End();
                    }
                }
                else
                {
                    // render the model as a billboard
                    for (int i = 0; i < _meshes.Length; i++)
                    {
                        Math.Vector3 meshPosition;
                        if (_useBillboardCenter)
                        {
                            meshPosition = _billboardCenter;
                        }
                        else
                        {
                            meshPosition = _meshes[i].CalcBoundingBoxCenter();
                        }
                        
                        Math.Matrix billboardMatrix;
                        camera.CreateBillboardMatrix(meshPosition, true, out billboardMatrix);

                        _effect.SetParameter("ModelViewProjection",  billboardMatrix * _meshes[i].TransformMatrix * camera.ViewProjection);
                        _effect.Begin();
                        _effect.BeginPass(0);

                        foreach (ModMeshSection section in _meshes[i].sections)
                        {
                            section.textureResource.Bind();

                            RendererManager.CurrentRenderer.RenderIndices(_elements, PrimitiveType.Triangles, 0, section.indices.Length, section.indices, section.vertices);
                        }

                        _effect.EndPass();
                        _effect.End();
                    }
                }

                foreach (ModMesh mesh in _meshes)
                {
                    BoundingBoxRenderer.Render(camera, Math.Vector3.Zero, mesh.TransformedBoundingBox);
                }
            }
        }

        public void RenderAt(Math.Vector3 position, float angle, Camera camera)
        {
            if (_loaded == true)
            {
                Math.Matrix world = Math.Matrix.RotateY(angle)
                    * Math.Matrix.Translate(position);

                Gl.glEnable(Gl.GL_TEXTURE_2D);

                foreach (ModMesh mesh in _meshes)
                {
                    Math.Matrix worldview = mesh.TransformMatrix * world * camera.ViewProjection;

                    foreach (ModMeshSection section in mesh.sections)
                    {
                        _effect.SetParameter("ModelViewProjection", worldview);
                        _effect.Begin();
                        _effect.BeginPass(0);

                        section.textureResource.Bind();

                        RendererManager.CurrentRenderer.RenderIndices(_elements, PrimitiveType.Triangles, 0, section.indices.Length, section.indices, section.vertices);
                    
                        _effect.EndPass();
                        _effect.End();
                    }
                }

                

                foreach (ModMesh mesh in _meshes)
                {
                    BoundingBoxRenderer.Render(camera, position, mesh.TransformedBoundingBox);
                }
            }
        }

        public bool CollideRay(Math.Vector3 modelPosition, Math.Vector3 origin, Math.Vector3 direction, float length, out float distance)
        {
            foreach (ModMesh mesh in _meshes)
            {
                if (Gk3Main.Utils.TestRayAABBCollision(modelPosition, origin, direction, mesh.TransformedBoundingBox, out distance))
                    return true;
            }

            distance = float.MinValue;
            return false;
        }

        public override void Dispose()
        {
            // nothing
        }

        private ModMesh[] _meshes;
        private Effect _effect;
        private bool _isBillboard;
        private bool _useBillboardCenter;
        private Math.Vector3 _billboardCenter;
        private static VertexElementSet _elements;
    }

    public class ModelResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            try
            {
                System.IO.Stream stream = FileSystem.Open(name);

                ModelResource resource = new ModelResource(name, stream);

                stream.Close();

                return resource;
            }
            catch (System.IO.FileNotFoundException)
            {
                Logger.WriteError("Unable to find model: {0}", name);

                return new ModelResource(name);
            }
        }

        public string[] SupportedExtensions { get { return m_supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return true; } }

        private static string[] m_supportedExtensions = new string[] { "MOD" };
    }
}
