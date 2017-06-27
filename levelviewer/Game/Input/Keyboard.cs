using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    enum KeyState
    {
        Up = 0,
        Down
    }

    struct KeyboardState
    {
        uint _state1;
        uint _state2;
        uint _state3;
        uint _state4;
        uint _state5;
        uint _state6;
        uint _state7;
        uint _state8;

        public KeyState GetState(Keys key)
        {
            uint stateIndex = (uint)key >> 5;
		    if (stateIndex >= 8) return KeyState.Up;

		    uint blah = (uint)1 << (int)key;

            uint state = 0;
            if (stateIndex == 0)
                state = _state1;
            else if (stateIndex == 1)
                state = _state2;
            else if (stateIndex == 2)
                state = _state3;
            else if (stateIndex == 3)
                state = _state4;
            else if (stateIndex == 4)
                state = _state5;
            else if (stateIndex == 5)
                state = _state6;
            else if (stateIndex == 6)
                state = _state7;
            else if (stateIndex == 7)
                state = _state8;

		    if ((blah & state) == 0)
			    return KeyState.Up;

		    return KeyState.Down;
        }

		public bool IsKeyDown(Keys key) { return GetState(key) == KeyState.Down; }
		public bool IsKeyUp(Keys key) { return GetState(key) == KeyState.Up; }

        public Keys[] GetPressedKeys()
        {
            List<Keys> keys = new List<Keys>();

		    for (int i = 0; i < 256; i++)
		    {
                if (GetState((Keys)i) == KeyState.Down)
                    keys.Add((Keys)i);
		    }

            return keys.ToArray();
        }

        public void SetState(Keys key, KeyState state)
        {
            uint stateIndex = (uint)key >> 5;

            if (state == KeyState.Down)
            {
                if (stateIndex == 0)
                    _state1 |= ((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 1)
                    _state2 |= ((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 2)
                    _state3 |= ((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 3)
                    _state4 |= ((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 4)
                    _state5 |= ((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 5)
                    _state6 |= ((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 6)
                    _state7 |= ((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 7)
                    _state8 |= ((uint)(1 << (int)key) & 0xffffffff);
            }
            else
            {
                if (stateIndex == 0)
                    _state1 &= ~((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 1)
                    _state2 &= ~((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 2)
                    _state3 &= ~((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 3)
                    _state4 &= ~((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 4)
                    _state5 &= ~((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 5)
                    _state6 &= ~((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 6)
                    _state7 &= ~((uint)(1 << (int)key) & 0xffffffff);
                else if (stateIndex == 7)
                    _state8 &= ~((uint)(1 << (int)key) & 0xffffffff);
            }
        }
    }

    class Keyboard
    {
        static KeyboardState _current;

        public static KeyboardState GetState() { return _current; }

        public static void UpdateFromOpenTK(OpenTK.Input.KeyboardState keyboard)
        {
            for (int i = 0; i < (int)OpenTK.Input.Key.LastKey; i++)
            {
                var key = (OpenTK.Input.Key)i;
                _current.SetState(translateOpenTKKey(key), keyboard.IsKeyDown(key) ? KeyState.Down : KeyState.Up);
            }
        }

        /*public static void UpdateFromSDLKeys(byte[] sdlkeys)
        {
            for (int i = 0; i < sdlkeys.Length; i++)
                _current.SetState(translateSDLK((SDL2.SDL.SDL_Keycode)i), sdlkeys[i] == 0 ? KeyState.Up : KeyState.Down); 
        }*/

        private static Keys translateOpenTKKey(OpenTK.Input.Key key)
        {
            switch(key)
            {
                case OpenTK.Input.Key.Tilde: return Keys.OemTilde;
                case OpenTK.Input.Key.Left: return Keys.Left;
                case OpenTK.Input.Key.Up: return Keys.Up;
                case OpenTK.Input.Key.Right: return Keys.Right;
                case OpenTK.Input.Key.Down: return Keys.Down;
                case OpenTK.Input.Key.LShift: return Keys.LeftShift;
                case OpenTK.Input.Key.RShift: return Keys.RightShift;
                case OpenTK.Input.Key.Space: return Keys.Space;
                case OpenTK.Input.Key.BackSpace: return Keys.Back;
                case OpenTK.Input.Key.Enter: return Keys.Enter;
                case OpenTK.Input.Key.Semicolon: return Keys.OemSemicolon;
                case OpenTK.Input.Key.Plus: return Keys.OemPlus;
                case OpenTK.Input.Key.Minus: return Keys.OemMinus;
                case OpenTK.Input.Key.Comma: return Keys.OemComma;
                case OpenTK.Input.Key.Period: return Keys.OemPeriod;
                case OpenTK.Input.Key.Slash: return Keys.OemQuestion;
                case OpenTK.Input.Key.BackSlash: return Keys.OemPipe;
                case OpenTK.Input.Key.Quote: return Keys.OemQuotes;
                default:
                    if ((int)key >= (int)OpenTK.Input.Key.A && (int)key <= (int)OpenTK.Input.Key.Z)
                        return Keys.A + (int)key - (int)OpenTK.Input.Key.A;
                    if ((int)key >= (int)OpenTK.Input.Key.Number0 && (int)key <= (int)OpenTK.Input.Key.Number9)
                        return Keys.D0 + (int)key - (int)OpenTK.Input.Key.Number0;

                    return Keys.X;
            }
        }

        /*private static Keys translateSDLK(SDL2.SDL.SDL_Keycode sdlk)
        {
            if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_BACKQUOTE)
                return Keys.OemTilde;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_LEFT)
                return Keys.Left;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_UP)
                return Keys.Up;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_RIGHT)
                return Keys.Right;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_DOWN)
                return Keys.Down;
            else if (sdlk >= SDL2.SDL.SDL_Keycode.SDLK_a && sdlk <= SDL2.SDL.SDL_Keycode.SDLK_z)
                return Keys.A + (sdlk - SDL2.SDL.SDL_Keycode.SDLK_a);
            else if (sdlk >= SDL2.SDL.SDL_Keycode.SDLK_0 && sdlk <= SDL2.SDL.SDL_Keycode.SDLK_9)
                return Keys.D0 + (sdlk - SDL2.SDL.SDL_Keycode.SDLK_0);
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_LSHIFT)
                return Keys.LeftShift;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_RSHIFT)
                return Keys.RightShift;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_SPACE)
                return Keys.Space;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE)
                return Keys.Back;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_RETURN)
                return Keys.Enter;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_SEMICOLON)
                return Keys.OemSemicolon;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_PLUS)
                return Keys.OemPlus;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_COMMA)
                return Keys.OemComma;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_MINUS)
                return Keys.OemMinus;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_PERIOD)
                return Keys.OemPeriod;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_SLASH)
                return Keys.OemQuestion;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_BACKSLASH)
                return Keys.OemPipe;
            else if (sdlk == SDL2.SDL.SDL_Keycode.SDLK_QUOTE)
                return Keys.OemQuotes;

            return Keys.X;
        }*/
    }
}
