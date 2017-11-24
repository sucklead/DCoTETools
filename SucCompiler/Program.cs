// Copyright © Microsoft Corporation.  All rights reserved.”  
// This posting is provided “AS IS” with no warranties of any kind and confers no rights.  
// Use of samples included in this posting are subject to the terms specified at http://www.microsoft.com/info/cpyright.htm.

using System;
using System.IO;

namespace CocCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = "";
            string target = "";
            string filename = "";

            bool addFunctions = false;

            if (args.Length < 2)
            {
                Console.WriteLine("SucCompiler by sucklead (http://dcotetools.sucklead.com/p/succompiler.html)");
                Console.WriteLine("To compile a single file");
                Console.WriteLine("SucCompiler {source directory} {binary directory} {source filename}");
                Console.WriteLine(@"e.g. To compile src\gamescripts\01_house\$debug\debugblack.hfs into directory bin");
                Console.WriteLine("from directory scripts:");
                Console.WriteLine(@"SucCompiler src bin gamescripts\01_house\$debug\debugblack.hfs");
                Console.WriteLine();
                Console.WriteLine("To compile all file in a directory");
                Console.WriteLine("SucCompiler {source directory} {binary directory}");
                Console.WriteLine("e.g. to compile everything in src to bin");
                Console.WriteLine("from directory scripts:");
                Console.WriteLine("SucCompiler src bin");
                return;
            }

            //set source and target
            source = args[0];
            target = args[1];

            Console.WriteLine("SucCompiler by sucklead invoked at {0}", DateTime.Now);
            Console.WriteLine("Source -> {0}", source);
            Console.WriteLine("Target -> {0}", target); 

            //addfunctions parameter?
            //if (args.Length > 3)
            //{
            //    if (bool.TryParse(args[1], out addFunctions))
            //    {
            //        addFunctions = false;
            //    }
            //}

            ////debug a script?
            //if (Debugger.IsAttached)
            //{
            //    Directory.SetCurrentDirectory(@"F:\Games\Call Of Cthulhu DCoTE\Scripts");

            //    filename = @"gamescripts\06_refinery\animatedreachtargets\goldvault\goldvaultsouthvalveturn.bin";
            //    //addFunctions = true;
            //}

            FunctionTable.LoadData();

            //set the options
            Compiler compiler = new Compiler();
            compiler.AddFunctions = addFunctions;
            compiler.SourceDirectory = source;
            compiler.TargetDirectory = target;

            if (args.Length > 2)
            {
                for (int a = 2; a < args.Length; a++)
                {
                    filename = args[a];
                    if (filename.StartsWith(source))
                    {
                        filename = filename.Substring(source.Length + 1);
                    }

                    if (File.Exists(Path.Combine(source, filename)))
                    {
                        compiler.CompileFile(filename);
                    }
                    else
                    {
                        Console.WriteLine("Error, Compile source {0} does not exist", Path.Combine(source, filename));
                        return;
                    }
                }
            }
            else
            {
                filename = "";
                if (Directory.Exists(source))
                {
                    compiler.CompileDirectory(source);
                }
                else
                {
                    Console.WriteLine("Error, Compile source {0} does not exist", source);
                    return;
                }
            }
            //save function table if anything has changed
            FunctionTable.SaveData();

            //Console.WriteLine("\n\nPress <Enter> to exit..");
            //Console.ReadLine();
        }

    }
}
