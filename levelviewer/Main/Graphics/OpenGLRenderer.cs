using System;
using System.Collections.Generic;

using Tao.OpenGl;
using Tao.Cg;

namespace Gk3Main.Graphics
{
    public static class RendererManager
    {
        private static IRenderer _renderer = null;

        // HACK: too much stuff depends on this property. Plus it should be read-only.
        public static IRenderer CurrentRenderer
        {
            get { return _renderer; }
            set { _renderer = value; }
        }
    }

    public class CgEffect : Effect
    {
        private IntPtr _effect;
        private IntPtr _technique;
        private List<IntPtr> _passes;
        private IntPtr _currentPass;
        private static string[] _compilerOptions = new string[] { "-DOPENGL" };

        internal CgEffect(string name, System.IO.Stream stream, IntPtr cgContext)
            : base(name, stream)
        {
            IntPtr cgEffect = Cg.cgCreateEffect(cgContext, Text, _compilerOptions);

            if (cgEffect == IntPtr.Zero)
                throw new Resource.InvalidResourceFileFormat("Unable to create effect: " + Cg.cgGetLastListing(cgContext));

            _effect = cgEffect;
            _technique = Cg.cgGetNamedTechnique(_effect, "GL");

            if (_technique == null || Cg.cgValidateTechnique(_technique) == 0)
                throw new Resource.InvalidResourceFileFormat("Unable to validate technique");

            _passes = new List<IntPtr>();
            IntPtr pass = Cg.cgGetFirstPass(_technique);
            while(pass != IntPtr.Zero)
            {
                _passes.Add(pass);
                pass = Cg.cgGetNextPass(pass);
            }
        }

        public override void Dispose()
        {
            if (_effect != IntPtr.Zero)
            {
                Cg.cgDestroyEffect(_effect);
                _effect = IntPtr.Zero;
            }
        }

        public override void Begin()
        {
            // nothing
        }

        public override void End()
        {
            // nothing
        }

        public override void BeginPass(int index)
        {
            _currentPass = _passes[index];
            Cg.cgSetPassState(_currentPass);
        }

        public override void EndPass()
        {
            Cg.cgResetPassState(_currentPass);
            _currentPass = IntPtr.Zero;
        }

        public override void SetParameter(string name, float parameter)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            Cg.cgSetParameter1f(param, parameter);
        }

