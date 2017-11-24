using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_LESS_THAN : OpCode
    {
        public OP_LESS_THAN()
            : base()
        {
            this.Type = OpCodeType.OP_LESS_THAN;
        }
    }
}
