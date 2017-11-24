using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CocCompiler
{
    public static class FunctionTable
    {
        //public static Dictionary<Int16, Int16> FunctionConversion = new Dictionary<Int16, Int16>();
        //public static Dictionary<Int16, string> OldFunctionValues = new Dictionary<Int16, string>();
        //public static Dictionary<Int16, string> NewFunctionValues = new Dictionary<Int16, string>();
        public static Dictionary<string, Int16> Functions = new Dictionary<string, Int16>();

        private static string Filename { get; set; }

        private static int FunctionTableCount { get; set; }

        public static void LoadData()
        {
            try
            {
                //string[] lines = File.ReadAllLines("functiontable.csv");
                if (File.Exists("functiontable.csv"))
                {
                    Filename = "functiontable.csv";
                }
                else
                {
                    Filename = @"S:\workspaces\vs\DCoTETools\SucCompiler\functiontable.csv";
                }
                string[] lines = File.ReadAllLines(Filename);
                foreach (string line in lines)
                {
                    if (line == "")
                    {
                        continue;
                    }

                    string[] splits = line.Split(',');
                    if (splits.Length < 2)
                    {
                        continue;
                    }

                    short oldValue;
                    //short newValue;
                    short.TryParse(splits[1], out oldValue);
                    //short.TryParse(splits[2], out newValue);

                    try
                    {

                        //FunctionConversion.Add(oldValue, newValue);
                        //OldFunctionValues.Add(oldValue, splits[0]);
                        //NewFunctionValues.Add(newValue, splits[0]);
                        Functions.Add(splits[0], oldValue);
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
                //save this amount of functions
                FunctionTable.FunctionTableCount = Functions.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load functiontable.csv {0}", ex);
            }
        }

        public static short FindFunction(string filename, string functionName, int offset)
        {
            byte[] fileBytes = File.ReadAllBytes(filename);

            int position = offset + 276;

            short functionPointer = BitConverter.ToInt16(fileBytes, position);
                //Functions.Add(splits[0], oldValue);

            Functions.Add(functionName, functionPointer);

            return functionPointer;

            //return fileBytes[offset];
        }

        public static bool IsCorrectOpCodeSize(string filename, short opCodes)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Couldn't location binary file {0}", filename);
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(filename);

            int position = 256;

            short fileOpCodes = BitConverter.ToInt16(fileBytes, position);

            return fileOpCodes == opCodes;
        }


        public static void SaveData()
        {
            //anything to save
            if (FunctionTable.FunctionTableCount == Functions.Count)
            {
                return;
            }
            Console.WriteLine("Saving changes to function table.");

            List<String> strings = new List<string>();
            foreach (var function in Functions)
            {
                strings.Add(string.Format("{0},{1}", function.Key, function.Value));
            }

            File.WriteAllLines(Filename, strings.ToArray());
        }
    }

}
