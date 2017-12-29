// Copyright © Microsoft Corporation.  All rights reserved.”  
// This posting is provided “AS IS” with no warranties of any kind and confers no rights.  
// Use of samples included in this posting are subject to the terms specified at http://www.microsoft.com/info/cpyright.htm.

using System;
using System.IO;
using System.Reflection;

namespace CocCompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            string source = "";
            string target = "";
            string filename = "";

            bool addFunctions = false;

            if (args.Length < 2)
            {
                Console.WriteLine("SucCompiler by sucklead (http://dcotetools.sucklead.com/p/succompiler.html)");
                Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine();
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
                Console.WriteLine();
                Console.WriteLine("Easiest way to run it is from within the Scripts directory:");
                Console.WriteLine("SucCompiler src newbin");
                return -1;
            }

            //set source and target
            source = args[0];
            target = args[1];

            Console.WriteLine("SucCompiler by sucklead started at {0}", DateTime.Now);
            Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);
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

                    //convert to windows
                    filename = filename.Replace("/","\\");

                    if (File.Exists(Path.Combine(source, filename)))
                    {
                        compiler.CompileFile(filename);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Compile source {0} does not exist", Path.Combine(source, filename));
                        return -2;
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
                    Console.WriteLine("ERROR: Compile source {0} does not exist", source);
                    return -3;
                }
            }
            //save function table if anything has changed
            FunctionTable.SaveData();

            Console.WriteLine("Compile completed at {0}.", DateTime.Now);
            //Console.WriteLine("\n\nPress <Enter> to exit..");
            //Console.ReadLine();
            return 0;
        }

    }
}
