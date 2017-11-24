using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_OR : OpCode
    {
        public OP_OR()
            : base()
        {
            this.Type = OpCodeType.OP_OR;
        }
    }
}
