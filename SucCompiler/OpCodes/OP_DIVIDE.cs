using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_DIVIDE : OpCode
    {
        public OP_DIVIDE()
            : base()
        {
            this.Type = OpCodeType.OP_DIVIDE;
        }
    }
}
