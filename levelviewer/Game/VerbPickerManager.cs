using System;
using System.Collections.Generic;
using System.Text;


namespace Game
{
    class VerbPickerManager : Gk3Main.Gui.IGuiLayer
    {
        private bool _isActive;
        private static Gk3Main.Gui.VerbButtonSet _vbs;
        private static int _mouseDownX, _mouseDownY;
        private static Gk3Main.Game.Nouns _lastNoun;
        private static int _lastNounVerbCount;
        private static Gk3Main.Resource.ResourceManager _content = new Gk3Main.Resource.ResourceManager();

        public static Gk3Main.Gui.VerbButtonSet VerbButtonSet
        {
            get { return _vbs; }
        }

        public static void Dismiss()
        {
            if (_vbs != null && _vbs.Active == true)
            {
                _vbs.Dismiss();
                _vbs = null;
            }
        }

        #region IGuiLayer

        public void Render(Gk3Main.Graphics.SpriteBatch sb, int tickCount)
        {
           if (_vbs != null)
           {
              if (_vbs.Active)
                 _vbs.Render(sb, tickCount);
              else
                 _vbs = null;
           }
        }

        public void OnMouseDown(int button, int x, int y)
        {
            if (_vbs != null)
                _vbs.OnMouseDown(x, y);

            _mouseDownX = x;
            _mouseDownY = y;
        }

        public void OnMouseUp(int button, int x, int y)
        {
            Gk3Main.Graphics.Camera camera = Gk3Main.SceneManager.CurrentCamera;
            if (camera != null && button == 0)
            {
                if (_vbs != null)
                    _vbs.OnMouseUp(x, y);
                else if (x == _mouseDownX && y == _mouseDownY && camera != null)
                {
                    List<Gk3Main.Game.NounVerbCase> nvcs = getNounVerbCasesUnderCursor(camera, x, y);

                    if (nvcs != null)
                    {
                        _vbs = new Gk3Main.Gui.VerbButtonSet(_content, x, y, nvcs, true);
                        _vbs.KeepInsideViewport(Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport);
                    }
                }
            }
        }

        public void OnMouseMove(int tickCount, int x, int y)
        {
            if (_vbs != null)
                _vbs.OnMouseMove(x, y);
        }

        public bool IsActive { get { return true; } }
        public bool IsPopup { get { return false; } }
        public bool InterceptMouse { get { return false; } }

        #endregion

        

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
