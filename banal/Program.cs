using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using banallib;

namespace banal
{
    class Program
    {
        static void Main(string[] args)
        {

            FunctionTable.LoadData();

            Analyse analyse = new Analyse();

            //analyse.Files.Add(@"F:\Games\Call Of Cthulhu DCoTE\Testing\0.bin");
            //analyse.Files.Add(@"F:\Games\Call Of Cthulhu DCoTE\Testing\1.bin");
            //analyse.Files.Add(@"F:\Games\Call Of Cthulhu DCoTE\Testing\2.bin");
            //analyse.Files.Add(@"F:\Games\Call Of Cthulhu DCoTE\Testing\3.bin");
            //analyse.Files.Add(@"F:\Games\Call Of Cthulhu DCoTE\Testing\4.bin");
            //analyse.Files.Add(args[0]);

            if (args.Length > 1)
            {
                analyse.SaveAsNew = (args[1] != "asold");
                analyse.BuildNew = (args[1] == "buildnew");
            }

            analyse.NewBase = 605;
            //1AE = 430
            //25D = 605
            //2F8 = 760
            if (args.Length > 2)
            {
                short parseShort = 0;
                short.TryParse(args[2], out parseShort);
                analyse.NewBase = parseShort;
            }

            ParsedContent parsedContent = analyse.AnalyseFile(args[0]);
            analyse.PostAnalysis(parsedContent);

            Console.WriteLine("Finished.");
            //Console.ReadLine();
        }
    }
}

