using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    static class Input
    {
        private static bool _leftMousePressed;
        private static bool _middleMousePressed;
        private static bool _rightMousePressed;

        private static bool _oldLeftMousePressed;
        private static bool _oldMiddleMousePressed;
        private static bool _oldRightMousePressed;

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

        public static void Refresh(bool leftMousePressed, bool middleMousePressed, bool rightMousePressed)
        {
            _oldLeftMousePressed = _leftMousePressed;
            _oldMiddleMousePressed = _middleMousePressed;
            _oldRightMousePressed = _rightMousePressed;

            _leftMousePressed = leftMousePressed;
            _middleMousePressed = middleMousePressed;
            _rightMousePressed = rightMousePressed;
        }
    }
}
