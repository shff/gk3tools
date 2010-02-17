using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class TimeBlockSplash : IDisposable 
    {
        private Gk3Main.Graphics.TextureResource _background;
        private List<Gk3Main.Graphics.TextureResource> _title;
        private Gk3Main.Sound.Sound _ticktock;
 
        private int _timeAtStartRender;
        private const int _titleX = 14;
        private const int _titleY = 346;
        private const int _msPerTitleFrame = 60;

        public TimeBlockSplash(Gk3Main.Game.Timeblock timeblock)
        {
            string timeblockName = Gk3Main.Game.GameManager.GetTimeBlockString(timeblock);
            _background = (Gk3Main.Graphics.TextureResource)Gk3Main.Resource.ResourceManager.Load("TBT" + timeblockName + ".BMP");

            _title = new List<Gk3Main.Graphics.TextureResource>();
            int counter = 0;
            while(true)
            {
                Gk3Main.Graphics.TextureResource title = (Gk3Main.Graphics.TextureResource)Gk3Main.Resource.ResourceManager.Load("D" + timeblockName + "_" + (counter+1).ToString("00") + ".BMP");
                if (title == null || title.Loaded == false) break;
                
                _title.Add(title);

                counter++;
            }

            _ticktock = (Gk3Main.Sound.Sound)Gk3Main.Resource.ResourceManager.Load("CLOCKTIMEBLOCK.WAV");
            _ticktock.Play2D(Gk3Main.Sound.SoundTrackChannel.UI);
        }

        public void Dispose()
        {
            Gk3Main.Resource.ResourceManager.Unload(_background);
        }

        public void Render(Gk3Main.Graphics.SpriteBatch sb)
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
            if (_timeAtStartRender == 0) _timeAtStartRender = Gk3Main.Game.GameManager.TickCount;
            int frame = calcCurrentFrame(_timeAtStartRender, Gk3Main.Game.GameManager.TickCount);
            if (frame >= _title.Count) frame = _title.Count - 1;
            if (frame >= 0)
                sb.Draw(_title[frame], new Gk3Main.Math.Vector2(backgroundX + _titleX, backgroundY + _titleY));
        }

        private static int calcCurrentFrame(int timeAtLastFrame, int currentTime)
        {
            return (currentTime - timeAtLastFrame) / _msPerTitleFrame;
        }
    }
}
