using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using banallib;

namespace banallib
{
    public class BinFile
    {
        public string FileName { get; set; }
        public int FileLength { get; set; }
        private byte[] Contents { get; set; }

        public BinFile(string fileName)
        {
            FileName = fileName;
        }

        public bool Load()
        {
            Contents = null;

            try
            {
                Contents = File.ReadAllBytes(FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to open file check permissions");
                Console.WriteLine(ex.Message);
                return false;
            }

            //check if null
            if (Contents == null)
            {
                Console.WriteLine("ERROR: memory is null");
                return false;
            }

            FileLength = Contents.Length;

            return true;

        }
        
        public bool Save(bool asNew)
        {
            return Save(asNew, this.Contents);
        }

        public bool Save(bool asNew, byte[] contents)
        {

            //check if null
            if (contents == null)
            {
                Console.WriteLine("ERROR: memory is empty!");
                return false;
            }

            if (asNew)
            {
                File.WriteAllBytes(FileName + ".new", contents);
            }
            else
            {
                File.WriteAllBytes(FileName, contents);
            }

            return true;
        }

        public string ReadString(BinaryReader reader)
        {
            StringBuilder nameBuilder = new StringBuilder();
            while (reader.PeekChar() != 0)
            {
                nameBuilder.Append(reader.ReadChar());
            }
            //move past 0
            reader.ReadChar();
            return nameBuilder.ToString();
        }

        private string ReadString(BinaryReader reader, int position)
        {

            //store current position
            long currentPosition = reader.BaseStream.Position;
            //move to reference position
            reader.BaseStream.Position = position;

            string value = this.ReadString(reader);

            //back to previous position
            reader.BaseStream.Position = currentPosition;

            return value;
        }

        public ParsedContent Parse()
        {
            ParsedContent parsedContent = new ParsedContent();
            parsedContent.OriginalFile = this;

            using (MemoryStream memoryStream = new MemoryStream(Contents))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    parsedContent.ScriptName = this.ReadString(reader);
                    Console.WriteLine("Script Name: {0}", parsedContent.ScriptName);

                    reader.BaseStream.Position = 256;

                    parsedContent.LengthOfOpCodes = reader.ReadInt32();
                    Console.WriteLine("LengthOfOpCodes: {0}", parsedContent.LengthOfOpCodes);

                    parsedContent.StartOfOpCodes = reader.ReadInt32();
                    Console.WriteLine("StartOfOpCodes: {0}", parsedContent.StartOfOpCodes);

                    parsedContent.NumberOfValues = reader.ReadInt32();
                    Console.WriteLine("NumberOfValues: {0}", parsedContent.NumberOfValues);

                    parsedContent.BaseAddress = (Int16)reader.ReadInt32();
                    Console.WriteLine("BaseAddress: {0}", parsedContent.BaseAddress);
                    if (parsedContent.BaseAddress == 430)
                    {
                        parsedContent.IsOldFile = true;
                    }

                    parsedContent.StartOfValues = reader.ReadInt32();
                    Console.WriteLine("StartOfValues in file: {0}", parsedContent.StartOfValues);

                    //make sure we are at start of opcodes
                    //reader.BaseStream.Position = parsedContent.StartOfOpCodes;
                    //parsedContent.OpCodeList = new List<Operation>();
                    //read through NumberOfSomething
                    //for (int i = 0; i < (parsedContent.LengthOfOpCodes / 4); i++)
                    //{
                    //    Unknown1 unknown1 = new Unknown1();
                    //    unknown1.PossibleAddress = reader.ReadInt16();
                    //    unknown1.UnknownByte1 = reader.ReadByte();
                    //    unknown1.UnknownByte2 = reader.ReadByte();

                    //    parsedContent.Unknown1List.Add(unknown1);
                    //}



                    ParseValues(parsedContent, reader);

                    ParseOpCodes(parsedContent, reader);


                }
            }

            return parsedContent;
        }

