using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public class WaitHandle
    {
        private bool _finished;

        public virtual bool Finished
        {
            get { return _finished; }
            set { _finished = false; }
        }

        public override string ToString()
        {
            return "Finished = " + Finished.ToString();
        }
    }
}
