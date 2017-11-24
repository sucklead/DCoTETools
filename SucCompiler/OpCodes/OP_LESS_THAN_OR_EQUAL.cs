using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_LESS_THAN_OR_EQUAL : OpCode
    {
        public OP_LESS_THAN_OR_EQUAL()
            : base()
        {
            this.Type = OpCodeType.OP_LESS_THAN_OR_EQUAL;
        }
    }
}
