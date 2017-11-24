using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_CONCAT : OpCode
    {
        public OP_CONCAT()
            : base()
        {
            this.Type = OpCodeType.OP_CONCAT;
        }
    }
}
