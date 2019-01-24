using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

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
        private bool _debugOutputSupported;
        private int _maxAnisotropy;
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

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern IntPtr wglGetCurrentContext();

        [System.Runtime.InteropServices.DllImport("opengl32.dll")]
        private static extern IntPtr wglGetProcAddress(string name);

        public OpenGLRenderer(RenderWindow parentWindow)
        {
            GL.GetError();
          //  OpenTK.Toolkit.Init();
          //  var ctx = wglGetCurrentContext();
          //  var c = new OpenTK.Graphics.GraphicsContext(new OpenTK.ContextHandle(ctx), (name) => wglGetProcAddress(name), () => new OpenTK.ContextHandle(wglGetCurrentContext()));
           // c.MakeCurrent(new OpenTK.wparentWindow.Handle);

            // load extensions
            //_renderToTextureSupported = Gl.IsExtensionSupported("GL_ARB_framebuffer_object");
            _renderToTextureSupported = false;
            _samplerObjectsSupported = true;// GL..arb.IsExtensionSupported("GL_ARB_sampler_objects");
            _debugOutputSupported = true;// Gl.IsExtensionSupported("GL_ARB_debug_output");
            _maxAnisotropy = 0;
           /* if (Gl.IsExtensionSupported("GL_EXT_texture_filter_anisotropic"))
            {
                float max;
                Gl.glGetFloatv(Gl.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out max);

                _maxAnisotropy = (int)max;

                // OpenGL considers 1.0 to be none, but we consider 0 to be none (like Direct3D does)
                if (_maxAnisotropy == 1)
                    _maxAnisotropy = 0;
            }
            else
            {
                _maxAnisotropy = 0;
            }*/

            // set default render states
            BlendState = BlendState.Opaque;

            if (_debugOutputSupported)
            {
               // GL.DebugMessageCallback(glDebugLog, IntPtr.Zero);

                //GL.Enable(EnableCap.DebugOutputSynchronous);
            }

            _currentSamplerStates.SamplerChanged += new SamplerStateCollection.SamplerChangedHandler(samplerStateChanged);
            SamplerState.PointWrap = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, Filter = TextureFilter.Point, MaxAnisotropy = 4 });
            SamplerState.PointClamp = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, Filter = TextureFilter.Point, MaxAnisotropy = 4 });
            SamplerState.LinearWrap = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, Filter = TextureFilter.Linear, MaxAnisotropy = 4 });
            SamplerState.LinearClamp = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, Filter = TextureFilter.Linear, MaxAnisotropy = 4 });
            SamplerState.AnisotropicWrap = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, Filter = TextureFilter.Anisoptropic, MaxAnisotropy = 4 });
            SamplerState.AnisotropicClamp = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, Filter = TextureFilter.Anisoptropic, MaxAnisotropy = 4 });

           
            SamplerStates[0] = SamplerState.LinearWrap;
            SamplerStates[1] = SamplerState.LinearClamp;

            uint vao;
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            _parentWindow = parentWindow;
        }

        #region Render states

        public SamplerState CreateSampler(SamplerStateDesc desc)
        {
            int sampler;

            GL.GenSamplers(1, out sampler);
            GL.SamplerParameter(sampler, SamplerParameterName.TextureWrapS, convertTextureAddressMode(desc.AddressU));
            GL.SamplerParameter(sampler, SamplerParameterName.TextureWrapT, convertTextureAddressMode(desc.AddressV));
            GL.SamplerParameter(sampler, SamplerParameterName.TextureWrapR, convertTextureAddressMode(desc.AddressW));
            GL.SamplerParameter(sampler, SamplerParameterName.TextureMinFilter, convertTextureFilter(desc.Filter, true, true));
            GL.SamplerParameter(sampler, SamplerParameterName.TextureMagFilter, convertTextureFilter(desc.Filter, false, true));

            var newSamplerObject = new SamplerState(sampler, 0, desc);
            return newSamplerObject;
        }

        public bool BlendEnabled
        {
            get { return GL.IsEnabled(EnableCap.Blend) == true; }
            set { if (value) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend); }
        }

        public bool DepthTestEnabled
        {
            get { return GL.IsEnabled(EnableCap.DepthTest) == true; }
            set { if (value) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest); }
        }

        public bool DepthWriteEnabled
        {
            get { int enabled; GL.GetInteger(GetPName.DepthWritemask, out enabled); return enabled != 0; }
            set { GL.DepthMask(value); }
        }

        public CullMode CullMode
        {
            get
            {
                if (GL.IsEnabled(EnableCap.CullFace) == false)
                    return CullMode.None;

                int param;
                GL.GetInteger(GetPName.FrontFace, out param);

                // OpenGL stores what NOT to cull, so we need to reverse it
                if (param == (int)All.Cw)
                    return CullMode.CounterClockwise;
                else
                    return CullMode.Clockwise;
            }
            set
            {
                if (value == CullMode.None)
                    GL.Disable(EnableCap.CullFace);
                else
                {
                    GL.Enable(EnableCap.CullFace);

                    if (value == CullMode.Clockwise)
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    else
                        GL.FrontFace(FrontFaceDirection.Cw);
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

                GL.BlendFuncSeparate((BlendingFactorSrc)colorSrc, (BlendingFactorDest)colorDest, (BlendingFactorSrc)alphaSrc, (BlendingFactorDest)alphaDest);

                GL.BlendEquationSeparate(convertBlendFunc(value.ColorBlendFunction),
                    convertBlendFunc(value.AlphaBlendFunction));
            }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return _currentSamplerStates; }
        }

        private void samplerStateChanged(SamplerState newSampler, SamplerState oldSampler, int index)
        {
            GL.BindSampler(index, newSampler.GLHandle);
        }

        #endregion Render states

        public Viewport Viewport
        {
            get
            {
                int[] viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);

                Viewport v = new Viewport();
                v.X = viewport[0];
                v.Y = viewport[1];
                v.Width = viewport[2];
                v.Height = viewport[3];

                return v;
            }
            set
            {
                GL.Viewport(value.X, value.Y, value.Width, value.Height);
            }
        }

        #region Texture creation

        public TextureResource CreateTexture(string name, BitmapSurface colorSurface, bool mipmapped)
        {
            return new GlTexture(this, name, colorSurface);
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
            GL.GetError();

            _vertexPointersNeedSetup = true;
            _vertexDeclaration = buffer.VertexElements;
            
            GlVertexBuffer glVertices = (GlVertexBuffer)buffer;

            glVertices.Bind();
            GlException.ThrowExceptionIfErrorExists();

            _currentVertexBuffer = glVertices;
        }

        public void DrawIndexed(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int indexCount)
        {
            if (_vertexPointersNeedSetup)
                setupVertexBufferPointers();

            OpenTK.Graphics.OpenGL.PrimitiveType type;
            switch (primitiveType)
            {
                case PrimitiveType.Triangles:
                    type = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
                    break;
                case PrimitiveType.Lines:
                    type = OpenTK.Graphics.OpenGL.PrimitiveType.Lines;
                    break;
                case PrimitiveType.LineStrip:
                    type = OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip;
                    break;
                default:
                    throw new NotSupportedException();
            }

            GL.DrawElements(type, indexCount, DrawElementsType.UnsignedInt,
                Gk3Main.Utils.IncrementIntPtr(IntPtr.Zero, startIndex * sizeof(int)));
        }

        public void Draw(PrimitiveType primitiveType, int startVertex, int vertexCount)
        {
            if (_vertexPointersNeedSetup)
                setupVertexBufferPointers();

            OpenTK.Graphics.OpenGL.PrimitiveType type;
            switch (primitiveType)
            {
                case PrimitiveType.Triangles:
                    type = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
                    break;
                case PrimitiveType.Lines:
                    type = OpenTK.Graphics.OpenGL.PrimitiveType.Lines;
                    break;
                case PrimitiveType.LineStrip:
                    type = OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip;
                    break;
                default:
                    throw new NotSupportedException();
            }

            GL.DrawArrays(type, startVertex, vertexCount);
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            else
            {
                GlRenderTarget rt = (GlRenderTarget)renderTarget;
                rt.Bind();
            }
        }

        public bool RenderToTextureSupported { get { return _renderToTextureSupported; } }
        public int MaxAnisotropy { get { return _maxAnisotropy; } }

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

                GL.EnableVertexAttribArray(attrib.GlHandle);
                GlException.ThrowExceptionIfErrorExists();
                GL.VertexAttribPointer(attrib.GlHandle, (int)_vertexDeclaration.Elements[i].Format, VertexAttribPointerType.Float, false, _vertexDeclaration.Stride,
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
                return (int)BlendingFactorSrc.One;
            if (mode == BlendMode.Zero)
                return (int)BlendingFactorSrc.Zero;
            if (mode == BlendMode.SourceAlpha)
                return (int)BlendingFactorSrc.SrcAlpha;
            if (mode == BlendMode.DestinationAlpha)
                return (int)BlendingFactorSrc.DstAlpha;
            if (mode == BlendMode.InverseSourceAlpha)
                return (int)BlendingFactorSrc.OneMinusSrcAlpha;
            if (mode == BlendMode.InverseDestinationAlpha)
                return (int)BlendingFactorSrc.OneMinusDstAlpha;

            return (int)BlendingFactorSrc.Zero;
        }

        private static BlendEquationMode convertBlendFunc(BlendFunction func)
        {
            if (func == BlendFunction.Add)
                return BlendEquationMode.FuncAdd;
            else if (func == BlendFunction.Subtract)
                return BlendEquationMode.FuncSubtract;
            else if (func == BlendFunction.ReverseSubtract)
                return BlendEquationMode.FuncReverseSubtract;
            else if (func == BlendFunction.Min)
                return BlendEquationMode.Min;
            else
                return BlendEquationMode.Max;
        }

        private static int convertTextureAddressMode(TextureAddressMode mode)
        {
            if (mode == TextureAddressMode.Clamp)
                return (int)All.ClampToEdge;
            else if (mode == TextureAddressMode.Mirror)
                return (int)All.MirroredRepeat;
            else
                return (int)All.Repeat;
        }

        private static int convertTextureFilter(TextureFilter filter, bool min, bool mipmap)
        {
            if (mipmap)
            {
                if (filter == TextureFilter.Point)
                {
                    if (min) return (int)All.NearestMipmapNearest;
                    else return (int)All.Nearest;
                }
                else if (filter == TextureFilter.Linear ||
                    filter == TextureFilter.Anisoptropic)
                {
                    if (min) return (int)All.LinearMipmapLinear;
                    else return (int)All.Linear;
                }
                else if (filter == TextureFilter.PointMipLinear)
                {
                    if (min) return (int)All.NearestMipmapLinear;
                   else return (int)All.Nearest;
                }
                else if (filter == TextureFilter.LinearMipPoint)
                {
                    if (min) return (int)All.LinearMipmapNearest;
                    else return (int)All.Linear;
                }
                else
                {
                    // TODO: implement the rest!
                    if (min) return (int)All.LinearMipmapLinear;
                    else return (int)All.Linear;
                }
            }
            else
            {
                if (filter == TextureFilter.Point ||
                    filter == TextureFilter.PointMipLinear)
                {
                    if (min) return (int)All.Nearest;
                    else return (int)All.Nearest;
                }
                else if (filter == TextureFilter.Linear ||
                    filter == TextureFilter.LinearMipPoint ||
                    filter == TextureFilter.Anisoptropic)
                {
                    if (min) return (int)All.Linear;
                    else return (int)All.Linear;
                }
                else
                {
                    // TODO: implement the rest!
                    if (min) return (int)All.Linear;
                    else return (int)All.Linear;
                }
            }
        }

        private static void glDebugLog(int source, int type, uint id, int severity, int length, string message, IntPtr userParam)
        {
            Console.CurrentConsole.WriteLine(message);
        }

        #region Extensions

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void glDebugMessageCallbackFunctionDelegate(int source, int type, uint id, int severity, int length, string message, IntPtr userParam);
        internal delegate void glDebugMessageCallbackDelegate(glDebugMessageCallbackFunctionDelegate callback, IntPtr userParam);
        internal delegate void glGenVertexArraysDelegate(int count, out uint arrays);
        internal delegate void glBindVertexArrayDelegate(uint array);
        internal delegate void glGenSamplersDelegate(int count, out uint samplers);
        internal delegate void glSamplerParameteriDelegate(uint sampler, int pname, int param);
        internal delegate void glBindSamplerDelegate(uint textureUnit, uint sampler);
        internal delegate void glDeleteSamplersDelegate(int count, out uint samplers);

        internal static glDebugMessageCallbackDelegate glDebugMessageCallback;
        //internal static glGenVertexArraysDelegate glGenVertexArrays;
       // internal static glBindVertexArrayDelegate glBindVertexArray;
       // internal static glGenSamplersDelegate glGenSamplers;
       // internal static glSamplerParameteriDelegate glSamplerParameteri;
       // internal static glBindSamplerDelegate glBindSampler;
       // internal static glDeleteSamplersDelegate glDeleteSamplers;

        private const int GL_DEBUG_OUTPUT_SYNCHRONOUS_ARB = 0x8242;

        #endregion
    }
}
