using System;
using System.Collections.Generic;
using System.Text;


namespace Game
{
    static class VerbPickerManager
    {
        private static Gk3Main.Gui.VerbButtonSet _vbs;
        private static int _mouseDownX, _mouseDownY;
        private static Gk3Main.Game.Nouns _lastNoun;
        private static int _lastNounVerbCount;
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

        public static void Dismiss()
        {
            if (_vbs != null && _vbs.Active == true)
            {
                _vbs.Dismiss();
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

            int count = getNounVerbCaseCountUnderCursor(camera, mx, my);

            if (count == 0)
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
                Gk3Main.Game.Nouns noun = Gk3Main.SceneManager.GetModelNoun(model);

                if (noun != Gk3Main.Game.Nouns.N_NONE)
                {
                    List<Gk3Main.Game.NounVerbCase> nvcs = Gk3Main.Game.NvcManager.GetNounVerbCases(noun, true);

                    if (nvcs.Count > 0)
                    {
                        return nvcs;
                    }
                }
            }

            return null;
        }

        private static int getNounVerbCaseCountUnderCursor(Gk3Main.Graphics.Camera camera, int mx, int my)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            Gk3Main.Math.Vector3 unprojected = camera.Unproject(new Gk3Main.Math.Vector3(mx, my, 0));


            string model = Gk3Main.SceneManager.GetCollisionModel(camera.Position, (unprojected - camera.Position).Normalize(), 1000.0f);
            Gk3Main.Game.Nouns noun;

            if (model != null)
            {
                noun = Gk3Main.SceneManager.GetModelNoun(model);

                if (_lastNoun != noun)
                {
                    _lastNoun = noun;

                    if (noun != Gk3Main.Game.Nouns.N_NONE)
                    {
                        int count = Gk3Main.Game.NvcManager.GetNounVerbCases(noun, true).Count;
                        _lastNounVerbCount = count;
                    }
                    else
                    {
                        _lastNounVerbCount = 0;
                    }
                }
            }
            else
            {
                _lastNoun = Gk3Main.Game.Nouns.N_NONE;
                _lastNounVerbCount = 0;
            }

            return _lastNounVerbCount;
        }
    }
}
