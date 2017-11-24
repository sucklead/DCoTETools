using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_PRINT : OpCode
    {
        public OP_PRINT()
            : base()
        {
            this.Type = OpCodeType.OP_PRINT;
        }
    }
}