        public override void SetParameter(string name, Math.Vector4 parameter)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            Cg.cgSetParameter4f(param, parameter.X, parameter.Y, parameter.Z, parameter.W);
        }

        public override void SetParameter(string name, Gk3Main.Math.Matrix parameter)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            Cg.cgSetMatrixParameterfc(param, out parameter.M11);
        }

        public override void SetParameter(string name, TextureResource parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            GlTexture glTexture = (GlTexture)parameter;

            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            CgGl.cgGLSetTextureParameter(param, glTexture.OpenGlTexture);
        }

        public override void SetParameter(string name, CubeMapResource parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            GlCubeMap glTexture = (GlCubeMap)parameter;

            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            CgGl.cgGLSetTextureParameter(param, glTexture.OpenGlTexture);
        }

        public override void DisableTextureParameter(string name)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            CgGl.cgGLDisableTextureParameter(param);
        }

        public override void EnableTextureParameter(string name)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            CgGl.cgGLEnableTextureParameter(param);
        }

        public override void UpdatePassParameters()
        {
            Cg.cgGetError();
            cgUpdatePassParameters(_currentPass);
            if (Cg.cgGetError() != Cg.CG_NO_ERROR)
                throw new Exception("OH NO!");
        }


        // TODO: hopefully Tao will expose this method someday.
        // When it does we can remove this.
        [System.Runtime.InteropServices.DllImport("cg")]
        private static extern void cgUpdatePassParameters(IntPtr pass);

    }

    public class GlTexture : TextureResource
    {
        private int _glTexture;

        /// <summary>
        /// Creates a 1x1 white texture
        /// </summary>
        internal GlTexture(bool loaded)
            : base("default_white", loaded)
        {
            // create a 1x1 white pixel
            _pixels = new byte[] { 255, 255, 255, 255 };
            _width = 1;
            _height = 1;

            convertToOpenGlTexture(false, true);
        }

        internal GlTexture(string name, int glTexture, bool loaded)
            : base(name, loaded)
        {
            _glTexture = glTexture;
        }

        public GlTexture(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            convertToOpenGlTexture(true, false);
        }

        public GlTexture(string name, System.IO.Stream stream, bool clamp)
            : base(name, stream)
        {
            convertToOpenGlTexture(true, clamp);
        }

        public GlTexture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream)
            :base(name, colorStream, alphaStream)
        {
            convertToOpenGlTexture(true, true);
        }

        public override void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
        }

        public int OpenGlTexture { get { return _glTexture; } }

        private void convertToOpenGlTexture(bool resizeToPowerOfTwo, bool clamp)
        {
            byte[] pixels = _pixels;
            _actualPixelWidth = _width;
            _actualPixelHeight = _height;

            _actualWidth = 1.0f;
            _actualHeight = 1.0f;

            if (resizeToPowerOfTwo &&
                ((_width & (_width - 1)) != 0 ||
                (_height & (_height - 1)) != 0))
            {
                byte[] newPixels;
                ConvertToPowerOfTwo(pixels, _width, _height, out newPixels, out _actualPixelWidth, out _actualPixelHeight);

                _actualWidth = _width / (float)_actualPixelWidth;
                _actualHeight = _height / (float)_actualPixelHeight;

                pixels = fixupAlpha(newPixels, false);
            }
            else
            {
                pixels = fixupAlpha(null, false);
                if (pixels == null)
                    pixels = _pixels;
            }
            

            Gl.glEnable(Gl.GL_TEXTURE_2D);

            int[] textures = new int[1];
            textures[0] = 0;
            Gl.glGenTextures(1, textures);
            _glTexture = textures[0];
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);

            Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, _actualPixelWidth, _actualPixelHeight,
                Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            if (clamp)
            {
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
            }
        }
    }

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

    public class GlCubeMap : CubeMapResource
    {
        private int _glTexture;

        public GlCubeMap(string name, string front, string back, string left, string right,
            string up, string down)
            : base(name)
        {
            System.IO.Stream frontStream = null;
            System.IO.Stream backStream = null;
            System.IO.Stream leftStream = null;
            System.IO.Stream rightStream = null;
            System.IO.Stream upStream = null;
            System.IO.Stream downStream = null;

            frontStream = FileSystem.Open(front);
            backStream = FileSystem.Open(back);
            leftStream = FileSystem.Open(left);
            rightStream = FileSystem.Open(right);
            upStream = FileSystem.Open(up);

            try
            {
                downStream = FileSystem.Open(down);
            }
            catch (System.IO.FileNotFoundException)
            {
                // oh well, we tried.
            }

            try
            {
                Gl.glGetError();
                Gl.glGenTextures(1, out _glTexture);
                Gl.glEnable(Gl.GL_TEXTURE_CUBE_MAP);
                Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, _glTexture);

                int internalFormat = Gl.GL_RGBA;

                byte[] pixels;
                int width, height;
                loadFace(new System.IO.BinaryReader(frontStream), out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(new System.IO.BinaryReader(backStream), out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_X, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(new System.IO.BinaryReader(rightStream), out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Z, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(new System.IO.BinaryReader(leftStream), out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(new System.IO.BinaryReader(upStream), out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Y, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                if (downStream != null)
                {
                    loadFace(new System.IO.BinaryReader(downStream), out pixels, out width, out height);
                    Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, 0, Gl.GL_RGBA, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
                }
                else
                {
                    // apparently the "down" face isn't needed. we'll just reuse the top.
                    Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, 0, Gl.GL_RGBA, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
                }

                Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);

                if (Gl.glGetError() != Gl.GL_NO_ERROR)
                    throw new InvalidOperationException();
            }
            finally
            {
                frontStream.Close();
                backStream.Close();
                leftStream.Close();
                rightStream.Close();
                upStream.Close();

                if (downStream != null)
                    downStream.Close();
            }
        }

        public override void Bind()
        {
            Gl.glEnable(Gl.GL_TEXTURE_CUBE_MAP);
            Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, _glTexture);
            //Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, 0);
        }

        public override void Unbind()
        {
            Gl.glDisable(Gl.GL_TEXTURE_CUBE_MAP);
        }

        public int OpenGlTexture { get { return _glTexture; } }
    }

    public class GlRenderTarget : RenderTarget
    {
        private int _fbo;
        private int _depthBuffer;
        private int _colorBuffer;
        private GlTexture _texture;

        public GlRenderTarget(int width, int height)
        {
            // generate the FBO
            Gl.glGenFramebuffersEXT(1, out _fbo);
            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, _fbo);

            // generate a depth buffer
            Gl.glGenRenderbuffersEXT(1, out _depthBuffer);
            Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, _depthBuffer);
            Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_DEPTH_COMPONENT16, width, height);
            Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT, Gl.GL_RENDERBUFFER_EXT, _depthBuffer);

            // generate a color buffer
            Gl.glGenTextures(1, out _colorBuffer);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _colorBuffer);
            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, IntPtr.Zero);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
            Gl.glFramebufferTexture2DEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT, Gl.GL_TEXTURE_2D, _colorBuffer, 0);

            // all done... or are we...?
            int status = Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT);
            if (status != Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
                throw new Exception("Unable to create RenderTarget: " + status.ToString());

            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);
        }

        public void Bind()
        {
            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, _fbo);
        }

        public override TextureResource Texture
        {
            get
            {
                if (_texture == null)
                    _texture = new GlTexture("RenderTarget texture", _colorBuffer, true);

                return _texture;
            }
        }
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
            _renderToTextureSupported = Gl.IsExtensionSupported("GL_ARB_framebuffer_object");
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
            return new CgEffect(name, stream, _cgContext);
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

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(3, Gl.GL_FLOAT, vertices.Stride, null);

            Gl.glDrawElements(Gl.GL_TRIANGLES, indices.Length, Gl.GL_UNSIGNED_INT, null);

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
                    foreach (VertexElement element in _vertexDeclaration.Elements)
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
                    }
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
            

            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
            for (int i = 2; i >= 0; i--)
            {
                Gl.glClientActiveTexture(Gl.GL_TEXTURE0 + i);
                Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            }
            Gl.glDisableClientState(Gl.GL_NORMAL_ARRAY);
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


        const int CG_IMMEDIATE_PARAMETER_SETTING = 4132;
        const int CG_DEFERRED_PARAMETER_SETTING = 4133;

        // TODO: hopefully Tao will expose this method someday.
        // When it does we can remove this.
        [System.Runtime.InteropServices.DllImport("cg")]
        private static extern void cgSetParameterSettingMode(IntPtr context, int value);
    }

    public class GlVertexBuffer : VertexBuffer
    {
        private int _buffer;
        private int _length;

        public GlVertexBuffer(float[] data, int stride)
        {
            _stride = stride;
            _length = data.Length;

            Gl.glGenBuffers(1, out _buffer);
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
            Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(data.Length * sizeof(float)), data, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public override void Dispose()
        {
            Gl.glDeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
        }

        public void Unbind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public override int Length
        {
            get { return _length; }
        }
    }

    public class GlIndexBuffer : IndexBuffer
    {
        private int _buffer;
        private int _length;

        public GlIndexBuffer(uint[] data)
        {
            _length = data.Length;

            Gl.glGenBuffers(1, out _buffer);
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, _buffer);
            Gl.glBufferData(Gl.GL_ELEMENT_ARRAY_BUFFER, (IntPtr)(data.Length * sizeof(uint)), data, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public override void Dispose()
        {
            Gl.glDeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, _buffer);
        }

        public void Unbind()
        {
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public override int Length
        {
            get { return _length; }
        }
    }
}
