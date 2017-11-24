using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OpCode
    {
        public static short NextAddress { get; set; }

        public OpCodeType Type  { get; protected set; }
        public short Address { get; protected set; }

        public OpCode()
        {
            this.Address = OpCode.NextAddress;
            OpCode.NextAddress++;
        }

        new public string ToString()
        {
            return string.Format("{0} {1}", this.Address, this.Type);
        }
    }
}
