using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace Game
{
    static class VerbPickerManager
    {
        private static Gk3Main.Gui.VerbButtonSet _vbs;
        private static int _mouseDownX, _mouseDownY;

        public static Gk3Main.Gui.VerbButtonSet VerbButtonSet
        {
            get { return _vbs; }
        }

        public static void Render(int tickCount)
        {
            if (_vbs != null)
                _vbs.Render(tickCount);
        }

        public static void Process()
        {
            if (_vbs != null && _vbs.Active == false)
            {
                _vbs.Dispose();
                _vbs = null;
            }
        }

        public static void Show(string noun)
        {
            if (_vbs == null)
            {

            }
        }

        public static void MouseDown(int button, int x, int y)
        {
            if (_vbs != null)
                _vbs.OnMouseDown(x, y);

            _mouseDownX = x;
            _mouseDownY = y;
        }

        public static void MouseUp(Gk3Main.Graphics.Camera camera, int button, int x, int y)
        {
            if (_vbs != null)
                _vbs.OnMouseUp(x, y);
            else if (x == _mouseDownX && y == _mouseDownY)
            {
                List<Gk3Main.Game.NounVerbCase> nvcs = getNounVerbCasesUnderCursor(camera, x, y);

                if (nvcs != null)
                    _vbs = new Gk3Main.Gui.VerbButtonSet(x, y, nvcs, true);
            }
        }

        public static void MouseMove(int x, int y)
        {
            if (_vbs != null)
                _vbs.OnMouseMove(x, y);
        }

        public static void RenderProperCursor(Gk3Main.Graphics.Camera camera, int mx, int my, Gk3Main.Gui.CursorResource point, Gk3Main.Gui.CursorResource zoom)
        {
            if (_vbs != null)
            {
                point.Render(mx, my);
                return;
            }

            List<Gk3Main.Game.NounVerbCase> nvcs = getNounVerbCasesUnderCursor(camera, mx, my);

            if (nvcs == null || nvcs.Count == 0)
            {
                point.Render(mx, my);
            }
            else
            {
                zoom.Render(mx, my);
            }

        }

        public static bool VerbButtonsVisible
        {
            get { return _vbs != null && _vbs.Active; }
        }

        private static List<Gk3Main.Game.NounVerbCase> getNounVerbCasesUnderCursor(Gk3Main.Graphics.Camera camera, int mx, int my)
        {
            // TODO: replace this junk with our own matrix unproject stuff and get rid of the OpenGL stuff

            double[] modelMatrix = new double[16];
            double[] projectionMatrix = new double[16];
            int[] viewport = new int[4];

            Gl.glGetDoublev(Gl.GL_MODELVIEW_MATRIX, modelMatrix);
            Gl.glGetDoublev(Gl.GL_PROJECTION_MATRIX, projectionMatrix);
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            double x, y, z;
            Glu.gluUnProject(mx, viewport[3] - my, 0, modelMatrix, projectionMatrix, viewport, out x, out y, out z);

            string model = Gk3Main.SceneManager.GetCollisionModel(camera.Position, new Gk3Main.Math.Vector3((float)x, (float)y, (float)z) - camera.Position, 1000.0f);

            if (model != null)
            {
                string noun = Gk3Main.SceneManager.GetModelNoun(model);

                if (noun != null)
                {
                    List<Gk3Main.Game.NounVerbCase> nvcs = Gk3Main.SceneManager.GetNounVerbCasesForNoun(noun);

                    if (nvcs.Count > 0)
                    {
                        return nvcs;
                    }
                }
            }

            return null;
        }
    }
}
