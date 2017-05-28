using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ObbExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, List<FilePart>> data = new Dictionary<string, List<FilePart>>();
            foreach (string file in Directory.GetFiles(Path.Combine(args[0], "Data")))
            {
                if (Path.GetExtension(file).IndexOf(".split") == 0)
                {
                    if (!data.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                    {
                        data.Add(Path.GetFileNameWithoutExtension(file), new List<FilePart>());
                    }
                    int fileNum = int.Parse(Path.GetExtension(file)[6].ToString());
                    if (Path.GetExtension(file).Length > 7)
                    {
                        fileNum *= 10;
                        fileNum += int.Parse(Path.GetExtension(file)[7].ToString());
                    }
                    data[Path.GetFileNameWithoutExtension(file)].Add(new FilePart(fileNum, File.ReadAllBytes(file)));
                }
                else
                {
                    //File.Copy(file, Path.Combine(args[0], "out/" + Path.GetFileName(file)));
                }
            }
            foreach (KeyValuePair<string, List<FilePart>> kv in data)
            {
                List<byte> fileData = new List<byte>();
                int partCount = 0;
                while (partCount < kv.Value.Count)
                {
                    foreach (FilePart part in kv.Value)
                    {
                        if (part.count == partCount)
                        {
                            fileData.AddRange(part.data);
                            partCount++;
                        }
                    }
                }
                File.WriteAllBytes(Path.Combine(args[0], "out/" + Path.GetFileName(kv.Key)), fileData.ToArray());
            }
        }
    }

    public class FilePart
    {
        public int count = 0;
        public byte[] data;

        public FilePart(int c, byte[] d)
        {
            count = c;
            data = d;
        }
    }
}
