using NUnit.Framework;
using ValkyrieTools;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for IniRead class - INI file parsing functionality
    /// </summary>
    [TestFixture]
    public class IniReadTests
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

        [Test]
        public void ReadFromString_SimpleSingleSection_ParsesCorrectly()
        {
            // Arrange
            string iniContent = @"[Section1]
key1=value1
key2=value2";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("value1", result.Get("Section1", "key1"));
            Assert.AreEqual("value2", result.Get("Section1", "key2"));
        }

        [Test]
        public void ReadFromString_MultipleSections_ParsesAllSections()
        {
            // Arrange
            string iniContent = @"[Section1]
key1=value1

[Section2]
key2=value2";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("value1", result.Get("Section1", "key1"));
            Assert.AreEqual("value2", result.Get("Section2", "key2"));
        }

        [Test]
        public void ReadFromString_CommentsIgnored_OnlyParsesData()
        {
            // Arrange
            string iniContent = @"# This is a comment
[Section1]
; This is also a comment
key1=value1";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("value1", result.Get("Section1", "key1"));
        }

        [Test]
        public void ReadFromString_KeyWithoutValue_ParsesAsEmptyValue()
        {
            // Arrange
            string iniContent = @"[Section1]
keyonly";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("", result.Get("Section1", "keyonly"));
        }

        [Test]
        public void ReadFromString_QuotedValue_TrimsQuotes()
        {
            // Arrange
            string iniContent = @"[Section1]
key=""quoted value""";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("quoted value", result.Get("Section1", "key"));
        }

        [Test]
        public void ReadFromString_EmptyInput_ReturnsEmptyData()
        {
            // Arrange
            string iniContent = "";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ReadFromString_WhitespaceAroundValues_TrimsProperly()
        {
            // Arrange
            string iniContent = @"[Section1]
  key1  =  value1  ";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("value1", result.Get("Section1", "key1"));
        }

        [Test]
        public void IniData_Add_AddsNewSection()
        {
            // Arrange
            IniData data = new IniData();

            // Act
            data.Add("NewSection", "key", "value");

            // Assert
            Assert.AreEqual("value", data.Get("NewSection", "key"));
        }

        [Test]
        public void IniData_Remove_RemovesEntry()
        {
            // Arrange
            IniData data = new IniData();
            data.Add("Section", "key", "value");

            // Act
            data.Remove("Section", "key");

            // Assert
            Assert.AreEqual("", data.Get("Section", "key"));
        }

        [Test]
        public void IniData_RemoveSection_RemovesEntireSection()
        {
            // Arrange
            IniData data = new IniData();
            data.Add("Section", "key1", "value1");
            data.Add("Section", "key2", "value2");

            // Act
            data.Remove("Section");

            // Assert
            Assert.IsNull(data.Get("Section"));
        }

        [Test]
        public void IniData_GetNonExistentSection_ReturnsNull()
        {
            // Arrange
            IniData data = new IniData();

            // Act
            var result = data.Get("NonExistent");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void IniData_GetNonExistentKey_ReturnsEmptyString()
        {
            // Arrange
            IniData data = new IniData();
            data.Add("Section", "key", "value");

            // Act
            var result = data.Get("Section", "nonexistent");

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void IniData_ToString_OutputsValidIniFormat()
        {
            // Arrange
            IniData data = new IniData();
            data.Add("Section", "key", "value");

            // Act
            string result = data.ToString();

            // Assert
            Assert.IsTrue(result.Contains("[Section]"));
            Assert.IsTrue(result.Contains("key=value"));
        }

        [Test]
        public void IniData_AddDuplicateKey_ReplacesValue()
        {
            // Arrange
            IniData data = new IniData();
            data.Add("Section", "key", "value1");

            // Act
            data.Add("Section", "key", "value2");

            // Assert
            Assert.AreEqual("value2", data.Get("Section", "key"));
        }

        [Test]
        public void ReadFromString_DuplicateSections_IgnoresSecondOccurrence()
        {
            // Arrange
            string iniContent = @"[Section1]
key1=value1

[Section1]
key2=value2";

            // Act
            IniData result = IniRead.ReadFromString(iniContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("value1", result.Get("Section1", "key1"));
            // Second section is ignored, so key2 should not exist
            Assert.AreEqual("", result.Get("Section1", "key2"));
        }
    }
}
