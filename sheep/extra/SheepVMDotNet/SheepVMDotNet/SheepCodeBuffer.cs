using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheepVMDotNet
{
    class SheepCodeBuffer : System.IO.BinaryReader
    {
        public SheepCodeBuffer(System.IO.Stream stream)
            : base(stream)
        {
        }
    }
}
