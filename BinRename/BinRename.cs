using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BinRename
{
    public class BinRename
    {
        private byte[] batfileMemory = null;

        public string BatFilename { get; set; }

        public void Run()
        {
            if (!LoadInputFile(BatFilename, out batfileMemory))
            {
                return;
            }

            string hfsFilename = Path.Combine(Path.GetDirectoryName(BatFilename), Path.GetFileNameWithoutExtension(BatFilename) + ".hfs");

            string currentName = null;

            using (MemoryStream memoryStream = new MemoryStream(batfileMemory))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    StringBuilder nameBuilder = new StringBuilder();

                    while (reader.PeekChar() != 0)
                    {
                        nameBuilder.Append(reader.ReadChar());
                    }

                    currentName = nameBuilder.ToString();
                    Console.WriteLine(string.Format("Current Name is {0}", currentName));

                    //Console.WriteLine(string.Format("File length was {0} bytes, read to byte position {1}.", memoryStream.Length, memoryStream.Position));
                    //Console.WriteLine(string.Format("Wrote filelist to {0}.", FileList));
                }
            }

            using (MemoryStream memoryStream = new MemoryStream(batfileMemory))
            {
                Console.WriteLine(string.Format("Writing new name of {0}", hfsFilename));
                //write new name
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    char[] newName = hfsFilename.ToCharArray();

                    writer.Write(newName);
                    writer.Write((byte)0x00);

                    int difference = currentName.Length - newName.Length;

                    //fill with CD
                    for (int i=difference; i > 0; i--) 
                    {
                        writer.Write((byte)0xCD);
                    }
                }

            }

             //       string scriptName = null;
            //string filename = Path.Combine(OutputDirectory, Path.GetDirectoryName(scriptName));
            //filename = Path.Combine(filename, Path.GetFileNameWithoutExtension(scriptName) + ".bin");
            //Console.WriteLine(string.Format("-> {0}", filename));

            //Directory.CreateDirectory(Path.GetDirectoryName(filename));
            File.WriteAllBytes(BatFilename, batfileMemory);
            Console.WriteLine(string.Format("Wrote new file."));

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
