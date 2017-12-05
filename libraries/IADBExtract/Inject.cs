using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFG.IA;
using System.IO;

namespace IADBExtract
{
    public static class Inject
    {
        public static string outputDir = "C:\\Users\\Bruce\\Desktop\\IA\\";
        public static void Extract()
        {
            Extract(SingletonBehaviour<PersistentGameObject>.Instance.Database);
        }

        public static void Extract(IA_Database db)
        {
            Directory.CreateDirectory(outputDir);
            foreach(IA_ProductModel product in db.Products)
            {
                ExtractProduct(product);
            }
            foreach (IA_CampaignModel campaign in db.Campaigns)
            {

            }
        }

        public static void ExtractProduct(IA_ProductModel product)
        {
            Directory.CreateDirectory(outputDir + product.Code);

            List<string> packData = new List<string>();

            packData.Add("[ContentPack]");
            packData.Add("name={ffg:" + product.NameKey + "}");
            packData.Add("description={ffg:" + product.DescriptionKey + "}");
            packData.Add("; Contents " + product.ContentsKey);
            packData.Add("image=\"{import}/img/\"" + product.Image.Path + "\"");
            packData.Add("id=" + product.Code);
            packData.Add("type=" + product.Type.ToString());
            packData.Add("");

            packData.Add("[ContentPackData]");
            foreach (IA_EnemyTypeModel enemy in product.EnemyTypes)
            {

            }

            foreach (IA_InventoryItemModel item in product.Items)
            {

            }

            if (product.Heroes.Length > 0)
            {
                packData.Add("heroes.ini");
                packData.Add("skills.ini");
                List<string> heroData = new List<string>();
                List<string> skillData = new List<string>();
                foreach (IA_HeroModel hero in product.Heroes)
                {
                    heroData.Add("[Hero" + hero.NameKey + "]");
                    heroData.Add("name={ffg:" + hero.NameKey + "}");
                    heroData.Add("image=\"{import}/img/" + hero.Icon.Path + "\"");
                    heroData.Add("; Portrait " + hero.Portrait.Path);
                    heroData.Add("class=" + "Class" + hero.NameKey);
                    heroData.Add("; Weapon Type " + hero.WeaponType.ToString());
                    string heroTraits = "traits=" + hero.Gender.ToString();
                    foreach (IA_HeroModel.HeroTraits trait in Enum.GetValues(typeof(IA_HeroModel.HeroTraits)))
                    {
                        if ((hero.Traits & trait) != 0)
                        {
                            heroTraits += " " + trait.ToString().ToLower();
                        }
                    }

                    heroData.Add("[Class" + hero.NameKey + "]");
                    string startingItems = "items=";
                    foreach (IA_InventoryItemModel item in hero.StartingItems)
                    {
                        if (startingItems[startingItems.Length - 1] != '=')
                        {
                            startingItems += " ";
                        }
                        startingItems += "Item" + item.NameKey;
                    }
                    heroData.Add(startingItems);
                    heroData.Add("");

                    foreach (IA_SkillModel skill in hero.Skills)
                    {
                        skillData.Add("[Skill" + hero.NameKey + skill.NameKey + "]");
                        skillData.Add("name={ffg:" + skill.NameKey + "}");
                        skillData.Add("xp=" + skill.Cost);
                        string skillExceptions = "; Excpetions";
                        foreach (IA_SkillModel.SkillExceptions except in Enum.GetValues(typeof(IA_SkillModel.SkillExceptions)))
                        {
                            if ((skill.Exceptions & except) != 0)
                            {
                                skillExceptions += " " + except.ToString().ToLower();
                            }
                        }
                        skillData.Add(skillExceptions);
                        skillData.Add("");
                    }
                }
            }

            foreach (IA_AllyModel ally in product.Allies)
            {

            }

            File.WriteAllLines(outputDir + "\\content_pack.ini", packData.ToArray());
        }
    }
}
