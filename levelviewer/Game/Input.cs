using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    enum Keys
    {
        None,
        Back,
        Tab,
        Enter,

        Space = 32,

        Left = 37,
        Up = 38,
        Right = 39,
        Down = 40,

        D0 = 48,
        D1 = 49,
        D2 = 50,
        D3 = 51,
        D4 = 52,
        D5 = 53,
        D6 = 54,
        D7 = 55,
        D8 = 56,
        D9 = 57,

        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,

        LeftShift = 160,
        RightShift = 161,

        OemSemicolon = 186,
        OemPlus = 187,
        OemComma = 188,
        OemMinus = 189,
        OemPeriod = 190,
        OemQuestion = 191,
        OemTilde = 192,
        OemPipe = 220,
        OemQuotes = 222
    }

    static class Input
    {
        private static int _mouseX;
        private static int _mouseY;
        private static int _oldMouseX;
        private static int _oldMouseY;
        private static int _relativeMouseX;
        private static int _relativeMouseY;

        private static bool _leftMousePressed;
        private static bool _middleMousePressed;
        private static bool _rightMousePressed;

        private static bool _oldLeftMousePressed;
        private static bool _oldMiddleMousePressed;
        private static bool _oldRightMousePressed;

        private static KeyboardState _oldKeys;
        private static KeyboardState _currentKeys;

        public static int MouseX
        {
            get { return _mouseX; }
        }

        public static int MouseY
        {
            get { return _mouseY; }
        }

        public static int RelMouseX
        {
            get { return _relativeMouseX; }
        }

        public static int RelMouseY
        {
            get { return _relativeMouseY; }
        }

        public static bool LeftMousePressed
        {
            get { return _leftMousePressed; }
        }

        public static bool RightMousePressed
        {
            get { return _rightMousePressed; }
        }

        public static bool LeftMousePressedFirstTime
        {
            get { return _leftMousePressed && !_oldLeftMousePressed; }
        }

        public static bool RightMousePressedFirstTime
        {
            get { return _rightMousePressed && !_oldRightMousePressed; }
        }

        public static bool LeftMouseReleasedFirstTime
        {
            get { return !_leftMousePressed && _oldLeftMousePressed; }
        }

        public static bool RightMouseReleasedFirstTime
        {
            get { return !_rightMousePressed && _oldRightMousePressed; }
        }

        public static bool KeyboardButtonPressedFirstTime(Keys key)
        {
            return _currentKeys.IsKeyDown(key) && _oldKeys.IsKeyUp(key);
        }

        public static KeyboardState CurrentKeys { get { return _currentKeys; } }
        public static KeyboardState PreviousKeys { get { return _oldKeys; } }

        public static void Refresh(int mouseX, int mouseY, bool leftMousePressed, bool middleMousePressed, bool rightMousePressed, byte[] keys)
        {
            _oldMouseX = _mouseX;
            _oldMouseY = _mouseY;
            _mouseX = mouseX;
            _mouseY = mouseY;
            _relativeMouseX = _mouseX - _oldMouseX;
            _relativeMouseY = _mouseY - _oldMouseY;

            _oldLeftMousePressed = _leftMousePressed;
            _oldMiddleMousePressed = _middleMousePressed;
            _oldRightMousePressed = _rightMousePressed;

            _leftMousePressed = leftMousePressed;
            _middleMousePressed = middleMousePressed;
            _rightMousePressed = rightMousePressed;

            _oldKeys = _currentKeys;
            Keyboard.UpdateFromSDLKeys(keys);
            _currentKeys = Keyboard.GetState();
        }
    }
}
