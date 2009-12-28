using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class TimedWaitHandle : WaitHandle
    {
        public int TimeAtExpiration;
        public int Duration;

        public TimedWaitHandle(int duration)
        {
            Duration = duration;
            TimeAtExpiration = GameManager.TickCount + duration;
        }

        public override bool Finished
        {
            get
            {
                return TimeAtExpiration <= GameManager.TickCount;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }

    public struct GameTimer
    {
        public string Noun;
        public string Verb;
        public int Duration;
        public int TimeAtExpiration;
    }
}
