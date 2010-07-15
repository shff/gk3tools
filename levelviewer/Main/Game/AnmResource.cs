using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class AnmWaitHandle : WaitHandle
    {
        private MomResource _mom;

        public AnmWaitHandle(MomResource mom)
        {
            _mom = mom;
        }

        public override bool Finished
        {
            get
            {
                return _mom.IsFinished;
            }
            set
            {
                base.Finished = value;
            }
        }
    }
}
