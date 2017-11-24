using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_FUNCTION : OpCode
    {
        public OP_FUNCTION()
            : base()
        {
            this.Type = OpCodeType.OP_FUNCTION;
        }
    }
}
