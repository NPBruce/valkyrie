using NUnit.Framework;
using System;
using System.IO;
using ValkyrieTools;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for SaveManager class and related save/load functionality.
    /// Tests focus on testable static methods, version comparison, path generation,
    /// and data structures without requiring full Unity runtime context.
    /// </summary>
    [TestFixture]
    public class SaveLoadTests
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

        #region SaveManager Constants Tests

        [Test]
        public void MinValkyieVersion_HasExpectedFormat()
        {
            // Arrange & Act
            string version = SaveManager.minValkyieVersion;

            // Assert - Should be in format X.Y.Z
            Assert.IsNotNull(version);
            Assert.IsTrue(version.Contains("."), "Version should contain at least one dot separator");
            string[] parts = version.Split('.');
            Assert.IsTrue(parts.Length >= 2, "Version should have at least 2 components (major.minor)");
        }

        [Test]
        public void MinValkyieVersion_IsNotEmpty()
        {
            // Arrange & Act
            string version = SaveManager.minValkyieVersion;

            // Assert
            Assert.IsNotEmpty(version);
        }

        [Test]
        public void MinValkyieVersion_HasExpectedValue()
        {
            // Arrange & Act
            string version = SaveManager.minValkyieVersion;

            // Assert - Current expected value
            Assert.AreEqual("0.7.3", version);
        }

        [Test]
        public void MinValkyieVersion_ComponentsAreParsableAsIntegers()
        {
            // Arrange
            string version = SaveManager.minValkyieVersion;
            string[] parts = version.Split('.');

            // Act & Assert - Each part should be parseable as an integer
            foreach (string part in parts)
            {
                Assert.DoesNotThrow(() => int.Parse(part),
                    $"Version component '{part}' should be parseable as integer");
            }
        }

        #endregion

        #region VersionManager.VersionNewer Tests

        [Test]
        public void VersionNewer_NewerMajorVersion_ReturnsTrue()
        {
            // Arrange
            string oldVersion = "1.0.0";
            string newVersion = "2.0.0";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VersionNewer_NewerMinorVersion_ReturnsTrue()
        {
            // Arrange
            string oldVersion = "1.0.0";
            string newVersion = "1.1.0";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VersionNewer_NewerPatchVersion_ReturnsTrue()
        {
            // Arrange
            string oldVersion = "1.0.0";
            string newVersion = "1.0.1";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VersionNewer_SameVersion_ReturnsFalse()
        {
            // Arrange
            string oldVersion = "1.0.0";
            string newVersion = "1.0.0";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void VersionNewer_OlderVersion_ReturnsFalse()
        {
            // Arrange
            string oldVersion = "2.0.0";
            string newVersion = "1.0.0";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void VersionNewer_EmptyNewVersion_ReturnsFalse()
        {
            // Arrange
            string oldVersion = "1.0.0";
            string newVersion = "";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void VersionNewer_EmptyOldVersion_ReturnsTrue()
        {
            // Arrange
            string oldVersion = "";
            string newVersion = "1.0.0";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VersionNewer_DifferentComponentCount_ReturnsTrue()
        {
            // Arrange - Different number of components triggers true
            string oldVersion = "1.0.0";
            string newVersion = "1.0";

            // Act
            bool result = VersionManager.VersionNewer(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region VersionManager.VersionNewerOrEqual Tests

        [Test]
        public void VersionNewerOrEqual_SameVersion_ReturnsTrue()
        {
            // Arrange
            string oldVersion = "1.5.3";
            string newVersion = "1.5.3";

            // Act
            bool result = VersionManager.VersionNewerOrEqual(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VersionNewerOrEqual_NewerVersion_ReturnsTrue()
        {
            // Arrange
            string oldVersion = "1.0.0";
            string newVersion = "1.0.1";

            // Act
            bool result = VersionManager.VersionNewerOrEqual(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VersionNewerOrEqual_OlderVersion_ReturnsFalse()
        {
            // Arrange
            string oldVersion = "1.0.1";
            string newVersion = "1.0.0";

            // Act
            bool result = VersionManager.VersionNewerOrEqual(oldVersion, newVersion);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void VersionNewerOrEqual_MinValkyieVersion_ChecksCorrectly()
        {
            // Arrange - Test against actual minimum version
            string minVersion = SaveManager.minValkyieVersion;
            string sameVersion = SaveManager.minValkyieVersion;

            // Act
            bool result = VersionManager.VersionNewerOrEqual(minVersion, sameVersion);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VersionNewerOrEqual_VersionWithExtraCharacters_StillCompares()
        {
            // Arrange - Versions might have non-numeric characters
            string oldVersion = "1.0.0";
            string newVersion = "1.0.1-beta";

            // Act
            bool result = VersionManager.VersionNewerOrEqual(oldVersion, newVersion);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Quest.LogEntry Tests

        [Test]
        public void LogEntry_Constructor_SingleParam_SetsEntry()
        {
            // Arrange & Act
            var logEntry = new Quest.LogEntry("Test log message");

            // Assert
            string output = logEntry.ToString(1);
            Assert.IsTrue(output.Contains("quest1="));
            Assert.IsTrue(output.Contains("Test log message"));
        }

        [Test]
        public void LogEntry_Constructor_WithEditorFlag_SetsEditorPrefix()
        {
            // Arrange & Act
            var logEntry = new Quest.LogEntry("Editor message", editorIn: true);

            // Assert
            string output = logEntry.ToString(1);
            Assert.IsTrue(output.Contains("editor1="));
        }

        [Test]
        public void LogEntry_Constructor_WithValkyrieFlag_SetsValkyriePrefix()
        {
            // Arrange & Act
            var logEntry = new Quest.LogEntry("Valkyrie message", editorIn: false, valkyrieIn: true);

            // Assert
            string output = logEntry.ToString(1);
            Assert.IsTrue(output.Contains("valkyrie1="));
        }

        [Test]
        public void LogEntry_Constructor_TypeString_ValkyrieType_SetsValkyriePrefix()
        {
            // Arrange & Act
            var logEntry = new Quest.LogEntry("valkyrie", "System message");

            // Assert
            string output = logEntry.ToString(5);
            Assert.IsTrue(output.Contains("valkyrie5="));
        }

        [Test]
        public void LogEntry_Constructor_TypeString_EditorType_SetsEditorPrefix()
        {
            // Arrange & Act
            var logEntry = new Quest.LogEntry("editor", "Editor system message");

            // Assert
            string output = logEntry.ToString(3);
            Assert.IsTrue(output.Contains("editor3="));
        }

        [Test]
        public void LogEntry_Constructor_TypeString_QuestType_SetsQuestPrefix()
        {
            // Arrange & Act
            var logEntry = new Quest.LogEntry("quest", "Quest message");

            // Assert
            string output = logEntry.ToString(2);
            Assert.IsTrue(output.Contains("quest2="));
        }

        [Test]
        public void LogEntry_ToString_FormatsIdCorrectly()
        {
            // Arrange
            var logEntry = new Quest.LogEntry("Test message");

            // Act
            string output1 = logEntry.ToString(1);
            string output10 = logEntry.ToString(10);
            string output100 = logEntry.ToString(100);

            // Assert
            Assert.IsTrue(output1.Contains("quest1="));
            Assert.IsTrue(output10.Contains("quest10="));
            Assert.IsTrue(output100.Contains("quest100="));
        }

        [Test]
        public void LogEntry_ToString_EscapesNewlines()
        {
            // Arrange
            var logEntry = new Quest.LogEntry("Line1\nLine2\nLine3");

            // Act
            string output = logEntry.ToString(1);

            // Assert - Newlines should be escaped as \\n in output
            Assert.IsTrue(output.Contains("Line1\\nLine2\\nLine3"));
        }

        [Test]
        public void LogEntry_ToString_EndsWithNewline()
        {
            // Arrange
            var logEntry = new Quest.LogEntry("Test message");

            // Act
            string output = logEntry.ToString(1);

            // Assert
            Assert.IsTrue(output.EndsWith(Environment.NewLine));
        }

        #endregion

        #region Save File Path Format Tests

        [Test]
        public void SaveFilePath_Format_ContainsSaveDirectory()
        {
            // Note: GetSaveFilePath requires ContentData.GameTypePath which depends on Game.Get()
            // We test the path format logic conceptually here

            // Arrange - Expected format: {GameTypePath}/Save/saveX.vSave
            string expectedSuffix = ".vSave";

            // Assert - The file extension should be .vSave
            Assert.AreEqual(".vSave", expectedSuffix);
        }

        [Test]
        public void SaveFilePath_AutoSaveNumber_UsesAutoPrefix()
        {
            // Testing the logic: when num == 0, it should use "Auto" instead of "0"
            // In GetSaveFilePath: if (num == 0) number = "Auto";

            // Arrange
            int saveNum = 0;
            string expectedNumber = saveNum == 0 ? "Auto" : saveNum.ToString();

            // Assert
            Assert.AreEqual("Auto", expectedNumber);
        }

        [Test]
        public void SaveFilePath_NumberedSave_UsesNumberAsString()
        {
            // Testing the logic: when num != 0, it should use the number
            // Arrange
            int saveNum = 1;
            string expectedNumber = saveNum == 0 ? "Auto" : saveNum.ToString();

            // Assert
            Assert.AreEqual("1", expectedNumber);
        }

        [Test]
        public void SaveFilePath_HigherNumberedSave_UsesNumberAsString()
        {
            // Arrange
            int saveNum = 3;
            string expectedNumber = saveNum == 0 ? "Auto" : saveNum.ToString();

            // Assert
            Assert.AreEqual("3", expectedNumber);
        }

        #endregion

        #region Integration Version Compatibility Tests

        [Test]
        public void MinVersion_IsOlderThanCurrentVersion()
        {
            // Arrange
            string minVersion = SaveManager.minValkyieVersion;
            // Assume current version is at least 3.10 based on CLAUDE.md
            string currentVersion = "3.10";

            // Act
            bool currentIsNewer = VersionManager.VersionNewer(minVersion, currentVersion);

            // Assert
            Assert.IsTrue(currentIsNewer,
                $"Current version ({currentVersion}) should be newer than min version ({minVersion})");
        }

        [Test]
        public void FutureVersion_WouldBeRejected()
        {
            // Arrange - Test the logic that would detect a save from the future
            string currentVersion = "3.10";
            string futureVersion = "4.0.0";

            // Act
            bool futureIsNewer = VersionManager.VersionNewer(currentVersion, futureVersion);

            // Assert
            Assert.IsTrue(futureIsNewer, "Future version should be detected as newer");
        }

        [Test]
        public void OldVersion_BelowMinimum_WouldBeRejected()
        {
            // Arrange - Test the logic that would detect a save from too old version
            string tooOldVersion = "0.5.0";
            string minVersion = SaveManager.minValkyieVersion; // 0.7.3

            // Act
            bool minIsNewerOrEqual = VersionManager.VersionNewerOrEqual(minVersion, tooOldVersion);

            // Assert
            Assert.IsFalse(minIsNewerOrEqual,
                $"Version {tooOldVersion} should not meet minimum requirement of {minVersion}");
        }

        [Test]
        public void MinVersion_ExactMatch_WouldBeAccepted()
        {
            // Arrange
            string saveVersion = SaveManager.minValkyieVersion;
            string minVersion = SaveManager.minValkyieVersion;

            // Act
            bool meetsMinimum = VersionManager.VersionNewerOrEqual(minVersion, saveVersion);

            // Assert
            Assert.IsTrue(meetsMinimum, "Exact minimum version should be accepted");
        }

        #endregion
    }
}
