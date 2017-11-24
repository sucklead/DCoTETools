using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_GETTOP : OpCodeWithData
    {
        public OP_GETTOP()
            : base()
        {
            this.Type = OpCodeType.OP_GETTOP;
        }
    }
}
