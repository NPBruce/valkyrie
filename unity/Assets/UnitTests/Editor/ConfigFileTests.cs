using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for ConfigFile-related functionality
    /// Tests focus on IniData parsing and manipulation which is the core of ConfigFile
    /// without depending on Game.Get() or file system operations
    /// </summary>
    [TestFixture]
    public class ConfigFileTests
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

        #region Boolean Value Parsing Tests

        [Test]
        public void BooleanParsing_TrueString_ParsesAsTrue()
        {
            // Arrange
            string iniContent = @"[Settings]
enabled=True";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string value = data.Get("Settings", "enabled");
            bool result = bool.Parse(value);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void BooleanParsing_FalseString_ParsesAsFalse()
        {
            // Arrange
            string iniContent = @"[Settings]
enabled=False";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string value = data.Get("Settings", "enabled");
            bool result = bool.Parse(value);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void BooleanParsing_LowercaseTrue_ParsesAsTrue()
        {
            // Arrange
            string iniContent = @"[Settings]
enabled=true";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string value = data.Get("Settings", "enabled");
            bool result = bool.Parse(value);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void BooleanParsing_MissingKey_ReturnsEmptyString()
        {
            // Arrange
            string iniContent = @"[Settings]
other=value";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string value = data.Get("Settings", "enabled");

            // Assert
            Assert.AreEqual("", value);
        }

        #endregion

        #region Integer Value Parsing Tests

        [Test]
        public void IntegerParsing_ValidInteger_ParsesCorrectly()
        {
            // Arrange
            string iniContent = @"[Settings]
volume=75";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string value = data.Get("Settings", "volume");
            int result = int.Parse(value);

            // Assert
            Assert.AreEqual(75, result);
        }

        [Test]
        public void IntegerParsing_NegativeInteger_ParsesCorrectly()
        {
            // Arrange
            string iniContent = @"[Settings]
offset=-10";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string value = data.Get("Settings", "offset");
            int result = int.Parse(value);

            // Assert
            Assert.AreEqual(-10, result);
        }

        [Test]
        public void IntegerParsing_Zero_ParsesCorrectly()
        {
            // Arrange
            string iniContent = @"[Settings]
count=0";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string value = data.Get("Settings", "count");
            int result = int.Parse(value);

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion

        #region String Value Handling Tests

        [Test]
        public void StringValue_SimpleString_ReturnsAsIs()
        {
            // Arrange
            string iniContent = @"[Settings]
language=English";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string result = data.Get("Settings", "language");

            // Assert
            Assert.AreEqual("English", result);
        }

        [Test]
        public void StringValue_StringWithSpaces_ReturnsWithSpaces()
        {
            // Arrange
            string iniContent = @"[Settings]
path=my game path";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string result = data.Get("Settings", "path");

            // Assert
            Assert.AreEqual("my game path", result);
        }

        [Test]
        public void StringValue_QuotedString_QuotesRemoved()
        {
            // Arrange
            string iniContent = @"[Settings]
message=""Hello World""";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string result = data.Get("Settings", "message");

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        #endregion

        #region Default Value Handling Tests

        [Test]
        public void DefaultValue_MissingSection_ReturnsEmptyString()
        {
            // Arrange
            IniData data = new IniData();

            // Act
            string result = data.Get("NonExistent", "key");

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void DefaultValue_MissingKey_ReturnsEmptyString()
        {
            // Arrange
            string iniContent = @"[Settings]
existing=value";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            string result = data.Get("Settings", "nonexistent");

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void DefaultValue_EmptyData_ReturnsEmptyString()
        {
            // Arrange
            IniData data = new IniData();

            // Act
            string result = data.Get("Any", "key");

            // Assert
            Assert.AreEqual("", result);
        }

        #endregion

        #region Pack Management Tests (simulating ConfigFile behavior)

        [Test]
        public void GetPacks_ExistingPacks_ReturnsPackKeys()
        {
            // Arrange - simulating D2EPacks section
            string iniContent = @"[D2EPacks]
base_game=
conversion_kit=
manor_of_ravens=";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            var packs = data.Get("D2EPacks")?.Keys ?? Enumerable.Empty<string>();

            // Assert
            Assert.AreEqual(3, packs.Count());
            Assert.IsTrue(packs.Contains("base_game"));
            Assert.IsTrue(packs.Contains("conversion_kit"));
            Assert.IsTrue(packs.Contains("manor_of_ravens"));
        }

        [Test]
        public void GetPacks_NoPacks_ReturnsEmpty()
        {
            // Arrange
            IniData data = new IniData();

            // Act
            var packs = data.Get("D2EPacks")?.Keys ?? Enumerable.Empty<string>();

            // Assert
            Assert.AreEqual(0, packs.Count());
        }

        [Test]
        public void AddPack_NewPack_AddsToSection()
        {
            // Arrange
            IniData data = new IniData();
            string gameType = "D2E";
            string pack = "new_expansion";

            // Act
            data.Add(gameType + "Packs", pack, "");

            // Assert
            var packs = data.Get(gameType + "Packs")?.Keys;
            Assert.IsNotNull(packs);
            Assert.IsTrue(packs.Contains(pack));
        }

        [Test]
        public void RemovePack_ExistingPack_RemovesFromSection()
        {
            // Arrange
            IniData data = new IniData();
            string section = "D2EPacks";
            data.Add(section, "pack1", "");
            data.Add(section, "pack2", "");

            // Act
            data.Remove(section, "pack1");

            // Assert
            var packs = data.Get(section)?.Keys;
            Assert.IsNotNull(packs);
            Assert.IsFalse(packs.Contains("pack1"));
            Assert.IsTrue(packs.Contains("pack2"));
        }

        [Test]
        public void GetPackLanguages_PacksWithLanguages_ReturnsDictionary()
        {
            // Arrange
            string iniContent = @"[MoMPacks]
base_game=English
expansion1=French
expansion2=";
            IniData data = IniRead.ReadFromString(iniContent);

            // Act
            var packLanguages = data.Get("MoMPacks") ?? new Dictionary<string, string>();

            // Assert
            Assert.AreEqual(3, packLanguages.Count);
            Assert.AreEqual("English", packLanguages["base_game"]);
            Assert.AreEqual("French", packLanguages["expansion1"]);
            Assert.AreEqual("", packLanguages["expansion2"]);
        }

        #endregion
    }
}
