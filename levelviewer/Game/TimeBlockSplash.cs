using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class TimeBlockSplash : IDisposable 
    {
        private Gk3Main.Graphics.TextureResource _background;
        private Gk3Main.Graphics.TextureResource[] _title;
        private Gk3Main.Sound.Sound _ticktock;
 
        private int _timeAtStartRender;
        private const int _titleX = 14;
        private const int _titleY = 346;
        private const int _titleLength = 14;
        private const int _msPerTitleFrame = 60;

        public TimeBlockSplash(string name)
        {
            _background = (Gk3Main.Graphics.TextureResource)Gk3Main.Resource.ResourceManager.Load("TBT" + name + ".BMP");

            _title = new Gk3Main.Graphics.TextureResource[_titleLength];
            for (int i = 0; i < _titleLength; i++)
            {
                _title[i] = (Gk3Main.Graphics.TextureResource)Gk3Main.Resource.ResourceManager.Load("D" + name + "_" + (i+1).ToString("00") + ".BMP");
            }

            _ticktock = (Gk3Main.Sound.Sound)Gk3Main.Resource.ResourceManager.Load("CLOCKTIMEBLOCK.WAV");
            _ticktock.Play2D();
        }

        public void Dispose()
        {
            Gk3Main.Resource.ResourceManager.Unload(_background);
        }

        public void Render()
        {
            Gk3Main.Graphics.IRenderer renderer = Gk3Main.Graphics.RendererManager.CurrentRenderer;

            // draw the background centered in the screen
            Gk3Main.Graphics.Viewport viewport = renderer.Viewport;
            int centerX = viewport.Width / 2 + viewport.X;
            int centerY = viewport.Height / 2 + viewport.Y;
            int backgroundX = centerX - _background.Width / 2;
            int backgroundY = centerY - _background.Height / 2;

            Gk3Main.Graphics.Utils.Blit(backgroundX, backgroundY, _background);

            // draw the title
            if (_timeAtStartRender == 0) _timeAtStartRender = Gk3Main.Game.GameManager.TickCount;
            int frame = calcCurrentFrame(_timeAtStartRender, Gk3Main.Game.GameManager.TickCount);
            if (frame >= _titleLength) frame = _titleLength - 1;
            Gk3Main.Graphics.Utils.Blit(backgroundX + _titleX, backgroundY + _titleY, _title[frame]);
        }

        private static int calcCurrentFrame(int timeAtLastFrame, int currentTime)
        {
            return (currentTime - timeAtLastFrame) / _msPerTitleFrame;
        }
    }
}
