using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_EQUAL : OpCode
    {
        public OP_EQUAL()
            : base()
        {
            this.Type = OpCodeType.OP_EQUAL;
        }
    }
}
