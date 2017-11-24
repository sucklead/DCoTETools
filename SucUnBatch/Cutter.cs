using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UnBatch
{
    public class Cutter
    {
        private byte[] batfileMemory = null;
        
        public string BatFilename { get; set; }
        public string OutputDirectory { get; set; }
        public string FileList { get; set; }

        public void Cut()
        {
            if (!LoadInputFile(BatFilename, out batfileMemory))
            {
                return;
            }

            FileList = Path.Combine(".", Path.GetFileNameWithoutExtension(BatFilename) + ".lst");
            
            using (StreamWriter fileListWriter = new StreamWriter(FileList)) //File.OpenWrite(FileList))
            {

                using (MemoryStream memoryStream = new MemoryStream(batfileMemory))
                {
                    using (BinaryReader reader = new BinaryReader(memoryStream))
                    {
                        int numberOfScripts = reader.ReadInt32();
                        Console.WriteLine(string.Format("{0} scripts in file.", numberOfScripts));

                        for (int i = 1; i <= numberOfScripts; i++)
                        {
                            //if (memoryStream.Position >= memoryStream.Length)
                            //{
                            //    break;
                            //}
                            int scriptLength = reader.ReadInt32();
                            Console.Write(string.Format("Script {0}", i));
                            Console.Write(string.Format(" ({0} bytes)", scriptLength));

                            byte[] script = null;

                            //read in script
                            script = reader.ReadBytes(scriptLength);

                            string scriptName = null;
                            //read out script name
                            using (MemoryStream scriptStream = new MemoryStream(script))
                            {
                                using (BinaryReader scriptReader = new BinaryReader(scriptStream))
                                {
                                    StringBuilder nameBuilder = new StringBuilder();

                                    //int nameLength = 0;
                                    while (scriptReader.PeekChar() != 0)
                                    {
                                        nameBuilder.Append(scriptReader.ReadChar());
                                    }

                                    scriptName = nameBuilder.ToString();
                                    //Console.WriteLine(string.Format(" {0}", scriptName));
                                }
                            }

                            fileListWriter.WriteLine(Path.Combine(Path.GetDirectoryName(scriptName), Path.GetFileNameWithoutExtension(scriptName) + ".bin"));

                            string filename = Path.Combine(OutputDirectory, Path.GetDirectoryName(scriptName));
                            filename = Path.Combine(filename, Path.GetFileNameWithoutExtension(scriptName) + ".bin");
                            Console.WriteLine(string.Format(" {0}", filename));

                            Directory.CreateDirectory(Path.GetDirectoryName(filename));
                            File.WriteAllBytes(filename, script);

                        }

                        Console.WriteLine(string.Format("File length was {0} bytes, read to byte position {1}.", memoryStream.Length, memoryStream.Position));
                        Console.WriteLine(string.Format("Wrote filelist to {0}.", FileList));
                    }
                }
            }
        }

        public bool LoadInputFile(string filename, out byte[] memArea)
        {
            memArea = null;

            try
            {
                memArea = File.ReadAllBytes(BatFilename);

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to open file check permissions");
                Console.WriteLine(ex.Message);
                return false;
            }

            //check if null
            if (memArea == null)
            {
                Console.WriteLine("ERROR: memory is null");
                return false;
            }

            return true;
        }

    }
}
