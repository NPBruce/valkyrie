using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace FFG.MoM
{
    public class Class1
    {
        public static string Get(string key)
        {
            List<string> outText = new List<string>();
            outText.Add("Set: " + Localization.localizationHasBeenSet);
            if (Localization.loadFunction == null)
            {
                outText.Add("Function Null");
            }
            TextAsset textAsset = Resources.Load<TextAsset>("Localization");
            if (textAsset != null)
            {
                outText.Add("Size: " + textAsset.bytes.Length);
            }

            File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\inject.txt", outText.ToArray());

            return "";
            //return Localization.Get(key);
        }
        
    }
}
