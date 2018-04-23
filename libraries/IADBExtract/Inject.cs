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
        public static int Extract(bool unused)
        {
            if (File.Exists(Extractor.outputDir + "log.txt")) return 0;
            Extractor extractor = new Extractor(SingletonBehaviour<PersistentGameObject>.Instance.Database);
            try
            {
                extractor.Extract();
            }
            catch (Exception e)
            {
                extractor.WriteLog(e.Message);
            }
            return 0;
        }
    }

    public class Extractor
    {
        public static string outputDir = "C:\\Users\\Bruce\\Desktop\\IA\\";
        public static Extractor e;

        List<string> logData;
        IA_Database db;
        public Dictionary<string, ContentPack> packs = new Dictionary<string, ContentPack>();

        public Extractor(IA_Database database)
        {
            e = this;
            db = database;
        }

        public void Log(string toLog)
        {
            logData.Add(toLog);
        }

        public void WriteLog(string final = null)
        {
            if (final != null)
            {
                logData.Add(final);
            }
            File.WriteAllLines(outputDir + "log.txt", logData);
        }

        public void Extract()
        {
            Log("Starting Extraction");
            Directory.CreateDirectory(outputDir);
            foreach(IA_ProductModel product in db.Products)
            {
                Log("Next Product");
                ExtractProduct(product);
            }
            foreach (IA_CampaignModel campaign in db.Campaigns)
            {
                Log("Next Campaign");
                ExtractCampaign(campaign);
            }
        }

        public void ExtractCampaign(IA_CampaignModel campaign)
        {
            Log("Campaign " + campaign.Title.Key);
            Directory.CreateDirectory(outputDir + "campaign");
            List<string> campaignData = new List<string>();
            campaignData.Add("Title: " + campaign.Title.Key);
            campaignData.Add("Description: " + campaign.Description.Key);
            campaignData.Add("Contents: " + campaign.Contents.Key);
            if (campaign.RequiredProduct != null)
            {
                campaignData.Add("RequiredProduct: " + campaign.RequiredProduct.NameKey);
            }
            campaignData.Add("Image: " + campaign.Image.Path);
            campaignData.Add("Fame Insignificant: " + campaign.CampaignFameThresholds.Insignificant + " Noteworthy: " + campaign.CampaignFameThresholds.Noteworthy + " Impressive: " + campaign.CampaignFameThresholds.Impressive + " Celebrated: " + campaign.CampaignFameThresholds.Celebrated + " Heroic: " + campaign.CampaignFameThresholds.Heroic + " Legendary: " + campaign.CampaignFameThresholds.Legendary);
            campaignData.Add("Normal Gold per Hero: " + campaign.DifficultyNormal.StartingGoldPerHero + " Start Peril: " + campaign.DifficultyNormal.QuestStartingPeril + " Reduce Peril: " + campaign.DifficultyNormal.PerilReduction);
            campaignData.Add("Hard Gold per Hero: " + campaign.DifficultyHard.StartingGoldPerHero + " Start Peril: " + campaign.DifficultyHard.QuestStartingPeril + " Reduce Peril: " + campaign.DifficultyHard.PerilReduction);

            foreach (IA_CampaignTaskModel task in campaign.Tasks)
            {
                // TODO: Tasks details
                campaignData.Add("Task: " + task.NameKey);
            }
            foreach (IA_PerilModel peril in campaign.Perils)
            {
                // TODO: Perils
                campaignData.Add("Peril: " + peril.Level.ToString());
            }

            foreach (IA_MissionModel mission in campaign.StoryQuests)
            {
                campaignData.Add("Story Mission: " + mission.NameKey);
                ExtractMission(mission);
            }
            foreach (IA_MissionModel mission in campaign.SideQuests)
            {
                campaignData.Add("Side Mission: " + mission.NameKey);
                ExtractMission(mission);
            }
            campaignData.Add("HasTutorial: " + campaign.HasTutorial);
            campaignData.Add("StartingMission: " + campaign.StartingMission.NameKey);
            if (campaign.NewPlayersStartingMission != null)
            {
                campaignData.Add("NewPlayersStartingMission: " + campaign.NewPlayersStartingMission.NameKey);
            }
            File.WriteAllLines(outputDir + "campaign\\" + campaign.Title.Key + ".txt", campaignData.ToArray());
        }

        public void ExtractMission(IA_MissionModel mission)
        {
            Directory.CreateDirectory(outputDir + "campaign");
            List<string> missionData = new List<string>();
            missionData.Add("Title: " + mission.NameKey);
            missionData.Add("Description: " + mission.DescriptionKey);
            missionData.Add("Image: " + mission.Image.Path);
            missionData.Add("IsFinale: " + mission.IsFinale);
            missionData.Add("CanFail: " + mission.CanFail);
            missionData.Add("IncapacitatedInsteadOfWithdrawn: " + mission.IncapacitatedInsteadOfWithdrawn);
            missionData.Add("IncapacitatedMessageKey: " + mission.IncapacitatedMessageKey);
            missionData.Add("ParFame: " + mission.ParFame);
            missionData.Add("ParRound: " + mission.ParRound);
            missionData.Add("ThreatLevel: " + mission.ThreatLevel);
            missionData.Add("XPValueAwardedOnFinish: " + mission.XPValueAwardedOnFinish);
            missionData.Add("CreditsAwardedOnFinish: " + mission.CreditsAwardedOnFinish);
            missionData.Add("CreditsAwardedPerHeroOnFinish: " + mission.CreditsAwardedPerHeroOnFinish);
            missionData.Add("CreditsAwardedPerHeroOnVictory: " + mission.CreditsAwardedPerHeroOnVictory);
            missionData.Add("BaseMinorPerilLevel: " + mission.BaseMinorPerilLevel);
            missionData.Add("BaseMajorPerilLevel: " + mission.BaseMajorPerilLevel);
            missionData.Add("BaseDeadlyPerilLevel: " + mission.BaseDeadlyPerilLevel);
            missionData.Add("BlockEnemyDefeatFameAward: " + mission.BlockEnemyDefeatFameAward);
            missionData.Add("BlockEndMissionRewards: " + mission.BlockEndMissionRewards);
            missionData.Add("CanStartWithAllies: " + mission.CanStartWithAllies);
            missionData.Add("RandomGroupThreatFloor: " + mission.RandomGroupThreatFloor);
            missionData.Add("RandomGroupThreatCeiling: " + mission.RandomGroupThreatCeiling);
            missionData.Add("Mission Type: " + mission.Type.ToString());
            missionData.Add("Location: " + mission.Location.NameKey);
            foreach (IA_InventoryItemModel item in mission.ItemsAwardedOnFinish)
            {
                missionData.Add("Finish Item: " + item.NameKey);
            }
            foreach (IA_EnemyGroupModel enemy in mission.BlacklistedEnemyGroups)
            {
                missionData.Add("BlacklistedEnemy: " + enemy.EnemyType.KeySingular);
            }
            foreach (IA_EnemyGroupModel enemy in mission.ReservedEnemyGroups)
            {
                missionData.Add("ReservedEnemy: " + enemy.EnemyType.KeySingular);
            }
            foreach (IA_AllyModel ally in mission.BlacklistedAllies)
            {
                missionData.Add("Blacklisted Ally: " + ally.NameKey);
            }
            foreach (IA_ProductModel product in mission.RequiredProducts)
            {
                missionData.Add("Required Product: " + product.NameKey);
            }

            File.WriteAllLines(outputDir + "campaign\\" + mission.NameKey + ".txt", missionData.ToArray());
        }

        public void ExtractProduct(IA_ProductModel product)
        {
            packs.Add(product.Code, new ContentPack(product));
        }
    }

    public class ContentPack
    {
        public IA_ProductModel product;
        public List<string> includes = new List<string>();

        public ContentPack(IA_ProductModel p)
        {
            product = p;
            Extractor.e.Log("Product Code: product.Code");
            Write();
        }

        public void Write()
        {
            if (product.EnemyTypes.Length > 0)
            {
                includes.Add("enemies.ini");
                includes.Add("activations.ini");
                WriteEnemies();
            }

            if (product.Items.Length > 0)
            {
                includes.Add("items.ini");
                WriteItems();
            }

            if (product.Heroes.Length > 0)
            {
                includes.Add("heroes.ini");
                includes.Add("skills.ini");
                WriteHeroes();
            }

            //IA_AllyModel product.Allies

            WritePack();
        }

        string TargetPath()
        {
            return Extractor.outputDir + product.Type.ToString() +"/" + product.Code + "/";
        }

        string PathStrip(string toConvert)
        {
            return Path.GetFileNameWithoutExtension(toConvert);
        }

        void WriteEnemies()
        {
            List<string> enemyData = new List<string>();
            List<string> activations = new List<string>();
            foreach (IA_EnemyTypeModel enemy in product.EnemyTypes)
            {
                Extractor.e.Log("Product Code: " + product.Code + " Enemy " + enemy.KeySingular);
                List<string> enemyDataElite = new List<string>();

                enemyData.Add("[Monster" + enemy.KeySingular + "]");
                enemyDataElite.Add("[MonsterElite" + enemy.KeySingular + "]");

                enemyData.Add("name={ffg:" + enemy.KeySingular + "}");
                enemyDataElite.Add("name={ffg:" + enemy.KeySingular + "}");

                enemyData.Add("; Plural: " + enemy.KeyPlural);
                enemyDataElite.Add("; Plural: " + enemy.KeyPlural);

                enemyData.Add("; Use Article: " + enemy.UseAnArticle);
                enemyDataElite.Add("; Use Article: " + enemy.UseAnArticle);

                enemyData.Add("image=\"{import}/img/" + PathStrip(enemy.Icon.Path) + "\"");
                enemyDataElite.Add("image=\"{import}/img/" + PathStrip(enemy.Icon.Path) + "\"");

                enemyData.Add("imageplace=\"{import}/img/" + PathStrip(enemy.ImagePlacement.Path) + "\"");
                enemyDataElite.Add("imageplace=\"{import}/img/" + PathStrip(enemy.ImagePlacement.Path) + "\"");

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

                foreach (UnityEngine.AudioClip audio in enemy.RevealSounds)
                {
                    enemyData.Add("; Audio: " + audio.ToString());
                    enemyDataElite.Add("; Audio: " + audio.ToString());
                }

                string enemyTraits = "traits=" + enemy.Size.ToString();
                foreach (EnemyTraitsSet1 trait in Enum.GetValues(typeof(EnemyTraitsSet1)))
                {
                    if ((enemy.TraitsSet1 & trait) != 0)
                    {
                        enemyTraits += " " + trait.ToString();
                    }
                }
                enemyData.Add(enemyTraits);
                enemyDataElite.Add(enemyTraits);

                enemyData.Add("");
                enemyData.AddRange(enemyDataElite);
                enemyData.Add("");

                for (int activation = 0; activation < enemy.ActionsReg.Length; activation++)
                {
                    if (enemy.BonusReg.Length <= activation || enemy.TargetTraitsReg.Length <= activation || enemy.TargetsReg.Length <= activation)
                    {
                        activations.Add("; activation error");
                        continue;
                    }
                    activations.Add("[MonsterActivation" + enemy.KeySingular + activation + "]");

                    List<string> activationDetails = new List<string>();
                    activationDetails.Add("ability={ffg:" + enemy.BonusReg[activation].Key + "}");
                    activationDetails.Add("; Additional range: " + enemy.BonusReg[activation].AdditionalRange);
                    activationDetails.Add("master={ffg:" + enemy.ActionsReg[activation].Key + "}");
                    activationDetails.Add("; Default range: " + enemy.ActionsReg[activation].DefaultRange);
                    activationDetails.Add("; Secondary Priority: " + enemy.ActionsReg[activation].SecondaryPriority);
                    activationDetails.Add("; Target: " + enemy.TargetsReg[activation].Key);
                    foreach (IA_HeroModel.HeroTraits trait in Enum.GetValues(typeof(IA_HeroModel.HeroTraits)))
                    {
                        if ((enemy.TargetTraitsReg[activation] & trait) != 0)
                        {
                            activationDetails.Add("; Target Trait: " + trait.ToString());
                        }
                    }
                    activationDetails.Add("");

                    activations.AddRange(activationDetails);
                    if (enemy.UseRegularActivations)
                    {
                        activations.Add("[MonsterActivationElite" + enemy.KeySingular + activation + "]");
                        activations.AddRange(activationDetails);
                    }
                }

                if (!enemy.UseRegularActivations)
                {
                    for (int activation = 0; activation < enemy.ActionsElite.Length; activation++)
                    {
                        if (enemy.BonusElite.Length <= activation || enemy.TargetsElite.Length <= activation || enemy.TargetTraitsElite.Length <= activation)
                        {
                            activations.Add("; activation error");
                            continue;
                        }

                        activations.Add("[MonsterActivationElite" + enemy.KeySingular + activation + "]");
                        activations.Add("ability={ffg:" + enemy.BonusElite[activation].Key + "}");
                        activations.Add("; Additional range: " + enemy.BonusElite[activation].AdditionalRange);
                        activations.Add("master={ffg:" + enemy.ActionsElite[activation].Key + "}");
                        activations.Add("; Default range: " + enemy.ActionsElite[activation].DefaultRange);
                        activations.Add("; Secondary Priority: " + enemy.ActionsElite[activation].SecondaryPriority);
                        activations.Add("; Target: " + enemy.TargetsElite[activation].Key);
                        foreach (IA_HeroModel.HeroTraits trait in Enum.GetValues(typeof(IA_HeroModel.HeroTraits)))
                        {
                            if ((enemy.TargetTraitsElite[activation] & trait) != 0)
                            {
                                activations.Add("; Target Trait: " + trait.ToString());
                            }
                        }
                        activations.Add("");
                    }
                }
            }
            File.WriteAllLines(TargetPath() + "enemies.ini", enemyData.ToArray());
            File.WriteAllLines(TargetPath() + "activations.ini", activations.ToArray());
        }

        public void WriteItems()
        {
            List<string> itemData = new List<string>();
            foreach (IA_InventoryItemModel item in product.Items)
            {
                itemData.Add("[Item" + item.NameKey + "]");
                itemData.Add("name={ffg:" + item.NameKey + "}");
                itemData.Add("image=\"{import}/img/" + PathStrip(item.Icon.Path) + "\"");
                itemData.Add("price=" + item.CostBuy);
                itemData.Add("; priceFame=" + item.CostFame);
                itemData.Add("; priceSell=" + item.CostSell);
                itemData.Add("; Number of mod slots: " + item.NumberOfModSlots);
                itemData.Add("; Starting Item: " + item.IsStartingItem);
                string itemTraits = "traits=deck" + item.Deck.ToString() + " " + item.Type.ToString();
                /*if (item.ModType != ModTraits.None)
                {
                    itemTraits += " mod" + item.ModType.ToString();
                }*/
                if (item.WeaponType != WeaponTraits.None)
                {
                    itemTraits += " weapon" + item.WeaponType.ToString();
                }
                itemData.Add(itemTraits);
                itemData.Add("");
            }
            File.WriteAllLines(TargetPath() + "items.ini", itemData.ToArray());
        }

        public void WriteHeroes()
        {
            List<string> heroData = new List<string>();
            List<string> skillData = new List<string>();
            foreach (IA_HeroModel hero in product.Heroes)
            {
                heroData.Add("[Hero" + hero.NameKey + "]");
                heroData.Add("name={ffg:" + hero.NameKey + "}");
                heroData.Add("image=\"{import}/img/" + PathStrip(hero.Icon.Path) + "\"");
                heroData.Add("; Portrait " + hero.Portrait.Path);
                heroData.Add("class=" + "Class" + hero.NameKey);
                heroData.Add("; Weapon Type " + hero.WeaponType.ToString());
                string heroTraits = "traits=" + hero.Gender.ToString();
                foreach (IA_HeroModel.HeroTraits trait in Enum.GetValues(typeof(IA_HeroModel.HeroTraits)))
                {
                    if ((hero.Traits & trait) != 0)
                    {
                        heroTraits += " " + trait.ToString();
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
                            skillExceptions += " " + except.ToString();
                        }
                    }
                    skillData.Add(skillExceptions);
                    skillData.Add("");
                }
            }
            File.WriteAllLines(TargetPath() + "heroes.ini", heroData.ToArray());
            File.WriteAllLines(TargetPath() + "skills.ini", skillData.ToArray());
        }

        public void WritePack()
        {
            List<string> packData = new List<string>();

            packData.Add("[ContentPack]");
            packData.Add("name={ffg:" + product.NameKey + "}");
            packData.Add("description={ffg:" + product.DescriptionKey + "}");
            packData.Add("; Contents " + product.ContentsKey);
            packData.Add("image=\"{import}/img/\"" + PathStrip(product.Image.Path) + "\"");
            if (product.Type != IA_ProductModel.ExpansionType.CoreSet)
            {
                foreach (IA_ProductModel.ExpansionType packType in Enum.GetValues(typeof(IA_ProductModel.ExpansionType)))
                {
                    if (packType == IA_ProductModel.ExpansionType.CoreSet) continue;
                    packData.Add("");
                    packData.Add("[Pack" + packType.ToString() + "]");
                    packData.Add("name={pck:" + packType.ToString().ToUpper() + "}");
                }
                packData.Add("");
                packData.Add("[LanguageData]");
                packData.Add("pck Localization.English.txt");
            }
            else
            {
                packData.Add("id=" + product.Code);
                packData.Add("type=" + product.Type.ToString());
            }
            packData.Add("");

            packData.Add("[ContentPackData]");
            packData.AddRange(includes);

            File.WriteAllLines(TargetPath() + "content_pack.ini", packData.ToArray());
        }
    }
}
