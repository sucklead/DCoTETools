using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using banallib;

namespace banallib
{
    public class Analyse
    {
        public bool SaveAsNew { get; set; }
        public short NewBase { get; set; }
        public bool BuildNew { get; set; }

        private List<byte> newContent = new List<byte>();

        //public List<string> Files = new List<string>();

        //public void DoAnalyses()
        //{
        //    Console.WriteLine("--------------------------------------------------");
        //    //foreach (var filename in Files)
        //    //{
        //        AnalyseFile(filename);
        //        Console.WriteLine("--------------------------------------------------");
        //    //}
        //}

        public ParsedContent AnalyseFile(string filename)
        {
            BinFile binFile = new BinFile(filename);
            if (!File.Exists(filename)
                || !binFile.Load())
            {
                Console.WriteLine("Failed to load file");
                return null;
            }
            Console.WriteLine("Loading file {0} ({1})...", binFile.FileName, binFile.FileLength);
            ParsedContent parsedContent = binFile.Parse();
            //if (parsedContent.ValuesList != null)
            //{
            //    int i = 0;
            //    foreach (Value value in parsedContent.ValuesList)
            //    {
            //        i++;
            //        //Console.WriteLine("{0}: {1} ({2})", i, op.OpName, op.ReferenceValue);
            //        //Console.WriteLine("{0}: @[{1}] {2} ({3})", i, parsedContent.BaseAddress + i + 1, value.DataType, value.Reference);
            //        Console.WriteLine("{1}: {2} ({3})", i, value.Address, value.DataType, value.Reference);
            //        foreach (var subValue in value.SubValues)
            //        {
            //            if (subValue is string)
            //            {
            //                Console.WriteLine("    \"{0}\"", subValue);
            //            }
            //            else if (subValue is Int32 || subValue is float)
            //            {
            //                Console.WriteLine("    {0}", subValue);
            //            }
            //        }
            //    }
            //}

            //if (parsedContent.OpCodeList != null)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("OP CODES");
            //    Console.WriteLine("--------");

            //    DisplayOpCodes(parsedContent);
            //}

            return parsedContent;
        }

        public void PostAnalysis(ParsedContent parsedContent)
        {

            //if (parsedContent.IsOldFile)
            //{
            //    bool rewroteOK = ReWriteOpcodes(parsedContent);

            //    if (rewroteOK)
            //    {
            //        Console.WriteLine();
            //        Console.WriteLine("NEW OP CODES");
            //        Console.WriteLine("--------");

            //        DisplayOpCodes(parsedContent);

            //        parsedContent.OriginalFile.ReWriteOpcodes(parsedContent);

            //        if (!parsedContent.OriginalFile.Save(this.SaveAsNew))
            //        {
            //            Console.WriteLine("Failed to save file!!!");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("!!! FAILED TO REWRITE !!!");
            //    }
            //}
            //else if (this.BuildNew)
            //{
            //    BuildNewMemory(parsedContent.OriginalFile, parsedContent);
            //}

        }

