using System;
using System.Collections.Generic;
using System.Text;

using Tao.Sdl;

namespace gk3levelviewer
{
    static class Input
    {
        public static bool Tick()
        {
            Sdl.SDL_Event sdlevent;
            while (Sdl.SDL_PollEvent(out sdlevent) != 0)
            {
                switch (sdlevent.type)
                {
                    case Sdl.SDL_QUIT:
                        return false;
                }
            }

            int numkeys;
            _keys = Sdl.SDL_GetKeyState(out numkeys);

            if (_keys[Sdl.SDLK_ESCAPE] != 0)
                return false;

            return true;
        }

        public static bool IsKeyPressed(int key)
        {
            if (key >= _keys.Length) return false;

            return _keys[key] != 0;
        }

        public static void GetRelMouseCoords(out int x, out int y)
        {
            Sdl.SDL_GetRelativeMouseState(out x, out y);
        }

        private static byte[] _keys;
    }
}
