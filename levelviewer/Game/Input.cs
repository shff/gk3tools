using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
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

        private static byte[] _keys;

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

        public static byte[] Keys
        {
            get { return _keys; }
        }

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

            _keys = keys;
        }
    }
}
