using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SucDecompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            string source = "";
            string target = "";
            string filename = "";

            if (args.Length < 2)
            {
                Console.WriteLine("SucDecompiler by sucklead (http://dcotetools.sucklead.com/p/sucdecompiler.html)");
                Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine();
                Console.WriteLine("To decompile a single file");
                Console.WriteLine("SucDecompiler {binary directory} {target directory} {binary filename}");
                Console.WriteLine(@"e.g. To decompile bin\gamescripts\01_house\$debug\debugblack.bin into directory src");
                Console.WriteLine("from the directory below bin:");
                Console.WriteLine(@"SucDecompiler bin src gamescripts\01_house\$debug\debugblack.hfs");
                Console.WriteLine();
                Console.WriteLine("To decompile all files in a directory");
                Console.WriteLine("SucDecompiler {binary directory} {target directory}");
                Console.WriteLine("e.g. to decompile everything in bin to src");
                Console.WriteLine("from the directory below bin:");
                Console.WriteLine("SucDecompiler bin src");
                Console.WriteLine();
                Console.WriteLine("Easiest way to run it is from within the Scripts directory:");
                Console.WriteLine("SucDecompiler bin src");
                Console.WriteLine();
                Console.WriteLine("The file fixtable.csv holds a list of fixes for specific files.");
                Console.WriteLine("They are to fix bugs in the original source and once those bugs");
                Console.WriteLine("are fixed they won't be needed anymore.");
                return throwError(1);
            }

            //set source and target
            source = args[0].Replace("/","\\");
            target = args[1].Replace("/", "\\");

            Console.WriteLine("SucDecompiler by sucklead started at {0}", DateTime.Now);
            Console.WriteLine("Version [{0}]", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Binaries -> {0}", source);
            Console.WriteLine("Target Source -> {0}", target);

            //FunctionTable.LoadData();

            string fixTableFile = "fixtable.csv";
            if (!File.Exists(fixTableFile))
            {
                fixTableFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "fixtable.csv");
                if (!File.Exists(fixTableFile))
                {
                    Console.Error.WriteLine("Fix table fixtable.csv not found!");
                    return throwError(-5);
                }
            }
            FixTable.LoadFixTable(fixTableFile);

            //set the options
            DeCompiler deCompiler = new DeCompiler();

            try
            {
                deCompiler.Initialise();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: {0}", ex);
                return throwError(-6);
            }

            deCompiler.SourceDirectory = source;
            deCompiler.TargetDirectory = target;

            if (args.Length > 2)
            {
                for (int a = 2; a < args.Length; a++)
                {
                    filename = args[a].Replace("/", "\\");
                    filename = filename.Replace(".hfs",".bin");
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
                        Console.Error.WriteLine("ERROR: Decompile target {0} does not exist", Path.Combine(source, filename));

                        if (Debugger.IsAttached)
                        {
                            Console.ReadLine();
                        }
                        return throwError(3);
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
                    Console.Error.WriteLine("ERROR: Decompile source {0} does not exist", source);
                    return throwError(4);
                }
            }
            //save function table if anything has changed
            //FunctionTable.SaveData();

            Console.WriteLine("Decompile completed at {0}.", DateTime.Now);
            //Console.WriteLine("\n\nPress <Enter> to exit..");
            return throwError(0);
        }

        static int throwError(int errorCode)
        {
            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
            return errorCode;
        }
    }
}
