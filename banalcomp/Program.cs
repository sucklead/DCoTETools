using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using banallib;
using System.Diagnostics;
using System.IO;

namespace banalcomp
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine($"Incorrect number of arguments!");
                if (Debugger.IsAttached)

                {
                    Console.ReadLine();
                }
                return -1;
            }

            string file1 = args[0];
            string file2 = args[1];

            if (!File.Exists(file1))
            {
                Console.Error.WriteLine($"File [{file1}] does not exist!");
                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }
                return -2;
            }

            if (!File.Exists(file2))
            {
                Console.Error.WriteLine($"File [{file2}] does not exist!");
                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }
                return -3;
            }

            FunctionTable.LoadData();

            Analyse analyse = new Analyse();

            ParsedContent parsedContent1 = analyse.AnalyseFile(file1);
            ParsedContent parsedContent2 = analyse.AnalyseFile(file2);

            string dumpFile1 = Path.GetFileNameWithoutExtension(file1) + "1.ops";
            string dumpFile2 = Path.GetFileNameWithoutExtension(file2) + "2.ops";

            DumpFile(parsedContent1, dumpFile1);
            DumpFile(parsedContent2, dumpFile2);

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Program Files (x86)\Beyond Compare 3\BComp.exe", string.Format("{0} {1}", dumpFile1, dumpFile2));
            p.Start();

            Console.WriteLine("Finished.");
            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
            return 0;
        }

        static void DumpFile(ParsedContent parsedContent, string outfile)
        {
            using (StreamWriter sw = new StreamWriter(outfile))
            {
                foreach (var opCode in parsedContent.OpCodeList)
                {
                    sw.Write(opCode.Address);
                    sw.Write(new String(' ', 6 - opCode.Address.ToString().Length));
                    sw.Write(opCode.OpCode);
                    sw.Write(new String(' ', 15 - opCode.OpCode.ToString().Length));
                    if (opCode.DataIndex != null)
                    {
                        if (opCode.DataIndex.IsNegative)
                        {
                            sw.Write("-");
                        }
                        else
                        {
                            sw.Write(" ");
                        }
                        sw.Write(opCode.DataIndex.Value);
                        sw.Write(new String(' ', 10 - opCode.DataIndex.Value.ToString().Length));

                        if (opCode.OpCode == OpCodeType.OP_JMP
                            || opCode.OpCode == OpCodeType.OP_JMPF
                            || opCode.OpCode == OpCodeType.JUMPTARGET)
                        {
                        }
                        else if (opCode.DataIndex.IsFunctionPointer)
                        {
                        }
                        else
                        {
                            Value dataValue = parsedContent.ValuesList.Where(v => v.Address == opCode.DataIndex.Value).First();
                            sw.Write(dataValue.DataType);
                            sw.Write(new String(' ', 15 - dataValue.DataType.ToString().Length));

                            for (int i = 0; i < dataValue.SubValues.Count; i++)
                            {
                                if (dataValue.SubValues[i] != null)
                                {
                                    if (i != 0)
                                    {
                                        sw.Write(", ");
                                    }
                                    sw.Write(dataValue.SubValues[i]);
                                }
                            }
                        }
                    }
                    sw.WriteLine();
                }

                sw.WriteLine("------------------------------------------------------------");
                foreach (var value in parsedContent.ValuesList)
                {
                    sw.Write(value.Address);
                    sw.Write(new String(' ', 15 - value.Address.ToString().Length));

                    sw.Write(value.DataType);
                    sw.Write(new String(' ', 15 - value.DataType.ToString().Length));

                    for (int i = 0; i < value.SubValues.Count; i++)
                    {
                        if (value.SubValues[i] != null)
                        {
                            if (i != 0)
                            {
                                sw.Write(", ");
                            }
                            sw.Write(value.SubValues[i]);
                        }
                    }
                    sw.WriteLine();
                }

                sw.Flush();
            }
        }
    }
}
