﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace banalg
{
    public class Variable
    {
        public Variable()
        {
            Static = true;
        }

        public short Address { get; set; }
        public string DataType { get; set; }
        public string Name { get; set; }
        public bool Static { get; set; }
    }
}
