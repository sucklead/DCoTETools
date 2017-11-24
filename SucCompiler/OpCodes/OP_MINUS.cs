using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_MINUS : OpCode
    {
        public OP_MINUS()
            : base()
        {
            this.Type = OpCodeType.OP_MINUS;
        }
    }
}
