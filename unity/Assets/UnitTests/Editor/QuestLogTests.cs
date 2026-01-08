using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ValkyrieTools;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for QuestLog class and Quest.LogEntry class - Quest logging functionality
    /// </summary>
    [TestFixture]
    public class QuestLogTests
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

        #region QuestLog Constructor Tests

        [Test]
        public void Constructor_Default_CreatesEmptyLog()
        {
            // Arrange & Act
            var questLog = new QuestLog();

            // Assert
            Assert.IsNotNull(questLog);
            Assert.AreEqual(0, questLog.Count());
        }

        #endregion

        #region QuestLog.Add Tests

        [Test]
        public void Add_SingleEntry_IncreasesCount()
        {
            // Arrange
            var questLog = new QuestLog();
            var entry = new Quest.LogEntry("Test message");

            // Act
            questLog.Add(entry);

            // Assert
            Assert.AreEqual(1, questLog.Count());
        }

        [Test]
        public void Add_MultipleEntries_IncreasesCountCorrectly()
        {
            // Arrange
            var questLog = new QuestLog();

            // Act
            questLog.Add(new Quest.LogEntry("Entry 1"));
            questLog.Add(new Quest.LogEntry("Entry 2"));
            questLog.Add(new Quest.LogEntry("Entry 3"));

            // Assert
            Assert.AreEqual(3, questLog.Count());
        }

        [Test]
        public void Add_MultipleEntries_MaintainsOrder()
        {
            // Arrange
            var questLog = new QuestLog();

            // Act
            questLog.Add(new Quest.LogEntry("First"));
            questLog.Add(new Quest.LogEntry("Second"));
            questLog.Add(new Quest.LogEntry("Third"));

            // Assert
            var entries = questLog.ToList();
            Assert.AreEqual("First\n\n", entries[0].GetEntry());
            Assert.AreEqual("Second\n\n", entries[1].GetEntry());
            Assert.AreEqual("Third\n\n", entries[2].GetEntry());
        }

        #endregion

        #region QuestLog Enumeration Tests

        [Test]
        public void GetEnumerator_EmptyLog_ReturnsEmptyEnumerator()
        {
            // Arrange
            var questLog = new QuestLog();

            // Act
            var entries = new List<Quest.LogEntry>();
            foreach (var entry in questLog)
            {
                entries.Add(entry);
            }

            // Assert
            Assert.AreEqual(0, entries.Count);
        }

        [Test]
        public void GetEnumerator_WithEntries_EnumeratesAllEntries()
        {
            // Arrange
            var questLog = new QuestLog();
            questLog.Add(new Quest.LogEntry("One"));
            questLog.Add(new Quest.LogEntry("Two"));

            // Act
            var entries = new List<Quest.LogEntry>();
            foreach (var entry in questLog)
            {
                entries.Add(entry);
            }

            // Assert
            Assert.AreEqual(2, entries.Count);
        }

        [Test]
        public void IEnumerableGetEnumerator_WithEntries_EnumeratesAllEntries()
        {
            // Arrange
            var questLog = new QuestLog();
            questLog.Add(new Quest.LogEntry("Test"));

            // Act - Using IEnumerable interface explicitly
            var enumerable = (System.Collections.IEnumerable)questLog;
            var count = 0;
            foreach (var entry in enumerable)
            {
                count++;
            }

            // Assert
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ToList_WithMultipleEntries_ReturnsAllEntries()
        {
            // Arrange
            var questLog = new QuestLog();
            questLog.Add(new Quest.LogEntry("A"));
            questLog.Add(new Quest.LogEntry("B"));
            questLog.Add(new Quest.LogEntry("C"));

            // Act
            var list = questLog.ToList();

            // Assert
            Assert.AreEqual(3, list.Count);
        }

        #endregion

        #region LogEntry Constructor Tests

        [Test]
        public void LogEntry_SimpleConstructor_CreatesQuestEntry()
        {
            // Arrange & Act
            var entry = new Quest.LogEntry("Simple message");

            // Assert
            Assert.AreEqual("Simple message\n\n", entry.GetEntry());
        }

        [Test]
        public void LogEntry_WithEditorFlag_CreatesEditorEntry()
        {
            // Arrange & Act
            var entry = new Quest.LogEntry("Editor message", true);

            // Assert - Editor entries are hidden by default
            Assert.AreEqual("", entry.GetEntry(false));
            // Visible when editor mode is enabled
            Assert.AreEqual("Editor message\n\n", entry.GetEntry(true));
        }

        [Test]
        public void LogEntry_WithValkyrieFlag_CreatesValkyrieEntry()
        {
            // Arrange & Act
            var entry = new Quest.LogEntry("Valkyrie message", false, true);

            // Assert - Valkyrie entries are only visible in editor
            // Note: Application.isEditor is false in test context
            Assert.AreEqual("", entry.GetEntry());
        }

        [Test]
        public void LogEntry_TypeStringConstructor_QuestType_CreatesQuestEntry()
        {
            // Arrange & Act
            var entry = new Quest.LogEntry("quest0", "Quest message");

            // Assert
            Assert.AreEqual("Quest message\n\n", entry.GetEntry());
        }

        [Test]
        public void LogEntry_TypeStringConstructor_EditorType_CreatesEditorEntry()
        {
            // Arrange & Act
            var entry = new Quest.LogEntry("editor0", "Editor message");

            // Assert
            Assert.AreEqual("", entry.GetEntry(false));
            Assert.AreEqual("Editor message\n\n", entry.GetEntry(true));
        }

        [Test]
        public void LogEntry_TypeStringConstructor_ValkyrieType_CreatesValkyrieEntry()
        {
            // Arrange & Act
            var entry = new Quest.LogEntry("valkyrie0", "Valkyrie message");

            // Assert
            Assert.AreEqual("", entry.GetEntry());
        }

        #endregion

        #region LogEntry.ToString Tests

        [Test]
        public void LogEntry_ToString_QuestEntry_FormatsCorrectly()
        {
            // Arrange
            var entry = new Quest.LogEntry("Test message");

            // Act
            var result = entry.ToString(5);

            // Assert
            Assert.AreEqual("quest5=Test message\r\n", result);
        }

        [Test]
        public void LogEntry_ToString_EditorEntry_FormatsCorrectly()
        {
            // Arrange
            var entry = new Quest.LogEntry("Editor message", true);

            // Act
            var result = entry.ToString(3);

            // Assert
            Assert.AreEqual("editor3=Editor message\r\n", result);
        }

        [Test]
        public void LogEntry_ToString_ValkyrieEntry_FormatsCorrectly()
        {
            // Arrange
            var entry = new Quest.LogEntry("Valkyrie message", false, true);

            // Act
            var result = entry.ToString(7);

            // Assert
            Assert.AreEqual("valkyrie7=Valkyrie message\r\n", result);
        }

        [Test]
        public void LogEntry_ToString_WithNewlines_EscapesNewlines()
        {
            // Arrange
            var entry = new Quest.LogEntry("Line1\nLine2\nLine3");

            // Act
            var result = entry.ToString(0);

            // Assert
            Assert.IsTrue(result.Contains("Line1\\nLine2\\nLine3"));
        }

        #endregion

        #region LogEntry.GetEntry Tests

        [Test]
        public void LogEntry_GetEntry_WithNewlines_UnescapesNewlines()
        {
            // Arrange - Entry stored with escaped newlines
            var entry = new Quest.LogEntry("quest0", "Line1\\nLine2");

            // Act
            var result = entry.GetEntry();

            // Assert - Should unescape \n to actual newlines
            Assert.AreEqual("Line1\nLine2\n\n", result);
        }

        [Test]
        public void LogEntry_GetEntry_EditorFalse_HidesEditorEntries()
        {
            // Arrange
            var entry = new Quest.LogEntry("Notice: Debug info", true);

            // Act
            var result = entry.GetEntry(false);

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void LogEntry_GetEntry_EditorTrue_ShowsEditorEntries()
        {
            // Arrange
            var entry = new Quest.LogEntry("Notice: Debug info", true);

            // Act
            var result = entry.GetEntry(true);

            // Assert
            Assert.AreEqual("Notice: Debug info\n\n", result);
        }

        [Test]
        public void LogEntry_GetEntry_QuestEntry_AlwaysVisible()
        {
            // Arrange
            var entry = new Quest.LogEntry("Important quest message");

            // Act
            var resultWithoutEditor = entry.GetEntry(false);
            var resultWithEditor = entry.GetEntry(true);

            // Assert
            Assert.AreEqual("Important quest message\n\n", resultWithoutEditor);
            Assert.AreEqual("Important quest message\n\n", resultWithEditor);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void QuestLog_AddAndEnumerate_WorksCorrectly()
        {
            // Arrange
            var questLog = new QuestLog();
            var messages = new[] { "Start quest", "Found item", "Defeated monster", "Quest complete" };

            // Act
            foreach (var msg in messages)
            {
                questLog.Add(new Quest.LogEntry(msg));
            }

            // Assert
            var entries = questLog.ToList();
            Assert.AreEqual(4, entries.Count);
            for (int i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i] + "\n\n", entries[i].GetEntry());
            }
        }

        [Test]
        public void QuestLog_MixedEntryTypes_FiltersCorrectly()
        {
            // Arrange
            var questLog = new QuestLog();
            questLog.Add(new Quest.LogEntry("Quest entry"));
            questLog.Add(new Quest.LogEntry("Editor entry", true));
            questLog.Add(new Quest.LogEntry("Another quest entry"));

            // Act - Get visible entries without editor mode
            var visibleCount = 0;
            foreach (var entry in questLog)
            {
                if (entry.GetEntry(false).Length > 0)
                {
                    visibleCount++;
                }
            }

            // Assert
            Assert.AreEqual(2, visibleCount);
        }

        [Test]
        public void QuestLog_SerializationRoundTrip_MaintainsData()
        {
            // Arrange
            var entry = new Quest.LogEntry("Test message for roundtrip");

            // Act - Serialize
            var serialized = entry.ToString(0);

            // Parse the serialized string to extract type and message
            var parts = serialized.Split('=');
            var type = parts[0];
            var message = parts[1].TrimEnd('\r', '\n');

            // Create new entry from parsed data
            var newEntry = new Quest.LogEntry(type, message);

            // Assert
            Assert.AreEqual(entry.GetEntry(), newEntry.GetEntry());
        }

        #endregion
    }
}
