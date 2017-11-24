using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler.OpCodes
{
    class JUMPTARGET : OpCodeWithData
    {
        public JUMPTARGET()
            : base()
        {
            this.Type = OpCodeType.JUMPTARGET;
        }
    }
}
