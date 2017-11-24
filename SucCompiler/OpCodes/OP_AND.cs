using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_AND : OpCode
    {
        public OP_AND()
            : base()
        {
            this.Type = OpCodeType.OP_AND;
        }
    }
}
