using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SucBatch
{
    class Batcher
    {
        public string InputFilename { get; set; }

        public string OutputFilename { get; set; }

        public string BinFileDirectory { get; set; }

        internal bool CreateBatch()
        {
            string[] scriptsToAdd = File.ReadAllLines(InputFilename);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    //write scripts in file
                    binaryWriter.Write(scriptsToAdd.Length);

                    foreach (string script in scriptsToAdd)
                    {
                        if (script == string.Empty)
                        {
                            continue;
                        }

                        string scriptName = Path.Combine(BinFileDirectory, script);

                        Console.WriteLine("Adding {0}...", scriptName);

                        if (!File.Exists(scriptName))
                        {
                            Console.WriteLine("ERROR: Invalid file in file list {0}", script);
                            return false;
                        }

                        //get file contents
                        byte[] scriptmemory = File.ReadAllBytes(scriptName);

                        //write script length in bytes
                        binaryWriter.Write(scriptmemory.Length);
                        //write script
                        binaryWriter.Write(scriptmemory);
                    }
                }

                Console.WriteLine("Writing output file {0}..", OutputFilename);

                try
                {
                    File.WriteAllBytes(OutputFilename, memoryStream.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: could not write file!");
                }
                Console.WriteLine("Batched {0} scripts into output file {1}", scriptsToAdd.Length, OutputFilename);
            }
            return true;
        }
    }
}
