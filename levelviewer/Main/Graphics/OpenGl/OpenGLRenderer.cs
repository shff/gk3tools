using System;
using System.Collections.Generic;

using Tao.OpenGl;


namespace Gk3Main.Graphics.OpenGl
{
    public class OpenGLRenderer : IRenderer
    {
        private RenderWindow _parentWindow;
        private VertexElementSet _vertexDeclaration;
        private GlVertexBuffer _currentVertexBuffer;
        private GlIndexBuffer _currentIndexBuffer;
        private GlslEffect _currentEffect;
        private TextureResource _defaultTexture;
        private TextureResource _errorTexture;
        private bool _renderToTextureSupported;
        private bool _samplerObjectsSupported;
        private BlendState _currentBlendState;
        private SamplerStateCollection _currentSamplerStates = new SamplerStateCollection();
        private bool _vertexPointersNeedSetup = true;

        internal bool VertexPointersNeedSetup
        {
            get { return _vertexPointersNeedSetup; }
            set { _vertexPointersNeedSetup = value; }
        }

        internal GlslEffect CurrentEffect
        {
            get { return _currentEffect; }
            set { _currentEffect = value; }
        }

        public OpenGLRenderer(RenderWindow parentWindow)
        {
            // load extensions
            //_renderToTextureSupported = Gl.IsExtensionSupported("GL_ARB_framebuffer_object");
            _renderToTextureSupported = false;
            _samplerObjectsSupported = Gl.IsExtensionSupported("GL_ARB_sampler_objects");

            // set default render states
            BlendState = BlendState.Opaque;

            // according to the GL3 spec we need a VAO bound... for some reason...
            glGenVertexArrays = (glGenVertexArraysDelegate)Gl.GetDelegate("glGenVertexArrays", typeof(glGenVertexArraysDelegate));
            glBindVertexArray = (glBindVertexArrayDelegate)Gl.GetDelegate("glBindVertexArray", typeof(glBindVertexArrayDelegate));

            glGenSamplers = (glGenSamplersDelegate)Gl.GetDelegate("glGenSamplers", typeof(glGenSamplersDelegate));
            glSamplerParameteri = (glSamplerParameteriDelegate)Gl.GetDelegate("glSamplerParameteri", typeof(glSamplerParameteriDelegate));
            glBindSampler = (glBindSamplerDelegate)Gl.GetDelegate("glBindSampler", typeof(glBindSamplerDelegate));
            glDeleteSamplers = (glDeleteSamplersDelegate)Gl.GetDelegate("glDeleteSamplers", typeof(glDeleteSamplersDelegate));

            _currentSamplerStates.SamplerChanged += new SamplerStateCollection.SamplerChangedHandler(samplerStateChanged);
            SamplerStates[0] = SamplerState.LinearWrap;
            SamplerStates[1] = SamplerState.LinearClamp;

            uint vao;
            glGenVertexArrays(1, out vao);
            glBindVertexArray(vao);

            _parentWindow = parentWindow;
        }

        #region Render states
        public bool BlendEnabled
        {
            get { return Gl.glIsEnabled(Gl.GL_BLEND) == Gl.GL_TRUE; }
            set { if (value) Gl.glEnable(Gl.GL_BLEND); else Gl.glDisable(Gl.GL_BLEND); }
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

        public TextureResource CreateTexture(string name, BitmapSurface colorSurface, BitmapSurface alphaSurface, bool mipmapped)
        {
            return new GlTexture(this, name, colorSurface, alphaSurface);
        }

        public TextureResource CreateTexture(string name, BitmapSurface surface, bool mipmapped, bool premultiplyAlpha)
        {
            return new GlTexture(this, name, surface, mipmapped, premultiplyAlpha);
        }

        public UpdatableTexture CreateUpdatableTexture(string name, int width, int height)
        {
            return new GlUpdatableTexture(this, name, width, height);
        }
        #endregion Texture creation

        public CubeMapResource CreateCubeMap(string name, BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right,
            BitmapSurface up, BitmapSurface down)
        {
            return new GlCubeMap(this, name, front, back, left, right, up, down);
        }

        public Effect CreateEffect(string name, System.IO.Stream stream)
        {
            return new OpenGl.GlslEffect(name, stream);
            //return new CgEffect(name, stream, _cgContext);
        }

        public VertexBuffer CreateVertexBuffer<T>(VertexBufferUsage usage, T[] data, int numVertices, VertexElementSet declaration) where T : struct
        {
            return GlVertexBuffer.CreateBuffer(usage, data, numVertices, declaration);
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
                    _defaultTexture = new GlTexture(this, true);

                return _defaultTexture;
            }
        }

        public TextureResource ErrorTexture
        {
            get
            {
                if (_errorTexture == null)
                    _errorTexture = new GlTexture(this, false);

                return _errorTexture;
            }
        }

