using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SucBatch
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("SucBatch by sucklead (http://dcotetools.sucklead.com/p/sucbatch.html)");
                Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine();
                Console.WriteLine("To batch a single lst file into a batch file");
                Console.WriteLine("SucBatch {binfiles directory} {input list filename} {output batchfile} {no backups}");
                Console.WriteLine("e.g. To build 01_house.lst into 01_house.bat with the bin files");
                Console.WriteLine("from directory bin:");
                Console.WriteLine("SucBatch bin 01_house.lst 01_house.bat");
                Console.WriteLine();
                Console.WriteLine("To batch all list files in a directory into matching bat files");
                Console.WriteLine("SucBatch {binfiles directory} {file list directory} {output batchfile directory} {no backups}");
                Console.WriteLine("e.g. To build all .lst files in current directory into matching");
                Console.WriteLine("bat files from directory bin:");
                Console.WriteLine("SucBatch bin . .");
                Console.WriteLine();
                Console.WriteLine("Easiest way to run it is from within the Scripts directory:");
                Console.WriteLine("SucBatch newbin list newbat");
                return;
            }

            Console.WriteLine("SucBatch by sucklead started at {0}", DateTime.Now);
            Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);

            try
            {

                Batcher batcher = new Batcher();

                string binariesDirectory = args[0];
                if (!Directory.Exists(binariesDirectory))
                {
                    Console.WriteLine("Binaries directory not found!");
                    return;
                }

                batcher.BinFileDirectory = args[0];

                bool noBackups = false;
                if (args.Length == 4)
                {
                    noBackups = true;
                }

                //directory based?
                if (Directory.Exists(args[1]))
                {
                    string inputDirectory = args[1];
                    if (!Directory.Exists(inputDirectory))
                    {
                        Console.WriteLine("Input directory not found!");
                        return;
                    }

                    string listfilesDirectory = args[1];
                    string batchfilesDirectory = args[2];
                    if (!Directory.Exists(batchfilesDirectory))
                    {
                        Directory.CreateDirectory(batchfilesDirectory);
                    }

                    string[] lstFiles = Directory.GetFiles(listfilesDirectory, "*.lst");

                    if (lstFiles.Length == 0)
                    {
                        Console.WriteLine("No .lst files found!");
                        return;
                    }

                    foreach (string file in lstFiles)
                    {
                        batcher.InputFilename = file;
                        batcher.OutputFilename = Path.Combine(batchfilesDirectory, Path.GetFileNameWithoutExtension(file) + ".bat");

                        if (File.Exists(batcher.OutputFilename)
                            && !File.Exists(batcher.OutputFilename + ".orig")
                            && !noBackups)
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

            Console.WriteLine("Batch completed at {0}.", DateTime.Now);
        }
    }
}
