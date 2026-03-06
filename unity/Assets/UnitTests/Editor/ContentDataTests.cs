using NUnit.Framework;
using Assets.Scripts.Content;
using System.Collections.Generic;

namespace Valkyrie.UnitTests
{
    [TestFixture]
    public class ContentDataTests
    {
        [SetUp]
        public void Setup()
        {
            // Clear existing dictionaries to avoid side effects
            LocalizationRead.dicts.Clear();

            // Set up a dummy pck dictionary
            DictionaryI18n pckDict = new DictionaryI18n();
            pckDict.AddEntry("BOXED", "Boxed Content");
            LocalizationRead.AddDictionary("pck", pckDict);
        }

        [Test]
        public void GetContentName_CustomContentPack_NoTranslation_ReturnsTranslatedOrPlain()
        {
            // Arrange
            ContentData cd = (ContentData)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ContentData)); 
            cd.allPacks = new List<ContentPack>();

            ContentPack cp = new ContentPack();
            cp.id = "SOTP";
            cp.name = "Sands ofthe Past Content Pack";
            cd.allPacks.Add(cp);

            // Act
            string name = cd.GetContentName("SOTP");

            // Assert
            Assert.AreEqual("Sands ofthe Past Content Pack", name);
        }
        
        [Test]
        public void GetContentName_OfficialPack_WithTranslation_ReturnsTranslated()
        {
            // Arrange
            ContentData cd = (ContentData)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ContentData)); 
            cd.allPacks = new List<ContentPack>();

            ContentPack cp = new ContentPack();
            cp.id = "BOX";
            cp.name = "{pck:BOXED}";
            cd.allPacks.Add(cp);

            // Act
            string name = cd.GetContentName("BOX");

            // Assert
            Assert.AreEqual("Boxed Content", name);
        }
    }
}