        public IndexBuffer Indices
        {
            get
            {
                return _currentIndexBuffer;
            }
            set
            {
                _currentIndexBuffer = (GlIndexBuffer)value;
                if (_currentIndexBuffer != null) _currentIndexBuffer.Bind();
            }
        }

        public void SetVertexBuffer(VertexBuffer buffer)
        {
            Gl.glGetError();

            _vertexPointersNeedSetup = true;
            _vertexDeclaration = buffer.VertexElements;
            
            GlVertexBuffer glVertices = (GlVertexBuffer)buffer;

            glVertices.Bind();
            GlException.ThrowExceptionIfErrorExists();

            _currentVertexBuffer = glVertices;
        }

        public void RenderPrimitives(int firstVertex, int vertexCount)
        {
            Gl.glGetError();

            if (_vertexPointersNeedSetup)
                setupVertexBufferPointers();

            Gl.glDrawArrays(Gl.GL_TRIANGLES, firstVertex, vertexCount);
            GlException.ThrowExceptionIfErrorExists();
        }

        public void RenderIndexedPrimitives(int firstIndex, int primitiveCount)
        {
            Gl.glGetError();

            if (_vertexPointersNeedSetup)
                setupVertexBufferPointers();

            Gl.glDrawElements(Gl.GL_TRIANGLES, primitiveCount * 3, Gl.GL_UNSIGNED_INT, 
                 Gk3Main.Utils.IncrementIntPtr(IntPtr.Zero, firstIndex * sizeof(int)));

            GlException.ThrowExceptionIfErrorExists();
        }

        public void RenderPrimitives<T>(PrimitiveType type, int startIndex, int vertexCount, T[] vertices, VertexElementSet declaration) where T: struct
        {
            _vertexDeclaration = declaration;

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

        public void RenderIndices(PrimitiveType type, int startIndex, int vertexCount, int[] indices)
        {
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
                System.Runtime.InteropServices.GCHandle indicesHandle =
                    System.Runtime.InteropServices.GCHandle.Alloc(indices,
                    System.Runtime.InteropServices.GCHandleType.Pinned);

                IntPtr indicesptr = indicesHandle.AddrOfPinnedObject();

                try
                {
                    Gl.glDrawElements(glType, indices.Length - startIndex, Gl.GL_UNSIGNED_INT,
                        Gk3Main.Utils.IncrementIntPtr(indicesptr, startIndex * sizeof(int)));
                }
                finally
                {
                    indicesHandle.Free();
                }
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

            return new GlRenderTarget(this, width, height);
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

        private void setupVertexBufferPointers()
        {
            for (int i = 0; i < _vertexDeclaration.Elements.Length; i++)
            {
                GlslEffect.Attribute attrib = _currentEffect.GetAttribute(_vertexDeclaration.Elements[i].Usage, _vertexDeclaration.Elements[i].UsageIndex);
                if (GlslEffect.Attribute.IsValidAttribute(attrib) == false)
                    continue;

                Gl.glEnableVertexAttribArray(attrib.GlHandle);
                GlException.ThrowExceptionIfErrorExists();
                Gl.glVertexAttribPointer(attrib.GlHandle, (int)_vertexDeclaration.Elements[i].Format, Gl.GL_FLOAT, 0, _vertexDeclaration.Stride,
                    Gk3Main.Utils.IncrementIntPtr(IntPtr.Zero, _vertexDeclaration.Elements[i].Offset));
                GlException.ThrowExceptionIfErrorExists();
            }
            GlException.ThrowExceptionIfErrorExists();

            // disable the rest
            // TODO: we need to figure out what the maximum number of vertex elements is and use that!
            for (int i = _vertexDeclaration.Elements.Length; i < 12; i++)
            {
           //     Gl.glDisableVertexAttribArray(i);
            }

            _vertexPointersNeedSetup = false;
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

        #region Extensions

        internal delegate void glGenVertexArraysDelegate(int count, out uint arrays);
        internal delegate void glBindVertexArrayDelegate(uint array);
        internal delegate void glGenSamplersDelegate(int count, out uint samplers);
        internal delegate void glSamplerParameteriDelegate(uint sampler, int pname, int param);
        internal delegate void glBindSamplerDelegate(uint textureUnit, uint sampler);
        internal delegate void glDeleteSamplersDelegate(int count, out uint samplers);

        internal static glGenVertexArraysDelegate glGenVertexArrays;
        internal static glBindVertexArrayDelegate glBindVertexArray;
        internal static glGenSamplersDelegate glGenSamplers;
        internal static glSamplerParameteriDelegate glSamplerParameteri;
        internal static glBindSamplerDelegate glBindSampler;
        internal static glDeleteSamplersDelegate glDeleteSamplers;

        #endregion
    }
}
