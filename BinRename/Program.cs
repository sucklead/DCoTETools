using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinRename
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("BinRename {input batchfile}");
                return;
            }

            BinRename binRename = new BinRename();
            binRename.BatFilename = args[0];
            Console.WriteLine("Renaming internally...");
            binRename.Run();

        }
    }
}
