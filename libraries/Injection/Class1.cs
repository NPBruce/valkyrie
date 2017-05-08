using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FFG.Common;
using FFG.RtL;

namespace Injection
{
    public class Class1
    {
        public static int alwaysFive(bool ownedOnly)
        {
            foreach (ProductModel product in UserCollectionManager.GetProductCollection(false))
            {
                List<string> outText = new List<string>();
                outText.Add("Type: " + product.Type);
                outText.Add("Name: " + product.Name);
                outText.Add("Description: " + product.Description);
                outText.Add("Contents: " + product.Contents);
                outText.Add("Image: " + product.Image.Path);
                outText.Add("Size: " + product.PowerMeterValue);
                outText.Add("");

                foreach (HeroModel hm in product.GetHeroes())
                {
                    outText.Add("Heroes:");
                    outText.Add("Name:" + hm.NameKey);
                    outText.Add("");
                }

                foreach (ClassModel hm in product.GetClasses())
                {
                    outText.Add("Classes:");
                    outText.Add("");
                }

                foreach (InventoryItemModel im in product.GetItems())
                {
                    outText.Add("Items:");
                    outText.Add("");
                }

                foreach (MonsterGroupModel mm in product.GetMonsters())
                {
                    outText.Add("Monsters:");
                    outText.Add("");
                }

                foreach (ProductMonsterActivationModel am in product.AdditionalActivations)
                {
                    outText.Add("Activations:");
                    outText.Add("");
                }

                File.WriteAllLines("C:\\Users\\Bruce\\Desktop\\ffg\\" + product.Code + ".txt", outText.ToArray());
            }

            return 5;
        }
    }
}
