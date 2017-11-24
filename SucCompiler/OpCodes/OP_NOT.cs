using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_NOT : OpCode
    {
        public OP_NOT()
            : base()
        {
            this.Type = OpCodeType.OP_NOT;
        }
    }
}
