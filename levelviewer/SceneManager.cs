// Copyright (c) 2007 Brad Farris
// This file is part of the GK3 Scene Viewer.

// The GK3 Scene Viewer is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// The GK3 Scene Viewer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

namespace gk3levelviewer
{
    enum ShadeMode
    {
        Wireframe,
        Flat,
        Colored,
        Textured
    }

    static class SceneManager
    {
        public static void LoadScene(string scn)
        {
            ScnFileReader reader = new ScnFileReader(scn);

            string bspFile = Utils.GetFilenameWithoutExtension(scn) + ".BSP";
            foreach (ScnLine line in reader.Lines)
            {
                if (line.Type == ScnLineType.KeyValue)
                {
                    if (line.Key == "BSP")
                        bspFile = line.Value.ToUpper() + ".BSP";
                }
            }

            // load the BSP
            if (_currentRoom != null)
                Resource.ResourceManager.Unload(_currentRoom);

            _currentRoom = (Graphics.BspResource)Resource.ResourceManager.Load(bspFile);

            // load the lightmaps
            if (_currentLightmaps != null)
                Resource.ResourceManager.Unload(_currentLightmaps);

            string lightmapFile = Utils.GetFilenameWithoutExtension(scn) + ".MUL";
            _currentLightmaps = (Graphics.LightmapResource)Resource.ResourceManager.Load(lightmapFile);
        }

        public static void Render()
        {
            if (_currentCamera != null)
                _currentCamera.Update();

            // render the room
            if (_currentRoom != null)
                _currentRoom.Render(_currentLightmaps);

            // render the models

            // present the wonderful results!
            Graphics.Video.Present();
        }

        public static bool LightmapsEnabled
        {
            get { return _lightmapsEnabled; }
            set { _lightmapsEnabled = value; }
        }

        public static ShadeMode CurrentShadeMode
        {
            get { return _shadeMode; }
            set { _shadeMode = value; }
        }

        public static Camera CurrentCamera
        {
            get { return _currentCamera; }
            set { _currentCamera = value; }
        }

        private static Graphics.BspResource _currentRoom;
        private static Graphics.LightmapResource _currentLightmaps;

        private static ShadeMode _shadeMode = ShadeMode.Textured;
        private static bool _lightmapsEnabled = false;

        private static Camera _currentCamera;
    }

    #region SCN file stuff

    enum ScnLineType
    {
        Comment,
        Label,
        ListItem,
        KeyValue
    }

    struct ScnLine
    {
        public ScnLine(ScnLineType type, string text)
        {
            Type = type;
            Text = text;
        }

        public ScnLineType Type;
        public string Text;

        public string Key
        {
            get
            {
                if (Type != ScnLineType.KeyValue)
                    return Text;

                string[] keyvalue = Text.Split('=');
                return keyvalue[0];
            }
        }

        public string Value
        {
            get
            {
                if (Type != ScnLineType.KeyValue)
                    return Text;

                string[] keyvalue = Text.Split('=');
                return keyvalue[1];
            }
        }
    }

    class ScnFileReader
    {
        public ScnFileReader(string name)
        {
            _lines = new List<ScnLine>();

            Resource.TextResource res = (Resource.TextResource)Resource.ResourceManager.Load(name);

            string[] lines = res.Text.Split('\n');
            foreach(string line in lines)
            {
                if (line.StartsWith("//"))
                    _lines.Add(new ScnLine(ScnLineType.Comment, line.Trim()));
                else if (line.StartsWith("[") && line.EndsWith("]"))
                    _lines.Add(new ScnLine(ScnLineType.Label, line.Trim()));
                else if (line.Contains("="))
                    _lines.Add(new ScnLine(ScnLineType.KeyValue, line.Trim()));
                else
                    _lines.Add(new ScnLine(ScnLineType.ListItem, line.Trim()));
            }

            Resource.ResourceManager.Unload(res);
        }

        public List<ScnLine> Lines { get { return _lines; } }

        private List<ScnLine> _lines;
    }

    #endregion
}
