using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_DISCARD : OpCode
    {
        public OP_DISCARD()
            : base()
        {
            this.Type = OpCodeType.OP_DISCARD;
        }
    }
}