        private void ParseOpCodes(ParsedContent parsedContent, BinaryReader reader)
        {
            int endOfOpcodes = parsedContent.StartOfOpCodes + parsedContent.LengthOfOpCodes;

            //make sure we are at start of opcodes
            reader.BaseStream.Position = parsedContent.StartOfOpCodes;
            parsedContent.OpCodeList = new List<Operation>();

            //read through commmands
            while (reader.BaseStream.Position < endOfOpcodes)
            {
                Operation op = new Operation();
                op.Address = (short)(reader.BaseStream.Position - parsedContent.StartOfOpCodes);

                op.OpCode = (OpCodeType)reader.ReadByte();

                //push has an index parameter
                if (op.OpCode == OpCodeType.OP_PUSH
                    || op.OpCode == OpCodeType.OP_GETTOP)
                {
                    //op.Parameters.Add(reader.ReadInt16());
                    op.DataIndex = new DataIndex();
                    op.DataIndex.Value = reader.ReadInt16();
                    if (op.DataIndex.Value < parsedContent.BaseAddress)
                    {
                        op.DataIndex.IsFunctionPointer = true;
                    }
                }
                else if (op.OpCode == OpCodeType.OP_JMP
                         || op.OpCode == OpCodeType.OP_JMPF
                         || op.OpCode == OpCodeType.JUMPTARGET)
                {
                    //op.Parameters.Add(reader.ReadInt16());
                    op.DataIndex = new DataIndex();
                    op.DataIndex.Value = reader.ReadInt16();
                }

                //else if (op.OpCode == OpCodeType.OP_FUNCTION)
                //{
                //    //set last push as a function pointer
                //    parsedContent.OpCodeList[parsedContent.OpCodeList.Count].IsFunctionPointer = true;
                //}

                //add operation to list
                parsedContent.OpCodeList.Add(op);
                
            }

        }

        public void ReWriteOpcodes(ParsedContent parsedContent)
        {
            using (MemoryStream memoryStream = new MemoryStream(Contents))
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    writer.BaseStream.Position = 268;
                    writer.Write(parsedContent.BaseAddress);

                    //make sure we are at start of opcodes
                    writer.BaseStream.Position = parsedContent.StartOfOpCodes;

                    foreach (Operation op in parsedContent.OpCodeList)
                    {
                        //write the 
                        writer.Write((byte)op.OpCode);

                        if (op.DataIndex != null)
                        {
                            writer.Write((Int16)op.DataIndex.Value);
                        }
                    }

                }
            }

        }

        private void ParseValues(ParsedContent parsedContent, BinaryReader reader)
        {
            //make sure we are at start of values
            reader.BaseStream.Position = parsedContent.StartOfValues;
            parsedContent.ValuesList = new List<Value>();


            //read through commmands
            for (short i = 0; i < parsedContent.NumberOfValues; i++)
            {
                Value value = new Value();
                value.Address = (short)(parsedContent.BaseAddress + i);
                value.DataType = (DataTypeType)reader.ReadByte();

                if (value.DataType == DataTypeType.Character
                    || value.DataType == DataTypeType.String
                    || value.DataType == DataTypeType.Point
                    || value.DataType == DataTypeType.Quaternion)
                {
                    //always a reference
                    value.Reference = reader.ReadInt32();

                    long position = reader.BaseStream.Position;

                    //move to reference
                    reader.BaseStream.Position = value.Reference;
                    if (value.DataType == DataTypeType.String)
                    {
                        //print has one char reference
                        value.SubValues.Add(this.ReadString(reader));
                    }
                    else if (value.DataType == DataTypeType.Character)
                    {

                        //print has one char reference
                        value.SubValues.Add((Int32)reader.ReadInt32());
                        reader.ReadByte(); //a zero here
                        value.SubValues.Add(this.ReadString(reader));
                        value.SubValues.Add((Int32)reader.ReadInt32());
                    }
                    else if (value.DataType == DataTypeType.Point)
                    {
                        //x, y, z floats
                        value.SubValues.Add((Int32)reader.ReadInt32());
                        value.SubValues.Add((Int32)reader.ReadInt32());
                        value.SubValues.Add((Int32)reader.ReadInt32());
                    }
                    else if (value.DataType == DataTypeType.Quaternion)
                    {
                        //x, y, z floats
                        value.SubValues.Add((Int32)reader.ReadInt32());
                        value.SubValues.Add((Int32)reader.ReadInt32());
                        value.SubValues.Add((Int32)reader.ReadInt32());
                        value.SubValues.Add((Int32)reader.ReadInt32());
                    }

                    //go back to correct position
                    reader.BaseStream.Position = position;
                }
                else if (value.DataType == DataTypeType.Int
                         || value.DataType == DataTypeType.Float)
                {
                    value.Reference = (Int32)(reader.BaseStream.Position);
                    // no params
                    value.SubValues.Add(reader.ReadInt32());
                }



                parsedContent.ValuesList.Add(value);
            }
        }

    }
}
