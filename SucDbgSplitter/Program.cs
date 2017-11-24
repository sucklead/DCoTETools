using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace DbgSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                //Console.WriteLine("To SucDbgSplitter {input debugfile|*} {optional opcodes}");
                //Console.WriteLine("SucDbgSplitter {input debugfile|*} {optional opcodes}");
                //Console.WriteLine("e.g. SucDbgSplitter * - Extract all .dbg files without opcodes");
                //Console.WriteLine("e.g. SucDbgSplitter * opcodes - Extract all .dbg files with opcodes");

                Console.WriteLine("SucDbgSplitter by sucklead (http://dcotetools.sucklead.com/p/sucdbgsplitter.html)");
                Console.WriteLine("To split a single debug file into an output directory");
                Console.WriteLine("SucDbgSplitter {input debugfile} {output directory}");
                Console.WriteLine("e.g to extract 01_house.dbg files to src directory:");
                Console.WriteLine("SucDbgSplitter 01_house.dbg src");
                Console.WriteLine();
                Console.WriteLine("To split all debug files in a directory into an output directory");
                Console.WriteLine("SucUnBatch {input directory} {output directory}");
                Console.WriteLine("e.g To extract all .dbg files in current directory to src directory:");
                Console.WriteLine("SucDbgSplitter . src");
                Console.WriteLine();
                Console.WriteLine("To include opcode files in the output add opcodes to the command line");
                Console.WriteLine("e.g. SucDbgSplitter . src opcodes");

                return;
            }

            try
            {
                DbgSplitter dbgSplitter = new DbgSplitter();
                //dbgSplitter.DebugFilename = @"M:\cociso\Scripts\06_refinery_explosion.dbg";

                dbgSplitter.OutputDirectory = args[1];

                if (args.Length > 2)
                {
                    dbgSplitter.WriteOpcodes = (args[2] == "opcodes");
                }

                string debugFile = args[0];

                if (Directory.Exists(debugFile))
                {
                    string[] files = Directory.GetFiles(debugFile, "*.dbg");
                    foreach (string file in files)
                    {
                        Console.WriteLine("Splitting {0}...", file);
                        dbgSplitter.DebugFilename = file;
                        dbgSplitter.Run();
                    }
                }
                else
                {
                    Console.WriteLine("Splitting {0}...", args[0]);
                    dbgSplitter.DebugFilename = args[0];
                    if (Path.GetExtension(dbgSplitter.DebugFilename) != ".dbg")
                    {
                        Console.WriteLine("Input file not a .dbg file!");
                        return;
                    }
                    dbgSplitter.Run();
                }

                //Console.WriteLine("Splitting complete.");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error encountered splitting debug files:");
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
