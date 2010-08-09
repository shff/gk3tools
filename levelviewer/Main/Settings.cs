using System;
using System.Collections.Generic;
using System.IO;

namespace Gk3Main
{
    public static class Settings
    {
        private static string _settingsFilename;
        private static string _renderer;
        private static int _screenWidth, _screenHeight;
        private static int _colorDepth;
        private static bool _fullscreen;

        private struct LineInfo
        {
            public string RawString;
            public string Variable;
            public string Value;
        }

        public static void Save()
        {
            Save(_settingsFilename);
        }

        public static void Save(string filename)
        {
            // first open the file and parse it
            List<LineInfo> lines = readLines(filename);

            updateLine(lines, "renderer", _renderer);
            updateLine(lines, "screenWidth", _screenWidth.ToString());
            updateLine(lines, "screenHeight", _screenHeight.ToString());

            using (StreamWriter writer = File.CreateText(filename))
            {
                foreach (LineInfo line in lines)
                {
                    if (string.IsNullOrEmpty(line.Variable))
                        writer.WriteLine(line.RawString);
                    else
                    {
                        writer.Write(line.Variable);
                        writer.Write("=");
                        writer.WriteLine(line.Value);
                    }
                }
            }

            
        }

        public static void Load(string filename)
        {
            _settingsFilename = filename;
            List<LineInfo> lines = readLines(filename);

            _renderer = getStringSetting(lines, "renderer");
            _screenWidth = getIntSetting(lines, "screenWidth", 640);
            _screenHeight = getIntSetting(lines, "screenHeight", 480);
        }

        public static string Renderer
        {
            get { return _renderer; }
            set { _renderer = value; }
        }

        public static int ScreenWidth
        {
            get { return _screenWidth; }
            set { _screenWidth = value; }
        }

        public static int ScreenHeight
        {
            get { return _screenHeight; }
            set { _screenHeight = value; }
        }

        public static int ColorDepth
        {
            get { return _colorDepth; }
            set { _colorDepth = value; }
        }

        public static bool Fullscreen
        {
            get { return _fullscreen; }
            set { _fullscreen = value; }
        }

        private static List<LineInfo> readLines(string filename)
        {
            List<LineInfo> lines = new List<LineInfo>();
            try
            {
                using (StreamReader reader = File.OpenText(filename))
                {
                    while (reader.Peek() >= 0)
                    {
                        LineInfo line = new LineInfo();
                        line.RawString = reader.ReadLine();

                        int equal = line.RawString.IndexOf('=');
                        if (equal >= 0)
                        {
                            line.Variable = line.RawString.Substring(0, equal).Trim();
                            line.Value = line.RawString.Substring(equal + 1).Trim();
                        }

                        lines.Add(line);
                    }
                }
            }
            catch
            {
                // guess we couldn't read the file... oh well.
            }

            return lines;
        }

        private static void updateLine(List<LineInfo> lines, string variable, string value)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Variable) == false &&
                    lines[i].Variable.Equals(variable, StringComparison.OrdinalIgnoreCase))
                {
                    LineInfo updatedLine = lines[i];
                    updatedLine.Value = value;
                    lines[i] = updatedLine;

                    return;
                }
            }

            // guess we couldn't find an existing line with that variable, so add it
            LineInfo line;
            line.Variable = variable;
            line.Value = value;
            line.RawString = variable + "=" + value;

            lines.Add(line);
        }

        private static int getIntSetting(List<LineInfo> lines, string variable, int defaultSetting)
        {
            foreach (LineInfo line in lines)
            {
                if (string.IsNullOrEmpty(line.Variable) == false &&
                    line.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase))
                {
                    int result;
                    if (int.TryParse(line.Value, out result))
                        return result;
                }
            }

            // we couldn't find the setting :(
            return defaultSetting;
        }

        private static bool getBoolSetting(List<LineInfo> lines, string variable, bool defaultSetting)
        {
            foreach (LineInfo line in lines)
            {
                if (string.IsNullOrEmpty(line.Variable) == false && 
                    line.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase))
                {
                    bool result;
                    if (bool.TryParse(line.Value, out result))
                        return result;
                }
            }

            // we couldn't find the setting :(
            return defaultSetting;
        }

        private static string getStringSetting(List<LineInfo> lines, string variable)
        {
            foreach (LineInfo line in lines)
            {
                if (string.IsNullOrEmpty(line.Variable) == false && 
                    line.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Value;
                }
            }

            // we couldn't find the setting :(
            return null;
        }
    }
}
