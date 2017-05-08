using System;
using System.Collections.Generic;
using System.IO;
using FFG.RtL;

namespace Injection
{
    public class Class1
    {
        public static int alwaysFive(bool ownedOnly)
        {
            foreach (ProductModel product in UserCollectionManager.GetProductCollection(false))
            {
                Directory.CreateDirectory("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code);

                List<string> outText = new List<string>();
                outText.Add("Type: " + product.Type);
                outText.Add("Name: " + product.Name);
                outText.Add("Description: " + product.Description);
                outText.Add("Contents: " + product.Contents);
                outText.Add("Image: " + product.Image.Path);
                outText.Add("Size: " + product.PowerMeterValue);

                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\" + product.Code + ".txt", outText.ToArray());

                outText = new List<string>();
                foreach (HeroModel hm in product.GetHeroes())
                {
                    outText.Add("[" + GenerateIniName(hm.NameKey) + "]");
                    outText.Add("name={ffg:" + hm.NameKey + "}");
                    outText.Add("image=" + ImagePath(hm.Image.Path, product));
                    outText.Add("archetype=" + hm.Archetype.ToString().ToLower());
                    outText.Add("traits=" + hm.Gender.ToString().ToLower());
                    outText.Add("");
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\heroes.ini", outText.ToArray());

                outText = new List<string>();
                List<string> outItems = new List<string>();
                foreach (ClassModel cm in product.GetClasses())
                {
                    outText.Add("[" + GenerateIniName(cm.ClassName.Key) + "]");
                    outText.Add("name={ffg:" + cm.ClassName.Key + "}");
                    //cm.GetSkills
                    if (cm.IsHybrid)
                    {
                        outText.Add("hybridarchetype=" + cm.HybridArchetype);
                    }
                    foreach (InventoryItemModel im in cm.GetStartingItems())
                    {
                        outItems.Add(ParseItem(im, product));
                    }
                    outText.Add("");
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\classes.ini", outText.ToArray());

                foreach (InventoryItemModel im in product.GetItems())
                {
                    outItems.Add(ParseItem(im, product));
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\items.ini", outItems.ToArray());

                outText = new List<string>();
                foreach (MonsterGroupModel mm in product.GetMonsters())
                {
                    outText.Add("");
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\monsters.ini", outText.ToArray());

                outText = new List<string>();
                foreach (ProductMonsterActivationModel am in product.AdditionalActivations)
                {
                    outText.Add("");
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\activations.ini", outText.ToArray());

                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + ".txt", outText.ToArray());
            }

            return 5;
        }

        public static string GenerateIniName(string input)
        {
            string name = "";
            string[] sections = input.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in sections)
            {
                char[] lower = s.ToLower().ToCharArray();
                lower[0] = s[0];
                name += new string(lower);
            }
            return name;
        }

        public static string ParseItem(InventoryItemModel item, ProductModel p)
        {
            string outText = "[" + GenerateIniName(item.ItemName.Key) + "]\n";
            outText += "name={ffg:" + item.ItemName.Key + "}\n";
            outText += "image=" + ImagePath(item.Icon.Path, p) + "\n";
            outText += "traits=";
            foreach (ItemTraits trait in Enum.GetValues(typeof(ItemTraits)))
            {
                if ((item.Traits & trait) != 0)
                {
                    outText += trait.ToString().ToLower() + " ";
                }
            }
            outText += item.Deck.ToString().ToLower() + "\n";
            outText += "minfame=" + item.MinGroupLevel.ToString().ToLower() + "\n";
            outText += "maxfame=" + item.MaxGroupLevel.ToString().ToLower() + "\n";
            outText += "gold=" + item.CostBuy + "\n";
            return outText;
        }

        public static string ImagePath(string path, ProductModel p)
        {
            if (p.Type == ExpansionType.CORE_SET)
            {
                return path.Replace("Assets/Resources/Textures/Items", "../ffg/img");
            }
            return path.Replace("Assets/Resources/Textures/Items", "../../ffg/img");
        }
    }
}
