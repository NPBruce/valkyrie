using NUnit.Framework;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Content;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for additional coverage on utility classes:
    /// ValkyrieConstants, CommonStringKeys, ContentPack, ManifestManager, and related classes.
    /// </summary>
    [TestFixture]
    public class AdditionalCoverageTests
    {
        [SetUp]
        public void Setup()
        {
            // Disable ValkyrieDebug to prevent Unity logging during tests
            ValkyrieDebug.enabled = false;

            // Initialize LocalizationRead dictionaries for StringKey tests
            LocalizationRead.dicts["val"] = null;
            LocalizationRead.dicts["qst"] = null;
            LocalizationRead.dicts["ffg"] = null;
        }

        [TearDown]
        public void TearDown()
        {
            ValkyrieDebug.enabled = true;
            LocalizationRead.dicts.Clear();
        }

        #region ValkyrieConstants Tests

        [Test]
        public void ValkyrieConstants_TypeMom_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("MoM", ValkyrieConstants.typeMom);
        }

        [Test]
        public void ValkyrieConstants_TypeDescent_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("D2E", ValkyrieConstants.typeDescent);
        }

        [Test]
        public void ValkyrieConstants_DefaultLanguage_IsEnglish()
        {
            // Assert
            Assert.AreEqual("English", ValkyrieConstants.DefaultLanguage);
        }

        [Test]
        public void ValkyrieConstants_CustomCategoryLabel_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("Custom", ValkyrieConstants.customCategoryLabel);
        }

        [Test]
        public void ValkyrieConstants_CustomCategoryName_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("Custom", ValkyrieConstants.customCategoryName);
        }

        [Test]
        public void ValkyrieConstants_ScenarioDownloadContainerExtension_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(".valkyrie", ValkyrieConstants.ScenarioDownloadContainerExtension);
        }

        [Test]
        public void ValkyrieConstants_ContentPackDownloadContainerExtensionAllFileReference_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("*.valkyrie", ValkyrieConstants.ContentPackDownloadContainerExtensionAllFileReference);
        }

        [Test]
        public void ValkyrieConstants_ContentPackDownloadContainerExtension_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual(".valkyrieContentPack", ValkyrieConstants.ContentPackDownloadContainerExtension);
        }

        [Test]
        public void ValkyrieConstants_ScenarioManifestPath_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("/manifest.ini", ValkyrieConstants.ScenarioManifestPath);
        }

        [Test]
        public void ValkyrieConstants_ContentPackManifestPath_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("/manifest.ini", ValkyrieConstants.ContentPackManifestPath);
        }

        [Test]
        public void ValkyrieConstants_QuestIniFilePath_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("/quest.ini", ValkyrieConstants.QuestIniFilePath);
        }

        [Test]
        public void ValkyrieConstants_RemoteContentPackIniType_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("RemoteContentPack", ValkyrieConstants.RemoteContentPackIniType);
        }

        [Test]
        public void ValkyrieConstants_ContentPackIniFile_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("content_pack.ini", ValkyrieConstants.ContentPackIniFile);
        }

        [Test]
        public void ValkyrieConstants_Instance_ReturnsSingletonInstance()
        {
            // Act
            var instance1 = ValkyrieConstants.Instance;
            var instance2 = ValkyrieConstants.Instance;

            // Assert
            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        #endregion

        #region CommonStringKeys Tests

        [Test]
        public void CommonStringKeys_BACK_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.BACK.dict);
            Assert.AreEqual("BACK", CommonStringKeys.BACK.key);
        }

        [Test]
        public void CommonStringKeys_CLOSE_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.CLOSE.dict);
            Assert.AreEqual("CLOSE", CommonStringKeys.CLOSE.key);
        }

        [Test]
        public void CommonStringKeys_EXIT_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.EXIT.dict);
            Assert.AreEqual("EXIT", CommonStringKeys.EXIT.key);
        }

        [Test]
        public void CommonStringKeys_OK_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.OK.dict);
            Assert.AreEqual("OK", CommonStringKeys.OK.key);
        }

        [Test]
        public void CommonStringKeys_CANCEL_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.CANCEL.dict);
            Assert.AreEqual("CANCEL", CommonStringKeys.CANCEL.key);
        }

        [Test]
        public void CommonStringKeys_CONTINUE_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.CONTINUE.dict);
            Assert.AreEqual("CONTINUE", CommonStringKeys.CONTINUE.key);
        }

        [Test]
        public void CommonStringKeys_PLUS_IsLiteralSymbol()
        {
            // Assert - PLUS has null dict and is a literal
            Assert.IsNull(CommonStringKeys.PLUS.dict);
            Assert.AreEqual("+", CommonStringKeys.PLUS.key);
        }

        [Test]
        public void CommonStringKeys_MINUS_IsLiteralSymbol()
        {
            // Assert - MINUS has null dict and is a literal
            Assert.IsNull(CommonStringKeys.MINUS.dict);
            Assert.AreEqual("-", CommonStringKeys.MINUS.key);
        }

        [Test]
        public void CommonStringKeys_HASH_IsLiteralSymbol()
        {
            // Assert - HASH has null dict and is a literal
            Assert.IsNull(CommonStringKeys.HASH.dict);
            Assert.AreEqual("#", CommonStringKeys.HASH.key);
        }

        [Test]
        public void CommonStringKeys_TRUE_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.TRUE.dict);
            Assert.AreEqual("TRUE", CommonStringKeys.TRUE.key);
        }

        [Test]
        public void CommonStringKeys_FALSE_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.FALSE.dict);
            Assert.AreEqual("FALSE", CommonStringKeys.FALSE.key);
        }

        [Test]
        public void CommonStringKeys_QUEST_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.QUEST.dict);
            Assert.AreEqual("QUEST", CommonStringKeys.QUEST.key);
        }

        [Test]
        public void CommonStringKeys_DELETE_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.DELETE.dict);
            Assert.AreEqual("DELETE", CommonStringKeys.DELETE.key);
        }

        [Test]
        public void CommonStringKeys_LOG_HasCorrectDict()
        {
            // Assert
            Assert.AreEqual("val", CommonStringKeys.LOG.dict);
            Assert.AreEqual("LOG", CommonStringKeys.LOG.key);
        }

        #endregion

        #region ContentPack Tests

        [Test]
        public void ContentPack_DefaultConstruction_AllFieldsAreNull()
        {
            // Arrange & Act
            var contentPack = new ContentPack();

            // Assert
            Assert.IsNull(contentPack.name);
            Assert.IsNull(contentPack.image);
            Assert.IsNull(contentPack.description);
            Assert.IsNull(contentPack.id);
            Assert.IsNull(contentPack.type);
            Assert.IsNull(contentPack.iniFiles);
            Assert.IsNull(contentPack.localizationFiles);
            Assert.IsNull(contentPack.clone);
        }

        [Test]
        public void ContentPack_SetName_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();

            // Act
            contentPack.name = "Test Pack";

            // Assert
            Assert.AreEqual("Test Pack", contentPack.name);
        }

        [Test]
        public void ContentPack_SetImage_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();

            // Act
            contentPack.image = "pack_image.png";

            // Assert
            Assert.AreEqual("pack_image.png", contentPack.image);
        }

        [Test]
        public void ContentPack_SetDescription_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();

            // Act
            contentPack.description = "This is a test content pack";

            // Assert
            Assert.AreEqual("This is a test content pack", contentPack.description);
        }

        [Test]
        public void ContentPack_SetId_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();

            // Act
            contentPack.id = "test_pack_001";

            // Assert
            Assert.AreEqual("test_pack_001", contentPack.id);
        }

        [Test]
        public void ContentPack_SetType_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();

            // Act
            contentPack.type = "MoM";

            // Assert
            Assert.AreEqual("MoM", contentPack.type);
        }

        [Test]
        public void ContentPack_SetIniFiles_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();
            var iniFiles = new List<string> { "file1.ini", "file2.ini", "file3.ini" };

            // Act
            contentPack.iniFiles = iniFiles;

            // Assert
            Assert.IsNotNull(contentPack.iniFiles);
            Assert.AreEqual(3, contentPack.iniFiles.Count);
            Assert.AreEqual("file1.ini", contentPack.iniFiles[0]);
        }

        [Test]
        public void ContentPack_SetLocalizationFiles_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();
            var locFiles = new Dictionary<string, List<string>>
            {
                { "English", new List<string> { "en_strings.txt" } },
                { "German", new List<string> { "de_strings.txt" } }
            };

            // Act
            contentPack.localizationFiles = locFiles;

            // Assert
            Assert.IsNotNull(contentPack.localizationFiles);
            Assert.AreEqual(2, contentPack.localizationFiles.Count);
            Assert.IsTrue(contentPack.localizationFiles.ContainsKey("English"));
            Assert.IsTrue(contentPack.localizationFiles.ContainsKey("German"));
        }

        [Test]
        public void ContentPack_SetClone_StoresCorrectly()
        {
            // Arrange
            var contentPack = new ContentPack();
            var cloneList = new List<string> { "base_pack_1", "base_pack_2" };

            // Act
            contentPack.clone = cloneList;

            // Assert
            Assert.IsNotNull(contentPack.clone);
            Assert.AreEqual(2, contentPack.clone.Count);
            Assert.AreEqual("base_pack_1", contentPack.clone[0]);
        }

        [Test]
        public void ContentPack_FullyPopulated_AllFieldsAccessible()
        {
            // Arrange & Act
            var contentPack = new ContentPack
            {
                name = "Full Pack",
                image = "full.png",
                description = "A fully populated pack",
                id = "full_001",
                type = "D2E",
                iniFiles = new List<string> { "data.ini" },
                localizationFiles = new Dictionary<string, List<string>>
                {
                    { "English", new List<string> { "en.txt" } }
                },
                clone = new List<string> { "base" }
            };

            // Assert
            Assert.AreEqual("Full Pack", contentPack.name);
            Assert.AreEqual("full.png", contentPack.image);
            Assert.AreEqual("A fully populated pack", contentPack.description);
            Assert.AreEqual("full_001", contentPack.id);
            Assert.AreEqual("D2E", contentPack.type);
            Assert.AreEqual(1, contentPack.iniFiles.Count);
            Assert.AreEqual(1, contentPack.localizationFiles.Count);
            Assert.AreEqual(1, contentPack.clone.Count);
        }

        #endregion

        #region ManifestManager Tests

        [Test]
        public void ManifestManager_Constructor_SetsPath()
        {
            // Arrange & Act
            var manager = new ManifestManager("/test/path");

            // Assert
            Assert.AreEqual("/test/path", manager.Path);
        }

        [Test]
        public void ManifestManager_GetLocalQuestManifestIniData_NullPath_ThrowsException()
        {
            // Arrange
            var manager = new ManifestManager(null);

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => manager.GetLocalQuestManifestIniData());
        }

        [Test]
        public void ManifestManager_GetLocalQuestManifestIniData_EmptyPath_ThrowsException()
        {
            // Arrange
            var manager = new ManifestManager("");

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => manager.GetLocalQuestManifestIniData());
        }

        [Test]
        public void ManifestManager_GetLocalQuestManifestIniData_WhitespacePath_ThrowsException()
        {
            // Arrange
            var manager = new ManifestManager("   ");

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => manager.GetLocalQuestManifestIniData());
        }

        [Test]
        public void ManifestManager_GetLocalQuestManifestIniData_NonExistentPath_ReturnsEmptyIniData()
        {
            // Arrange
            var manager = new ManifestManager("/nonexistent/path/that/does/not/exist");

            // Act
            var result = manager.GetLocalQuestManifestIniData();

            // Assert - Should return empty IniData when file doesn't exist
            Assert.IsNotNull(result);
        }

        [Test]
        public void ManifestManager_GetLocalContentPackManifestIniData_NullPath_ThrowsException()
        {
            // Arrange
            var manager = new ManifestManager(null);

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => manager.GetLocalContentPackManifestIniData());
        }

        [Test]
        public void ManifestManager_GetLocalContentPackManifestIniData_EmptyPath_ThrowsException()
        {
            // Arrange
            var manager = new ManifestManager("");

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => manager.GetLocalContentPackManifestIniData());
        }

        [Test]
        public void ManifestManager_GetLocalContentPackManifestIniData_NonExistentPath_ReturnsEmptyIniData()
        {
            // Arrange
            var manager = new ManifestManager("/nonexistent/content/path");

            // Act
            var result = manager.GetLocalContentPackManifestIniData();

            // Assert - Should return empty IniData when file doesn't exist
            Assert.IsNotNull(result);
        }

        #endregion

        #region Quest.InArray Static Method Tests

        [Test]
        public void Quest_InArray_ItemExists_ReturnsTrue()
        {
            // Arrange
            string[] array = { "apple", "banana", "cherry" };

            // Act
            bool result = Quest.InArray(array, "banana");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Quest_InArray_ItemNotExists_ReturnsFalse()
        {
            // Arrange
            string[] array = { "apple", "banana", "cherry" };

            // Act
            bool result = Quest.InArray(array, "grape");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Quest_InArray_EmptyArray_ReturnsFalse()
        {
            // Arrange
            string[] array = { };

            // Act
            bool result = Quest.InArray(array, "apple");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Quest_InArray_FirstItem_ReturnsTrue()
        {
            // Arrange
            string[] array = { "first", "second", "third" };

            // Act
            bool result = Quest.InArray(array, "first");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Quest_InArray_LastItem_ReturnsTrue()
        {
            // Arrange
            string[] array = { "first", "second", "third" };

            // Act
            bool result = Quest.InArray(array, "third");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Quest_InArray_CaseSensitive_ReturnsFalseForDifferentCase()
        {
            // Arrange
            string[] array = { "Apple", "Banana", "Cherry" };

            // Act
            bool result = Quest.InArray(array, "apple");

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}
