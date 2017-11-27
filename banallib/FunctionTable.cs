using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace banallib
{
    public static class FunctionTable
    {
        //public static Dictionary<Int16, Int16> FunctionConversion = new Dictionary<Int16, Int16>();
        //public static Dictionary<Int16, string> OldFunctionValues = new Dictionary<Int16, string>();
        //public static Dictionary<Int16, string> NewFunctionValues = new Dictionary<Int16, string>();
        public static Dictionary<Int16, string> Functions = new Dictionary<Int16, string>();

        public static bool LoadData()
        {
            try
            {
                //string[] lines = File.ReadAllLines("functiontable.csv");
                //string[] lines = File.ReadAllLines(@"S:\Games\Call Of Cthulhu DCoTE\Scripts\functiontable.csv");
                string[] lines = null;
                if (File.Exists(@"functiontable.csv"))
                {
                    lines = File.ReadAllLines(@"functiontable.csv");
                }
                else if (File.Exists(@"S:\workspaces\DCoTETools\SucCompiler\functiontable.csv"))
                {
                    lines = File.ReadAllLines(@"S:\workspaces\DCoTETools\SucCompiler\functiontable.csv");
                }
                else
                {
                    return false;
                }

                foreach (string line in lines)
                {
                    if (line == "")
                    {
                        continue;
                    }

                    string[] splits = line.Split(',');
                    if (splits.Length < 2
                        || splits[0].StartsWith("//"))
                    {
                        continue;
                    }

                    short functionNumber;
                    short.TryParse(splits[1], out functionNumber);

                    try
                    {
                        Functions.Add(functionNumber, splits[0]);
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load functiontable.csv {0}", ex);
                return false;
            }

            return true;
        }

    }

}
