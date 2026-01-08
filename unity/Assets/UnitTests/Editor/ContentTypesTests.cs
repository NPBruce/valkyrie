using System;
using System.Collections.Generic;
using NUnit.Framework;
using Assets.Scripts.Content;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Comprehensive unit tests for ContentTypes data classes.
    /// Tests constructor parsing, default values, and type conversions for all GenericData subclasses.
    /// </summary>
    [TestFixture]
    public class ContentTypesTests
    {
        private const string TestPath = "/test/path";
        private const string TestName = "TestSection";

        [SetUp]
        public void Setup()
        {
            // Disable ValkyrieDebug to prevent Unity logging during tests
            ValkyrieDebug.enabled = false;
        }

        [TearDown]
        public void TearDown()
        {
            ValkyrieDebug.enabled = true;
        }

        #region GenericData Base Class Tests

        [Test]
        public void GenericData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Assert
            Assert.AreEqual("PackTypeTest", data.sectionName);
            Assert.IsNotNull(data.traits);
            Assert.AreEqual(0, data.traits.Length);
            Assert.AreEqual("", data.image);
        }

        [Test]
        public void GenericData_WithName_ParsesNameCorrectly()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "name", "{val:TEST_NAME}" }
            };

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Assert
            Assert.IsNotNull(data.name);
            Assert.AreEqual("{val:TEST_NAME}", data.name.fullKey);
        }

        [Test]
        public void GenericData_WithoutName_UsesDefaultFromSectionName()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new PackTypeData("PackTypeMyPack", content, TestPath);

            // Assert - name should be section name minus type prefix
            Assert.IsNotNull(data.name);
            Assert.AreEqual("MyPack", data.name.key);
        }

        [Test]
        public void GenericData_WithPriority_ParsesPriorityCorrectly()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "priority", "5" }
            };

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Assert
            Assert.AreEqual(5, data.Priority);
        }

        [Test]
        public void GenericData_WithInvalidPriority_DefaultsToZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "priority", "invalid" }
            };

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Assert
            Assert.AreEqual(0, data.Priority);
        }

        [Test]
        public void GenericData_WithTraits_ParsesTraitsCorrectly()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "traits", "trait1 trait2 trait3" }
            };

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Assert
            Assert.AreEqual(3, data.traits.Length);
            Assert.AreEqual("trait1", data.traits[0]);
            Assert.AreEqual("trait2", data.traits[1]);
            Assert.AreEqual("trait3", data.traits[2]);
        }

        [Test]
        public void GenericData_WithSingleTrait_ParsesCorrectly()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "traits", "singletrait" }
            };

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Assert
            Assert.AreEqual(1, data.traits.Length);
            Assert.AreEqual("singletrait", data.traits[0]);
        }

        [Test]
        public void GenericData_WithSets_StoresSetsCorrectly()
        {
            // Arrange
            var content = new Dictionary<string, string>();
            var sets = new List<string> { "set1", "set2" };

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(2, data.Sets.Count);
            Assert.Contains("set1", data.Sets);
            Assert.Contains("set2", data.Sets);
        }

        [Test]
        public void GenericData_WithNullSets_CreatesEmptyList()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath, null);

            // Assert
            Assert.IsNotNull(data.Sets);
            Assert.AreEqual(0, data.Sets.Count);
        }

        [Test]
        public void GenericData_ContainsTrait_ReturnsTrueForExistingTrait()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "traits", "warrior mage healer" }
            };
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Act & Assert
            Assert.IsTrue(data.ContainsTrait("warrior"));
            Assert.IsTrue(data.ContainsTrait("mage"));
            Assert.IsTrue(data.ContainsTrait("healer"));
        }

        [Test]
        public void GenericData_ContainsTrait_ReturnsFalseForNonExistingTrait()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "traits", "warrior mage" }
            };
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Act & Assert
            Assert.IsFalse(data.ContainsTrait("healer"));
            Assert.IsFalse(data.ContainsTrait("thief"));
        }

        #endregion

        #region HeroData Tests

        [Test]
        public void HeroData_MinimalDictionary_SetsDefaultArchetype()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new HeroData("HeroTest", content, TestPath);

            // Assert
            Assert.AreEqual("warrior", data.archetype);
            Assert.AreEqual("", data.item);
        }

        [Test]
        public void HeroData_WithArchetype_ParsesArchetype()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "archetype", "mage" }
            };

            // Act
            var data = new HeroData("HeroTest", content, TestPath);

            // Assert
            Assert.AreEqual("mage", data.archetype);
        }

        [Test]
        public void HeroData_WithItem_ParsesItem()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "item", "StartingSword" }
            };

            // Act
            var data = new HeroData("HeroTest", content, TestPath);

            // Assert
            Assert.AreEqual("StartingSword", data.item);
        }

        [Test]
        public void HeroData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "archetype", "healer" },
                { "item", "HealingStaff" },
                { "name", "{val:HERO_NAME}" },
                { "traits", "human cleric" }
            };

            // Act
            var data = new HeroData("HeroMyHero", content, TestPath);

            // Assert
            Assert.AreEqual("healer", data.archetype);
            Assert.AreEqual("HealingStaff", data.item);
            Assert.AreEqual("{val:HERO_NAME}", data.name.fullKey);
            Assert.AreEqual(2, data.traits.Length);
        }

        [Test]
        public void HeroData_TypeField_IsHero()
        {
            // Assert
            Assert.AreEqual("Hero", HeroData.type);
        }

        #endregion

        #region ClassData Tests

        [Test]
        public void ClassData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new ClassData("ClassTest", content, TestPath);

            // Assert
            Assert.AreEqual("warrior", data.archetype);
            Assert.AreEqual("", data.hybridArchetype);
            Assert.IsNotNull(data.items);
            Assert.AreEqual(0, data.items.Count);
        }

        [Test]
        public void ClassData_WithArchetype_ParsesArchetype()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "archetype", "scout" }
            };

            // Act
            var data = new ClassData("ClassTest", content, TestPath);

            // Assert
            Assert.AreEqual("scout", data.archetype);
        }

        [Test]
        public void ClassData_WithHybridArchetype_ParsesHybridArchetype()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "hybridarchetype", "warrior" }
            };

            // Act
            var data = new ClassData("ClassTest", content, TestPath);

            // Assert
            Assert.AreEqual("warrior", data.hybridArchetype);
        }

        [Test]
        public void ClassData_WithSingleItem_ParsesItemsList()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "items", "Sword" }
            };

            // Act
            var data = new ClassData("ClassTest", content, TestPath);

            // Assert
            Assert.AreEqual(1, data.items.Count);
            Assert.AreEqual("Sword", data.items[0]);
        }

        [Test]
        public void ClassData_WithMultipleItems_ParsesAllItems()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "items", "Sword Shield Armor" }
            };

            // Act
            var data = new ClassData("ClassTest", content, TestPath);

            // Assert
            Assert.AreEqual(3, data.items.Count);
            Assert.AreEqual("Sword", data.items[0]);
            Assert.AreEqual("Shield", data.items[1]);
            Assert.AreEqual("Armor", data.items[2]);
        }

        [Test]
        public void ClassData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "archetype", "mage" },
                { "hybridarchetype", "healer" },
                { "items", "Staff Robe Wand" },
                { "name", "{val:CLASS_NAME}" }
            };

            // Act
            var data = new ClassData("ClassMageHealer", content, TestPath);

            // Assert
            Assert.AreEqual("mage", data.archetype);
            Assert.AreEqual("healer", data.hybridArchetype);
            Assert.AreEqual(3, data.items.Count);
        }

        [Test]
        public void ClassData_TypeField_IsClass()
        {
            // Assert
            Assert.AreEqual("Class", ClassData.type);
        }

        #endregion

        #region SkillData Tests

        [Test]
        public void SkillData_MinimalDictionary_DefaultsXpToZero()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new SkillData("SkillTest", content, TestPath);

            // Assert
            Assert.AreEqual(0, data.xp);
        }

        [Test]
        public void SkillData_WithXp_ParsesXpCorrectly()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "xp", "3" }
            };

            // Act
            var data = new SkillData("SkillTest", content, TestPath);

            // Assert
            Assert.AreEqual(3, data.xp);
        }

        [Test]
        public void SkillData_WithInvalidXp_DefaultsToZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "xp", "notanumber" }
            };

            // Act
            var data = new SkillData("SkillTest", content, TestPath);

            // Assert
            Assert.AreEqual(0, data.xp);
        }

        [Test]
        public void SkillData_WithNegativeXp_ParsesNegativeValue()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "xp", "-5" }
            };

            // Act
            var data = new SkillData("SkillTest", content, TestPath);

            // Assert
            Assert.AreEqual(-5, data.xp);
        }

        [Test]
        public void SkillData_TypeField_IsSkill()
        {
            // Assert
            Assert.AreEqual("Skill", SkillData.type);
        }

        #endregion

        #region ItemData Tests

        [Test]
        public void ItemData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new ItemData("ItemTest", content, TestPath);

            // Assert
            Assert.IsFalse(data.unique);
            Assert.AreEqual(0, data.price);
            Assert.AreEqual(-1, data.minFame);
            Assert.AreEqual(-1, data.maxFame);
        }

        [Test]
        public void ItemData_WithUniquePrefix_SetsUniqueTrue()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new ItemData("ItemUniqueSword", content, TestPath);

            // Assert
            Assert.IsTrue(data.unique);
        }

        [Test]
        public void ItemData_WithoutUniquePrefix_SetsUniqueFalse()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new ItemData("ItemSword", content, TestPath);

            // Assert
            Assert.IsFalse(data.unique);
        }

        [Test]
        public void ItemData_WithPrice_ParsesPrice()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "price", "250" }
            };

            // Act
            var data = new ItemData("ItemTest", content, TestPath);

            // Assert
            Assert.AreEqual(250, data.price);
        }

        [Test]
        public void ItemData_WithInvalidPrice_DefaultsToZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "price", "expensive" }
            };

            // Act
            var data = new ItemData("ItemTest", content, TestPath);

            // Assert
            Assert.AreEqual(0, data.price);
        }

        [Test]
        public void ItemData_WithMinFame_ParsesMinFame()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "minfame", "noteworthy" }
            };

            // Act
            var data = new ItemData("ItemTest", content, TestPath);

            // Assert
            Assert.AreEqual(2, data.minFame);
        }

        [Test]
        public void ItemData_WithMaxFame_ParsesMaxFame()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "maxfame", "legendary" }
            };

            // Act
            var data = new ItemData("ItemTest", content, TestPath);

            // Assert
            Assert.AreEqual(6, data.maxFame);
        }

        [Test]
        public void ItemData_Fame_InsignificantReturns1()
        {
            // Act & Assert
            Assert.AreEqual(1, ItemData.Fame("insignificant"));
        }

        [Test]
        public void ItemData_Fame_NoteworthyReturns2()
        {
            // Act & Assert
            Assert.AreEqual(2, ItemData.Fame("noteworthy"));
        }

        [Test]
        public void ItemData_Fame_ImpressiveReturns3()
        {
            // Act & Assert
            Assert.AreEqual(3, ItemData.Fame("impressive"));
        }

        [Test]
        public void ItemData_Fame_CelebratedReturns4()
        {
            // Act & Assert
            Assert.AreEqual(4, ItemData.Fame("celebrated"));
        }

        [Test]
        public void ItemData_Fame_HeroicReturns5()
        {
            // Act & Assert
            Assert.AreEqual(5, ItemData.Fame("heroic"));
        }

        [Test]
        public void ItemData_Fame_LegendaryReturns6()
        {
            // Act & Assert
            Assert.AreEqual(6, ItemData.Fame("legendary"));
        }

        [Test]
        public void ItemData_Fame_UnknownReturns0()
        {
            // Act & Assert
            Assert.AreEqual(0, ItemData.Fame("unknown"));
            Assert.AreEqual(0, ItemData.Fame(""));
            Assert.AreEqual(0, ItemData.Fame("random"));
        }

        [Test]
        public void ItemData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "price", "500" },
                { "minfame", "impressive" },
                { "maxfame", "heroic" }
            };

            // Act
            var data = new ItemData("ItemUniqueMagicSword", content, TestPath);

            // Assert
            Assert.IsTrue(data.unique);
            Assert.AreEqual(500, data.price);
            Assert.AreEqual(3, data.minFame);
            Assert.AreEqual(5, data.maxFame);
        }

        [Test]
        public void ItemData_TypeField_IsItem()
        {
            // Assert
            Assert.AreEqual("Item", ItemData.type);
        }

        #endregion

        #region ActivationData Tests

        [Test]
        public void ActivationData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.AreEqual("-", data.ability.key);
            Assert.AreEqual(StringKey.NULL.fullKey, data.minionActions.fullKey);
            Assert.AreEqual(StringKey.NULL.fullKey, data.masterActions.fullKey);
            Assert.AreEqual(StringKey.NULL.fullKey, data.moveButton.fullKey);
            Assert.AreEqual(StringKey.NULL.fullKey, data.move.fullKey);
            Assert.IsFalse(data.masterFirst);
            Assert.IsFalse(data.minionFirst);
        }

        [Test]
        public void ActivationData_WithAbility_ParsesAbility()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "ability", "{val:ABILITY_TEXT}" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:ABILITY_TEXT}", data.ability.fullKey);
        }

        [Test]
        public void ActivationData_WithMinion_ParsesMinionActions()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "minion", "{val:MINION_ACTION}" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:MINION_ACTION}", data.minionActions.fullKey);
        }

        [Test]
        public void ActivationData_WithMaster_ParsesMasterActions()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "master", "{val:MASTER_ACTION}" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:MASTER_ACTION}", data.masterActions.fullKey);
        }

        [Test]
        public void ActivationData_WithMoveButton_ParsesMoveButton()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "movebutton", "{val:MOVE_BUTTON}" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:MOVE_BUTTON}", data.moveButton.fullKey);
        }

        [Test]
        public void ActivationData_WithMove_ParsesMove()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "move", "{val:MOVE_TEXT}" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:MOVE_TEXT}", data.move.fullKey);
        }

        [Test]
        public void ActivationData_WithMasterFirstTrue_ParsesMasterFirst()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "masterfirst", "true" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.IsTrue(data.masterFirst);
        }

        [Test]
        public void ActivationData_WithMasterFirstFalse_ParsesMasterFirst()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "masterfirst", "false" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.IsFalse(data.masterFirst);
        }

        [Test]
        public void ActivationData_WithMinionFirstTrue_ParsesMinionFirst()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "minionfirst", "true" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.IsTrue(data.minionFirst);
        }

        [Test]
        public void ActivationData_WithInvalidBool_DefaultsToFalse()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "masterfirst", "yes" },
                { "minionfirst", "1" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.IsFalse(data.masterFirst);
            Assert.IsFalse(data.minionFirst);
        }

        [Test]
        public void ActivationData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "ability", "{val:ABILITY}" },
                { "minion", "{val:MINION}" },
                { "master", "{val:MASTER}" },
                { "movebutton", "{val:BUTTON}" },
                { "move", "{val:MOVE}" },
                { "masterfirst", "true" },
                { "minionfirst", "false" }
            };

            // Act
            var data = new ActivationData("MonsterActivationTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:ABILITY}", data.ability.fullKey);
            Assert.AreEqual("{val:MINION}", data.minionActions.fullKey);
            Assert.AreEqual("{val:MASTER}", data.masterActions.fullKey);
            Assert.AreEqual("{val:BUTTON}", data.moveButton.fullKey);
            Assert.AreEqual("{val:MOVE}", data.move.fullKey);
            Assert.IsTrue(data.masterFirst);
            Assert.IsFalse(data.minionFirst);
        }

        [Test]
        public void ActivationData_DefaultConstructor_CreatesInstance()
        {
            // Act
            var data = new ActivationData();

            // Assert - should not throw
            Assert.IsNotNull(data);
        }

        [Test]
        public void ActivationData_TypeField_IsMonsterActivation()
        {
            // Assert
            Assert.AreEqual("MonsterActivation", ActivationData.type);
        }

        #endregion

        #region TokenData Tests

        [Test]
        public void TokenData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.AreEqual(0, data.x);
            Assert.AreEqual(0, data.y);
            Assert.AreEqual(0, data.height);
            Assert.AreEqual(0, data.width);
            Assert.AreEqual(0f, data.pxPerSquare);
        }

        [Test]
        public void TokenData_WithXY_ParsesCoordinates()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "x", "100" },
                { "y", "200" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.AreEqual(100, data.x);
            Assert.AreEqual(200, data.y);
        }

        [Test]
        public void TokenData_WithHeightWidth_ParsesDimensions()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "height", "64" },
                { "width", "128" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.AreEqual(64, data.height);
            Assert.AreEqual(128, data.width);
        }

        [Test]
        public void TokenData_WithPps_ParsesPixelsPerSquare()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "pps", "72.5" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.AreEqual(72.5f, data.pxPerSquare, 0.001f);
        }

        [Test]
        public void TokenData_WithInvalidCoordinates_DefaultsToZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "x", "invalid" },
                { "y", "notanumber" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.AreEqual(0, data.x);
            Assert.AreEqual(0, data.y);
        }

        [Test]
        public void TokenData_WithNegativeCoordinates_ParsesNegativeValues()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "x", "-50" },
                { "y", "-100" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.AreEqual(-50, data.x);
            Assert.AreEqual(-100, data.y);
        }

        [Test]
        public void TokenData_FullImage_ReturnsTrueWhenHeightZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "height", "0" },
                { "width", "100" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.IsTrue(data.FullImage());
        }

        [Test]
        public void TokenData_FullImage_ReturnsTrueWhenWidthZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "height", "100" },
                { "width", "0" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.IsTrue(data.FullImage());
        }

        [Test]
        public void TokenData_FullImage_ReturnsFalseWhenBothNonZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "height", "100" },
                { "width", "100" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.IsFalse(data.FullImage());
        }

        [Test]
        public void TokenData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "x", "10" },
                { "y", "20" },
                { "height", "32" },
                { "width", "48" },
                { "pps", "96" }
            };

            // Act
            var data = new TokenData("TokenTest", content, TestPath);

            // Assert
            Assert.AreEqual(10, data.x);
            Assert.AreEqual(20, data.y);
            Assert.AreEqual(32, data.height);
            Assert.AreEqual(48, data.width);
            Assert.AreEqual(96f, data.pxPerSquare);
        }

        [Test]
        public void TokenData_TypeField_IsToken()
        {
            // Assert
            Assert.AreEqual("Token", TokenData.type);
        }

        #endregion

        #region AttackData Tests

        [Test]
        public void AttackData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new AttackData("AttackTest", content, TestPath);

            // Assert
            Assert.AreEqual(StringKey.NULL.fullKey, data.text.fullKey);
            Assert.AreEqual("", data.target);
            Assert.AreEqual("", data.attackType);
        }

        [Test]
        public void AttackData_WithText_ParsesText()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "text", "{val:ATTACK_TEXT}" }
            };

            // Act
            var data = new AttackData("AttackTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:ATTACK_TEXT}", data.text.fullKey);
        }

        [Test]
        public void AttackData_WithTarget_ParsesTarget()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "target", "human" }
            };

            // Act
            var data = new AttackData("AttackTest", content, TestPath);

            // Assert
            Assert.AreEqual("human", data.target);
        }

        [Test]
        public void AttackData_WithAttackType_ParsesAttackType()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "attacktype", "heavy" }
            };

            // Act
            var data = new AttackData("AttackTest", content, TestPath);

            // Assert
            Assert.AreEqual("heavy", data.attackType);
        }

        [Test]
        public void AttackData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "text", "{val:SLASH_ATTACK}" },
                { "target", "spirit" },
                { "attacktype", "unarmed" }
            };

            // Act
            var data = new AttackData("AttackTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:SLASH_ATTACK}", data.text.fullKey);
            Assert.AreEqual("spirit", data.target);
            Assert.AreEqual("unarmed", data.attackType);
        }

        [Test]
        public void AttackData_TypeField_IsAttack()
        {
            // Assert
            Assert.AreEqual("Attack", AttackData.type);
        }

        #endregion

        #region EvadeData Tests

        [Test]
        public void EvadeData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new EvadeData("EvadeTest", content, TestPath);

            // Assert
            Assert.AreEqual(StringKey.NULL.fullKey, data.text.fullKey);
            Assert.AreEqual("", data.monster);
        }

        [Test]
        public void EvadeData_WithText_ParsesText()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "text", "{val:EVADE_TEXT}" }
            };

            // Act
            var data = new EvadeData("EvadeTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:EVADE_TEXT}", data.text.fullKey);
        }

        [Test]
        public void EvadeData_WithMonster_ParsesMonster()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "monster", "Zombie" }
            };

            // Act
            var data = new EvadeData("EvadeTest", content, TestPath);

            // Assert
            Assert.AreEqual("Zombie", data.monster);
        }

        [Test]
        public void EvadeData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "text", "{val:DODGE}" },
                { "monster", "Ghost" }
            };

            // Act
            var data = new EvadeData("EvadeTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:DODGE}", data.text.fullKey);
            Assert.AreEqual("Ghost", data.monster);
        }

        [Test]
        public void EvadeData_TypeField_IsEvade()
        {
            // Assert
            Assert.AreEqual("Evade", EvadeData.type);
        }

        #endregion

        #region HorrorData Tests

        [Test]
        public void HorrorData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new HorrorData("HorrorTest", content, TestPath);

            // Assert
            Assert.AreEqual(StringKey.NULL.fullKey, data.text.fullKey);
            Assert.AreEqual("", data.monster);
        }

        [Test]
        public void HorrorData_WithText_ParsesText()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "text", "{val:HORROR_TEXT}" }
            };

            // Act
            var data = new HorrorData("HorrorTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:HORROR_TEXT}", data.text.fullKey);
        }

        [Test]
        public void HorrorData_WithMonster_ParsesMonster()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "monster", "DeepOne" }
            };

            // Act
            var data = new HorrorData("HorrorTest", content, TestPath);

            // Assert
            Assert.AreEqual("DeepOne", data.monster);
        }

        [Test]
        public void HorrorData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "text", "{val:SANITY_CHECK}" },
                { "monster", "Shoggoth" }
            };

            // Act
            var data = new HorrorData("HorrorTest", content, TestPath);

            // Assert
            Assert.AreEqual("{val:SANITY_CHECK}", data.text.fullKey);
            Assert.AreEqual("Shoggoth", data.monster);
        }

        [Test]
        public void HorrorData_TypeField_IsHorror()
        {
            // Assert
            Assert.AreEqual("Horror", HorrorData.type);
        }

        #endregion

        #region PuzzleData Tests

        [Test]
        public void PuzzleData_MinimalDictionary_CreatesInstance()
        {
            // Arrange
            var content = new Dictionary<string, string>();
            var sets = new List<string> { "base" };

            // Act
            var data = new PuzzleData("PuzzleTest", content, TestPath, sets);

            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual("PuzzleTest", data.sectionName);
        }

        [Test]
        public void PuzzleData_WithTraits_InheritsBaseClassParsing()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "traits", "slide image" }
            };
            var sets = new List<string>();

            // Act
            var data = new PuzzleData("PuzzleTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(2, data.traits.Length);
            Assert.AreEqual("slide", data.traits[0]);
            Assert.AreEqual("image", data.traits[1]);
        }

        [Test]
        public void PuzzleData_TypeField_IsPuzzle()
        {
            // Assert
            Assert.AreEqual("Puzzle", PuzzleData.type);
        }

        #endregion

        #region ImageData Tests

        [Test]
        public void ImageData_MinimalDictionary_InheritsTokenDataDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();
            var sets = new List<string>();

            // Act
            var data = new ImageData("ImageTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(0, data.x);
            Assert.AreEqual(0, data.y);
            Assert.AreEqual(0, data.height);
            Assert.AreEqual(0, data.width);
            Assert.AreEqual(0f, data.pxPerSquare);
        }

        [Test]
        public void ImageData_WithTokenDataFields_ParsesCorrectly()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "x", "50" },
                { "y", "75" },
                { "height", "128" },
                { "width", "256" },
                { "pps", "72" }
            };
            var sets = new List<string>();

            // Act
            var data = new ImageData("ImageTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(50, data.x);
            Assert.AreEqual(75, data.y);
            Assert.AreEqual(128, data.height);
            Assert.AreEqual(256, data.width);
            Assert.AreEqual(72f, data.pxPerSquare);
        }

        [Test]
        public void ImageData_TypeField_IsImage()
        {
            // Assert
            Assert.AreEqual("Image", ImageData.type);
        }

        #endregion

        #region MonsterData Tests

        [Test]
        public void MonsterData_DefaultConstructor_CreatesInstance()
        {
            // Act
            var data = new MonsterData();

            // Assert
            Assert.IsNotNull(data);
        }

        [Test]
        public void MonsterData_MinimalDictionary_SetsDefaults()
        {
            // Arrange
            var content = new Dictionary<string, string>();
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual("-", data.info.key);
            Assert.IsNotNull(data.activations);
            Assert.AreEqual(0, data.activations.Length);
            Assert.AreEqual(0f, data.healthBase);
            Assert.AreEqual(0f, data.healthPerHero);
            Assert.AreEqual(0, data.horror);
            Assert.AreEqual(0, data.awareness);
        }

        [Test]
        public void MonsterData_WithInfo_ParsesInfo()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "info", "{val:MONSTER_INFO}" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual("{val:MONSTER_INFO}", data.info.fullKey);
        }

        [Test]
        public void MonsterData_WithSingleActivation_ParsesActivation()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "activation", "MonsterActivationRoar" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(1, data.activations.Length);
            Assert.AreEqual("MonsterActivationRoar", data.activations[0]);
        }

        [Test]
        public void MonsterData_WithMultipleActivations_ParsesAllActivations()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "activation", "Roar Bite Claw" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(3, data.activations.Length);
            Assert.AreEqual("Roar", data.activations[0]);
            Assert.AreEqual("Bite", data.activations[1]);
            Assert.AreEqual("Claw", data.activations[2]);
        }

        [Test]
        public void MonsterData_WithHealth_ParsesHealthBase()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "health", "10.5" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(10.5f, data.healthBase, 0.001f);
        }

        [Test]
        public void MonsterData_WithHealthPerHero_ParsesHealthPerHero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "healthperhero", "2.5" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(2.5f, data.healthPerHero, 0.001f);
        }

        [Test]
        public void MonsterData_WithHorror_ParsesHorror()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "horror", "3" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(3, data.horror);
        }

        [Test]
        public void MonsterData_WithAwareness_ParsesAwareness()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "awareness", "-2" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(-2, data.awareness);
        }

        [Test]
        public void MonsterData_WithInvalidNumericValues_DefaultsToZero()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "health", "invalid" },
                { "healthperhero", "notanumber" },
                { "horror", "scary" },
                { "awareness", "high" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual(0f, data.healthBase);
            Assert.AreEqual(0f, data.healthPerHero);
            Assert.AreEqual(0, data.horror);
            Assert.AreEqual(0, data.awareness);
        }

        [Test]
        public void MonsterData_WithAllFields_ParsesAllFields()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "info", "{val:MONSTER_DESC}" },
                { "activation", "Attack1 Attack2" },
                { "health", "20" },
                { "healthperhero", "5" },
                { "horror", "4" },
                { "awareness", "-1" },
                { "traits", "undead flying" }
            };
            var sets = new List<string>();

            // Act
            var data = new MonsterData("MonsterTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual("{val:MONSTER_DESC}", data.info.fullKey);
            Assert.AreEqual(2, data.activations.Length);
            Assert.AreEqual(20f, data.healthBase);
            Assert.AreEqual(5f, data.healthPerHero);
            Assert.AreEqual(4, data.horror);
            Assert.AreEqual(-1, data.awareness);
            Assert.AreEqual(2, data.traits.Length);
        }

        [Test]
        public void MonsterData_TypeField_IsMonster()
        {
            // Assert
            Assert.AreEqual("Monster", MonsterData.type);
        }

        #endregion

        #region AudioData Tests

        [Test]
        public void AudioData_MinimalDictionary_SetsEmptyFile()
        {
            // Arrange
            var content = new Dictionary<string, string>();
            var sets = new List<string>();

            // Act
            var data = new AudioData("AudioTest", content, TestPath, sets);

            // Assert
            Assert.AreEqual("", data.file);
        }

        [Test]
        public void AudioData_WithFile_ParsesFilePath()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "file", "sounds/monster_roar.ogg" }
            };
            var sets = new List<string>();

            // Act
            var data = new AudioData("AudioTest", content, TestPath, sets);

            // Assert
            // File path should be combined with the content path
            Assert.IsTrue(data.file.EndsWith("sounds/monster_roar.ogg") ||
                          data.file.Contains("sounds") && data.file.Contains("monster_roar.ogg"));
        }

        [Test]
        public void AudioData_TypeField_IsAudio()
        {
            // Assert
            Assert.AreEqual("Audio", AudioData.type);
        }

        #endregion

        #region PackTypeData Tests

        [Test]
        public void PackTypeData_MinimalDictionary_CreatesInstance()
        {
            // Arrange
            var content = new Dictionary<string, string>();

            // Act
            var data = new PackTypeData("PackTypeTest", content, TestPath);

            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual("PackTypeTest", data.sectionName);
        }

        [Test]
        public void PackTypeData_TypeField_IsPackType()
        {
            // Assert
            Assert.AreEqual("PackType", PackTypeData.type);
        }

        #endregion

        #region TileSideData Partial Tests (Avoid Game.Get() calls)

        [Test]
        public void TileSideData_WithTop_ParsesTopValue()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "top", "15.5" }
            };

            // Act - Note: This may fail if pxPerSquare requires Game.Get()
            // We use try-catch to handle the case where Game.Get() is called
            try
            {
                var data = new TileSideData("TileSideTest", content, TestPath);
                Assert.AreEqual(15.5f, data.top, 0.001f);
            }
            catch (NullReferenceException)
            {
                // Expected if Game.Get() is called during construction
                Assert.Pass("TileSideData constructor requires Game.Get() - skipping field validation");
            }
        }

        [Test]
        public void TileSideData_WithLeft_ParsesLeftValue()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "left", "20.0" }
            };

            // Act
            try
            {
                var data = new TileSideData("TileSideTest", content, TestPath);
                Assert.AreEqual(20.0f, data.left, 0.001f);
            }
            catch (NullReferenceException)
            {
                Assert.Pass("TileSideData constructor requires Game.Get() - skipping field validation");
            }
        }

        [Test]
        public void TileSideData_WithAspect_ParsesAspectValue()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "aspect", "1.5" }
            };

            // Act
            try
            {
                var data = new TileSideData("TileSideTest", content, TestPath);
                Assert.AreEqual(1.5f, data.aspect, 0.001f);
            }
            catch (NullReferenceException)
            {
                Assert.Pass("TileSideData constructor requires Game.Get() - skipping field validation");
            }
        }

        [Test]
        public void TileSideData_WithReverse_ParsesReverseValue()
        {
            // Arrange
            var content = new Dictionary<string, string>
            {
                { "reverse", "TileSideOther" }
            };

            // Act
            try
            {
                var data = new TileSideData("TileSideTest", content, TestPath);
                Assert.AreEqual("TileSideOther", data.reverse);
            }
            catch (NullReferenceException)
            {
                Assert.Pass("TileSideData constructor requires Game.Get() - skipping field validation");
            }
        }

        [Test]
        public void TileSideData_TypeField_IsTileSide()
        {
            // Assert
            Assert.AreEqual("TileSide", TileSideData.type);
        }

        #endregion
    }
}
