using System;
using System.Collections.Generic;

using Tao.OpenGl;

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
        private RenderWindow _parentWindow;
        private VertexElementSet _vertexDeclaration;
        private GlVertexBuffer _currentVertexBuffer;
        private TextureResource _defaultTexture;
        private TextureResource _errorTexture;
        private bool _renderToTextureSupported;
        private BlendState _currentBlendState;
        private SamplerStateCollection _currentSamplerStates = new SamplerStateCollection();

        public OpenGLRenderer(RenderWindow parentWindow)
        {
            // load extensions
            //_renderToTextureSupported = Gl.IsExtensionSupported("GL_ARB_framebuffer_object");
            _renderToTextureSupported = false;

            // set default render states
            BlendState = BlendState.Opaque;

            _currentSamplerStates.SamplerChanged += new SamplerStateCollection.SamplerChangedHandler(samplerStateChanged);
            SamplerStates[0] = SamplerState.LinearWrap;
            SamplerStates[1] = SamplerState.LinearClamp;

            _parentWindow = parentWindow;
        }

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

        public BlendState BlendState
        {
            get { return _currentBlendState; }
            set
            {
                _currentBlendState = value;

                // TODO: only set the values that have changed
                int colorSrc = convertBlendMode(value.ColorSourceBlend);
                int colorDest = convertBlendMode(value.ColorDestinationBlend);
                int alphaSrc = convertBlendMode(value.AlphaSourceBlend);
                int alphaDest = convertBlendMode(value.AlphaDestinationBlend);

                Gl.glBlendFuncSeparate(colorSrc, colorDest, alphaSrc, alphaDest);

                Gl.glBlendEquationSeparate(convertBlendFunc(value.ColorBlendFunction),
                    convertBlendFunc(value.AlphaBlendFunction));
            }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return _currentSamplerStates; }
        }

        private void samplerStateChanged(SamplerState newSampler, SamplerState oldSampler, int index)
        {
            // TODO: only modify the changed states
            Gl.glActiveTexture(Gl.GL_TEXTURE0 + index);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, convertTextureAddressMode(newSampler.AddressU));
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, convertTextureAddressMode(newSampler.AddressV));
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_R, convertTextureAddressMode(newSampler.AddressW));
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

        public VertexBuffer CreateVertexBuffer<T>(T[] data, int numVertices, VertexElementSet declaration) where T : struct
        {
            return GlVertexBuffer.CreateBuffer(data, numVertices, declaration);
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

        public void SetVertexBuffer(VertexBuffer buffer)
        {
            _vertexDeclaration = buffer.VertexElements;
            
            GlVertexBuffer glVertices = (GlVertexBuffer)buffer;

            glVertices.Bind();
            for (int i = 0; i < _vertexDeclaration.Elements.Length; i++)
            {
                Gl.glEnableVertexAttribArray(i);
                Gl.glVertexAttribPointer(i, (int)_vertexDeclaration.Elements[i].Format, Gl.GL_FLOAT, 0, _vertexDeclaration.Stride,
                    Gk3Main.Utils.IncrementIntPtr(IntPtr.Zero, _vertexDeclaration.Elements[i].Offset));
            }

            // disable the rest
            // TODO: we need to figure out what the maximum number of vertex elements is and use that!
            for (int i = _vertexDeclaration.Elements.Length; i < 12; i++)
            {
                Gl.glDisableVertexAttribArray(i);
            }

            _currentVertexBuffer = glVertices;
        }

        public void RenderBuffers(VertexBuffer vertices, IndexBuffer indices)
        {
            if (_currentVertexBuffer != null)
            {
                _currentVertexBuffer.Unbind();
                _currentVertexBuffer = null;
            }

            GlVertexBuffer glVertices = (GlVertexBuffer)vertices;

            glVertices.Bind();
            for (int i = 0; i < _vertexDeclaration.Elements.Length; i++)
            {
                Gl.glEnableVertexAttribArray(i);
                Gl.glVertexAttribPointer(i, (int)_vertexDeclaration.Elements[i].Format, Gl.GL_FLOAT, 0, _vertexDeclaration.Stride,
                    Gk3Main.Utils.IncrementIntPtr(IntPtr.Zero, _vertexDeclaration.Elements[i].Offset));
            }


            GlIndexBuffer glIndices = (GlIndexBuffer)indices;
            if (glIndices != null) glIndices.Bind();

            if (glIndices != null)
                Gl.glDrawElements(Gl.GL_TRIANGLES, indices.Length, Gl.GL_UNSIGNED_INT, null);
            else
                Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, vertices.Length);

            for (int i = 0; i < _vertexDeclaration.Elements.Length; i++)
            {
                Gl.glDisableVertexAttribArray(i);
            }

            glVertices.Unbind();

            if (glIndices != null) glIndices.Unbind();
        }

        public void RenderBuffers(int firstVertex, int vertexCount)
        {
            Gl.glDrawArrays(Gl.GL_TRIANGLES, firstVertex, vertexCount);
        }

        public void RenderPrimitives<T>(PrimitiveType type, int startIndex, int vertexCount, T[] vertices) where T: struct
        {
            if (_currentVertexBuffer != null)
            {
                _currentVertexBuffer.Unbind();
                _currentVertexBuffer = null;
            }

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
                }
                finally
                {
                    ptrptr.Free();
                }
            }

            int glType;
            if (type == PrimitiveType.LineStrip)
            {
                glType = Gl.GL_LINE_STRIP;
            }
            else
            {
                glType = Gl.GL_TRIANGLES;
            }

            Gl.glDrawArrays(glType, startIndex, vertexCount);

            for (int i = 0; i < _vertexDeclaration.Elements.Length; i++)
            {
                Gl.glDisableVertexAttribArray(i);
            }
        }

        public void RenderIndices<T>(PrimitiveType type, int startIndex, int vertexCount, int[] indices, T[] vertices) where T: struct
        {
            if (_currentVertexBuffer != null)
            {
                _currentVertexBuffer.Unbind();
                _currentVertexBuffer = null;
            }

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
                }
                finally
                {
                    ptrptr.Free();
                }
            }

            int glType;
            if (type == PrimitiveType.Triangles)
            {
                glType = Gl.GL_TRIANGLES;
            }
            else if (type == PrimitiveType.Lines)
            {
                glType = Gl.GL_LINES;
            }
            else
            {
                glType = Gl.GL_POINT;
            }

            unsafe
            {
                System.Runtime.InteropServices.GCHandle ptrptr =
                   System.Runtime.InteropServices.GCHandle.Alloc(indices,
                   System.Runtime.InteropServices.GCHandleType.Pinned);

                IntPtr indicesptr = ptrptr.AddrOfPinnedObject();

                try
                {
                    Gl.glDrawElements(glType, indices.Length - startIndex, Gl.GL_UNSIGNED_INT, 
                        Gk3Main.Utils.IncrementIntPtr(indicesptr, startIndex * sizeof(int)));
                }
                finally
                {
                    ptrptr.Free();
                }
            }

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

        public ZClipMode ZClipMode
        {
            get { return ZClipMode.NegativeOne; }
        }

        public string ShaderFilenameSuffix
        {
            get { return ".glsl"; }
        }

        public RenderWindow ParentWindow
        {
            get { return _parentWindow; }
        }

        private static int convertBlendMode(BlendMode mode)
        {
            if (mode == BlendMode.One)
                return Gl.GL_ONE;
            if (mode == BlendMode.Zero)
                return Gl.GL_ZERO;
            if (mode == BlendMode.SourceAlpha)
                return Gl.GL_SRC_ALPHA;
            if (mode == BlendMode.DestinationAlpha)
                return Gl.GL_DST_ALPHA;
            if (mode == BlendMode.InverseSourceAlpha)
                return Gl.GL_ONE_MINUS_SRC_ALPHA;
            if (mode == BlendMode.InverseDestinationAlpha)
                return Gl.GL_ONE_MINUS_DST_ALPHA;

            return Gl.GL_ZERO;
        }

        private static int convertBlendFunc(BlendFunction func)
        {
            if (func == BlendFunction.Add)
                return Gl.GL_FUNC_ADD;
            else if (func == BlendFunction.Subtract)
                return Gl.GL_FUNC_SUBTRACT;
            else if (func == BlendFunction.ReverseSubtract)
                return Gl.GL_FUNC_REVERSE_SUBTRACT;
            else if (func == BlendFunction.Min)
                return Gl.GL_MIN;
            else
                return Gl.GL_MAX;
        }

        private static int convertTextureAddressMode(TextureAddressMode mode)
        {
            if (mode == TextureAddressMode.Clamp)
                return Gl.GL_CLAMP_TO_EDGE;
            else if (mode == TextureAddressMode.Mirror)
                return Gl.GL_MIRRORED_REPEAT;
            else
                return Gl.GL_REPEAT;
        }
    }
}
