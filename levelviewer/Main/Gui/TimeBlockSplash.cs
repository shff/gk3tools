using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class TimeBlockSplash : IGuiLayer
    {
        private Gk3Main.Graphics.TextureResource _background;
        private List<Gk3Main.Graphics.TextureResource> _title;
        private Gk3Main.Sound.Sound _ticktock;
        private bool _isActive;
        private int _timeAtStart;
        private EventHandler _onFinished;
 
        private int _timeAtStartRender;
        private const int _titleX = 14;
        private const int _titleY = 346;
        private const int _msPerTitleFrame = 60;

        public TimeBlockSplash(Resource.ResourceManager globalContent, Gk3Main.Game.Timeblock timeblock, int currentTickCount)
        {
            _isActive = true;
            _timeAtStart = currentTickCount;
            string timeblockName = Gk3Main.Game.GameManager.GetTimeBlockString(timeblock);
            _background = globalContent.Load<Gk3Main.Graphics.TextureResource>("TBT" + timeblockName);

            _title = new List<Gk3Main.Graphics.TextureResource>();
            int counter = 0;
            while(true)
            {
                Gk3Main.Graphics.TextureResource title = globalContent.Load<Gk3Main.Graphics.TextureResource>("D" + timeblockName + "_" + (counter + 1).ToString("00"));
                if (title == null || title.Loaded == false) break;
                
                _title.Add(title);

                counter++;
            }

            _ticktock = globalContent.Load<Gk3Main.Sound.Sound>("CLOCKTIMEBLOCK");
            _ticktock.Play2D(Gk3Main.Sound.SoundTrackChannel.UI);
        }

        public void Dismiss()
        {
            _isActive = false;
        }

        public void Render(Gk3Main.Graphics.SpriteBatch sb, int tickCount)
        {
            Gk3Main.Graphics.IRenderer renderer = Gk3Main.Graphics.RendererManager.CurrentRenderer;

            // draw the background centered in the screen
            Gk3Main.Graphics.Viewport viewport = renderer.Viewport;
            int centerX = viewport.Width / 2 + viewport.X;
            int centerY = viewport.Height / 2 + viewport.Y;
            int backgroundX = centerX - _background.Width / 2;
            int backgroundY = centerY - _background.Height / 2;

            sb.Draw(_background, new Gk3Main.Math.Vector2(backgroundX, backgroundY));

            // draw the title
            if (_timeAtStartRender == 0) _timeAtStartRender = tickCount;
            int frame = calcCurrentFrame(_timeAtStartRender, tickCount);
            if (frame >= _title.Count) frame = _title.Count - 1;
            if (frame >= 0)
                sb.Draw(_title[frame], new Gk3Main.Math.Vector2(backgroundX + _titleX, backgroundY + _titleY));

            if (tickCount > _timeAtStart + 4000)
            {
                if (_onFinished != null)
                    _onFinished(this, EventArgs.Empty);
            }
        }

        public void OnMouseMove(int tickCount, int mx, int my)
        {
            // nothing
        }

        public void OnMouseDown(int button, int mx, int my)
        {
            // nothing
        }

        public void OnMouseUp(int button, int mx, int my)
        {
            // nothing
        }

        public bool IsActive
        {
            get { return _isActive; }
        }

        public bool IsPopup { get { return false; } }

        public event EventHandler OnFinished
        {
            add { _onFinished += value; }
            remove { _onFinished -= value; }
        }

        private static int calcCurrentFrame(int timeAtLastFrame, int currentTime)
        {
            return (currentTime - timeAtLastFrame) / _msPerTitleFrame;
        }
    }
}
