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
                    outText.Add("[Hero" + GenerateIniName(hm.NameKey) + "]");
                    outText.Add("name={ffg:" + hm.NameKey + "}");
                    outText.Add("image=" + ImagePath(hm.Image.Path, product));
                    outText.Add("archetype=" + hm.Archetype.ToString().Split(" ".ToCharArray())[0].ToLower());
                    outText.Add("traits=" + hm.Gender.ToString().ToLower());
                    outText.Add("");
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\heroes.ini", outText.ToArray());

                outText = new List<string>();
                List<string> outItems = new List<string>();
                foreach (ClassModel cm in product.GetClasses())
                {
                    outText.Add("[Class" + GenerateIniName(cm.ClassName) + "]");
                    outText.Add("name={ffg:" + cm.ClassName.Key + "}");
                    //cm.GetSkills
                    if (cm.IsHybrid)
                    {
                        outText.Add("hybridarchetype=" + cm.HybridArchetype.ToString().Split(" ".ToCharArray())[0].ToLower());
                    }
                    string itemList = "items=";
                    foreach (InventoryItemModel im in cm.GetStartingItems())
                    {
                        outItems.Add(ParseItem(im, product));
                        itemList += GenerateIniName(im.ItemName.Key) + " ";
                    }
                    outText.Add(itemList.Substring(0, itemList.Length - 1));
                    outText.Add("");

                    foreach (ClassModel.Skill s in cm.GetSkills())
                    {
                        outText.Add("[Skill" + GenerateIniName(cm.ClassName) + GenerateIniNameKey(s.NameKey) + "]");
                        outText.Add("name={ffg:" + s.NameKey + "}");
                        outText.Add("xp=" + s.Cost);
                        outText.Add("");
                    }
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\classes.ini", outText.ToArray());

                foreach (InventoryItemModel im in product.GetItems())
                {
                    outItems.Add(ParseItem(im, product));
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\items.ini", outItems.ToArray());

                List<string> activationText = new List<string>();
                outText = new List<string>();
                foreach (MonsterGroupModel mm in product.GetMonsters())
                {
                    outText.Add("[Monster" + GenerateIniName(mm.MonsterGroupLocalizedName) + "]");
                    outText.Add("info={ffg:" + mm.Instructions.Key + "}");
                    outText.Add("image=" + ImagePath(mm.Monster.Image.Path, product));
                    outText.Add("imageplace=" + ImagePath(mm.Monster.ImagePlacement.Path, product));
                    string traits="traits=";
                    foreach (MonsterTraitsSet1 trait in Enum.GetValues(typeof(MonsterTraitsSet1)))
                    {
                        if ((mm.TraitsSet1 & trait) != 0)
                        {
                            traits += trait.ToString().ToLower() + " ";
                        }
                    }

                    foreach (MonsterTraitsSet2 trait in Enum.GetValues(typeof(MonsterTraitsSet2)))
                    {
                        if ((mm.TraitsSet2 & trait) != 0)
                        {
                            traits += trait.ToString().ToLower() + " ";
                        }
                    }
                    if (mm.IsLieutenant)
                    {
                        traits += "lieutenant ";
                    }

                    if (mm.Size == MonsterSize.SMALL_1x1)
                    {
                        traits += "small";
                    }
                    if (mm.Size == MonsterSize.MEDIUM_1x2)
                    {
                        traits += "medium";
                    }
                    if (mm.Size == MonsterSize.LARGE_2x2)
                    {
                        traits += "huge";
                    }
                    if (mm.Size == MonsterSize.HUGE_2x3)
                    {
                        traits += "massive";
                    }

                    outText.Add(traits);
                    outText.Add("; Exclude Minions: 2: " + mm.ExcludeMinions2Players + " 3: " + mm.ExcludeMinions3Players + " 4: " + mm.ExcludeMinions4Players);

                    string tempString = "";
                    foreach (MonsterActivationTypeModel at in mm.ActivationTypes)
                    {
                        tempString = "activation=";
                        foreach (MonsterActivationModel a in at.activations)
                        {
                            tempString += GenericActivation(a.ActivationEffect.Key) + " ";
                        }
                        outText.Add(tempString.Substring(0, tempString.Length - 1));
                        tempString = "; Stunned Activations: ";
                        foreach (MonsterActivationModel a in at.stunnedActivations)
                        {
                            tempString += a.ActivationEffect.Key + " ";
                        }
                        outText.Add(tempString.Substring(0, tempString.Length - 1));
                    }

                    tempString = "; Stunned activations: ";
                    foreach (MonsterActivationModel a in mm.ActivationsStunned)
                    {
                        tempString += GenericActivation(a.ActivationEffect.Key) + " ";
                    }
                    outText.Add(tempString.Substring(0, tempString.Length - 1));
                    outText.Add("");

                    foreach (MonsterActivationModel a in mm.Activations)
                    {
                        activationText.Add(GetActivation(a, GenerateIniName(mm.MonsterGroupLocalizedName)));
                    }
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\monsters.ini", outText.ToArray());

                foreach (ProductMonsterActivationModel aa in product.AdditionalActivations)
                {
                    foreach (MonsterActivationModel am in aa.Activations)
                    {
                        activationText.Add(GetActivation(am, GenerateIniName(aa.Monster.MonsterGroupLocalizedName)));
                    }
                }
                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + "\\activations.ini", activationText.ToArray());
            }

            List<string> cText = new List<string>();
            foreach (CampaignModel cm in RtL_DB_Manager.DB.GetCampaigns())
            {
                cText.Add(cm.name);
                cText.Add(FameToString(cm.CampaignFameThresholds));
            }
            File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\campaigns.txt", cText.ToArray());

            return 5;
        }



        public static string FameToString(CampaignModel.FameThresholds fame)
        {
            string returnStr = "";
            returnStr += "Celebrated: " + fame.Celebrated + "\n";
            returnStr += "Heroic: " + fame.Heroic + "\n";
            returnStr += "Impressive: " + fame.Impressive + "\n";
            returnStr += "Insignificant: " + fame.Insignificant + "\n";
            returnStr += "Legendary: " + fame.Legendary + "\n";
            returnStr += "Noteworthy: " + fame.Noteworthy + "\n";
            return returnStr;
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

        public static string GenerateIniName(LocalizationPacket input)
        {
            return GenerateIniNameKey(input.Key);
        }
        public static string GenerateIniNameKey(string key)
        {
            string name = Localization.Get(key);
            string[] sections = name.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string outputName = "";
            foreach (string s in sections)
            {
                char[] lower = s.ToLower().ToCharArray();
                lower[0] = s[0];
                outputName += new string(lower);
            }
            return outputName;
        }

        public static string ParseItem(InventoryItemModel item, ProductModel p)
        {
            bool relic = false;
            if (item.ItemName.Key.IndexOf("RELIC_") == 0)
            {
                relic = true;
            }
            string outText = "[" + GenerateIniName(item.ItemName.Key.Replace("RELIC_", "ITEM_")) + "]\n";
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
            if (!relic)
            {
                outText += "minfame=" + item.MinGroupLevel.ToString().ToLower() + "\n";
                outText += "maxfame=" + item.MaxGroupLevel.ToString().ToLower() + "\n";
                outText += "price=" + item.CostBuy + "\n";
            }
            return outText;
        }

        public static string ImagePath(string path, ProductModel p)
        {
            if (p.Type == ExpansionType.CORE_SET)
            {
                return path.Replace("Assets/Resources/Textures/Items", "\"../ffg/img").Replace(".png", ".dds") + "\"";
            }
            return path.Replace("Assets/Resources/Textures/Items", "\"../../ffg/img").Replace(".png", ".dds") + "\"";
        }

        public static string GenericActivation(string inName)
        {
            string[] elements = inName.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length < 3) return inName;
            char[] name = elements[1].ToLower().ToCharArray();
            name[0] = elements[1][0];
            return new string(name) + elements[2];
        }

        public static string GetActivation(MonsterActivationModel a, string monster)
        {
            string activation = "[MonsterActivation" + monster + GetActivationNumber(a.ActivationEffect.Key) + "]\n";
            activation += "ability={ffg:" + a.ActivationEffect.Key + "}\n";
            foreach (LocalizationPacket lp in a.MasterActivations)
            {
                activation += "master={ffg:" + lp.Key + "}\n";
            }
            foreach (LocalizationPacket lp in a.MinionActivations)
            {
                activation += "minion={ffg:" + lp.Key + "}\n";
            }
            if (a.MasterPriority)
            {
                activation += "masterfirst=true\n";
            }
            else
            {
                activation += "minionfirst=true\n";
            }
            return activation;
        }

        public static string GetActivationNumber(string a)
        {
            string[] elements = a.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in elements)
            {
                if (char.IsNumber(s[0]))
                {
                    return s;
                }
            }
            return "0";
        }
    }
}
