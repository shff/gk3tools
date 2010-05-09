using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class FaceDefinition : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string Name;
        public string FaceName;

        // eye stuff
        public string LeftEyeName;
        public string RightEyeName;
        public Math.Vector2 LeftEyeOffset;
        public Math.Vector2 RightEyeOffset;
        public Math.Vector2 LeftEyeBias;
        public Math.Vector2 RightEyeBias;
        public Math.Vector2 MaxLookDistance;
        public Math.Vector2 JitterFrequency;
        public Math.Vector2 MaxJitterDistance;
        public Math.Vector2 BlinkFrequency;

        // 3D eye stuff
        public Math.Vector2 EyeFieldOfView;
        public Math.Vector2 EyeShortFieldOfView;
        public float EyeSeparation;
        public float HeadRadius;
        public Math.Vector3 HeadCenterOffset;

        // forehead stuff

        // eyelid stuff

        // mouth stuff
        public Math.Vector2 MouthOffset;
        public Math.Vector2 MouthSize;
    }

    public static class FaceDefinitions
    {
        private static FaceDefinition _default;
        private static Dictionary<string, FaceDefinition> _faces = new Dictionary<string,FaceDefinition>(StringComparer.OrdinalIgnoreCase);

        public static void Load()
        {
            Resource.TextResource faces = new Resource.TextResource("faces.txt", FileSystem.Open("faces.txt"));

            string[] lines = faces.Text.Split('\n');

            FaceDefinition currentDefinition = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("["))
                {
                    // read the header
                    int ending = lines[i].IndexOf(']');
                    string name = lines[i].Substring(1, ending - 1);

                    if (name.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition = new FaceDefinition();
                        currentDefinition.Name = name;
                        _default = currentDefinition;
                    }
                    else if (name.Equals("EYES", StringComparison.OrdinalIgnoreCase))
                    {
                        // skip the eyes section for now
                        while (true)
                        {
                            i++;
                            if (lines[i].StartsWith("["))
                            {
                                i--;
                                break;
                            }
                        }
                    }
                    else
                    {
                        currentDefinition = (FaceDefinition)_default.Clone();
                        currentDefinition.Name = name;
                        currentDefinition.FaceName = name + "_face";
                        _faces.Add(currentDefinition.Name, currentDefinition);
                    }
                }
                else if (lines[i].StartsWith("//") == false)
                {
                    int equals = lines[i].IndexOf('=');
                    if (equals < 0) continue;

                    string attributeName = lines[i].Substring(0, equals).Trim();
                    string data = lines[i].Substring(equals + 1);

                    // remove any trailing comments from data
                    int indexOfComment = data.IndexOf("//");
                    if (indexOfComment >= 0)
                        data = data.Substring(0, indexOfComment);
                    
                    data = data.Trim();

                    float fValue = 0;
                    Math.Vector3 v3Value = Math.Vector3.Zero;
                    Math.Vector2 v2Value = Math.Vector2.Zero;

                    // parse as float
                    float.TryParse(data, out fValue);

                    // parse as vector
                    int firstComma = data.IndexOf(',');
                    int secondComma = data.IndexOf(',', firstComma+1);
                    int firstX = data.IndexOf('x');
                    int trailingComment = data.IndexOf('/');

                    // ignore anything after the //
                    if (trailingComment >= 0)
                    {
                        if (trailingComment < firstComma)
                            firstComma = -1;
                        if (trailingComment < secondComma)
                            secondComma = -1;
                        if (trailingComment < firstX)
                            firstX = -1;
                    }

                    if (firstComma >= 0 && (firstX < 0 || firstComma < firstX))
                    {
                        Utils.TryParseFloat(data, 0, firstComma, out v3Value.X);

                        if (secondComma >= 0)
                        {
                            Utils.TryParseFloat(data, firstComma + 1, secondComma - firstComma - 1, out v3Value.Y);

                            if (trailingComment >= 0)
                                Utils.TryParseFloat(data, secondComma + 1, trailingComment - secondComma - 1, out v3Value.Z);
                            else
                                Utils.TryParseFloat(data, secondComma + 1, out v3Value.Z);
                        }
                        else
                        {
                            if (trailingComment >= 0)
                                Utils.TryParseFloat(data, firstComma + 1, trailingComment - firstComma - 1, out v3Value.Y);
                            else
                                Utils.TryParseFloat(data, firstComma + 1, out v3Value.Y);
                        }
                    }
                    else if (firstX >= 0)
                    {
                        // some fields look like "NUMBERxNUMBER"
                        Utils.TryParseFloat(data, 0, firstX, out v3Value.X);

                        if (trailingComment >= 0)
                            Utils.TryParseFloat(data, firstX + 1, trailingComment - firstX - 1, out v3Value.Y);
                        else
                            Utils.TryParseFloat(data, firstX + 1, out v3Value.Y);
                    }
                    v2Value.X = v3Value.X;
                    v2Value.Y = v3Value.Y;


                    if (attributeName.Equals("Face Name", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.FaceName = data;
                    }
                    // eye stuff
                    else if (attributeName.Equals("Left Eye Name", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.LeftEyeName = data;
                    }
                    else if (attributeName.Equals("Right Eye Name", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.RightEyeName = data;
                    }
                    else if (attributeName.Equals("Left Eye Offset", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.LeftEyeOffset = v2Value;
                    }
                    else if (attributeName.Equals("Right Eye Offset", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.RightEyeOffset = v2Value;
                    }
                    else if (attributeName.Equals("Left Eye Bias", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.LeftEyeBias = v2Value;
                    }
                    else if (attributeName.Equals("Right Eye Bias", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.RightEyeBias = v2Value;
                    }
                    else if (attributeName.Equals("Max Look Distance", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.MaxLookDistance = v2Value;
                    }
                    else if (attributeName.Equals("Jitter Frequency", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.JitterFrequency = v2Value;
                    }
                    else if (attributeName.Equals("Max Jitter Distance", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.MaxJitterDistance = v2Value;
                    }

                    // 3D eye stuff
                    else if (attributeName.Equals("Eye Field Of View", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.EyeFieldOfView = v2Value;
                    }
                    else if (attributeName.Equals("Eye Short Field Of View", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.EyeShortFieldOfView = v2Value;
                    }
                    else if (attributeName.Equals("Eye Separation", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.EyeSeparation = fValue;
                    }
                    else if (attributeName.Equals("Head Radius", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.HeadRadius = fValue;
                    }
                    else if (attributeName.Equals("Head Center Offset", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.HeadCenterOffset = v3Value;
                    }
                    else if (attributeName.Equals("Blink Frequency", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.BlinkFrequency = v2Value;
                    }

                    // mouth stuff
                    else if (attributeName.Equals("Mouth Offset", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.MouthOffset = v2Value;
                    }
                    else if (attributeName.Equals("Mouth Size", StringComparison.OrdinalIgnoreCase))
                    {
                        currentDefinition.MouthSize = v2Value;
                    }
                }
            }
        }

        public static FaceDefinition GetFaceDefinition(string actorName)
        {
            FaceDefinition result;
            _faces.TryGetValue(actorName, out result);

            return result;
        }
    }
}
