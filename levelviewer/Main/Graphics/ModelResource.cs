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
        public float unknown2;
        public uint unknown3;
    }

    struct ModMeshSection
    {
        public uint heading;
        public string texture;
        public TextureResource textureResource;

        public uint unknown1;
        public uint numFaces;
        public uint numVerts;
        public uint numTriangles;
        public uint numLODs;
        public uint unknown2;

        public float[] vertices;
        public float[] normals;
        public float[] texCoords;
        public int[] indices;
    }

    struct ModMesh
    {
        public uint heading;
        public float[] transform;
        public uint numSections;
        public float[] boundingBox;

        public ModMeshSection[] sections;
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
            header.unknown2 = reader.ReadUInt32();
            header.unknown3 = reader.ReadUInt32();

            if (header.minorVersion == 9 && header.majorVersion == 1)
            {
                // read through the "extension" header,
                // which is completely unknown right now
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
            }

            // read the meshes
            _meshes = new ModMesh[header.numMeshes];
            for (uint i = 0; i < header.numMeshes; i++)
            {
                ModMesh mesh = new ModMesh();

                // it's dirty hack time! I don't know the format of the LODK sections, so I can't
                // accurately calculate how to skip it, so I've got to basically do a search for
                // the next MESH section.

                while (reader.PeekChar() != -1)
                {
                    mesh.heading = reader.ReadUInt32();
                    if (mesh.heading == 0x4D455348)
                    {
                        break;
                    }

                    // back up 3 bytes and continue
                    reader.BaseStream.Seek(-3, System.IO.SeekOrigin.Current);
                }

                // if we didn't find it then we're screwed
                if (mesh.heading != 0x4D455348)
                {
                    throw new Resource.InvalidResourceFileFormat("Not a valid model file! Unable to find MESH section!");
                }

                mesh.transform = new float[16];
                mesh.transform[0] = reader.ReadSingle();
                mesh.transform[1] = reader.ReadSingle();
                mesh.transform[2] = reader.ReadSingle();
                mesh.transform[3] = 0;

                mesh.transform[4] = reader.ReadSingle();
                mesh.transform[5] = reader.ReadSingle();
                mesh.transform[6] = reader.ReadSingle();
                mesh.transform[7] = 0;

                mesh.transform[8] = reader.ReadSingle();
                mesh.transform[9] = reader.ReadSingle();
                mesh.transform[10] = reader.ReadSingle();
                mesh.transform[11] = 0;
                 
                mesh.transform[12] = reader.ReadSingle();
                mesh.transform[13] = reader.ReadSingle();
                mesh.transform[14] = reader.ReadSingle();
                mesh.transform[15] = 1.0f;

                Math.Matrix transform = new Gk3Main.Math.Matrix(mesh.transform);

                mesh.numSections = reader.ReadUInt32();

                Math.Vector3 bbMin, bbMax;
                bbMin.X = reader.ReadSingle();
                bbMin.Y = reader.ReadSingle();
                bbMin.Z = reader.ReadSingle();
                bbMax.X = reader.ReadSingle();
                bbMax.Y = reader.ReadSingle();
                bbMax.Z = reader.ReadSingle();

                bbMin = transform * bbMin;
                bbMax = transform * bbMax;

                // set the transformed bounding box back.
                // make sure the AABB is still min < max, since the transformation
                // may have changed stuff
                mesh.boundingBox = new float[]
                    {
                        System.Math.Min(bbMin.Z, bbMax.Z),
                        System.Math.Min(bbMin.Y, bbMax.Y),
                        System.Math.Min(bbMin.X, bbMax.X),
                        System.Math.Max(bbMin.Z, bbMax.Z),
                        System.Math.Max(bbMin.Y, bbMax.Y),
                        System.Math.Max(bbMin.X, bbMax.X)
                    };


                mesh.sections = new ModMeshSection[mesh.numSections];
                for (int j = 0; j < mesh.numSections; j++)
		        {
                    ModMeshSection meshSection = new ModMeshSection();
        			
			        while(reader.PeekChar() != -1)
			        {
                        meshSection.heading = reader.ReadUInt32();
        				
				        if (meshSection.heading == 0x4D475250)
				        {
					        break;
				        }
        				
				        // back up 3 bytes and continue
				        reader.BaseStream.Seek(-3, System.IO.SeekOrigin.Current);
			        }
        			
			        // if we didn't find it then we're screwed
			        if (meshSection.heading != 0x4D475250)
			        {
				        throw new Resource.InvalidResourceFileFormat("Not a valid model file! Unable to find valid mesh section header!");
			        }

                    meshSection.texture = Gk3Main.Utils.ConvertAsciiToString(reader.ReadBytes(32));
                    meshSection.textureResource = (TextureResource)Resource.ResourceManager.Load(meshSection.texture + ".BMP");
                    meshSection.unknown1 = reader.ReadUInt32();
                    meshSection.numFaces = reader.ReadUInt32();
                    meshSection.numVerts = reader.ReadUInt32();
                    meshSection.numTriangles = reader.ReadUInt32();
                    meshSection.numLODs = reader.ReadUInt32();
                    meshSection.unknown2 = reader.ReadUInt32();

                    // read the vertices
                    const int vertexStride = 3 + 3 + 2;
                    meshSection.vertices = new float[meshSection.numVerts * vertexStride];
                    Math.Vector3 dummy = new Gk3Main.Math.Vector3();
                    for (uint k = 0; k < meshSection.numVerts; k++)
                    {
                        dummy.X = reader.ReadSingle();
                        dummy.Y = reader.ReadSingle();
                        dummy.Z = reader.ReadSingle();

                        dummy = transform * dummy;

                        meshSection.vertices[k * vertexStride + 0] = dummy.Z;
                        meshSection.vertices[k * vertexStride + 1] = dummy.Y;
                        meshSection.vertices[k * vertexStride + 2] = dummy.X;
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
                }

                _meshes[i] = mesh;
            }

            _effect = (Effect)Resource.ResourceManager.Load("basic_textured.fx");
        }

        public void Render(Camera camera)
        {
            if (_loaded == true)
            {
                _effect.SetParameter("ModelViewProjection", camera.ModelViewProjection);
                _effect.Begin();
                _effect.BeginPass(0);

                Gl.glEnable(Gl.GL_TEXTURE_2D);

                //Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
                //Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

                foreach (ModMesh mesh in _meshes)
                {
                    foreach (ModMeshSection section in mesh.sections)
                    {
                        section.textureResource.Bind();

                        RendererManager.CurrentRenderer.RenderIndices(_elements, PrimitiveType.Triangles, 0, section.indices.Length / 3, section.indices, section.vertices);
                        //Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, section.vertices);
                        //Gl.glNormalPointer(Gl.GL_FLOAT, 0, section.normals);
                        //Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, section.texCoords);

                        //Gl.glDrawElements(Gl.GL_TRIANGLES, section.indices.Length, Gl.GL_UNSIGNED_SHORT, section.indices);
                    }
                }

                //Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                //Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);

                _effect.EndPass();
                _effect.End();

                foreach (ModMesh mesh in _meshes)
                {
                    BoundingBoxRenderer.Render(camera, mesh.boundingBox);
                }
            }
        }

        public bool CollideRay(Math.Vector3 origin, Math.Vector3 direction, float length, out float distance)
        {
            foreach (ModMesh mesh in _meshes)
            {
                if (Gk3Main.Utils.TestRayAABBCollision(origin, direction, mesh.boundingBox, out distance))
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
