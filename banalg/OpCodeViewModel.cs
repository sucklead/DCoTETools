using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using banallib;

namespace banalg
{
    public class OpCodeViewModel
    {
        private OpCodeType _opCode;
        public OpCodeType OpCode
        {
            get
            {
                return _opCode;
            }
            set
            {
                _opCode = value;
                OpCodeHex = value.ToString("X");
            }
        }
        public string OpCodeHex { get; set; }

        //public DataIndex DataIndex { get; set; }
        public Int16 DataIndexValue { get; set; }
        public Int16 DataIndexValueHex { get; set; }
        public Int16 DataIndexValueBase { get; set; }
        public bool DataIndexIsFunctionPointer { get; set; }

        public string FunctionPointerName { get; set; }

        public short Address { get; set; }
        public short AddressHex { get; set; }

    }
}
