using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocCompiler
{
    public class Variable
    {
        public string Name { get; set; }
        public DataTypeType DataType { get; set; }
        public object Value { get; set; }
        public short Address { get; set; }
        public Int32 Reference { get; set; }
        public bool IsConstant { get; set; }
        public bool IsNegative { get; set; }
    }
}
