using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Gk3Main.Game
{
    public class RadiosityMap
    {
        private int _width;
        private int _height;
        private float[] _map;
        private Graphics.TextureResource _memTex;

        public RadiosityMap(int width, int height)
        {
            _width = width;
            _height = height;

            _map = new float[width * height * 3];

            for (int i = 0; i < _map.Length; i++)
            {
                _map[i] = -1.0f;
            }
        }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public float[] Map
        {
            get { return _map; }
        }

        public Graphics.TextureResource MemoryTexture
        {
            get { return _memTex; }
            set { _memTex = value; }
        }

        public void SetColor(int x, int y, float r, float g, float b)
        {
            _map[(y * _width + x) * 3 + 0] = r;
            _map[(y * _width + x) * 3 + 1] = g;
            _map[(y * _width + x) * 3 + 2] = b;
        }
    }

    public class RadiosityMaps
    {
        private RadiosityMap[] _maps;

        public RadiosityMaps(LightmapSpecs specs)
        {
            _maps = new RadiosityMap[specs.Surfaces.Count];

            for (int i = 0; i < _maps.Length; i++)
            {
                _maps[i] = new RadiosityMap(specs.Surfaces[i].Width, specs.Surfaces[i].Height);
            }
        }

        public RadiosityMap[] Maps
        {
            get { return _maps; }
        }

        public Graphics.LightmapResource ConvertToLightmap(string name, float exposure)
        {
            Graphics.OpenGl.OpenGLRenderer renderer = (Graphics.OpenGl.OpenGLRenderer)Graphics.RendererManager.CurrentRenderer;
            Graphics.LightmapResource lightmap = new Graphics.LightmapResource(name, _maps.Length);

            for (int i = 0; i < _maps.Length; i++)
            {
                Graphics.BitmapSurface surface = Radiosity.ConvertColorMapToLightmap(_maps[i].Width, _maps[i].Height, exposure, _maps[i].Map);

                // go back and look for any unused texels
                for (int j = 0; j < _maps[i].Width * _maps[i].Height; j++)
                {
                    if (_maps[i].Map[j * 3] < 0)
                    {
                        surface.Pixels[j * 4 + 0] = 0;
                        surface.Pixels[j * 4 + 1] = 255;
                        surface.Pixels[j * 4 + 2] = 0;
                    }
                }

                lightmap.Maps[i] = surface;
            }

            lightmap.GenTextureAtlas();

            return lightmap;
        }

        public Graphics.LightmapResource CreateBigMemoryTexture(string name)
        {
            Graphics.LightmapResource lightmap = new Graphics.LightmapResource(name, _maps.Length);

            for (int i = 0; i < _maps.Length; i++)
                lightmap.Maps[i] = new Graphics.BitmapSurface(_maps[i].MemoryTexture.Width, _maps[i].MemoryTexture.Height, _maps[i].MemoryTexture.Pixels);

            lightmap.GenTextureAtlas();

            return lightmap;
        }
    }

    public class Radiosity
    {
        public enum HemicubeRenderType
        {
            Front = 0,
            Left,
            Right,
            Top,
            Bottom
        }

        public delegate void RenderDelegate(HemicubeRenderType type,
            int viewportX, int viewportY, int viewportWidth, int viewportHeight,
            float eyeX, float eyeY, float eyeZ,
            float directionX, float directionY, float directionZ,
            float upX, float upY, float upZ);

        public static void Init(RenderDelegate renderCallback)
        {
            if (renderCallback == null)
                throw new ArgumentNullException("renderCallback");

            _callbackHandle = GCHandle.Alloc(renderCallback);

            rad_Init(Marshal.GetFunctionPointerForDelegate(renderCallback));
        }

        public static void Shutdown()
        {
            rad_Close();

            _callbackHandle.Free();
        }

        public static Graphics.TextureResource GenerateMemoryTexture(int width, int height, float red, float green, float blue)
        {
            Graphics.OpenGl.OpenGLRenderer renderer = Graphics.RendererManager.CurrentRenderer as Graphics.OpenGl.OpenGLRenderer;
            
            if (renderer != null)
            {
                LightmapSurface surface = rad_GenerateMemoryTexture(width, height, red, green, blue);

                Graphics.OpenGl.GlTexture tex = new Graphics.OpenGl.GlTexture(renderer, "memtex", (int)surface.glTex, width, height, true);

                return tex;
            }

            return null;
        }

        public static Graphics.BitmapSurface ConvertColorMapToLightmap(int width, int height, float exposure, float[] map)
        {
            byte[] lightmapPixels = new byte[width * height * 3];

            GCHandle lightmapHandle = GCHandle.Alloc(lightmapPixels, GCHandleType.Pinned);
            GCHandle colorHandle = GCHandle.Alloc(map, GCHandleType.Pinned);

            rad_ConvertColorMapToLightmap(colorHandle.AddrOfPinnedObject(), lightmapHandle.AddrOfPinnedObject(), width, height, exposure);

            lightmapHandle.Free();
            colorHandle.Free();

            Graphics.BitmapSurface surface = new Graphics.BitmapSurface(width, height, null);
            
            // convert the 3 bpp lightmap to 4 bpp
            for (int i = 0; i < width * height; i++)
            {
                surface.Pixels[i * 4 + 0] = lightmapPixels[i * 3 + 0];
                surface.Pixels[i * 4 + 1] = lightmapPixels[i * 3 + 1];
                surface.Pixels[i * 4 + 2] = lightmapPixels[i * 3 + 2];
                surface.Pixels[i * 4 + 3] = 255;
            }
            
            return surface;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LightmapTexel
        {
            public float PosX, PosY, PosZ;
            public float NormalX, NormalY, NormalZ;
            public float UpX, UpY, UpZ;
            public float Red, Green, Blue;

            public IntPtr Tag;
        }

        public static void CalcPass(LightmapTexel[] texels, int numTexels)
        {
            if (numTexels > texels.Length)
                throw new ArgumentException();

            unsafe
            {
                GCHandle h = GCHandle.Alloc(texels, GCHandleType.Pinned);
                rad_CalcPassBatched(h.AddrOfPinnedObject(), numTexels);
                h.Free();
            }
        }

        /// <summary>
        /// Renders a hemicube (this is used for debugging)
        /// </summary>
        public static void RenderHemicube(Math.Vector3 position, Math.Vector3 normal, Math.Vector3 up)
        {
            rad_RenderHemicube(position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z, up.X, up.Y, up.Z);
        }

        public static void GenerateOmniLight(int radius, Math.Vector3 color, out Graphics.TextureResource memoryTex, out Graphics.TextureResource alphaMask)
        {
            memoryTex = GenerateMemoryTexture(radius * 2, radius * 2, color.X, color.Y, color.Z);

            Graphics.BitmapSurface alphaSurface = new Graphics.BitmapSurface(radius * 2, radius * 2, null);
            for (int y = 0; y < radius * 2; y++)
            {
                for (int x = 0; x < radius * 2; x++)
                {
                    if ((x - radius) * (x - radius) + (y - radius) * (y - radius) > radius * radius)
                        alphaSurface.Pixels[(y * radius * 2 + x) * 4 + 3] = 0;
                    else
                        alphaSurface.Pixels[(y * radius * 2 + x) * 4 + 3] = 255;
                }
            }
            alphaMask = Graphics.RendererManager.CurrentRenderer.CreateTexture("alphaMask", alphaSurface, false, false);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LightmapSurface
        {
            public int width, height;
            public uint glTex;
            public IntPtr colorBuffer;
        }

        [DllImport("radiosity", CallingConvention = CallingConvention.Cdecl)]
        static extern void rad_Init(IntPtr callback);

        [DllImport("radiosity", CallingConvention = CallingConvention.Cdecl)]
        static extern LightmapSurface rad_GenerateMemoryTexture(int width, int height, float red, float green, float blue);

        [DllImport("radiosity", CallingConvention = CallingConvention.Cdecl)]
        static extern void rad_RenderHemicube(float posX, float posY, float posZ, float normalX, float normalY, float normalZ, float upX, float upY, float upZ);

        [DllImport("radiosity", CallingConvention=CallingConvention.Cdecl)]
        unsafe static extern void rad_CalcPassBatched(IntPtr texels, int numTexels);

        [DllImport("radiosity", CallingConvention = CallingConvention.Cdecl)]
        static extern void rad_ConvertColorMapToLightmap(IntPtr colors, IntPtr lightmap, int width, int height, float exposure);

        [DllImport("radiosity", CallingConvention = CallingConvention.Cdecl)]
        static extern void rad_Close();

        static GCHandle _callbackHandle;
    }
}
