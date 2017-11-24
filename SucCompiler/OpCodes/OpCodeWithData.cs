using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OpCodeWithData : OpCode
    {
        public short DataIndex { get; set; }

        public OpCodeWithData()
            : base()
        {
            OpCode.NextAddress += 2;
        }

        new public string ToString()
        {
            return string.Format("{0} {1} -> A: {2}", this.Address, this.Type, DataIndex.ToString());
        }
    }
}
