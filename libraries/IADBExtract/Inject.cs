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
            if (product.EnemyTypes.Length > 0)
            {
                packData.Add("enemies.ini");
                List<string> enemyData = new List<string>();
                List<string> enemyDataElite = new List<string>();
                foreach (IA_EnemyTypeModel enemy in product.EnemyTypes)
                {
                    enemyData.Add("[Monster" + enemy.KeySingular + "]");
                    enemyDataElite.Add("[Monster" + enemy.KeySingular + "Elite]");

                    enemyData.Add("name={ffg:" + enemy.KeySingular + "}");
                    enemyDataElite.Add("name={ffg:" + enemy.KeySingular + "}");

                    enemyData.Add("; Plural: " + enemy.KeyPlural);
                    enemyDataElite.Add("; Plural: " + enemy.KeyPlural);

                    enemyData.Add("; Use Article: " + enemy.UseAnArticle);
                    enemyDataElite.Add("; Use Article: " + enemy.UseAnArticle);

                    enemyData.Add("image=\"{import}/img/" + enemy.Icon.Path + "\"");
                    enemyDataElite.Add("image=\"{import}/img/" + enemy.Icon.Path + "\"");

                    enemyData.Add("imageplace=\"{import}/img/" + enemy.ImagePlacement.Path + "\"");
                    enemyDataElite.Add("imageplace=\"{import}/img/" + enemy.ImagePlacement.Path + "\"");

                    enemyData.Add("; Portrait: " + enemy.Portrait.Path + "\"");
                    enemyDataElite.Add("; Portrait: " + enemy.Portrait.Path + "\"");

                    enemyData.Add("info={ffg:" + enemy.KeyInstructionsReg + "}");
                    enemyDataElite.Add("info={ffg:" + enemy.KeyInstructionsElite + "}");

                    enemyData.Add("; Piority Override: " + enemy.KeySecondaryPriorityOverride);
                    enemyDataElite.Add("; Piority Override: " + enemy.KeySecondaryPriorityOverride);

                    string surgeOrder = "; Surge Order: ";
                    foreach (string s in enemy.SurgePrioritiesReg)
                    {
                        surgeOrder += s;
                    }
                    enemyData.Add(surgeOrder);
                    string surgeOrderElite = "; Surge Order: ";
                    foreach (string s in enemy.SurgePrioritiesElite)
                    {
                        surgeOrderElite += s;
                    }
                    enemyDataElite.Add(surgeOrderElite);

                    enemyData.Add("; Max Unit Count: " + enemy.MaxUnitCount);
                    enemyDataElite.Add("; Max Unit Count: " + enemy.MaxUnitCount);

                    enemyData.Add("; Max Group Count: " + enemy.MaxInPlayGroupCount);
                    enemyDataElite.Add("; Max Group Count: " + enemy.MaxInPlayGroupCount);

                    enemyData.Add("; Threat: " + enemy.UnitThreatReg);
                    enemyDataElite.Add("; Threat: " + enemy.UnitThreatElite);

                    enemyData.Add("; Group Threat: " + enemy.TotalGroupThreatReg);
                    enemyDataElite.Add("; Group Threat: " + enemy.TotalGroupThreatElite);

                    enemyData.Add("; Elites use normal activations: " + enemy.UseRegularActivations);
                    enemyDataElite.Add("; Elites use normal activations: " + enemy.UseRegularActivations);

                    foreach (AudioClip audio in enemy.RevealSounds)
                    {
                        enemyData.Add("; Audio: ");
                        enemyDataElite.Add("; Audio: ");
                    }

                    string enemyTraits = "traits=" + enemy.Size.ToString();
                    foreach (EnemyTraitsSet1 trait in Enum.GetValues(typeof(EnemyTraitsSet1)))
                    {
                        if ((enemy.TraitsSet1 & trait) != 0)
                        {
                            enemyTraits += " " + trait.ToString().ToLower();
                        }
                    }
                    enemyData.Add(enemyTraits);
                    enemyDataElite.Add(enemyTraits);

/*		[Header("Activations Regular")]
		public IA_EnemyActionModel[] ActionsReg;

		public IA_EnemyBonusModel[] BonusReg;

		public IA_EnemyTargetModel[] TargetsReg;

		[BitMask(typeof(IA_HeroModel.HeroTraits))]
		public IA_HeroModel.HeroTraits[] TargetTraitsReg;

		public IA_EnemyActionModel[] ActionsElite;

		public IA_EnemyBonusModel[] BonusElite;

		public IA_EnemyTargetModel[] TargetsElite;

		[BitMask(typeof(IA_HeroModel.HeroTraits))]
		public IA_HeroModel.HeroTraits[] TargetTraitsElite;*/



                }
traits=ranged cave building small goblin
activation=Range1 Range2 Range3
            }

            if (product.Items.Length > 0)
            {
                packData.Add("items.ini");
                List<string> itemData = new List<string>();
                foreach (IA_InventoryItemModel item in product.Items)
                {
                    itemData.Add("[Item" + item.NameKey + "]");
                    itemData.Add("name={ffg:" + item.NameKey + "}");
                    itemData.Add("image=\"{import}/img/" + item.Icon.Path + "\"");
                    itemData.Add("price=" + item.CostBuy);
                    itemData.Add("; priceFame=" + item.CostFame);
                    itemData.Add("; priceSell=" + item.CostSell);
                    itemData.Add("; Number of mod slots: " + item.NumberOfModSlots);
                    itemData.Add("; Starting Item: " + item.IsStartingItem);
                    string itemTraits = "traits=deck" + item.Deck.ToString() + " " + item.Type.ToString();
                    if (item.ModType != ModTraits.None)
                    {
                        itemTraits += " mod" + item.ModType.ToString();
                    }
                    if (item.WeaponType != WeaponTraits.None)
                    {
                        itemTraits += " weapon" + item.WeaponType.ToString();
                    }
                    itemData.Add(itemTraits);
                    itemData.Add("");
                }
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
                File.WriteAllLines(outputDir + product.Code + "\\heroes.ini", heroData.ToArray());
                File.WriteAllLines(outputDir + product.Code + "\\skills.ini", skillData.ToArray());
            }

            foreach (IA_AllyModel ally in product.Allies)
            {

            }

            File.WriteAllLines(outputDir + product.Code + "\\content_pack.ini", packData.ToArray());
        }
    }
}
