using NUnit.Framework;
using Assets.Scripts.Content;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for StringKey class - Localization string key parsing functionality
    /// </summary>
    [TestFixture]
    public class StringKeyTests
    {
        [SetUp]
        public void Setup()
        {
            // Disable ValkyrieDebug to prevent Unity logging during tests
            ValkyrieDebug.enabled = false;

            // Add a test dictionary to LocalizationRead so regex pattern works
            // This allows the StringKey constructor to recognize valid key formats
            LocalizationRead.dicts["tst"] = null;
            LocalizationRead.dicts["ffg"] = null;
            LocalizationRead.dicts["qst"] = null;
            LocalizationRead.dicts["val"] = null;
        }

        [TearDown]
        public void TearDown()
        {
            ValkyrieDebug.enabled = true;
            // Clean up test dictionaries
            LocalizationRead.dicts.Clear();
        }

        #region Constructor Tests - Single String Parameter

        [Test]
        public void Constructor_ValidKeyFormat_ParsesDictCorrectly()
        {
            // Arrange
            string input = "{tst:MY_KEY}";

            // Act
            StringKey result = new StringKey(input);

            // Assert
            Assert.AreEqual("tst", result.dict);
        }

        [Test]
        public void Constructor_ValidKeyFormat_ParsesKeyCorrectly()
        {
            // Arrange
            string input = "{tst:MY_KEY}";

            // Act
            StringKey result = new StringKey(input);

            // Assert
            Assert.AreEqual("MY_KEY", result.key);
        }

        [Test]
        public void Constructor_ValidKeyWithParameters_ParsesParametersCorrectly()
        {
            // Arrange
            string input = "{tst:MY_KEY:param1}";

            // Act
            StringKey result = new StringKey(input);

            // Assert
            Assert.AreEqual("tst", result.dict);
            Assert.AreEqual("MY_KEY", result.key);
            Assert.AreEqual("{tst:MY_KEY:param1}", result.fullKey);
        }

        [Test]
        public void Constructor_ValidKeyWithMultipleColonParameters_ParsesCorrectly()
        {
            // Arrange
            string input = "{tst:MY_KEY:param1:param2:param3}";

            // Act
            StringKey result = new StringKey(input);

            // Assert
            Assert.AreEqual("tst", result.dict);
            Assert.AreEqual("MY_KEY", result.key);
            // Parameters should contain everything after the second colon
            Assert.AreEqual("{tst:MY_KEY:param1:param2:param3}", result.fullKey);
        }

        [Test]
        public void Constructor_PlainTextNotKey_SetsKeyAsInput()
        {
            // Arrange
            string input = "This is plain text";

            // Act
            StringKey result = new StringKey(input);

            // Assert
            Assert.IsNull(result.dict);
            Assert.AreEqual("This is plain text", result.key);
        }

        [Test]
        public void Constructor_InvalidKeyFormat_TreatsAsPlainText()
        {
            // Arrange
            string input = "{invalid}";

            // Act
            StringKey result = new StringKey(input);

            // Assert
            Assert.IsNull(result.dict);
            Assert.AreEqual("{invalid}", result.key);
        }

        #endregion

        #region Constructor Tests - Dict and Key Parameters

        [Test]
        public void Constructor_DictAndKey_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            StringKey result = new StringKey("qst", "MONSTER_NAME");

            // Assert
            Assert.AreEqual("qst", result.dict);
            Assert.AreEqual("MONSTER_NAME", result.key);
        }

        [Test]
        public void Constructor_DictAndKeyWithDoLookupFalse_SetsPreventLookup()
        {
            // Arrange & Act
            StringKey result = new StringKey("qst", "MONSTER_NAME", false);

            // Assert
            Assert.AreEqual("qst", result.dict);
            Assert.AreEqual("MONSTER_NAME", result.key);
            // isKey() should still return true since dict is set
            Assert.IsTrue(result.isKey());
        }

        [Test]
        public void Constructor_DictAndKeyWithStringParam_SetsParameterFormat()
        {
            // Arrange & Act
            StringKey result = new StringKey("qst", "MY_KEY", "paramValue");

            // Assert
            Assert.AreEqual("qst", result.dict);
            Assert.AreEqual("MY_KEY", result.key);
            Assert.AreEqual("{qst:MY_KEY:{0}:paramValue}", result.fullKey);
        }

        [Test]
        public void Constructor_DictAndKeyWithIntParam_SetsParameterFormat()
        {
            // Arrange & Act
            StringKey result = new StringKey("qst", "MY_KEY", 42);

            // Assert
            Assert.AreEqual("qst", result.dict);
            Assert.AreEqual("MY_KEY", result.key);
            Assert.AreEqual("{qst:MY_KEY:{0}:42}", result.fullKey);
        }

        [Test]
        public void Constructor_DictAndKeyWithStringKeyParam_UsesFullKeyOfParam()
        {
            // Arrange
            StringKey paramKey = new StringKey("val", "PARAM_KEY");

            // Act
            StringKey result = new StringKey("qst", "MY_KEY", paramKey);

            // Assert
            Assert.AreEqual("qst", result.dict);
            Assert.AreEqual("MY_KEY", result.key);
            Assert.AreEqual("{qst:MY_KEY:{0}:{val:PARAM_KEY}}", result.fullKey);
        }

        [Test]
        public void Constructor_TemplateWithTwoParams_SetsParametersCorrectly()
        {
            // Arrange
            StringKey template = new StringKey("qst", "TEMPLATE_KEY");

            // Act
            StringKey result = new StringKey(template, "first", "second");

            // Assert
            Assert.AreEqual("qst", result.dict);
            Assert.AreEqual("TEMPLATE_KEY", result.key);
            Assert.AreEqual("{qst:TEMPLATE_KEY:first:second}", result.fullKey);
        }

        #endregion

        #region isKey Tests

        [Test]
        public void IsKey_ValidKeyFormat_ReturnsTrue()
        {
            // Arrange
            StringKey key = new StringKey("{qst:MY_KEY}");

            // Act & Assert
            Assert.IsTrue(key.isKey());
        }

        [Test]
        public void IsKey_PlainText_ReturnsFalse()
        {
            // Arrange
            StringKey key = new StringKey("plain text");

            // Act & Assert
            Assert.IsFalse(key.isKey());
        }

        [Test]
        public void IsKey_NullDict_ReturnsFalse()
        {
            // Arrange
            StringKey key = new StringKey(null, "key", false);

            // Act & Assert
            Assert.IsFalse(key.isKey());
        }

        #endregion

        #region fullKey Property Tests

        [Test]
        public void FullKey_NullDict_ReturnsKeyOnly()
        {
            // Arrange
            StringKey key = new StringKey(null, "plain_key", false);

            // Act
            string fullKey = key.fullKey;

            // Assert
            Assert.AreEqual("plain_key", fullKey);
        }

        [Test]
        public void FullKey_WithDict_ReturnsFormattedKey()
        {
            // Arrange
            StringKey key = new StringKey("ffg", "SOME_KEY");

            // Act
            string fullKey = key.fullKey;

            // Assert
            Assert.AreEqual("{ffg:SOME_KEY}", fullKey);
        }

        [Test]
        public void FullKey_WithDictAndParameters_ReturnsFullFormat()
        {
            // Arrange
            StringKey key = new StringKey("val", "MSG", "replacement");

            // Act
            string fullKey = key.fullKey;

            // Assert
            Assert.AreEqual("{val:MSG:{0}:replacement}", fullKey);
        }

        #endregion

        #region ToString Tests

        [Test]
        public void ToString_PlainKey_ReturnsKey()
        {
            // Arrange
            StringKey key = new StringKey("plain text with\\nnewline");

            // Act
            string result = key.ToString();

            // Assert
            // ToString should escape newlines
            Assert.AreEqual("plain text with\\nnewline", result);
        }

        [Test]
        public void ToString_FormattedKey_ReturnsFullKey()
        {
            // Arrange
            StringKey key = new StringKey("{qst:MY_KEY}");

            // Act
            string result = key.ToString();

            // Assert
            Assert.AreEqual("{qst:MY_KEY}", result);
        }

        #endregion

        #region NULL Static Field Test

        [Test]
        public void NULL_StaticField_HasNullDictAndEmptyKey()
        {
            // Act
            StringKey nullKey = StringKey.NULL;

            // Assert
            Assert.IsNull(nullKey.dict);
            Assert.AreEqual("", nullKey.key);
            Assert.IsFalse(nullKey.isKey());
        }

        #endregion
    }
}
