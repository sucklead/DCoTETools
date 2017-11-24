using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace banallib
{
    public class Value
    {
        public DataTypeType DataType { get; set; }

        public short Address { get; set; }
        public Int32 Reference { get; set; }
        public List<object> SubValues { get; set; }

        public Value()
        {
            SubValues = new List<object>();
        }
    }
}