        private void BuildNewMemory(BinFile binFile, ParsedContent parsedContent)
        {
            Console.WriteLine("Building new memory...");


            Console.WriteLine("Writing name.");
            int nameLength = parsedContent.ScriptName.Length;

            newContent.Clear();
            char[] name = parsedContent.ScriptName.ToCharArray();
            foreach (char c in name)
            {
                newContent.Add((byte)c);
            }
            newContent.Add(0x00);
            for (int i = 0; i < 255 - nameLength; i++)
            {
                newContent.Add(0xCD);
            }

            Console.WriteLine("Writing stub values.");

            int opCount = 0;
            //opcodes
            foreach (Operation op in parsedContent.OpCodeList)
            {
                opCount++;

                if (op.DataIndex != null)
                {
                    opCount += 2;
                }
            }

            foreach (Byte b in BitConverter.GetBytes((Int32)opCount))
            {
                newContent.Add(b);
            }

            //startofcodes
            foreach (Byte b in BitConverter.GetBytes((Int32)276))
            {
                newContent.Add(b);
            }
            
            //numberofvalues
            foreach (Byte b in BitConverter.GetBytes((Int32)parsedContent.ValuesList.Count))
            {
                newContent.Add(b);
            }

            //base address
            foreach (Byte b in BitConverter.GetBytes((Int32)760))
            {
                newContent.Add(b);
            }

            //startofvalues
            foreach (Byte b in BitConverter.GetBytes((Int32)opCount + 276))
            {
                newContent.Add(b);
            }

            //write the opcodes
            foreach (Operation op in parsedContent.OpCodeList)
            {
                newContent.Add((byte)op.OpCode);
                if (op.DataIndex != null)
                {
                    byte[] dataIndex = BitConverter.GetBytes(op.DataIndex.Value);
                    foreach (byte b in dataIndex)
                    {
                        newContent.Add(b);
                    }
                    
                }

            }

            //write the value list
            foreach (Value value in parsedContent.ValuesList)
            {
                newContent.Add((byte)value.DataType);
                if (value.DataType == DataTypeType.Character
                    || value.DataType == DataTypeType.String
                    || value.DataType == DataTypeType.Point
                    || value.DataType == DataTypeType.Quaternion)
                {
                    byte[] refBytes = BitConverter.GetBytes(value.Reference);
                    foreach (byte b in refBytes)
                    {
                        newContent.Add(b);
                    }

                }
                else if (value.DataType == DataTypeType.Int
                         || value.DataType == DataTypeType.Float)
                {
                    byte[] valueBytes = BitConverter.GetBytes((Int32)value.SubValues[0]);
                    foreach (byte b in valueBytes)
                    {
                        newContent.Add(b);
                    }
                }
            }

            //write the references
            foreach (Value value in parsedContent.ValuesList)
            {
                //if (value.DataType == DataTypeType.Character
                //    || value.DataType == DataTypeType.String
                //    || value.DataType == DataTypeType.Point
                //    || value.DataType == DataTypeType.Quaternion)
                //{
                //    foreach (object o in value.SubValues)
                //    {
                //        byte[] oBytes = BitConverter.GetBytes(o);
                //        foreach (byte b in oBytes)
                //        {
                //            newContent.Add(b);
                //        }
                //    }
                //}
                if (value.DataType == DataTypeType.Character)
                {
                    byte[] characterBytes = BitConverter.GetBytes((Int32)value.SubValues[0]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                    newContent.Add((byte)0x00);
                    char[] characters = ((string)value.SubValues[1]).ToCharArray();
                    foreach (char c in characters)
                    {
                        newContent.Add((byte)c);
                    }
                    newContent.Add((byte)0x00);

                    characterBytes = BitConverter.GetBytes((Int32)value.SubValues[2]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                }
                else if (value.DataType == DataTypeType.String)
                {
                    char[] characters = ((string)value.SubValues[0]).ToCharArray();
                    foreach (char c in characters)
                    {
                        newContent.Add((byte)c);
                    }
                    newContent.Add((byte)0x00);
                }
                else if (value.DataType == DataTypeType.Point)
                {
                    byte[] characterBytes = BitConverter.GetBytes((Int32)value.SubValues[0]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                    characterBytes = BitConverter.GetBytes((Int32)value.SubValues[1]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                    characterBytes = BitConverter.GetBytes((Int32)value.SubValues[2]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                }
                else if (value.DataType == DataTypeType.Quaternion)
                {
                    byte[] characterBytes = BitConverter.GetBytes((Int32)value.SubValues[0]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                    characterBytes = BitConverter.GetBytes((Int32)value.SubValues[1]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                    characterBytes = BitConverter.GetBytes((Int32)value.SubValues[2]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                    characterBytes = BitConverter.GetBytes((Int32)value.SubValues[3]);
                    foreach (byte b in characterBytes)
                    {
                        newContent.Add(b);
                    }
                }



            }

            //convert to byte[]
            byte[] newFile = newContent.ToArray();

            Console.WriteLine("Saving new file...");
            binFile.Save(true, newFile);
            Console.WriteLine("OK.");
        }

        private static void DisplayOpCodes(ParsedContent parsedContent)
        {
            foreach (Operation op in parsedContent.OpCodeList)
            {
                if (op.DataIndex != null)
                {
                    Console.Write("[{0}] {1}", op.Address, op.OpCode);

                    if (op.DataIndex.IsFunctionPointer)
                    {
                        //if (parsedContent.IsOldFile)
                        //{
                        //    //OldFunctionType oft = (OldFunctionType)op.DataIndex.Value;
                        //    string oldFunction;
                        //    if (FunctionTable.Functions.ContainsKey(op.DataIndex.Value))
                        //    {
                        //        oldFunction = FunctionTable.Functions[op.DataIndex.Value];
                        //    }
                        //    else
                        //    {
                        //        oldFunction = "?";
                        //    }

                        //    Console.WriteLine(string.Format(" -> {0} ({1})", op.DataIndex.Value, oldFunction));
                        //}
                        //else
                        //{
                            string newFunction;
                            if (FunctionTable.Functions.ContainsKey(op.DataIndex.Value))
                            {
                                newFunction = FunctionTable.Functions[op.DataIndex.Value];
                            }
                            else
                            {
                                newFunction = "?";
                            }

                            //NewFunctionType nft = (NewFunctionType)op.DataIndex.Value;
                            Console.WriteLine(string.Format(" -> {0} ({1})", op.DataIndex.Value, newFunction));
                        //}

                    }
                    else
                    {
                        var qValue = from value in parsedContent.ValuesList 
                                     where value.Address == op.DataIndex.Value
                                     select value;
                        Value v = qValue.FirstOrDefault();
                        if (v != null)
                        {
                            Console.WriteLine(" -> {0} [{1}] ({2})", op.DataIndex.Value, v.DataType, v.SubValues[0]);
                        }
                        else
                        {
                            Console.WriteLine(" -> " + op.DataIndex.Value.ToString());
                        }
                    }

                    if (op.OpCode == OpCodeType.OP_JMP
                        || op.OpCode == OpCodeType.OP_JMPF)
                    {
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("[{0}] {1}", op.Address, op.OpCode);
                    if (op.OpCode == OpCodeType.OP_PRINT
                        || op.OpCode == OpCodeType.OP_DISCARD)
                    {
                        Console.WriteLine();
                    }
                }

            }
        }

        //public bool ReWriteOpcodes(ParsedContent parsedContent)
        //{
        //    short addToAdresses = 175;

        //    addToAdresses = (short)(this.NewBase - parsedContent.BaseAddress);
        //    Console.WriteLine(string.Format("Add to addresses: {0}", addToAdresses));

        //    parsedContent.BaseAddress += addToAdresses;
        //    foreach (Operation op in parsedContent.OpCodeList)
        //    {
        //        if ((op.OpCode == OpCodeType.OP_PUSH
        //             || op.OpCode == OpCodeType.OP_GETTOP)
        //            && op.DataIndex != null)
        //        {
        //            if (op.DataIndex.IsFunctionPointer)
        //            {
        //                // have function conversion?
        //                if (!FunctionTable.FunctionConversion.ContainsKey(op.DataIndex.Value)
        //                    || FunctionTable.FunctionConversion[op.DataIndex.Value] < 0)
        //                {
        //                    return false;
        //                }
        //                //NewFunctionType nft = (NewFunctionType)FunctionTable.FunctionConversion[op.DataIndex.Value];
        //                //op.DataIndex.Value = (Int16)nft;
        //                op.DataIndex.Value = FunctionTable.FunctionConversion[op.DataIndex.Value];
        //            }
        //            else
        //            {
        //                op.DataIndex.Value += addToAdresses;
        //            }
        //        }
        //    }
        //    parsedContent.IsOldFile = false;

        //    return true;
        //}
    }
}
