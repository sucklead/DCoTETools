using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace banallib
{
    public class Operation
    {
        public OpCodeType OpCode { get; set; }

        public DataIndex DataIndex { get; set; }

        public short Address { get; set; }

        //public string OpName { get; set; }

        //public Int32 Reference { get; set; }
        //public string ReferenceValue { get; set; }
        //public List<object> Parameters { get; set; }

        public Operation()
        {
            //Parameters = new List<object>();
        }
    }
}
