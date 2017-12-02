using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SucDecompiler
{
    public class FixTable
    {
        public class Fix
        {
            //public string ScriptFile { get; set; }
            public string FixType { get; set; }
            public string FixValue { get; set; }
        }

        static Dictionary<string, List<Fix>> Fixes = new Dictionary<string, List<Fix>>();

        public static bool LoadFixTable(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }
            string[] fixLines = File.ReadAllLines(filename);
            foreach (string line in fixLines)
            {
                string[] values = line.Split(',');
                string scriptfile = values[0];

                if (scriptfile == string.Empty
                    || values.Length < 3)
                {
                    continue;
                }

                if (!Fixes.ContainsKey(scriptfile))
                {
                    List<Fix> fixlist = new List<Fix>();
                    Fixes.Add(scriptfile, fixlist);
                }

                Fix fix = new Fix()
                {
                    FixType = values[1],
                    FixValue = values[2]
                };
                Fixes[scriptfile].Add(fix);
            }

            return true;
        }

        public static bool isStaticFix(string scriptfile, string variablename)
        {
            if (!Fixes.ContainsKey(scriptfile))
            {
                return false;
            }

            foreach (Fix fix in Fixes[scriptfile])
            {
                if (fix.FixType == "UnassignedVariable"
                    && fix.FixValue == variablename)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
