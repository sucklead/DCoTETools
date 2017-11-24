using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace banallib
{
    public class ParsedContent
    {
        public BinFile OriginalFile { get; set; }

        public bool IsOldFile { get; set; }
        public string ScriptName { get; set; }
        public Int32 LengthOfOpCodes { get; set; }
        public Int32 StartOfOpCodes { get; set; }

        public Int32 NumberOfValues { get; set; }

        public Int16 BaseAddress { get; set; }
        public Int32 StartOfValues { get; set; }

        public List<Operation> OpCodeList { get; set; }
        public List<Value> ValuesList { get; set; }
    }
}
