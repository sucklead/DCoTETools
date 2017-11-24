using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SucBatch
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("SucBatch by sucklead (http://dcotetools.sucklead.com/p/sucbatch.html)");
                Console.WriteLine("To batch a single lst file into a batch file");
                Console.WriteLine("SucBatch {batchfiles directory} {input list filename} {output batchfile}");
                Console.WriteLine("e.g. To build 01_house.lst into 01_house.bat with the bin files");
                Console.WriteLine("from directory bin:");
                Console.WriteLine("SucBatch bin 01_house.lst 01_house.bat");
                Console.WriteLine();
                Console.WriteLine("To batch all list files in a directory into matching bat files");
                Console.WriteLine("SucBatch {batchfiles directory} {input list files directory}");
                Console.WriteLine("e.g. To build all .lst files in current directory into matching");
                Console.WriteLine("bin files from directory bin:");
                Console.WriteLine("SucBatch bin .");
                return;
            }

            try
            {

                Batcher batcher = new Batcher();

                string binariesDirectory = args[0];
                if (!Directory.Exists(binariesDirectory))
                {
                    Console.WriteLine("Binaries directory not found!");
                    return;
                }

                batcher.BatchFileDirectory = binariesDirectory;

                //directory based?
                if (args.Length == 2)
                {
                    string inputDirectory = args[1];
                    if (!Directory.Exists(inputDirectory))
                    {
                        Console.WriteLine("Input directory not found!");
                        return;
                    }

                    string[] lstFiles = Directory.GetFiles(inputDirectory, "*.lst");
                    foreach (string file in lstFiles)
                    {
                        batcher.InputFilename = file;
                        batcher.OutputFilename = Path.GetFileNameWithoutExtension(file) + ".bat";

                        if (File.Exists(batcher.OutputFilename)
                            && !File.Exists(batcher.OutputFilename + ".orig"))
                        {
                            File.Copy(batcher.OutputFilename, batcher.OutputFilename + ".orig");
                        }

                        batcher.CreateBatch();
                    }
                }
                else
                {
                    batcher.InputFilename = args[1];
                    batcher.OutputFilename = args[2];

                    //verify output file names
                    if (!File.Exists(batcher.InputFilename))
                    {
                        Console.WriteLine("Input file {0} doesn't exist", batcher.InputFilename);
                        return;
                    }
                    if (!batcher.InputFilename.EndsWith(".lst"))
                    {
                        Console.WriteLine("Input file must be a .lst file");
                        return;
                    }

                    if (!batcher.OutputFilename.EndsWith(".bat"))
                    {
                        Console.WriteLine("Output file must be a .bat file");
                        return;
                    }

                    if (File.Exists(batcher.OutputFilename)
                        && !File.Exists(batcher.OutputFilename + ".orig"))
                    {
                        File.Copy(batcher.OutputFilename, batcher.OutputFilename + ".orig");
                    }

                    try
                    {
                        File.Create(batcher.OutputFilename);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error creating output file.");
                        return;
                    }

                    batcher.CreateBatch();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error encountered batching scripts:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
