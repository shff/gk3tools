using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class Font
    {
        public Graphics.TextureResource Texture;
        public Graphics.Color DefaultColor;
        public string Characters;
        public int Baseline;
        public int LineHeight;

        public struct FontCharacterInfo
        {
            public Graphics.Rect SourceRect;
        }
        public FontCharacterInfo[] CharacterInfo;

        public FontInstance Instance;

        private void buildCharacterInfo(int lineCount, Graphics.BitmapSurface bmp)
        {
            // set the height of each character
            LineHeight = bmp.Height / lineCount;

            byte[] buffer = bmp.Pixels;

            // look for the baseline marker
            int[] markerColor = { buffer[4], buffer[5], buffer[6] };
            for (int i = LineHeight - 1; i >= 0; i--) // start at the bottom and work up, since some fonts have a mystery marker pixel at (0,1)...
            {
                if (buffer[i * 4 + 0] == markerColor[0] &&
                    buffer[i * 4 + 1] == markerColor[1] &&
                    buffer[i * 4 + 2] == markerColor[2])
                {
                    Baseline = i;
                    break;
                }
            }
            DefaultColor = new Graphics.Color(markerColor[0], markerColor[1], markerColor[2]);

            // now load each character's info
            CharacterInfo = new FontCharacterInfo[Characters.Length];
            int currentLine = 0;
            int currentMarkerPixel = 1;
            int nextMarkerPixel = 2; // first one should always be at 1
            for (int i = 0; i < Characters.Length; i++)
            {
                // find the next marker
                while (true)
                {
                    if (nextMarkerPixel >= bmp.Width)
                    {
                        if (currentLine + 1 == lineCount)
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

                    if (buffer[(currentLine * LineHeight * bmp.Width + nextMarkerPixel) * 4 + 0] == markerColor[0] &&
                        buffer[(currentLine * LineHeight * bmp.Width + nextMarkerPixel) * 4 + 1] == markerColor[1] &&
                        buffer[(currentLine * LineHeight * bmp.Width + nextMarkerPixel) * 4 + 2] == markerColor[2])
                    {
                        break;
                    }

                    nextMarkerPixel++;
                }

                CharacterInfo[i].SourceRect.Width = nextMarkerPixel - currentMarkerPixel;
                CharacterInfo[i].SourceRect.Height = LineHeight - 1.0f;
                CharacterInfo[i].SourceRect.X = currentMarkerPixel;
                CharacterInfo[i].SourceRect.Y = currentLine * LineHeight + 1.0f;

                currentMarkerPixel = nextMarkerPixel++;
            }

            Instance = new FontInstance();
        }

        private int mapUnicodeToFontCharacter(char c)
        {
            if (c == '\r' || c == '\n')
                return -1;

            for (int i = 0; i < Characters.Length; i++)
            {
                if (Characters[i] == c) return i;
            }

            // TODO: This should return the index of <SPACE>
            return 0;
        }

        public static void Print(FontInstance font, int x, int y, string text)
        {
            Graphics.SpriteBatch sb = new Gk3Main.Graphics.SpriteBatch();
            sb.Begin();

            Print(sb, font, x, y, text);

            sb.End();
        }

        public static void Print(Graphics.SpriteBatch sb, FontInstance font, int x, int y, string text)
        {
            if (font.Font == null) return;

            Math.Vector2 cursor = new Gk3Main.Math.Vector2((float)x, (float)y);

            foreach (char c in text)
            {
                if (c == '\t')
                {
                    var space = font.Font.mapUnicodeToFontCharacter(' ');
                    cursor.X += font.Font.CharacterInfo[space].SourceRect.Width * 5;
                    continue;
                }

                int index = font.Font.mapUnicodeToFontCharacter(c);

                if (index >= 0)
                {
                    sb.Draw(font.Font.Texture, cursor, font.Font.CharacterInfo[index].SourceRect, font.Color, 0);

                    cursor.X += font.Font.CharacterInfo[index].SourceRect.Width;
                }
            }
        }

        public static Math.Vector2 MeasureString(FontInstance font, string text)
        {
            if (font.Font == null) return Math.Vector2.Zero;

            float cursorX = 0;

            foreach (char c in text)
            {
                int index = font.Font.mapUnicodeToFontCharacter(c);

                if (index >= 0)
                    cursorX += font.Font.CharacterInfo[index].SourceRect.Width;
            }

            Math.Vector2 size;
            size.X = cursorX;
            size.Y = font.Font.LineHeight;

            return size;
        }

        public static Math.Vector2 MeasureString(FontInstance font, StringBuilder text)
        {
            if (font.Font == null) return Math.Vector2.Zero;

            float cursorX = 0;

            for (int i = 0; i < text.Length; i++)
            {
                int index = font.Font.mapUnicodeToFontCharacter(text[i]);

                if (index >= 0)
                    cursorX += font.Font.CharacterInfo[index].SourceRect.Width;
            }

            Math.Vector2 size;
            size.X = cursorX;
            size.Y = font.Font.LineHeight;

            return size;
        }

        public static FontInstance Load(FontSpec spec)
        {
            // see if this already loaded in our own cache
            var key = spec.Characters + "$" + spec.Bitmap;

            Font font;
            if (_fonts.TryGetValue(key, out font) == false)
            {
                font = new Font();
                font.Characters = spec.Characters;
                font.Texture = Resource.ResourceManager.Global.Load<Graphics.TextureResource>(spec.Bitmap);

                var bmp = spec.Bitmap;
                if (bmp.IndexOf('.') < 0)
                    bmp += ".BMP";

                Graphics.BitmapSurface map;
                using (System.IO.Stream stream = FileSystem.Open(bmp))
                {
                    map = new Gk3Main.Graphics.BitmapSurface(stream, Graphics.BitmapSurface.SourceType.Unknown, true);
                }

                font.buildCharacterInfo(spec.LineCount, map);

                // make the texture white
                for (int i = 0; i < map.Width * map.Height; i++)
                {
                    if (map.Pixels[i * 4 + 0] == font.DefaultColor.R &&
                        map.Pixels[i * 4 + 1] == font.DefaultColor.G &&
                        map.Pixels[i * 4 + 2] == font.DefaultColor.B)
                    {
                        map.Pixels[i * 4 + 0] = 255;
                        map.Pixels[i * 4 + 1] = 255;
                        map.Pixels[i * 4 + 2] = 255;
                    }
                }

                font.Texture = Graphics.RendererManager.CurrentRenderer.CreateTexture(spec.Bitmap, map, false);

                font.Instance.Font = font;
                font.Instance.Color = font.DefaultColor;

                _fonts.Add(key, font);
            }

            var result = new FontInstance();
            result.Font = font;
            result.Color = spec.DefaultColor;

            return result;
        }

        private static Dictionary<string, Font> _fonts = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
    }

    public struct FontInstance
    {
        public Graphics.Color Color;

        public Font Font;
    }

    public class FontSpec : Resource.TextResource
    {
        public string Characters;
        public string Bitmap;
        public string Alpha;
        public Font.FontCharacterInfo[] CharacterInfo;
        public Graphics.Color DefaultColor;
        public int LineCount;
        public int LineExtra;
        public int CharExtra;

        public FontSpec(string name, System.IO.Stream stream, Resource.ResourceManager content)
            : base(name, stream)
        {
            // joy! there's actually a document included with GK3 explaining the font file format!
            // See: GK3 FONTS.DOC

            // first, set some default values
            Bitmap = System.IO.Path.GetFileNameWithoutExtension(name);
            Alpha = null;
            CharExtra = 0;
            LineExtra = 0;
            LineCount = 1;

            int r = 255, g = 0, b = 255;

            DefaultColor.R = 255;
            DefaultColor.G = 255;
            DefaultColor.B = 255;
            DefaultColor.A = 255;

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
                        Characters = value;
                    else if (variable == "Bitmap Name")
                        Bitmap = value;
                    else if (variable == "Line Count")
                        LineCount = int.Parse(value);
                    else if (variable == "Char Extra")
                        CharExtra = int.Parse(value);
                    else if (variable == "Line Extra")
                        LineExtra = int.Parse(value);
                    else if (variable == "Alpha Channel")
                        Alpha = value;
                    else if (variable == "Color")
                    {
                        string[] colors = value.Split('/');
                        r = int.Parse(colors[0]);
                        g = int.Parse(colors[1]);
                        b = int.Parse(colors[2]);

                        DefaultColor = new Graphics.Color((byte)r, (byte)g, (byte)b, 255);
                    }
                    
                    // TODO: the spec lists "Alpha Blend" as one of the possible values for "Type."
                }
            }
        }

        public override void Dispose()
        {
            // TODO
        }
    }

    public class FontResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            if (name.IndexOf('.') < 0)
                name = name + ".FON";

            System.IO.Stream stream = FileSystem.Open(name);

            return new FontSpec(name, stream, content);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "FON" };
    }
}
