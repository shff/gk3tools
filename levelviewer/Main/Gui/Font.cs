using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class Font : Resource.TextResource
    {
        public Font(string name, System.IO.Stream stream, Resource.ResourceManager content)
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

                        _color.X = r / 255.0f;
                        _color.Y = g / 255.0f;
                        _color.Z = b / 255.0f;
                        _color.W = 1.0f;
                    }
                    
                    // TODO: the spec lists "Alpha Blend" as one of the possible values for "Type."
                }
            }

            // load the images
            _texture = content.Load<Graphics.TextureResource>(bitmap);
        
            buildCharacterInfo();
        }

        public override void Dispose()
        {
            // TODO
        }

        public void Print(int x, int y, string text)
        {
            Graphics.SpriteBatch sb = new Gk3Main.Graphics.SpriteBatch();
            sb.Begin();

            Print(sb, x, y, text);

            sb.End();
        }

        public void Print(Graphics.SpriteBatch sb, int x, int y, string text)
        {
            Math.Vector2 cursor = new Gk3Main.Math.Vector2((float)x, (float)y);

            foreach (char c in text)
            {
                int index = mapUnicodeToFontCharacter(c);

                sb.Draw(_texture, cursor, _characterInfo[index].SourceRect);

                cursor.X += _characterInfo[index].SourceRect.Width;
            }
        }

        public Graphics.Rect GetPrintedRect(string text)
        {
            float cursorX = 0;

            foreach (char c in text)
            {
                int index = mapUnicodeToFontCharacter(c);

                cursorX += _characterInfo[index].SourceRect.Width;
            }

            Graphics.Rect r;
            r.X = 0;
            r.Y = 0;
            r.Width = cursorX;
            r.Height = _height;

            return r;
        }

        private void buildCharacterInfo()
        {
            // set the height of each character
            _height = _texture.Height / _lineCount;

            byte[] buffer = _texture.Pixels;

            // look for the baseline marker
            int[] markerColor = { buffer[4], buffer[5], buffer[6] };
            for (int i = _height - 1; i >= 0; i--) // start at the bottom and work up, since some fonts have a mystery marker pixel at (0,1)...
            {
                if (buffer[i * 4 + 0] == markerColor[0] &&
                    buffer[i * 4 + 1] == markerColor[1] &&
                    buffer[i * 4 + 2] == markerColor[2])
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
                    if (nextMarkerPixel >= _texture.Width)
                    {
                        if (currentLine + 1 == _lineCount)
                        {
                            // no more lines, so this must be the last character
                            break;
                        }
                        else
                        {
                            // move to the next line
                            currentLine++;
                            nextMarkerPixel = 2;
                            currentMarkerPixel = 1;
                        }
                    }

                    if (buffer[(currentLine * _height * _texture.Width + nextMarkerPixel) * 4 + 0] == markerColor[0] &&
                        buffer[(currentLine * _height * _texture.Width + nextMarkerPixel) * 4 + 1] == markerColor[1] &&
                        buffer[(currentLine * _height * _texture.Width + nextMarkerPixel) * 4 + 2] == markerColor[2])
                    {
                        break;
                    }

                    nextMarkerPixel++;
                }

                _characterInfo[i].SourceRect.Width = nextMarkerPixel - currentMarkerPixel;
                _characterInfo[i].SourceRect.Height = _height - 1.0f;
                _characterInfo[i].SourceRect.X = currentMarkerPixel;
                _characterInfo[i].SourceRect.Y = currentLine * _height + 1.0f;

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
        private Math.Vector4 _color;
    }

    public class FontResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new Font(name, stream, content);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "FON" };
    }
}
