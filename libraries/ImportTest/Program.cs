using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFGAppImport;


namespace ImportTest
{
    class Program
    {
        static void Main(string[] args)
        {
            FFGImport d2e = new FFGImport(GameType.D2E, Platform.Windows, ".");
            d2e.Inspect();
            if (d2e.ImportAvailable())
                Console.Write("D2E Import Avaialable");
            if (d2e.NeedImport())
                Console.Write("D2E Import Needed");
            if (d2e.Import())
                Console.Write("D2E Import Complete");

            FFGImport mom = new FFGImport(GameType.MoM, Platform.Windows, ".");
            mom.Inspect();
            if (mom.ImportAvailable())
                Console.Write("MoM Import Avaialable");
            if (mom.NeedImport())
                Console.Write("MoM Import Needed");
            if (mom.Import())
                Console.Write("MoM Import Complete");
        }
    }
}
