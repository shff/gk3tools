using System;
using System.Collections.Generic;

using Tao.OpenGl;
using Tao.Cg;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlUpdatableTexture : UpdatableTexture
    {
        private int _glTexture;

        public GlUpdatableTexture(string name, int width, int height)
            : base(name, width, height)
        {
            if (Gk3Main.Utils.IsPowerOfTwo(width) == false ||
                Gk3Main.Utils.IsPowerOfTwo(height) == false)
                throw new ArgumentException("Width and height must be power-of-two");

            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glGenTextures(1, out _glTexture);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
        }

        public override void Update(byte[] pixels)
        {
            if (pixels.Length != _width * _height * 4)
                throw new ArgumentException("Pixel array is not the expected length");

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, _width, _height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
        }

        public override void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
        }

        public int OpenGlTexture { get { return _glTexture; } }
    }

    public class OpenGLRenderer : IRenderer
    {
        private IntPtr _cgContext;
        private VertexElementSet _vertexDeclaration;
        private TextureResource _defaultTexture;
        private TextureResource _errorTexture;
        private bool _renderToTextureSupported;

        public OpenGLRenderer()
        {
            _cgContext = Cg.cgCreateContext();
            CgGl.cgGLEnableProfile(Cg.CG_PROFILE_ARBVP1);

            CgGl.cgGLRegisterStates(_cgContext);

            Cg.cgGetError();
            cgSetParameterSettingMode(_cgContext, CG_DEFERRED_PARAMETER_SETTING);
            if (Cg.cgGetError() != Cg.CG_NO_ERROR)
                throw new Exception("Oh no!");

            // load extensions
            //_renderToTextureSupported = Gl.IsExtensionSupported("GL_ARB_framebuffer_object");
            _renderToTextureSupported = false;
        }

        public IntPtr CgContext { get { return _cgContext; } }
        public int DefaultCgProfile { get { return Cg.CG_PROFILE_ARBVP1; } }

        #region Render states
        public bool BlendEnabled
        {
            get { return Gl.glIsEnabled(Gl.GL_BLEND) == Gl.GL_TRUE; }
            set { if (value) Gl.glEnable(Gl.GL_BLEND); else Gl.glDisable(Gl.GL_BLEND); }
        }

        public bool AlphaTestEnabled
        {
            get { return Gl.glIsEnabled(Gl.GL_ALPHA_TEST) == Gl.GL_TRUE; }
            set { if (value) Gl.glEnable(Gl.GL_ALPHA_TEST); else Gl.glDisable(Gl.GL_ALPHA_TEST); }
        }

        public bool DepthTestEnabled
        {
            get { return Gl.glIsEnabled(Gl.GL_DEPTH_TEST) == Gl.GL_TRUE; }
            set { if (value) Gl.glEnable(Gl.GL_DEPTH_TEST); else Gl.glDisable(Gl.GL_DEPTH_TEST); }
        }

        public bool DepthWriteEnabled
        {
            get { int enabled; Gl.glGetIntegerv(Gl.GL_DEPTH_WRITEMASK, out enabled); return enabled != 0; }
            set { Gl.glDepthMask(value ? 1 : 0); }
        }

        public CullMode CullMode
        {
            get
            {
                if (Gl.glIsEnabled(Gl.GL_CULL_FACE) == Gl.GL_FALSE)
                    return CullMode.None;

                int param;
                Gl.glGetIntegerv(Gl.GL_FRONT_FACE, out param);

                // OpenGL stores what NOT to cull, so we need to reverse it
                if (param == Gl.GL_CW)
                    return CullMode.CounterClockwise;
                else
                    return CullMode.Clockwise;
            }
            set
            {
                if (value == CullMode.None)
                    Gl.glDisable(Gl.GL_CULL_FACE);
                else
                {
                    Gl.glEnable(Gl.GL_CULL_FACE);

                    if (value == CullMode.Clockwise)
                        Gl.glFrontFace(Gl.GL_CCW);
                    else
                        Gl.glFrontFace(Gl.GL_CW);
                }
            }
        }

        public CompareFunction AlphaTestFunction
        {
            get
            {
                int func;
                Gl.glGetIntegerv(Gl.GL_ALPHA_TEST_FUNC, out func);

                if (func == Gl.GL_ALWAYS)
                    return CompareFunction.Always;
                else if (func == Gl.GL_NEVER)
                    return CompareFunction.Never;
                else if (func == Gl.GL_EQUAL)
                    return CompareFunction.Equal;
                else if (func == Gl.GL_NOTEQUAL)
                    return CompareFunction.NotEqual;
                else if (func == Gl.GL_GREATER)
                    return CompareFunction.Greater;
                else if (func == Gl.GL_LESS)
                    return CompareFunction.Less;
                else if (func == Gl.GL_LEQUAL)
                    return CompareFunction.LessOrEqual;
                else if (func == Gl.GL_GEQUAL)
                    return CompareFunction.GreaterOrEqual;

                throw new NotImplementedException("Unknown OpenGL alpha test function");
            }
            set
            {
                if (value == CompareFunction.Always)
                    Gl.glAlphaFunc(Gl.GL_ALWAYS, AlphaTestReference);
                else if (value == CompareFunction.Greater)
                    Gl.glAlphaFunc(Gl.GL_GREATER, AlphaTestReference);
                else if (value == CompareFunction.GreaterOrEqual)
                    Gl.glAlphaFunc(Gl.GL_GEQUAL, AlphaTestReference);
            }
        }

        public float AlphaTestReference
        {
            get
            {
                float alphaRef;
                Gl.glGetFloatv(Gl.GL_ALPHA_TEST_REF, out alphaRef);

                return alphaRef;
            }
            set
            {
                int func;
                Gl.glGetIntegerv(Gl.GL_ALPHA_TEST_FUNC, out func);

                Gl.glAlphaFunc(func, value);
            }
        }

        #endregion Render states

        public Viewport Viewport
        {
            get
            {
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

                Viewport v = new Viewport();
                v.X = viewport[0];
                v.Y = viewport[1];
                v.Width = viewport[2];
                v.Height = viewport[3];

                return v;
            }
            set
            {
                Gl.glViewport(value.X, value.Y, value.Width, value.Height);
            }
        }

        #region Texture creation
        public TextureResource CreateTexture(string name, System.IO.Stream stream)
        {
            return new GlTexture(name, stream);
        }

        public TextureResource CreateTexture(string name, System.IO.Stream stream, bool clamp)
        {
            return new GlTexture(name, stream, clamp);
        }

        public TextureResource CreateTexture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream)
        {
            return new GlTexture(name, colorStream, alphaStream);
        }

        public UpdatableTexture CreateUpdatableTexture(string name, int width, int height)
        {
            return new GlUpdatableTexture(name, width, height);
        }
        #endregion Texture creation

        public CubeMapResource CreateCubeMap(string name, string front, string back, string left, string right,
            string up, string down)
        {
            return new GlCubeMap(name, front, back, left, right, up, down);
        }

        public Effect CreateEffect(string name, System.IO.Stream stream)
        {
            return new OpenGl.GlslEffect(name, stream);
            //return new CgEffect(name, stream, _cgContext);
        }

        public VertexBuffer CreateVertexBuffer(float[] data, int stride)
        {
            return new GlVertexBuffer(data, stride);
        }

        public IndexBuffer CreateIndexBuffer(uint[] data)
        {
            return new GlIndexBuffer(data);
        }

        public TextureResource DefaultTexture
        {
            get
            {
                if (_defaultTexture == null)
                    _defaultTexture = new GlTexture(true);

                return _defaultTexture;
            }
        }

        public TextureResource ErrorTexture
        {
            get
            {
                if (_errorTexture == null)
                    _errorTexture = new GlTexture(false);

                return _errorTexture;
            }
        }

        public VertexElementSet VertexDeclaration
        {
            set
            {
                _vertexDeclaration = value;
            }
        }

        public void SetBlendFunctions(BlendMode source, BlendMode destination)
        {
            int glSource = Gl.GL_ZERO, glDest = Gl.GL_ZERO;

            switch (source)
            {
                case BlendMode.Zero:
                    glSource = Gl.GL_ZERO;
                    break;
                case BlendMode.One:
                    glSource = Gl.GL_ONE;
                    break;
                case BlendMode.SourceAlpha:
                    glSource = Gl.GL_SRC_ALPHA;
                    break;
                case BlendMode.InverseSourceAlpha:
                    glSource = Gl.GL_ONE_MINUS_SRC_ALPHA;
                    break;
                case BlendMode.DestinationAlpha:
                    glSource = Gl.GL_DST_ALPHA;
                    break;
                case BlendMode.InverseDestinationAlpha:
                    glSource = Gl.GL_ONE_MINUS_DST_ALPHA;
                    break;
            }

            switch (destination)
            {
                case BlendMode.Zero:
                    glDest = Gl.GL_ZERO;
                    break;
                case BlendMode.One:
                    glDest = Gl.GL_ONE;
                    break;
                case BlendMode.SourceAlpha:
                    glDest = Gl.GL_SRC_ALPHA;
                    break;
                case BlendMode.InverseSourceAlpha:
                    glDest = Gl.GL_ONE_MINUS_SRC_ALPHA;
                    break;
                case BlendMode.DestinationAlpha:
                    glDest = Gl.GL_DST_ALPHA;
                    break;
                case BlendMode.InverseDestinationAlpha:
                    glDest = Gl.GL_ONE_MINUS_DST_ALPHA;
                    break;
            }

            Gl.glBlendFunc(glSource, glDest);
        }

        public void RenderBuffers(VertexBuffer vertices, IndexBuffer indices)
        {
            GlVertexBuffer glVertices = (GlVertexBuffer)vertices;
            GlIndexBuffer glIndices = (GlIndexBuffer)indices;

            glVertices.Bind();
            glIndices.Bind();

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, 0, 0, IntPtr.Zero);

            Gl.glDrawElements(Gl.GL_TRIANGLES, indices.Length, Gl.GL_UNSIGNED_INT, null);

            Gl.glDisableVertexAttribArray(0);

            glVertices.Unbind();
            glIndices.Unbind();
        }

        public void RenderPrimitives(PrimitiveType type, int startIndex, int count, float[] vertices)
        {
            int glType;

            if (type == PrimitiveType.LineStrip)
            {
                glType = Gl.GL_LINE_STRIP;
            }
            else
            {
                glType = Gl.GL_TRIANGLES;
            }

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, vertices);

            Gl.glDrawArrays(glType, startIndex, count);
        }

        public void RenderIndices(PrimitiveType type, int startIndex, int primitiveCount, int[] indices, float[] vertices)
        {
            unsafe
            {
                System.Runtime.InteropServices.GCHandle ptrptr=
                    System.Runtime.InteropServices.GCHandle.Alloc(vertices,
                    System.Runtime.InteropServices.GCHandleType.Pinned);

                IntPtr verticesptr = ptrptr.AddrOfPinnedObject();

                try
                {
                    for (int i = 0; i < _vertexDeclaration.Elements.Length; i++)
                    {
                        Gl.glEnableVertexAttribArray(i);
                        Gl.glVertexAttribPointer(i, (int)_vertexDeclaration.Elements[i].Format, Gl.GL_FLOAT, 0, _vertexDeclaration.Stride,
                            Gk3Main.Utils.IncrementIntPtr(verticesptr, _vertexDeclaration.Elements[i].Offset));
                    }

                    /*foreach (VertexElement element in _vertexDeclaration.Elements)
                    {
                        int numBytesBetween = _vertexDeclaration.Stride;// elements.Stride - (int)element.Format * sizeof(float);
                        if (element.Usage == VertexElementUsage.Position)
                        {
                            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
                            Gl.glVertexPointer((int)element.Format, Gl.GL_FLOAT, numBytesBetween,
                                Gk3Main.Utils.IncrementIntPtr(verticesptr, element.Offset));
                        }
                        else if (element.Usage == VertexElementUsage.TexCoord)
                        {
                            Gl.glClientActiveTexture(Gl.GL_TEXTURE0 + element.UsageIndex);
                            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                            Gl.glTexCoordPointer((int)element.Format, Gl.GL_FLOAT, numBytesBetween,
                                Gk3Main.Utils.IncrementIntPtr(verticesptr, element.Offset));
                        }
                        else if (element.Usage == VertexElementUsage.Normal)
                        {
                            Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);
                            Gl.glNormalPointer(Gl.GL_FLOAT, numBytesBetween, Gk3Main.Utils.IncrementIntPtr(verticesptr, element.Offset));
                        }
                        else if (element.Usage == VertexElementUsage.Color)
                            Gl.glColorPointer((int)element.Format, Gl.GL_FLOAT, numBytesBetween, vertices[element.Offset / sizeof(float)]);
                    }*/
                }
                finally
                {
                    ptrptr.Free();
                }
            }

            int glType, totalIndices;
            if (type == PrimitiveType.Triangles)
            {
                glType = Gl.GL_TRIANGLES;
                totalIndices = primitiveCount * 3;
            }
            else
            {
                glType = Gl.GL_POINT;
                totalIndices = primitiveCount;
            }

            unsafe
            {
                System.Runtime.InteropServices.GCHandle ptrptr =
                   System.Runtime.InteropServices.GCHandle.Alloc(indices,
                   System.Runtime.InteropServices.GCHandleType.Pinned);

                IntPtr indicesptr = ptrptr.AddrOfPinnedObject();

                try
                {
                    Gl.glDrawElements(glType, totalIndices, Gl.GL_UNSIGNED_INT, 
                        Gk3Main.Utils.IncrementIntPtr(indicesptr, startIndex * sizeof(int)));
                }
                finally
                {
                    ptrptr.Free();
                }
            }
            
            /*
            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
            for (int i = 2; i >= 0; i--)
            {
                Gl.glClientActiveTexture(Gl.GL_TEXTURE0 + i);
                Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            }
            Gl.glDisableClientState(Gl.GL_NORMAL_ARRAY);*/

            for (int i = 0; i < _vertexDeclaration.Elements.Length; i++)
            {
                Gl.glDisableVertexAttribArray(i);
            }
        }

        public void BeginScene()
        {
            // nothing
        }

        public void EndScene()
        {
            // nothing
        }

        public void Present()
        {
            // nothing
        }

        public void Clear()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
        }

        public RenderTarget CreateRenderTarget(int width, int height)
        {
            if (_renderToTextureSupported == false)
                throw new NotSupportedException();

            return new GlRenderTarget(width, height);
        }

        public void SetRenderTarget(RenderTarget renderTarget)
        {
            if (_renderToTextureSupported == false)
                throw new NotSupportedException();

            if (renderTarget == null)
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);
            else
            {
                GlRenderTarget rt = (GlRenderTarget)renderTarget;
                rt.Bind();
            }
        }

        public bool RenderToTextureSupported { get { return _renderToTextureSupported; } }

        public string ShaderFilenameSuffix
        {
            get { return ".glsl"; }
        }


        const int CG_IMMEDIATE_PARAMETER_SETTING = 4132;
        const int CG_DEFERRED_PARAMETER_SETTING = 4133;

        // TODO: hopefully Tao will expose this method someday.
        // When it does we can remove this.
        [System.Runtime.InteropServices.DllImport("cg")]
        private static extern void cgSetParameterSettingMode(IntPtr context, int value);
    }
}
