using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace Gk3Main.Gui
{
    public class Font : Resource.TextResource
    {
        public Font(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            // joy! there's actually a document included with GK3 explaining the font file format!
            // See: GK3 FONTS.DOC

            // first, set some default values
            string bitmap = System.IO.Path.GetFileNameWithoutExtension(name);
            string alpha = null;
            int charExtra = 0;
            int lineExtra = 0;
            _lineCount = 1;
            int r = 255, g = 0, b = 255;

            // load the data
            string[] lines = Text.Split('\n');
            foreach (string line in lines)
            {
                int equals = line.IndexOf('=');
                if (equals > 0)
                {
                    string variable = line.Substring(0, equals);
                    string value = line.Substring(equals + 1).Trim();

                    if (variable == "Font")
                        _characters = value;
                    else if (variable == "Bitmap Name")
                        bitmap = value;
                    else if (variable == "Line Count")
                        _lineCount = int.Parse(value);
                    else if (variable == "Char Extra")
                        charExtra = int.Parse(value);
                    else if (variable == "Line Extra")
                        lineExtra = int.Parse(value);
                    else if (variable == "Alpha Channel")
                        alpha = value;
                    else if (variable == "Color")
                    {
                        string[] colors = value.Split('/');
                        r = int.Parse(colors[0]);
                        g = int.Parse(colors[1]);
                        b = int.Parse(colors[2]);
                    }
                    
                    // TODO: the spec lists "Alpha Blend" as one of the possible values for "Type."
                }
            }

            // load the images
            if (bitmap.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase))
                _texture = (Graphics.TextureResource)Resource.ResourceManager.Load(bitmap);
            else
                _texture = (Graphics.TextureResource)Resource.ResourceManager.Load(bitmap + ".BMP");

            buildCharacterInfo();
        }

        public override void Dispose()
        {
            // TODO
        }

        public void Print(int x, int y, string text)
        {
            float cursorX = (float)x;
            float cursorY = (float)y;

            foreach (char c in text)
            {
                int index = mapUnicodeToFontCharacter(c);

                Graphics.Utils.Blit(cursorX, cursorY, _texture, _characterInfo[index].SourceRect);

                cursorX += _characterInfo[index].SourceRect.Width;
            }
        }

        private void buildCharacterInfo()
        {
            // set the height of each character
            _height = _texture.Height / _lineCount;

            byte[] buffer = new byte[_texture.ActualPixelWidth * _texture.ActualPixelHeight * 3];
            Gl.glGetTexImage(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, buffer);

            // look for the baseline marker
            int[] markerColor = { buffer[3], buffer[4], buffer[5] };
            for (int i = _height - 1; i >= 0; i--) // start at the bottom and work up, since some fonts have a mystery marker pixel at (0,1)...
            {
                if (buffer[i * 3 + 0] == markerColor[0] &&
                    buffer[i * 3 + 1] == markerColor[1] &&
                    buffer[i * 3 + 2] == markerColor[2])
                {
                    _baseline = i;
                    break;
                }
            }

            // now load each character's info
            _characterInfo = new FontCharacterInfo[_characters.Length];
            int currentLine = 0;
            int currentMarkerPixel = 1;
            int nextMarkerPixel = 2; // first one should always be at 1
            for (int i = 0; i < _characters.Length; i++)
            {
                // find the next marker
                while(true)
                {
                    if (nextMarkerPixel >= _texture.ActualPixelWidth)
                    {
                        currentLine++;
                        nextMarkerPixel = 2;
                        currentMarkerPixel = 1;
                    }

                    if (buffer[(currentLine * _height * _texture.ActualPixelWidth + nextMarkerPixel) * 3 + 0] == markerColor[0] &&
                        buffer[(currentLine * _height * _texture.ActualPixelWidth + nextMarkerPixel) * 3 + 1] == markerColor[1] &&
                        buffer[(currentLine * _height * _texture.ActualPixelWidth + nextMarkerPixel) * 3 + 2] == markerColor[2])
                    {
                        break;
                    }

                    nextMarkerPixel++;
                }

                _characterInfo[i].SourceRect.Width = nextMarkerPixel - currentMarkerPixel;
                _characterInfo[i].SourceRect.Height = _height;
                _characterInfo[i].SourceRect.X = currentMarkerPixel;
                _characterInfo[i].SourceRect.Y = currentLine * _height;

                currentMarkerPixel = nextMarkerPixel++;
            }
        }

        private int mapUnicodeToFontCharacter(char c)
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                if (_characters[i] == c) return i;
            }

            // TODO: This should return the index of <SPACE>
            return 0;
        }

        private struct FontCharacterInfo
        {
            public Graphics.Rect SourceRect;
        }

        private int _baseline;
        private int _height;
        private string _characters;
        private FontCharacterInfo[] _characterInfo;
        private int _lineCount;
        private char _defaultCharacter = ' ';
        private Graphics.TextureResource _texture;
        private Graphics.TextureResource _alpha;
    }

    public class FontResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new Font(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "FON" };
    }
}
