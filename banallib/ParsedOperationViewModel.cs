using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace banallib
{
    public class ParsedOperationViewModel
    {
        public short Address { get; set; }
        public OperationType OperationType { get; set; }

        public object Parameter1 { get; set; }
        public object Value1 { get; set; }
        public object Parameter2 { get; set; }
        public object Value2 { get; set; }
        public object Parameter3 { get; set; }
        public object Value3 { get; set; }
        public object Parameter4 { get; set; }
        public object Value4 { get; set; }
        public object Parameter5 { get; set; }
        public object Value5 { get; set; }
        public object Parameter6 { get; set; }
        public object Value6 { get; set; }

        public short ReturnValueTarget { get; set; }
    }
}
