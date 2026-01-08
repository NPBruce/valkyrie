using System;
using System.Reflection;
using NUnit.Framework;
using Assets.Scripts.Content;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for GameType classes - Game-specific settings and configuration
    /// Tests cover NoGameType, D2EGameType, MoMGameType, and IAGameType implementations
    /// Note: Font-related methods are skipped as they require Unity Resources
    /// Note: MoMGameType is internal, so it is accessed via reflection
    /// </summary>
    [TestFixture]
    public class GameTypeTests
    {
        // Helper to create MoMGameType via reflection since it's internal
        private GameType CreateMoMGameType()
        {
            Type momType = Type.GetType("MoMGameType, Assembly-CSharp");
            if (momType == null)
            {
                // Try without assembly specification
                momType = Type.GetType("MoMGameType");
            }
            if (momType == null)
            {
                // Search in all loaded assemblies
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    momType = assembly.GetType("MoMGameType");
                    if (momType != null) break;
                }
            }
            Assert.IsNotNull(momType, "MoMGameType should be found via reflection");
            return (GameType)Activator.CreateInstance(momType);
        }

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

        #region NoGameType Tests

        [Test]
        public void NoGameType_DataDirectory_ReturnsContentPath()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            string result = gameType.DataDirectory();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ContentData.ContentPath(), result);
        }

        [Test]
        public void NoGameType_HeroName_ReturnsD2EHeroNameKey()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            StringKey result = gameType.HeroName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("D2E_HERO_NAME", result.key);
        }

        [Test]
        public void NoGameType_HeroesName_ReturnsD2EHeroesNameKey()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            StringKey result = gameType.HeroesName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("D2E_HEROES_NAME", result.key);
        }

        [Test]
        public void NoGameType_MaxHeroes_ReturnsZero()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            int result = gameType.MaxHeroes();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void NoGameType_DefaultHeroes_ReturnsZero()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            int result = gameType.DefaultHeroes();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void NoGameType_TilePixelPerSquare_ReturnsOne()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            float result = gameType.TilePixelPerSquare();

            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void NoGameType_TypeName_ReturnsEmptyString()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            string result = gameType.TypeName();

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void NoGameType_TileOnGrid_ReturnsTrue()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            bool result = gameType.TileOnGrid();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void NoGameType_DisplayMorale_ReturnsFalse()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            bool result = gameType.DisplayMorale();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void NoGameType_MonstersGrouped_ReturnsTrue()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            bool result = gameType.MonstersGrouped();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void NoGameType_SelectionRound_ReturnsOne()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            float result = gameType.SelectionRound();

            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void NoGameType_TileRound_ReturnsOne()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            float result = gameType.TileRound();

            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void NoGameType_DisplayHeroes_ReturnsTrue()
        {
            // Arrange
            NoGameType gameType = new NoGameType();

            // Act
            bool result = gameType.DisplayHeroes();

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region D2EGameType Tests

        [Test]
        public void D2EGameType_DataDirectory_ReturnsD2EPath()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            string result = gameType.DataDirectory();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.EndsWith("D2E/"));
            Assert.AreEqual(ContentData.ContentPath() + "D2E/", result);
        }

        [Test]
        public void D2EGameType_HeroName_ReturnsD2EHeroNameKey()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            StringKey result = gameType.HeroName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("D2E_HERO_NAME", result.key);
        }

        [Test]
        public void D2EGameType_HeroesName_ReturnsD2EHeroesNameKey()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            StringKey result = gameType.HeroesName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("D2E_HEROES_NAME", result.key);
        }

        [Test]
        public void D2EGameType_MaxHeroes_ReturnsFour()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            int result = gameType.MaxHeroes();

            // Assert
            Assert.AreEqual(4, result);
        }

        [Test]
        public void D2EGameType_DefaultHeroes_ReturnsFour()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            int result = gameType.DefaultHeroes();

            // Assert
            Assert.AreEqual(4, result);
        }

        [Test]
        public void D2EGameType_TilePixelPerSquare_Returns105()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            float result = gameType.TilePixelPerSquare();

            // Assert
            Assert.AreEqual(105f, result);
        }

        [Test]
        public void D2EGameType_TypeName_ReturnsD2E()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            string result = gameType.TypeName();

            // Assert
            Assert.AreEqual("D2E", result);
        }

        [Test]
        public void D2EGameType_TileOnGrid_ReturnsTrue()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            bool result = gameType.TileOnGrid();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void D2EGameType_DisplayMorale_ReturnsTrue()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            bool result = gameType.DisplayMorale();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void D2EGameType_MonstersGrouped_ReturnsTrue()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            bool result = gameType.MonstersGrouped();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void D2EGameType_SelectionRound_ReturnsOne()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            float result = gameType.SelectionRound();

            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void D2EGameType_TileRound_ReturnsOne()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            float result = gameType.TileRound();

            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void D2EGameType_DisplayHeroes_ReturnsTrue()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            bool result = gameType.DisplayHeroes();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void D2EGameType_QuestName_ReturnsD2EQuestNameKey()
        {
            // Arrange
            D2EGameType gameType = new D2EGameType();

            // Act
            StringKey result = gameType.QuestName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("D2E_QUEST_NAME", result.key);
        }

        #endregion

        #region MoMGameType Tests (via Reflection)

        [Test]
        public void MoMGameType_DataDirectory_ReturnsMoMPath()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            string result = gameType.DataDirectory();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.EndsWith("MoM/"));
            Assert.AreEqual(ContentData.ContentPath() + "MoM/", result);
        }

        [Test]
        public void MoMGameType_HeroName_ReturnsMoMHeroNameKey()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            StringKey result = gameType.HeroName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("MOM_HERO_NAME", result.key);
        }

        [Test]
        public void MoMGameType_HeroesName_ReturnsMoMHeroesNameKey()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            StringKey result = gameType.HeroesName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("MOM_HEROES_NAME", result.key);
        }

        [Test]
        public void MoMGameType_MaxHeroes_ReturnsTen()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            int result = gameType.MaxHeroes();

            // Assert
            Assert.AreEqual(10, result);
        }

        [Test]
        public void MoMGameType_DefaultHeroes_ReturnsFive()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            int result = gameType.DefaultHeroes();

            // Assert
            Assert.AreEqual(5, result);
        }

        [Test]
        public void MoMGameType_TilePixelPerSquare_ReturnsExpectedValue()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            float result = gameType.TilePixelPerSquare();

            // Assert
            // On non-Android platforms, should return 1024f / 3.5f
            float expectedNonAndroid = 1024f / 3.5f;
            // On Android, should return 512f / 3.5f
            float expectedAndroid = 512f / 3.5f;
            // Result should be one of these values
            Assert.IsTrue(result == expectedNonAndroid || result == expectedAndroid,
                $"Expected {expectedNonAndroid} or {expectedAndroid}, but got {result}");
        }

        [Test]
        public void MoMGameType_TypeName_ReturnsMoM()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            string result = gameType.TypeName();

            // Assert
            Assert.AreEqual("MoM", result);
        }

        [Test]
        public void MoMGameType_TileOnGrid_ReturnsFalse()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            bool result = gameType.TileOnGrid();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void MoMGameType_DisplayMorale_ReturnsFalse()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            bool result = gameType.DisplayMorale();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void MoMGameType_MonstersGrouped_ReturnsFalse()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            bool result = gameType.MonstersGrouped();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void MoMGameType_SelectionRound_Returns1Point75()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            float result = gameType.SelectionRound();

            // Assert
            Assert.AreEqual(1.75f, result);
        }

        [Test]
        public void MoMGameType_TileRound_Returns3Point5()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            float result = gameType.TileRound();

            // Assert
            Assert.AreEqual(3.5f, result);
        }

        [Test]
        public void MoMGameType_DisplayHeroes_ReturnsFalse()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            bool result = gameType.DisplayHeroes();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void MoMGameType_QuestName_ReturnsMoMQuestNameKey()
        {
            // Arrange
            GameType gameType = CreateMoMGameType();

            // Act
            StringKey result = gameType.QuestName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("MOM_QUEST_NAME", result.key);
        }

        #endregion

        #region IAGameType Tests

        [Test]
        public void IAGameType_DataDirectory_ReturnsIAPath()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            string result = gameType.DataDirectory();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.EndsWith("IA/"));
            Assert.AreEqual(ContentData.ContentPath() + "IA/", result);
        }

        [Test]
        public void IAGameType_HeroName_ReturnsIAHeroNameKey()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            StringKey result = gameType.HeroName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("IA_HERO_NAME", result.key);
        }

        [Test]
        public void IAGameType_HeroesName_ReturnsIAHeroesNameKey()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            StringKey result = gameType.HeroesName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("IA_HEROES_NAME", result.key);
        }

        [Test]
        public void IAGameType_MaxHeroes_ReturnsFour()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            int result = gameType.MaxHeroes();

            // Assert
            Assert.AreEqual(4, result);
        }

        [Test]
        public void IAGameType_DefaultHeroes_ReturnsFour()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            int result = gameType.DefaultHeroes();

            // Assert
            Assert.AreEqual(4, result);
        }

        [Test]
        public void IAGameType_TilePixelPerSquare_Returns105()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            float result = gameType.TilePixelPerSquare();

            // Assert
            Assert.AreEqual(105f, result);
        }

        [Test]
        public void IAGameType_TypeName_ReturnsIA()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            string result = gameType.TypeName();

            // Assert
            Assert.AreEqual("IA", result);
        }

        [Test]
        public void IAGameType_TileOnGrid_ReturnsTrue()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            bool result = gameType.TileOnGrid();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IAGameType_DisplayMorale_ReturnsTrue()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            bool result = gameType.DisplayMorale();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IAGameType_MonstersGrouped_ReturnsFalse()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            bool result = gameType.MonstersGrouped();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IAGameType_SelectionRound_ReturnsOne()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            float result = gameType.SelectionRound();

            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void IAGameType_TileRound_ReturnsOne()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            float result = gameType.TileRound();

            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void IAGameType_DisplayHeroes_ReturnsTrue()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            bool result = gameType.DisplayHeroes();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IAGameType_QuestName_ReturnsIAQuestNameKey()
        {
            // Arrange
            IAGameType gameType = new IAGameType();

            // Act
            StringKey result = gameType.QuestName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("val", result.dict);
            Assert.AreEqual("IA_QUEST_NAME", result.key);
        }

        #endregion

        #region Cross-GameType Comparison Tests

        [Test]
        public void GameTypes_D2EAndIA_HaveSameTilePixelPerSquare()
        {
            // Arrange
            D2EGameType d2e = new D2EGameType();
            IAGameType ia = new IAGameType();

            // Act & Assert
            Assert.AreEqual(d2e.TilePixelPerSquare(), ia.TilePixelPerSquare());
        }

        [Test]
        public void GameTypes_D2EAndIA_HaveSameMaxHeroes()
        {
            // Arrange
            D2EGameType d2e = new D2EGameType();
            IAGameType ia = new IAGameType();

            // Act & Assert
            Assert.AreEqual(d2e.MaxHeroes(), ia.MaxHeroes());
            Assert.AreEqual(4, d2e.MaxHeroes());
        }

        [Test]
        public void GameTypes_D2EAndIA_HaveSameDefaultHeroes()
        {
            // Arrange
            D2EGameType d2e = new D2EGameType();
            IAGameType ia = new IAGameType();

            // Act & Assert
            Assert.AreEqual(d2e.DefaultHeroes(), ia.DefaultHeroes());
        }

        [Test]
        public void GameTypes_MoM_HasHigherMaxHeroesThanOthers()
        {
            // Arrange
            GameType mom = CreateMoMGameType();
            D2EGameType d2e = new D2EGameType();
            IAGameType ia = new IAGameType();

            // Act & Assert
            Assert.IsTrue(mom.MaxHeroes() > d2e.MaxHeroes());
            Assert.IsTrue(mom.MaxHeroes() > ia.MaxHeroes());
        }

        [Test]
        public void GameTypes_OnlyMoM_HasTileOnGridFalse()
        {
            // Arrange
            NoGameType no = new NoGameType();
            D2EGameType d2e = new D2EGameType();
            GameType mom = CreateMoMGameType();
            IAGameType ia = new IAGameType();

            // Act & Assert
            Assert.IsTrue(no.TileOnGrid());
            Assert.IsTrue(d2e.TileOnGrid());
            Assert.IsFalse(mom.TileOnGrid());
            Assert.IsTrue(ia.TileOnGrid());
        }

        [Test]
        public void GameTypes_TypeNames_AreUnique()
        {
            // Arrange
            NoGameType no = new NoGameType();
            D2EGameType d2e = new D2EGameType();
            GameType mom = CreateMoMGameType();
            IAGameType ia = new IAGameType();

            // Act
            string[] typeNames = new string[]
            {
                no.TypeName(),
                d2e.TypeName(),
                mom.TypeName(),
                ia.TypeName()
            };

            // Assert - all should be unique (NoGameType returns empty string)
            Assert.AreEqual("", typeNames[0]);
            Assert.AreEqual("D2E", typeNames[1]);
            Assert.AreEqual("MoM", typeNames[2]);
            Assert.AreEqual("IA", typeNames[3]);
        }

        [Test]
        public void GameTypes_DataDirectories_ContainTypeName()
        {
            // Arrange
            D2EGameType d2e = new D2EGameType();
            GameType mom = CreateMoMGameType();
            IAGameType ia = new IAGameType();

            // Act & Assert
            Assert.IsTrue(d2e.DataDirectory().Contains(d2e.TypeName()));
            Assert.IsTrue(mom.DataDirectory().Contains(mom.TypeName()));
            Assert.IsTrue(ia.DataDirectory().Contains(ia.TypeName()));
        }

        #endregion
    }
}
