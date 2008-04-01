using System;
using System.Collections.Generic;
using System.IO;
using Tao.OpenGl;

namespace Gk3Main.Game
{
    public class CursorResource : Resource.Resource
    {
        public CursorResource(string name, System.IO.Stream stream)
            : base(name, true)
        {
            StreamReader reader = new StreamReader(stream);
            string hotspot = reader.ReadLine();
            reader.Close();

            System.Text.RegularExpressions.Regex regex
                = new System.Text.RegularExpressions.Regex("Hotspot=([0-9]+),([0-9]+)");

            System.Text.RegularExpressions.Match match = regex.Match(hotspot);

            _hotX = int.Parse(match.Groups[1].Value);
            _hotY = int.Parse(match.Groups[2].Value);

            // load the texture
            _cursor = (Graphics.TextureResource)Resource.ResourceManager.Load(Utils.GetFilenameWithoutExtension(name) + ".BMP");
        }

        public void Render(int x, int y)
        {
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();

            Gl.glOrtho(viewport[0], viewport[2], viewport[3], viewport[1], -1.0f, 1.0f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();

            Gl.glTranslatef((float)(x - _hotX), (float)(y - _hotY), 0);

            Gl.glPushAttrib(Gl.GL_ENABLE_BIT);
            Gl.glDisable(Gl.GL_DEPTH_TEST);

            _cursor.Bind();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 0);
            Gl.glVertex3f(0, 0, 0);
            Gl.glTexCoord2f(_cursor.ActualWidth, 0);
            Gl.glVertex3f(_cursor.Width, 0, 0);
            Gl.glTexCoord2f(_cursor.ActualWidth, _cursor.ActualHeight);
            Gl.glVertex3f(_cursor.Width, _cursor.Height, 0);
            Gl.glTexCoord2f(0, _cursor.ActualHeight);
            Gl.glVertex3f(0, _cursor.Height, 0);
            Gl.glEnd();

            Gl.glPopAttrib();
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }

        public override void Dispose()
        {
            Resource.ResourceManager.Unload(_cursor);
        }

        private Graphics.TextureResource _cursor;
        private int _hotX, _hotY;
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
