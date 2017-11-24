using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_JMP : OpCodeWithData
    {
        public OP_JMP()
            : base()
        {
            this.Type = OpCodeType.OP_JMP;
        }
    }
}
