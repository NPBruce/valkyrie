using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Assets.Scripts.Content;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for utility classes: LinqUtil, FormatVersions, and TextAlignment
    /// </summary>
    [TestFixture]
    public class UtilityTests
    {
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

        #region LinqUtil.ToSet Tests

        [Test]
        public void ToSet_ListOfIntegers_ReturnsHashSet()
        {
            // Arrange
            List<int> list = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            HashSet<int> result = list.ToSet();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            Assert.IsTrue(result.Contains(1));
            Assert.IsTrue(result.Contains(5));
        }

        [Test]
        public void ToSet_ListWithDuplicates_RemovesDuplicates()
        {
            // Arrange
            List<int> list = new List<int> { 1, 2, 2, 3, 3, 3 };

            // Act
            HashSet<int> result = list.ToSet();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains(1));
            Assert.IsTrue(result.Contains(2));
            Assert.IsTrue(result.Contains(3));
        }

        [Test]
        public void ToSet_EmptyList_ReturnsEmptyHashSet()
        {
            // Arrange
            List<string> list = new List<string>();

            // Act
            HashSet<string> result = list.ToSet();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ToSet_ListOfStrings_ReturnsHashSet()
        {
            // Arrange
            List<string> list = new List<string> { "apple", "banana", "cherry" };

            // Act
            HashSet<string> result = list.ToSet();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains("apple"));
            Assert.IsTrue(result.Contains("banana"));
            Assert.IsTrue(result.Contains("cherry"));
        }

        [Test]
        public void ToSet_WithCustomComparer_UsesComparer()
        {
            // Arrange
            List<string> list = new List<string> { "Apple", "apple", "APPLE" };

            // Act - using case-insensitive comparer
            HashSet<string> result = list.ToSet(StringComparer.OrdinalIgnoreCase);

            // Assert - should have only 1 element due to case-insensitive comparison
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ToSet_WithNullComparer_UsesDefaultComparer()
        {
            // Arrange
            List<string> list = new List<string> { "Apple", "apple", "APPLE" };

            // Act - passing null comparer
            HashSet<string> result = list.ToSet(null);

            // Assert - should have 3 elements with default case-sensitive comparison
            Assert.AreEqual(3, result.Count);
        }

        #endregion

        #region TextAlignment Tests

        [Test]
        public void ParseAlignment_Top_ReturnsTop()
        {
            // Act
            TextAlignment result = TextAlignmentUtils.ParseAlignment("TOP");

            // Assert
            Assert.AreEqual(TextAlignment.TOP, result);
        }

        [Test]
        public void ParseAlignment_Center_ReturnsCenter()
        {
            // Act
            TextAlignment result = TextAlignmentUtils.ParseAlignment("CENTER");

            // Assert
            Assert.AreEqual(TextAlignment.CENTER, result);
        }

        [Test]
        public void ParseAlignment_Bottom_ReturnsBottom()
        {
            // Act
            TextAlignment result = TextAlignmentUtils.ParseAlignment("BOTTOM");

            // Assert
            Assert.AreEqual(TextAlignment.BOTTOM, result);
        }

        [Test]
        public void ParseAlignment_LowercaseTop_ReturnsTop()
        {
            // Act - case insensitive parsing
            TextAlignment result = TextAlignmentUtils.ParseAlignment("top");

            // Assert
            Assert.AreEqual(TextAlignment.TOP, result);
        }

        [Test]
        public void ParseAlignment_MixedCaseCenter_ReturnsCenter()
        {
            // Act
            TextAlignment result = TextAlignmentUtils.ParseAlignment("Center");

            // Assert
            Assert.AreEqual(TextAlignment.CENTER, result);
        }

        [Test]
        public void ParseAlignment_InvalidValue_ReturnsCenter()
        {
            // Act - invalid value should default to CENTER
            TextAlignment result = TextAlignmentUtils.ParseAlignment("INVALID");

            // Assert
            Assert.AreEqual(TextAlignment.CENTER, result);
        }

        [Test]
        public void ParseAlignment_EmptyString_ReturnsCenter()
        {
            // Act - empty string should default to CENTER
            TextAlignment result = TextAlignmentUtils.ParseAlignment("");

            // Assert
            Assert.AreEqual(TextAlignment.CENTER, result);
        }

        [Test]
        public void TextAlignmentEnum_HasExpectedValues()
        {
            // Assert - verify enum has all expected values
            Assert.IsTrue(Enum.IsDefined(typeof(TextAlignment), TextAlignment.TOP));
            Assert.IsTrue(Enum.IsDefined(typeof(TextAlignment), TextAlignment.CENTER));
            Assert.IsTrue(Enum.IsDefined(typeof(TextAlignment), TextAlignment.BOTTOM));
        }

        #endregion

        #region FormatVersions Tests



        [Test]
        public void QuestFormat_VersionOrdering_IsCorrect()
        {
            // Assert - verify versions are in ascending order
            Assert.IsTrue((int)QuestFormat.Versions.RICH_TEXT < (int)QuestFormat.Versions.SPLIT_BASE_MOM_AND_CONVERSION_KIT);
            Assert.IsTrue((int)QuestFormat.Versions.SPLIT_BASE_MOM_AND_CONVERSION_KIT < (int)QuestFormat.Versions.RELEASE_2_5_4);
            Assert.IsTrue((int)QuestFormat.Versions.RELEASE_2_5_4 < (int)QuestFormat.Versions.RELEASE_3_0_0);
            Assert.IsTrue((int)QuestFormat.Versions.RELEASE_3_0_0 < (int)QuestFormat.Versions.RELEASE_3_1_5);
        }

        [Test]
        public void QuestFormat_RichTextVersion_Is16()
        {
            // Assert
            Assert.AreEqual(16, (int)QuestFormat.Versions.RICH_TEXT);
        }

        [Test]
        public void QuestFormat_SplitBaseMomVersion_Is17()
        {
            // Assert
            Assert.AreEqual(17, (int)QuestFormat.Versions.SPLIT_BASE_MOM_AND_CONVERSION_KIT);
        }

        [Test]
        public void QuestFormat_Release254Version_Is18()
        {
            // Assert
            Assert.AreEqual(18, (int)QuestFormat.Versions.RELEASE_2_5_4);
        }

        [Test]
        public void QuestFormat_Release300Version_Is19()
        {
            // Assert
            Assert.AreEqual(19, (int)QuestFormat.Versions.RELEASE_3_0_0);
        }

        [Test]
        public void QuestFormat_Release315Version_Is20()
        {
            // Assert
            Assert.AreEqual(20, (int)QuestFormat.Versions.RELEASE_3_1_5);
        }

        [Test]
        public void QuestFormat_ScenariosRequiringConversionKit_ContainsExpectedScenarios()
        {
            // Assert - verify set contains expected scenarios (lowercase)
            Assert.IsTrue(QuestFormat.SCENARIOS_THAT_REQUIRE_CONVERSION_KIT.Contains("escape"));
            Assert.IsTrue(QuestFormat.SCENARIOS_THAT_REQUIRE_CONVERSION_KIT.Contains("holymansion"));
        }

        [Test]
        public void QuestFormat_ScenariosRequiringConversionKit_AllLowercase()
        {
            // Assert - all entries should be lowercase
            foreach (string scenario in QuestFormat.SCENARIOS_THAT_REQUIRE_CONVERSION_KIT)
            {
                Assert.AreEqual(scenario.ToLower(CultureInfo.InvariantCulture), scenario,
                    $"Scenario '{scenario}' is not lowercase");
            }
        }

        [Test]
        public void QuestFormat_ScenariosRequiringConversionKit_IsHashSet()
        {
            // Assert - verify it's a HashSet for O(1) lookup
            Assert.IsInstanceOf<HashSet<string>>(QuestFormat.SCENARIOS_THAT_REQUIRE_CONVERSION_KIT);
        }

        [Test]
        public void QuestFormat_ScenariosRequiringConversionKit_NoDuplicates()
        {
            // Assert - HashSet automatically removes duplicates, so count should match list
            var list = new List<string>
            {
                "Artefatos_Roubados",
                "BelieveorDie1",
                "BlackWoodsSecrets",
                "DemoniosEntreLosWilson",
                "EditorCenario8",
                "EditorScenario3",
                "Escape",
                "HolyMansion",
                "Horror_Haunts_Merinda",
                "InTheDark",
                "La_Follia_di_Arkham",
                "Main_Street_Market_Mayham",
                "OMalqueNuncaDorme",
                "Saviors",
                "StrainOnReality",
                "Stressandstrain",
                "TheLairofRlimShaikorth",
                "TheRitualScenario",
                "TheRobberyOfTheKadakianIdol",
                "wiltshire"
            }.Select(t => t.ToLower(CultureInfo.InvariantCulture)).ToList();

            Assert.AreEqual(list.Count, QuestFormat.SCENARIOS_THAT_REQUIRE_CONVERSION_KIT.Count);
        }

        #endregion
    }
}
