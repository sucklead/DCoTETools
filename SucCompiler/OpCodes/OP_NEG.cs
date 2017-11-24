using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_NEG : OpCode
    {
        public OP_NEG()
            : base()
        {
            this.Type = OpCodeType.OP_NEG;
        }
    }
}
