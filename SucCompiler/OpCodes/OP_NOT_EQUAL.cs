using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_NOT_EQUAL : OpCode
    {
        public OP_NOT_EQUAL()
            : base()
        {
            this.Type = OpCodeType.OP_NOT_EQUAL;
        }
    }
}
