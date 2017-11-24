using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DbgSplitter
{
    public class DbgSplitter
    {
        public string DebugFilename { get; set; }
        public string OutputDirectory { get; set; }
        public bool WriteOpcodes { get; set; }

        private byte[] debugfileMemory = null;
        List<HfScript> scripts = new List<HfScript>();

        public void Run()
        {
            scripts.Clear();

            if (!LoadInputFile(DebugFilename, out debugfileMemory))
            {
                return;
            }

            using (MemoryStream memoryStream = new MemoryStream(debugfileMemory))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    int numberOfScripts = reader.ReadInt32();
                    Console.WriteLine(string.Format("{0} scripts in file.", numberOfScripts));

                    for (int i = 1; i <= numberOfScripts; i++)
                    {
                        HfScript hfScript = new HfScript();

                        StringBuilder nameBuilder = new StringBuilder();
                        while (reader.PeekChar() != 0)
                        {
                            nameBuilder.Append(reader.ReadChar());
                        }
                        reader.ReadChar();
                        hfScript.FileName = nameBuilder.ToString();
                        hfScript.OpFileName = Path.Combine(Path.GetDirectoryName(hfScript.FileName), Path.GetFileNameWithoutExtension(hfScript.FileName) + ".opc");

                        //Console.WriteLine(string.Format("Script Name is {0}", hfScript.FileName));

                        hfScript.OpcodeStart = reader.ReadInt32();
                        //Console.WriteLine(string.Format("Section1Start={0}.", hfScript.OpcodeStart));
                        hfScript.OpcodeLength = reader.ReadInt32();
                        //Console.WriteLine(string.Format("Section1Length={0}.", hfScript.OpcodeLength));
                        hfScript.TextStart = reader.ReadInt32();
                        //Console.WriteLine(string.Format("Section2Start={0}.", hfScript.TextStart));
                        hfScript.TextLength = reader.ReadInt32();
                        //Console.WriteLine(string.Format("Section2Length={0}.", hfScript.TextLength));

                        scripts.Add(hfScript);
                    }
                    //Console.WriteLine(string.Format("Wrote filelist to {0}.", FileList));


                    foreach (HfScript script in scripts)
                    {
                        script.OpcodeBytes = reader.ReadBytes(script.OpcodeLength);
                        script.TextBytes = reader.ReadBytes(script.TextLength);
                    }
                    Console.WriteLine(string.Format("File length was {0} bytes, read to byte position {1}.", memoryStream.Length, memoryStream.Position));

                    //HfScript hfsScript = scripts[0]; 
                    //Console.WriteLine(string.Format("Script Name is {0}", hfsScript.FileName));
                    //Console.WriteLine(string.Format("OpcodeStart={0}.", hfsScript.OpcodeStart));
                    //Console.WriteLine(string.Format("OpcodeLength={0}.", hfsScript.OpcodeLength));
                    //Console.WriteLine(string.Format("TextStart={0}.", hfsScript.TextStart));
                    //Console.WriteLine(string.Format("TextLength={0}.", hfsScript.TextLength));
                    //Console.WriteLine(string.Format("TextBytes={0}.", ByteArrayToStr(hfsScript.TextBytes)));

                }
            }

            foreach (HfScript script in scripts)
            {
                Console.WriteLine(string.Format("Writing script {0}", script.FileName));
                Directory.CreateDirectory(Path.Combine(this.OutputDirectory, Path.GetDirectoryName(script.FileName)));
                File.WriteAllBytes(Path.Combine(this.OutputDirectory, script.FileName), script.TextBytes);
                if (this.WriteOpcodes)
                {
                    File.WriteAllBytes(Path.Combine(this.OutputDirectory, script.OpFileName), script.OpcodeBytes);
                }
            }
            
        }

        // C# to convert a string to a byte array.
        public static byte[] StrToByteArray(string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        // C# to convert a byte array to a string.
        public static string ByteArrayToStr(byte[] bytes)
        {
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            return enc.GetString(bytes);
        }

        public bool LoadInputFile(string filename, out byte[] memArea)
        {
            memArea = null;

            try
            {
                memArea = File.ReadAllBytes(filename);

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
