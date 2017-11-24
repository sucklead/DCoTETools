using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class OP_JMPF : OpCodeWithData
    {
        public OP_JMPF()
            : base()
        {
            this.Type = OpCodeType.OP_JMPF;
        }
    }
}
