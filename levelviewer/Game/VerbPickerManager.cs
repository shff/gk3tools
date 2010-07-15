using System;
using System.Collections.Generic;
using System.Text;


namespace Game
{
    static class VerbPickerManager
    {
        private static Gk3Main.Gui.VerbButtonSet _vbs;
        private static int _mouseDownX, _mouseDownY;
        private static Gk3Main.Resource.ResourceManager _content = new Gk3Main.Resource.ResourceManager();

        public static Gk3Main.Gui.VerbButtonSet VerbButtonSet
        {
            get { return _vbs; }
        }

        public static void Render(Gk3Main.Graphics.SpriteBatch sb, int tickCount)
        {
            if (_vbs != null)
                _vbs.Render(sb, tickCount);
        }

        public static void Process()
        {
            if (_vbs != null && _vbs.Active == false)
            {
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
                {
                    _vbs = new Gk3Main.Gui.VerbButtonSet(_content, x, y, nvcs, true);
                    _vbs.KeepInsideViewport(Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport);
                }
            }
        }

        public static void MouseMove(int x, int y)
        {
            if (_vbs != null)
                _vbs.OnMouseMove(x, y);
        }

        public static void RenderProperCursor(Gk3Main.Graphics.SpriteBatch sb, Gk3Main.Graphics.Camera camera, int mx, int my, Gk3Main.Gui.CursorResource point, Gk3Main.Gui.CursorResource zoom)
        {
            if (_vbs != null || camera == null)
            {
                point.Render(sb, mx, my);
                return;
            }

            List<Gk3Main.Game.NounVerbCase> nvcs = getNounVerbCasesUnderCursor(camera, mx, my);

            if (nvcs == null || nvcs.Count == 0)
            {
                point.Render(sb, mx, my);
            }
            else
            {
                zoom.Render(sb, mx, my);
            }

        }

        public static bool VerbButtonsVisible
        {
            get { return _vbs != null && _vbs.Active; }
        }

        private static List<Gk3Main.Game.NounVerbCase> getNounVerbCasesUnderCursor(Gk3Main.Graphics.Camera camera, int mx, int my)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            Gk3Main.Math.Vector3 unprojected = camera.Unproject(new Gk3Main.Math.Vector3(mx, my, 0));
            
           
            string model = Gk3Main.SceneManager.GetCollisionModel(camera.Position, (unprojected - camera.Position).Normalize(), 1000.0f);

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
