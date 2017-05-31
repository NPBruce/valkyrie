using System.Collections.Generic;
using System.Text;
using System.IO;

class ObbExtract
{
    public static string GetAssets(string obb)
    {
        ZipFile zip = ZipFile.Read(obb);
        zip.ExtractAll(Path.GetTempPath() + "/Valkyrie/Obb");
        zip.Dispose();

        Dictionary<string, int> splitFiles = new Dictionary<string, int>();
        foreach (string s in Directory.GetFiles(Path.GetTempPath() + "/Valkyrie/Obb", "*.split*", SearchOption.AllDirectories))
        {
            string key = Path.GetDirectoryName(s) + "/" + Path.GetFileNameWithoutExtension(s);

            int count = 0;
            int.TryParse(Path.GetExtension(s).Substring(6), out count);

            if (splitFiles.ContainsKey(key))
            {
                if (splitFiles[key] < count)
                {
                    splitFiles[key] = count;
                }
            }
            else
            {
                splitFiles.Add(key, count);
            }
        }

        foreach (KeyValuePair<string, int> kv in splitFiles)
        {
            List<byte> data = new List<byte>();
            for (int i = 0; i <= kv.Value; i++)
            {
                data.AddRange(File.ReadAllBytes(kv.Key + ".Split" + i));
                File.Delete(kv.Key + ".Split" + i);
            }
            File.WriteAllBytes(kv.Key, data.ToArray());
        }

        return Path.GetTempPath() + "/Valkyrie/Obb";
    }

    public static void CleanTemp()
    {
        Directory.Delete(Path.GetTempPath() + "/Valkyrie/Obb", true);
    }
}
