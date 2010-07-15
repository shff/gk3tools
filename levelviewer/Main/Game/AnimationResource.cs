using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    internal struct AnimationResourceSectionLineParam
    {
        public string StringValue;
        public int IntValue;
    }

    internal class AnimationResourceSectionLine
    {
        public int FrameNum;
        public List<AnimationResourceSectionLineParam> Params = new List<AnimationResourceSectionLineParam>();
    }

    internal class AnimationResourceSection
    {
        public string SectionName;
        public List<AnimationResourceSectionLine> Lines = new List<AnimationResourceSectionLine>();
    }

    public class AnimationResource : Resource.TextResource
    {
        private int _numFrames;
        private List<AnimationResourceSection> _sections;

        private class LineComparer : IComparer<AnimationResourceSectionLine>
        {
            public int Compare(AnimationResourceSectionLine line1, AnimationResourceSectionLine line2)
            {
                return line1.FrameNum - line2.FrameNum;
            }

            public static LineComparer Instance = new LineComparer();
        }

        public const int MillisecondsPerFrame = 67; // about 15 fps

        public AnimationResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            _sections = new List<AnimationResourceSection>();

            string[] lines = Text.Split('\n');

            bool readingFileHeaderSection = false;
            bool expectingLineCount = false;
            bool expectingSectionHeader = true;
            int currentSectionLineCount = 0;
            int linesReadInCurrentSection = 0;

            foreach (string line in lines)
            {
                if (expectingSectionHeader)
                {
                    // read the section header
                    if (line.StartsWith("["))
                    {
                        string sectionName = line.Substring(1, line.IndexOf(']') - 1);
                        expectingLineCount = true;
                        expectingSectionHeader = false;

                        // create a new section if this isn't the header
                        if (sectionName.Equals("HEADER", StringComparison.OrdinalIgnoreCase) == false)
                        {
                            AnimationResourceSection section = new AnimationResourceSection();
                            section.SectionName = sectionName;
                            _sections.Add(section);
                            readingFileHeaderSection = false;
                        }
                        else
                        {
                            readingFileHeaderSection = true;
                        }
                    }
                }
                else if (expectingLineCount)
                {
                    if (int.TryParse(line, out currentSectionLineCount))
                    {
                        if (currentSectionLineCount == 0)
                        {
                            expectingLineCount = false;
                            expectingSectionHeader = true;
                        }
                        else
                        {
                            expectingLineCount = false;
                            linesReadInCurrentSection = 0;
                        }
                        
                        // was this the header?
                        if (readingFileHeaderSection)
                        {
                            _numFrames = currentSectionLineCount;
                            expectingSectionHeader = true;
                        }
                    }
                }
                else
                {
                    // very first thing on each line should be an integer, so try that first
                    int frame;
                    if (Utils.TryParseInt(line, 0, line.IndexOf(','), out frame))
                    {
                        // yay! this must be a valid line!
                        AnimationResourceSectionLine sectionLine = new AnimationResourceSectionLine();
                        sectionLine.FrameNum = frame;

                        linesReadInCurrentSection++;

                        int nextComma = line.IndexOf(',');
                        while (nextComma > 0)
                        {
                            AnimationResourceSectionLineParam param = new AnimationResourceSectionLineParam();

                            int prevComma = nextComma;
                            nextComma = line.IndexOf(',', nextComma + 1);

                            string svalue;
                            if (nextComma < 0)
                                svalue = line.Substring(prevComma + 1).Trim();
                            else
                                svalue = line.Substring(prevComma + 1, nextComma - prevComma - 1);

                            int ivalue;
                            if (int.TryParse(svalue, out ivalue))
                            {
                                param.IntValue = ivalue;
                            }
                            else
                            {
                                param.StringValue = svalue;
                            }

                            // add the parameter to the line
                            sectionLine.Params.Add(param);
                        }

                        // now add the line to the section
                        _sections[_sections.Count - 1].Lines.Add(sectionLine);

                        if (linesReadInCurrentSection >= currentSectionLineCount)
                        {
                            // all lines were read, so we're ready for another section
                            expectingSectionHeader = true;

                            // sort the lines by frame # for easier usage later
                            _sections[_sections.Count - 1].Lines.Sort(LineComparer.Instance);
                        }
                    }
                }
            }
        }

        public int NumFrames { get { return _numFrames; } }
        internal List<AnimationResourceSection> Sections { get { return _sections; } }

        internal static void GetAllFramesSince(AnimationResourceSection section, int timeSinceStart, int duration, int millisecondsPerFrame,
            out int startIndex, out int count)
        {
            startIndex = -1;
            count = 0;

            int startFrame = timeSinceStart / millisecondsPerFrame;
            int stopFrame = (timeSinceStart + duration) / millisecondsPerFrame;
            for (int i = 0; i < section.Lines.Count; i++)
            {
                // is this frame inside the time range we're looking for?
                if (section.Lines[i].FrameNum >= startFrame && section.Lines[i].FrameNum <= stopFrame)
                {
                    // is this the first frame?
                    if (startIndex < 0)
                    {
                        startIndex = i;
                    }
                    
                    count++;
                }
                else if (startIndex >= 0)
                {
                    // we've already found the first frame, and now we've
                    // gone past the range of frames, so we're done!
                    return;
                }
            }
        }
    }
}
