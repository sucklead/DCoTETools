using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_PUSH : OpCodeWithData
    {
        public OP_PUSH()
            : base()
        {
            this.Type = OpCodeType.OP_PUSH;
        }
    }
}
