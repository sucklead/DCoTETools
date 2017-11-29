using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using banallib;

namespace banalg
{
    public class ValueViewModel
    {
        public DataTypeType DataType { get; set; }

        //private short _address;
        public short Address { get; set; }
        //{
        //    get
        //    {
        //        return _address;
        //    }
        //    set
        //    {
        //        _address = value;
        //        AddressHex = value.ToString("X");
        //    }
        //}
        public short AddressHex { get; set; }

        public Int32 Reference { get; set; }

        public Int32 ReferenceHex { get; set; }

        public object SubValue1 { get; set; }
        public object SubValue2 { get; set; }
        public object SubValue3 { get; set; }
        public object SubValue4 { get; set; }
        public object SubValue5 { get; set; }


        public short AddressHexBase { get; set; }
    }
}
