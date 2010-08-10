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

    public struct ModMeshSection
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

        // these are used when a model is animated.
        public float[] AnimatedVertices;
        public float[] AnimatedNormals;
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

    public struct ModMesh
    {
        public uint heading;
        public Math.Matrix TransformMatrix;
        public Math.Matrix? AnimatedTransformMatrix;
        public bool AnimatedTransformIsAbsolute;
        public uint numSections;
        public AxisAlignedBoundingBox OriginalBoundingBox;
        public AxisAlignedBoundingBox UpdatedBoundingBox;

        public ModMeshSection[] sections;

        public void SetAABB(AxisAlignedBoundingBox aabb)
        {
            if (AnimatedTransformMatrix.HasValue)
                UpdatedBoundingBox = aabb.Transform(AnimatedTransformMatrix.Value);
            else
                UpdatedBoundingBox = aabb.Transform(TransformMatrix);
        }

        public void SetTransform(Math.Matrix transform)
        {
            AnimatedTransformMatrix = transform;
            UpdatedBoundingBox = OriginalBoundingBox.Transform(transform);
        }
    }

    #endregion

    public class ModelResource : Resource.Resource
    {
        static ModelResource()
        {
            _elements = new VertexElementSet(new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Float3, VertexElementUsage.Position, 0),
                new VertexElement(3 * sizeof(float), VertexElementFormat.Float2, VertexElementUsage.TexCoord, 0),
                new VertexElement(5 * sizeof(float), VertexElementFormat.Float3, VertexElementUsage.Normal, 0)
            });
        }

        public static void LoadGlobalContent(Resource.ResourceManager globalContent)
        {
            _effect = globalContent.Load<Effect>("basic_textured.fx");
        }

        public ModelResource(string name, Resource.ResourceManager content)
            : base(name, false)
        {
            _content = content;
        }

        public ModelResource(string name, System.IO.Stream stream, Resource.ResourceManager content)
            : base(name, true)
        {
            _content = content;

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

                mesh.OriginalBoundingBox = new AxisAlignedBoundingBox(bbMin, bbMax);
                mesh.UpdatedBoundingBox = mesh.OriginalBoundingBox.Transform(mesh.TransformMatrix);

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
                    meshSection.textureResource = _content.Load<TextureResource>(meshSection.texture);
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
        }

        public void Render(Camera camera)
        {
            if (_loaded == true)
            {
                RendererManager.CurrentRenderer.VertexDeclaration = _elements;
                _effect.Bind();
                _effect.Begin();

                RenderBatch(camera);

                _effect.End();

                if (Settings.ShowBoundingBoxes)
                {
                    foreach (ModMesh mesh in _meshes)
                    {
                        mesh.UpdatedBoundingBox.Render(camera, Math.Matrix.Identity);
                    }
                }
            }
        }

        public void RenderAt(Math.Vector3 position, float angle, Camera camera)
        {
            if (_loaded == true)
            {
                RendererManager.CurrentRenderer.VertexDeclaration = _elements;
                _effect.Bind();
                _effect.Begin();

                RenderAtBatch(position, angle, camera);

                _effect.End();

                if (Settings.ShowBoundingBoxes)
                    RenderAABBAt(position, angle, camera);
            }
        }

        /// <summary>
        /// "Batched" version of Render(). This version is much faster, but
        /// it must be called between BeginBatchRender() and EndBatchRender().
        /// </summary>
        public void RenderBatch(Camera camera)
        {
            if (!_isBillboard)
            {
                foreach (ModMesh mesh in _meshes)
                {
                    Math.Matrix worldview;
                    if (mesh.AnimatedTransformMatrix.HasValue)
                        worldview = mesh.AnimatedTransformMatrix.Value * camera.ViewProjection;
                    else
                        worldview = mesh.TransformMatrix * camera.ViewProjection;



                    foreach (ModMeshSection section in mesh.sections)
                    {
                        if (section.textureResource.ContainsAlpha)
                            RendererManager.CurrentRenderer.AlphaTestEnabled = true;

                        _effect.SetParameter("ModelViewProjection", worldview);
                        _effect.SetParameter("Diffuse", section.textureResource, 0);
                        _effect.CommitParams();

                        RendererManager.CurrentRenderer.RenderIndices(PrimitiveType.Triangles, 0,
                            section.vertices.Length / (_elements.Stride / sizeof(float)), section.indices, section.vertices);

                        if (section.textureResource.ContainsAlpha)
                            RendererManager.CurrentRenderer.AlphaTestEnabled = false;
                    }

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
                        meshPosition = _meshes[i].OriginalBoundingBox.Center;
                    }

                    Math.Matrix billboardMatrix;
                    camera.CreateBillboardMatrix(meshPosition, true, out billboardMatrix);

                    foreach (ModMeshSection section in _meshes[i].sections)
                    {
                        if (section.textureResource.ContainsAlpha)
                            RendererManager.CurrentRenderer.AlphaTestEnabled = true;

                        _effect.SetParameter("ModelViewProjection", billboardMatrix * _meshes[i].TransformMatrix * camera.ViewProjection);
                        _effect.SetParameter("Diffuse", section.textureResource, 0);
                        _effect.CommitParams();

                        RendererManager.CurrentRenderer.RenderIndices(PrimitiveType.Triangles, 0,
                            section.vertices.Length / (_elements.Stride / sizeof(float)), section.indices, section.vertices);

                        if (section.textureResource.ContainsAlpha)
                            RendererManager.CurrentRenderer.AlphaTestEnabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// "Batched" version of RenderAt(). This version is much faster, but
        /// it must be called between BeginBatchRender() and EndBatchRender().
        /// </summary>
        public void RenderAtBatch(Math.Vector3 position, float angle, Camera camera)
        {
            Math.Matrix world = Math.Matrix.RotateY(angle)
                    * Math.Matrix.Translate(position);

            foreach (ModMesh mesh in _meshes)
            {
                Math.Matrix worldview;

                if (mesh.AnimatedTransformMatrix.HasValue)
                {
                    if (mesh.AnimatedTransformIsAbsolute)
                        worldview = mesh.AnimatedTransformMatrix.Value * camera.ViewProjection;
                    else
                        worldview = mesh.AnimatedTransformMatrix.Value * world * camera.ViewProjection;
                }
                else
                    worldview = mesh.TransformMatrix * world * camera.ViewProjection;

                _effect.SetParameter("ModelViewProjection", worldview);

                foreach (ModMeshSection section in mesh.sections)
                {
                    if (section.textureResource != null && section.textureResource.ContainsAlpha)
                        RendererManager.CurrentRenderer.AlphaTestEnabled = true;

                    _effect.SetParameter("Diffuse", section.textureResource, 0);
                    _effect.CommitParams();

                    float[] vertices;
                    if (section.AnimatedVertices != null)
                        vertices = section.AnimatedVertices;
                    else
                        vertices = section.vertices;

                    RendererManager.CurrentRenderer.RenderIndices(PrimitiveType.Triangles, 0,
                        vertices.Length / (_elements.Stride / sizeof(float)), section.indices, vertices);

                    if (section.textureResource != null && section.textureResource.ContainsAlpha)
                        RendererManager.CurrentRenderer.AlphaTestEnabled = true;
                }
            }
        }

        public void RenderAABB(Camera camera)
        {
            foreach (ModMesh mesh in _meshes)
            {
                mesh.UpdatedBoundingBox.Render(camera, Math.Matrix.Identity);
            }
        }

        public void RenderAABBAt(Math.Vector3 position, float angle, Camera camera)
        {
            foreach (ModMesh mesh in _meshes)
            {
                if (mesh.AnimatedTransformIsAbsolute)
                    mesh.UpdatedBoundingBox.Render(camera, Math.Matrix.Identity);
                else
                    mesh.UpdatedBoundingBox.Render(camera, Math.Matrix.Translate(position));
            }
        }

        public bool CollideRay(Math.Vector3 modelPosition, Math.Vector3 origin, Math.Vector3 direction, float length, out float distance)
        {
            foreach (ModMesh mesh in _meshes)
            {
                Math.Vector3 position;
                if (mesh.AnimatedTransformIsAbsolute)
                    position = Math.Vector3.Zero;
                else
                    position = modelPosition;

                if (mesh.UpdatedBoundingBox.TestRayAABBCollision(position, origin, direction, out distance))
                    return true;
            }

            distance = float.MinValue;
            return false;
        }

        public void ReplaceTexture(int meshIndex, int groupIndex, string textureName)
        {
            TextureResource texture = _content.Load<TextureResource>(textureName);
            _meshes[meshIndex].sections[groupIndex].textureResource = texture;
        }

        public override void Dispose()
        {
            // nothing
        }

        public ModMesh[] Meshes
        {
            get { return _meshes; }
        }

        /// <summary>
        /// Sets up a "batch" render.
        /// </summary>
        /// <remarks>
        /// Batching models together during rendering is *much* faster than
        /// rendering one at a time. If you can, use these batching render methods.
        /// The more models you can rendering in one batch the more bang for your
        /// buck you can get!
        /// </remarks>
        public static void BeginBatchRender()
        {
            RendererManager.CurrentRenderer.VertexDeclaration = _elements;
            _effect.Bind();
            _effect.Begin();
        }

        public static void EndBatchRender()
        {
            _effect.End();
        }

        private ModMesh[] _meshes;
        
        private bool _isBillboard;
        private bool _useBillboardCenter;
        private Math.Vector3 _billboardCenter;
        private Resource.ResourceManager _content;
        private static Effect _effect;
        private static VertexElementSet _elements;
    }

    public class ModelResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            try
            {
                if (name.IndexOf('.') < 0)
                    name += ".MOD";

                System.IO.Stream stream = FileSystem.Open(name);

                ModelResource resource = new ModelResource(name, stream, content);

                stream.Close();

                return resource;
            }
            catch (System.IO.FileNotFoundException)
            {
                Logger.WriteError("Unable to find model: {0}", name);

                return new ModelResource(name, content);
            }
        }

        public string[] SupportedExtensions { get { return m_supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return true; } }

        private static string[] m_supportedExtensions = new string[] { "MOD" };
    }
}
