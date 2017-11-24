using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_MULTIPLY : OpCode
    {
        public OP_MULTIPLY()
            : base()
        {
            this.Type = OpCodeType.OP_MULTIPLY;
        }
    }
}
