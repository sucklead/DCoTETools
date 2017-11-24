using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_MORE_THAN : OpCode
    {
        public OP_MORE_THAN()
            : base()
        {
            this.Type = OpCodeType.OP_MORE_THAN;
        }
    }
}
