using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace UnBatch
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("SucUnBatch by sucklead (http://dcotetools.sucklead.com/p/sucunbatch.html)");
                Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine();
                Console.WriteLine("To un-batch a single bat file into an output directory:");
                Console.WriteLine("SucUnBatch {input batchfile} {output directory} {file list directory}");
                Console.WriteLine("e.g to extract 01_house.bat files to bin directory:");
                Console.WriteLine("SucUnBatch 01_house.bat bin .");
                Console.WriteLine();
                Console.WriteLine("To un-batch all bat files in a directory into an output directory:");
                Console.WriteLine("SucUnBatch {input directory} {output directory} {file list directory}");
                Console.WriteLine("e.g To extract all .bat files in current directory to bin directory:");
                Console.WriteLine("SucUnBatch . bin .");
                Console.WriteLine();
                Console.WriteLine("Easiest way to run it is from within the Scripts directory:");
                Console.WriteLine("SucUnBatch . bin list");
                return;
            }

            Console.WriteLine("SucUnBatch by sucklead started at {0}", DateTime.Now);
            Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);

            try
            {
                string batfile = args[0];

                Cutter cutter = new Cutter();
                //cutter.OutputDirectory = @"C:\temp\cut";
                cutter.OutputDirectory = args[1];

                cutter.FileListDirectory = args[2];

                if (!Directory.Exists(cutter.OutputDirectory))
                {
                    Directory.CreateDirectory(cutter.OutputDirectory);
                    if (!Directory.Exists(cutter.OutputDirectory))
                    {
                        Console.WriteLine("Couldn't create bin directory");
                        return;
                    }
                }

                if (!Directory.Exists(cutter.FileListDirectory))
                {
                    Directory.CreateDirectory(cutter.FileListDirectory);
                    if (!Directory.Exists(cutter.FileListDirectory))
                    {
                        Console.WriteLine("Couldn't create list directory");
                        return;
                    }
                }

                //compile a directory
                if (Directory.Exists(batfile))
                {
                    string[] batFiles = Directory.GetFiles(batfile, "*.bat");
                    foreach (string file in batFiles)
                    {
                        if (file.Contains("original.bat"))
                        {
                            continue;
                        }

                        cutter.BatFilename = file;
                        cutter.Cut();
                    }
                }
                else
                {
                    cutter.BatFilename = batfile;

                    if (!File.Exists(cutter.BatFilename))
                    {
                        Console.WriteLine(string.Format("Input file {0} not found!", cutter.BatFilename));
                        return;
                    }

                    //Console.WriteLine("Unbatching...");
                    cutter.Cut();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error encountered unbatching scripts:");
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("UnBatch completed at {0}.", DateTime.Now);
        }
    }
}
