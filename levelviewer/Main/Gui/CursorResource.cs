using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tao.OpenGl;

namespace Gk3Main.Gui
{
    public class CursorResource : Resource.Resource
    {
        public CursorResource(string name, System.IO.Stream stream)
            : base(name, true)
        {
            _frameCount = 1;
            _frameRate = 0;
            string alphaTexture = null;

            StreamReader reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] tokens = line.Split('=');
                if (tokens[0] == "Hotspot")
                {
                    if (tokens[1] == "center")
                    {
                        _hotX = -1;
                        _hotY = -1;
                    }
                    else
                    {
                        Match match = new Regex("([0-9]+),([0-9]+)").Match(tokens[1]);
                        _hotX = int.Parse(match.Groups[1].Value);
                        _hotY = int.Parse(match.Groups[2].Value);
                    }
                }
                else if (tokens[0] == "Frame Count")
                {
                    _frameCount = int.Parse(tokens[1]);
                }
                else if (tokens[0] == "Frame Rate")
                {
                    _frameRate = int.Parse(tokens[1]);
                }
                else if (tokens[0] == "Alpha Channel")
                {
                    alphaTexture = tokens[1];
                }
            }

            reader.Close();

            if (alphaTexture == null)
                _cursor = new Gk3Main.Graphics.TextureResource(name, FileSystem.Open(Utils.GetFilenameWithoutExtension(name) + ".BMP"));
            else
                _cursor = new Gk3Main.Graphics.TextureResource(name, FileSystem.Open(Utils.GetFilenameWithoutExtension(name) + ".BMP"), FileSystem.Open(alphaTexture + ".BMP"));

            if (_hotX == -1) _hotX = _cursor.Height / 2;
            if (_hotY == -1) _hotY = _cursor.Height / 2;
        }

        public void Render(int x, int y)
        {
            int currentFrame = (int)((Game.GameManager.TickCount / 1000.0f) * _frameRate) % _frameCount;
            float uWidth = _cursor.ActualPixelWidth * (_cursor.ActualWidth / _frameCount);
            float u = uWidth * currentFrame;

            Graphics.Rect src;
            src.X = u;
            src.Y = 0;
            src.Width = uWidth;
            src.Height = _cursor.ActualPixelHeight;

            Graphics.Utils.Go2D();
            Graphics.Utils.Blit(x - _hotX, y - _hotY, _cursor, src);

            Graphics.Utils.End2D();
        }

        public override void Dispose()
        {
            Resource.ResourceManager.Unload(_cursor);
        }

        private Graphics.TextureResource _cursor;
        private int _hotX, _hotY;
        private int _frameCount, _frameRate;
    }

    public class CursorResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new CursorResource(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "CUR" };
    }
}
