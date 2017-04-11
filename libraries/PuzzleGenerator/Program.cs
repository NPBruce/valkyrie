using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            int level = 2;
            if (args.Length > 0) int.TryParse(args[0], out level);
            int index = 1;
            while (true)
            {
                PuzzleSlide ps = new PuzzleSlide(level);

                string write = ps.ToString(index.ToString());
                index++;

                using (StreamWriter sw = File.AppendText("output.ini"))
                {
                    sw.WriteLine(write);
                }
            }
        }
    }
}

public class Vector2
{
    public float x;
    public float y;

    public Vector2(float xIn, float yIn)
    {
        x = xIn;
        y = yIn;
    }
}