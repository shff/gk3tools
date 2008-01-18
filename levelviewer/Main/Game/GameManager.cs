using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    enum Timeblock
    {
        Day1_10AM,
        Day1_12PM,
        Day1_02PM,
        Day1_04PM,
        Day1_06PM,

        Day2_07AM,
        Day2_10AM,
        Day2_12PM,
        Day2_02PM,
        Day2_05PM,
        Day2_02AM,

        Day3_07AM,
        Day3_10AM,
        Day3_12PM,
        Day3_03PM,
        Day3_06PM,
        Day3_09PM
    }

    static class GameManager
    {
        public static Timeblock CurrentTime
        {
            get { return _currentTime; }
            set { _currentTime = value; }
        }

        public static string CurrentTimeString
        {
            get
            {
                switch (_currentTime)
                {
                    case Timeblock.Day1_10AM: return TimeCodes.Day1_10AM;
                    case Timeblock.Day1_12PM: return TimeCodes.Day1_12PM;
                    case Timeblock.Day1_02PM: return TimeCodes.Day1_02PM;
                    case Timeblock.Day1_04PM: return TimeCodes.Day1_04PM;
                    case Timeblock.Day1_06PM: return TimeCodes.Day1_06PM;
                    
                    case Timeblock.Day2_07AM: return TimeCodes.Day2_07AM;
                    case Timeblock.Day2_10AM: return TimeCodes.Day2_10AM;
                    case Timeblock.Day2_12PM: return TimeCodes.Day2_12PM;
                    case Timeblock.Day2_02PM: return TimeCodes.Day2_02PM;
                    case Timeblock.Day2_05PM: return TimeCodes.Day2_05PM;
                    case Timeblock.Day2_02AM: return TimeCodes.Day2_02AM;

                    case Timeblock.Day3_07AM: return TimeCodes.Day3_07AM;
                    case Timeblock.Day3_10AM: return TimeCodes.Day3_10AM;
                    case Timeblock.Day3_12PM: return TimeCodes.Day3_12PM;
                    case Timeblock.Day3_03PM: return TimeCodes.Day3_03PM;
                    case Timeblock.Day3_06PM: return TimeCodes.Day3_06PM;
                    case Timeblock.Day3_09PM: return TimeCodes.Day3_09PM;
                }

                throw new InvalidOperationException("Invalid TimeBlock");
            }
        }

        private static Timeblock _currentTime = Timeblock.Day1_10AM;
    }
}
