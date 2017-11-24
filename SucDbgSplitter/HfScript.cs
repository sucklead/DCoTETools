using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbgSplitter
{
    internal class HfScript
    {
        public string FileName { get; set; }
        public string OpFileName { get; set; }
        public Int32 OpcodeStart { get; set; }
        public Int32 OpcodeLength { get; set; }
        public byte[] OpcodeBytes { get; set; }

        public Int32 TextStart { get; set; }
        public Int32 TextLength { get; set; }
        public byte[] TextBytes { get; set; }

    }
}
