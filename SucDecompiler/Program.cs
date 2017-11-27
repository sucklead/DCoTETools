using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SucDecompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = "";
            string target = "";
            string filename = "";

            if (args.Length < 2)
            {
                Console.WriteLine("SucDeCompiler by sucklead (http://dcotetools.sucklead.com/p/sucdecompiler.html)");
                return;
            }

            //set source and target
            source = args[0];
            target = args[1];

            Console.WriteLine("SucDeCompiler by sucklead invoked at {0}", DateTime.Now);
            Console.WriteLine("Source -> {0}", source);
            Console.WriteLine("Target -> {0}", target);
            
            //FunctionTable.LoadData();

            //set the options
            DeCompiler deCompiler = new DeCompiler();
            deCompiler.SourceDirectory = source;
            deCompiler.TargetDirectory = target;

            if (args.Length > 2)
            {
                for (int a = 2; a < args.Length; a++)
                {
                    filename = args[a];
                    if (filename.StartsWith(source))
                    {
                        filename = filename.Substring(target.Length + 1);
                    }

                    if (File.Exists(Path.Combine(source, filename)))
                    {
                        deCompiler.DeCompileFile(filename);
                    }
                    else
                    {
                        Console.WriteLine("Error, DeCompile target {0} does not exist", Path.Combine(source, filename));
                        return;
                    }
                }
            }
            else
            {
                filename = "";
                if (Directory.Exists(source))
                {
                    deCompiler.DeCompileDirectory(source);
                }
                else
                {
                    Console.WriteLine("Error, Compile source {0} does not exist", source);
                    return;
                }
            }
            //save function table if anything has changed
            //FunctionTable.SaveData();

            Console.WriteLine("Compile complete.");
            //Console.WriteLine("\n\nPress <Enter> to exit..");
            //Console.ReadLine();
        }
    }
}
