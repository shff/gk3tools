using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gk3Main.Resource
{
    public class InfoResourceException : Exception
    {
        public InfoResourceException(string filename)
            : base(filename + " is not valid")
        {
        }

        public InfoResourceException(string filename, string message)
            : base(filename + " is not valid because: " + message)
        {

        }
    }

    /// <summary>
    /// Base class for loading "info" files, like .scn, .sif, etc. files
    /// </summary>
    public class InfoResource : TextResource
    {
        public InfoResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            int currentIndex = 0;
            string[] lines = Text.Split('\n');

            // parse the global section
            _globalSection = new InfoSection(lines, ref currentIndex, true);

            // parse the rest
            while (currentIndex < lines.Length - 1)
            {
                InfoSection section = new InfoSection(lines, ref currentIndex, false);

                _sections.Add(section);
            }
        }

        public static bool TryParse2f(string str, out float f1, out float f2)
        {
            int firstBracket = str.IndexOf('{');
            int comma = str.IndexOf(',');
            int lastBracket = str.IndexOf('}');

            if (Utils.TryParseFloat(str, firstBracket + 1, comma - firstBracket - 1, out f1) &&
                Utils.TryParseFloat(str, comma + 1, lastBracket - comma - 1, out f2))
                return true;

            f1 = f2 = 0;
            return false;
        }

        public static bool TryParse3f(string str, out float f1, out float f2, out float f3)
        {
            int firstBracket = str.IndexOf('{');
            int firstComma = str.IndexOf(',');
            int secondComma = str.IndexOf(',', firstComma + 1);
            int lastBracket = str.IndexOf('}');

            if (Utils.TryParseFloat(str, firstBracket + 1, firstComma - firstBracket - 1, out f1) &&
                Utils.TryParseFloat(str, firstComma + 1, secondComma - firstComma - 1, out f2) &&
                Utils.TryParseFloat(str, secondComma + 1, lastBracket - secondComma - 1, out f3))
                return true;

            f1 = f2 = f3 = 0;
            return false;
        }

        public InfoSection GlobalSection { get { return _globalSection; } }
        public List<InfoSection> Sections { get { return _sections; } }

        protected InfoSection _globalSection;
        protected List<InfoSection> _sections = new List<InfoSection>();
    }

    public class InfoSection
    {
        public InfoSection(string[] lines, ref int startIndex, bool global)
        {
            if (global == false)
            {
                // skip to the next section header
                while (lines[startIndex].StartsWith("[") == false) startIndex++;

                // now we should be at a section header, so parse it
                Match match = Regex.Match(lines[startIndex], @"^\[([\w|(|)]+)(?:={(.+)})?]?");

                if (match.Success == false)
                    throw new InfoResourceException("??", lines[startIndex] + " fails regex");

                _name = match.Groups[1].Value;
                _condition = match.Groups[2].Value;

                startIndex++;
            }

            while (startIndex < lines.Length &&
                lines[startIndex].StartsWith("[") == false)
            {
                string line = lines[startIndex].Trim();
                if (line.StartsWith("//") || line == "")
                {
                    startIndex++;
                    continue;
                }

                InfoLine infoline = new InfoLine(line, this);

                _lines.Add(infoline);

                startIndex++;
            }

        }

        public string Name { get { return _name; } }
        public string Condition { get { return _condition; } }
        public List<InfoLine> Lines { get { return _lines; } }

        private string _name;
        private string _condition;

        private List<InfoLine> _lines = new List<InfoLine>();
    }

    public class InfoLine
    {
        public InfoLine(string line, InfoSection section)
        {
            _section = section;

            // line should look like either
            //    foo=bar
            //    foo,bar=baz

            // remove any comments at the end
            int comment = line.IndexOf("//");
            if (comment >= 0)
                line = line.Substring(0, comment).Trim();
            
            // break apart the line
            //MatchCollection matches = Regex.Matches(line, @"([\w]+={[^}]+}?)|[^,]+");
            MatchCollection matches = Regex.Matches(line, @"([\w\.\t ]+={[^}]+})|([\w\.\t ]+=[^,]+)|([\w\.\-]+)");

            // each match represents a piece of data, either a single 'key'
            // or something like 'key=value' (where value could be something
            // like '{1,2,3}')
            foreach (Match match in matches)
            {
                if (match.Success == false)
                    throw new InfoResourceException("??", line + " failed regex");

                int equals = match.Value.IndexOf('=');

                // check for a value sitting at the beginning of the line without a '='
                if (equals == -1 && _attributes.Count == 0 && _value == null)
                {
                    _value = match.Value;
                }
                else
                {
                    KeyValuePair<string, string> keyvalue;

                    if (equals == -1)
                    {
                        // assume it's a boolean flag thingy
                        keyvalue = new KeyValuePair<string, string>(match.Value, "true");
                    }
                    else
                    {
                        keyvalue = new KeyValuePair<string, string>
                        (
                            match.Value.Substring(0, equals).Trim(),
                            match.Value.Substring(equals + 1).Trim()
                        );
                    }

                    _attributes.Add(keyvalue);
                }
            }
        }

        public bool TryGetAttribute(string name, out string value)
        {
            foreach (KeyValuePair<string, string> attribute in _attributes)
            {
                if (attribute.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    value = attribute.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public bool TryGetIntAttribute(string name, out int value)
        {
            foreach (KeyValuePair<string, string> attribute in _attributes)
            {
                if (attribute.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(attribute.Value, out value))
                        return true;
                }
            }

            value = 0;
            return false;
        }

        public bool TryGetFloatAttribute(string name, out float value)
        {
            foreach (KeyValuePair<string, string> attribute in _attributes)
            {
                if (attribute.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(attribute.Value, out value))
                        return true;
                }
            }

            value = 0;
            return false;
        }

        public bool TryGetFloat2Attribute(string name, out float v1, out float v2)
        {
            v1 = 0;
            v2 = 0;

            foreach (KeyValuePair<string, string> attribute in _attributes)
            {
                if (attribute.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    int open = attribute.Value.IndexOf('{');
                    int comma = attribute.Value.IndexOf(',');
                    int close = attribute.Value.IndexOf('}');

                    if (open < 0 || comma < 0 || close < 0)
                        break;

                    return float.TryParse(attribute.Value.Substring(open + 1, comma - open - 1), out v1) &&
                        float.TryParse(attribute.Value.Substring(comma + 1, close - comma - 1), out v2);
                }
            }

            return false;
        }

        public string Value { get { return _value; } }
        public List<KeyValuePair<string, string>> Attributes { get { return _attributes; } }

        private List<KeyValuePair<string, string>> _attributes = new List<KeyValuePair<string, string>>();
        private string _value;
        private InfoSection _section;
    }
}
