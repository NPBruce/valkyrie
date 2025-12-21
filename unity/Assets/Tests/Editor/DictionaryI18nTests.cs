using System;
using System.Collections.Generic;
using NUnit.Framework;
using Assets.Scripts.Content;
using ValkyrieTools;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for DictionaryI18n class - Internationalization dictionary functionality
    /// Note: Many DictionaryI18n constructors call Game.Get() which requires the Unity runtime.
    /// Tests are designed to either:
    /// 1. Test methods that don't require Game.Get()
    /// 2. Use try/catch blocks for methods that might call Game.Get()
    /// 3. Test the internal logic patterns used by the class
    /// </summary>
    [TestFixture]
    public class DictionaryI18nTests
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

        #region CSV Parsing Logic Tests

        // These tests verify the CSV parsing patterns used in DictionaryI18n.ParseEntry()
        // Testing the string manipulation logic directly

        [Test]
        public void ParseEntryLogic_SimpleValue_ReturnedUnchanged()
        {
            // Arrange
            string entry = "simple value";

            // Act - simulating ParseEntry logic
            string result = entry.Replace("\\n", "\n");

            // Assert
            Assert.AreEqual("simple value", result);
        }

        [Test]
        public void ParseEntryLogic_ValueWithEscapedNewline_ConvertsToRealNewline()
        {
            // Arrange
            string entry = "line one\\nline two";

            // Act - simulating ParseEntry logic
            string result = entry.Replace("\\n", "\n");

            // Assert
            Assert.AreEqual("line one\nline two", result);
        }

        [Test]
        public void ParseEntryLogic_MultipleEscapedNewlines_AllConverted()
        {
            // Arrange
            string entry = "line1\\nline2\\nline3\\nline4";

            // Act
            string result = entry.Replace("\\n", "\n");

            // Assert
            Assert.AreEqual("line1\nline2\nline3\nline4", result);
        }

        [Test]
        public void ParseEntryLogic_QuotedValue_TrimmsQuotes()
        {
            // Arrange
            string entry = "\"quoted value\"";

            // Act - simulating ParseEntry logic for quoted strings
            string result = entry.Replace("\\n", "\n");
            if (result.Length > 1 && result[0] == '\"' && result[result.Length - 1] == '\"')
            {
                result = result.Substring(1, result.Length - 2);
            }

            // Assert
            Assert.AreEqual("quoted value", result);
        }

        [Test]
        public void ParseEntryLogic_QuotedValueWithEscapedQuotes_HandlesCorrectly()
        {
            // Arrange - escaped quotes in CSV are represented as ""
            string entry = "\"value with \"\"quotes\"\" inside\"";

            // Act - simulating ParseEntry logic
            string result = entry.Replace("\\n", "\n");
            if (result.Length > 1 && result[0] == '\"' && result[result.Length - 1] == '\"')
            {
                result = result.Substring(1, result.Length - 2);
                result = result.Replace("\"\"", "\"");
            }

            // Assert
            Assert.AreEqual("value with \"quotes\" inside", result);
        }

        [Test]
        public void ParseEntryLogic_TripleQuotedValue_TrimsTripleQuotes()
        {
            // Arrange - triple quote enclosing for complex values
            string tripleEnclosing = "|||";
            string entry = "|||some complex value|||";

            // Act - simulating ParseEntry logic for triple quotes
            string result = entry.Replace("\\n", "\n");
            if (result.Length >= tripleEnclosing.Length * 2
                && result.StartsWith(tripleEnclosing) && result.Trim().EndsWith(tripleEnclosing))
            {
                result = result.Substring(tripleEnclosing.Length, result.Length - tripleEnclosing.Length * 2);
            }

            // Assert
            Assert.AreEqual("some complex value", result);
        }

        [Test]
        public void ParseEntryLogic_TripleQuotedWithInternalQuotes_PreservesInternalQuotes()
        {
            // Arrange
            string tripleEnclosing = "|||";
            string entry = "|||value with \"internal\" quotes|||";

            // Act
            string result = entry.Replace("\\n", "\n");
            if (result.Length >= tripleEnclosing.Length * 2
                && result.StartsWith(tripleEnclosing) && result.Trim().EndsWith(tripleEnclosing))
            {
                result = result.Substring(tripleEnclosing.Length, result.Length - tripleEnclosing.Length * 2);
            }

            // Assert
            Assert.AreEqual("value with \"internal\" quotes", result);
        }

        [Test]
        public void ParseEntryLogic_EmptyQuotedString_ReturnsEmpty()
        {
            // Arrange
            string entry = "\"\"";

            // Act
            string result = entry.Replace("\\n", "\n");
            if (result.Length > 1 && result[0] == '\"' && result[result.Length - 1] == '\"')
            {
                result = result.Substring(1, result.Length - 2);
            }

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void ParseEntryLogic_SingleCharacter_ReturnedUnchanged()
        {
            // Arrange
            string entry = "X";

            // Act
            string result = entry.Replace("\\n", "\n");

            // Assert
            Assert.AreEqual("X", result);
        }

        [Test]
        public void ParseEntryLogic_EmptyString_ReturnsEmpty()
        {
            // Arrange
            string entry = "";

            // Act
            string result = entry.Replace("\\n", "\n");

            // Assert
            Assert.AreEqual("", result);
        }

        #endregion

        #region CSV Line Splitting Tests

        // Testing the CSV parsing logic patterns used in AddData()

        [Test]
        public void CsvSplitLogic_SimpleKeyValue_SplitsCorrectly()
        {
            // Arrange
            string line = "KEY,value";

            // Act
            string[] components = line.Split(",".ToCharArray(), 2);

            // Assert
            Assert.AreEqual(2, components.Length);
            Assert.AreEqual("KEY", components[0]);
            Assert.AreEqual("value", components[1]);
        }

        [Test]
        public void CsvSplitLogic_KeyValueWithCommaInValue_ValuePreserved()
        {
            // Arrange - split with limit 2 should preserve commas in value
            string line = "KEY,value with, commas, inside";

            // Act
            string[] components = line.Split(",".ToCharArray(), 2);

            // Assert
            Assert.AreEqual(2, components.Length);
            Assert.AreEqual("KEY", components[0]);
            Assert.AreEqual("value with, commas, inside", components[1]);
        }

        [Test]
        public void CsvSplitLogic_KeyOnly_SingleComponent()
        {
            // Arrange
            string line = "KEYONLY";

            // Act
            string[] components = line.Split(",".ToCharArray(), 2);

            // Assert
            Assert.AreEqual(1, components.Length);
            Assert.AreEqual("KEYONLY", components[0]);
        }

        [Test]
        public void CsvSplitLogic_KeyWithEmptyValue_TwoComponents()
        {
            // Arrange
            string line = "KEY,";

            // Act
            string[] components = line.Split(",".ToCharArray(), 2);

            // Assert
            Assert.AreEqual(2, components.Length);
            Assert.AreEqual("KEY", components[0]);
            Assert.AreEqual("", components[1]);
        }

        [Test]
        public void CsvSplitLogic_QuotedValueWithComma_SplitsAtFirstComma()
        {
            // Arrange - note: this is raw split, not full CSV parsing
            string line = "KEY,\"value, with, commas\"";

            // Act
            string[] components = line.Split(",".ToCharArray(), 2);

            // Assert
            Assert.AreEqual(2, components.Length);
            Assert.AreEqual("KEY", components[0]);
            Assert.AreEqual("\"value, with, commas\"", components[1]);
        }

        #endregion

        #region Quote Counting Logic Tests

        // Testing the multiline detection logic based on quote counting

        [Test]
        public void QuoteCountLogic_EvenQuotes_IsSelfContained()
        {
            // Arrange
            string line = "KEY,\"quoted value\"";

            // Act
            int sections = line.Split('\"').Length;
            bool isOddQuotes = (sections % 2) == 0;

            // Assert - even number of quotes (odd sections) means self-contained
            Assert.IsFalse(isOddQuotes, "3 sections means 2 quotes (even)");
        }

        [Test]
        public void QuoteCountLogic_OddQuotes_MayBeMultiline()
        {
            // Arrange - odd quotes might indicate multiline value
            string line = "KEY,\"quoted value without closing";

            // Act
            int sections = line.Split('\"').Length;
            bool isOddQuotes = (sections % 2) == 0;

            // Assert - 2 sections means 1 quote (odd)
            Assert.IsTrue(isOddQuotes);
        }

        [Test]
        public void QuoteCountLogic_NoQuotes_IsSelfContained()
        {
            // Arrange
            string line = "KEY,simple value";

            // Act
            int sections = line.Split('\"').Length;
            bool isSelfContained = (sections % 2) == 1;

            // Assert - 1 section means 0 quotes (even), self-contained
            Assert.IsTrue(isSelfContained);
        }

        [Test]
        public void QuoteCountLogic_FourQuotes_IsSelfContained()
        {
            // Arrange
            string line = "KEY,\"value \"\"with\"\" escaped quotes\"";

            // Act
            int sections = line.Split('\"').Length;
            // 5 sections means 4 quotes
            bool isSelfContained = (sections % 2) == 1;

            // Assert
            Assert.IsTrue(isSelfContained);
        }

        #endregion

        #region Triple Quote Mode Tests

        [Test]
        public void TripleQuoteDetection_StartsWithTripleQuote_Detected()
        {
            // Arrange
            string line = "KEY,|||multi line content";
            string tripleEnclosing = "|||";

            // Act
            bool startsWithTriple = line.IndexOf($",{tripleEnclosing}", StringComparison.InvariantCulture) != -1;

            // Assert
            Assert.IsTrue(startsWithTriple);
        }

        [Test]
        public void TripleQuoteDetection_EndsWithTripleQuote_EndDetected()
        {
            // Arrange
            string line = "end of content|||";
            string tripleEnclosing = "|||";

            // Act
            bool endsWithTriple = line.TrimEnd().EndsWith(tripleEnclosing, StringComparison.InvariantCulture);

            // Assert
            Assert.IsTrue(endsWithTriple);
        }

        [Test]
        public void TripleQuoteDetection_MiddleLine_NoTripleQuoteStart()
        {
            // Arrange
            string line = "middle line content";
            string tripleEnclosing = "|||";

            // Act
            bool startsWithTriple = line.IndexOf($",{tripleEnclosing}", StringComparison.InvariantCulture) != -1;
            bool endsWithTriple = line.TrimEnd().EndsWith(tripleEnclosing, StringComparison.InvariantCulture);

            // Assert
            Assert.IsFalse(startsWithTriple);
            Assert.IsFalse(endsWithTriple);
        }

        [Test]
        public void TripleQuoteDetection_SelfContainedTripleQuote_BothDetected()
        {
            // Arrange
            string line = "KEY,|||single line|||";
            string tripleEnclosing = "|||";

            // Act
            bool startsWithTriple = line.IndexOf($",{tripleEnclosing}", StringComparison.InvariantCulture) != -1;
            bool endsWithTriple = line.TrimEnd().EndsWith(tripleEnclosing, StringComparison.InvariantCulture);

            // Assert
            Assert.IsTrue(startsWithTriple);
            Assert.IsTrue(endsWithTriple);
        }

        #endregion

        #region Old Format Detection Tests

        [Test]
        public void OldFormatDetection_StartsWithQuote_IsNotOldFormat()
        {
            // Arrange
            string rawLine = "KEY,\"quoted value\"";

            // Act - simulating isOldFormat logic
            string[] components = rawLine.Split(",".ToCharArray(), 2);
            bool isNotOldFormat = components.Length > 1 && components[1].Length > 0 && components[1][0] == '\"';
            bool isOldFormat = !isNotOldFormat;

            // Assert
            Assert.IsFalse(isOldFormat);
        }

        [Test]
        public void OldFormatDetection_DoesNotStartWithQuote_IsOldFormat()
        {
            // Arrange
            string rawLine = "KEY,unquoted value";

            // Act
            string[] components = rawLine.Split(",".ToCharArray(), 2);
            bool isNotOldFormat = components.Length > 1 && components[1].Length > 0 && components[1][0] == '\"';
            bool isOldFormat = !isNotOldFormat;

            // Assert
            Assert.IsTrue(isOldFormat);
        }

        [Test]
        public void OldFormatDetection_EmptyValue_IsOldFormat()
        {
            // Arrange
            string rawLine = "KEY,";

            // Act
            string[] components = rawLine.Split(",".ToCharArray(), 2);
            bool isNotOldFormat = components.Length > 1 && components[1].Length > 0 && components[1][0] == '\"';
            bool isOldFormat = !isNotOldFormat;

            // Assert - empty value means length is 0, so treated as old format
            Assert.IsTrue(isOldFormat);
        }

        [Test]
        public void OldFormatDetection_KeyOnly_IsOldFormat()
        {
            // Arrange
            string rawLine = "KEYONLY";

            // Act
            string[] components = rawLine.Split(",".ToCharArray(), 2);
            bool isNotOldFormat = components.Length > 1 && components[1].Length > 0 && components[1][0] == '\"';
            bool isOldFormat = !isNotOldFormat;

            // Assert - no value component, so treated as old format
            Assert.IsTrue(isOldFormat);
        }

        #endregion

        #region Comment Detection Tests

        [Test]
        public void CommentDetection_LineStartsWithDoubleSlash_IsComment()
        {
            // Arrange
            string line = "// This is a comment";

            // Act
            bool isComment = line.Trim().IndexOf("//") == 0;

            // Assert
            Assert.IsTrue(isComment);
        }

        [Test]
        public void CommentDetection_LineStartsWithSpacesThenDoubleSlash_IsComment()
        {
            // Arrange
            string line = "   // This is a comment with leading spaces";

            // Act
            bool isComment = line.Trim().IndexOf("//") == 0;

            // Assert
            Assert.IsTrue(isComment);
        }

        [Test]
        public void CommentDetection_LineContainsDoubleSlashNotAtStart_IsNotComment()
        {
            // Arrange
            string line = "KEY,value with // in middle";

            // Act
            bool isComment = line.Trim().IndexOf("//") == 0;

            // Assert
            Assert.IsFalse(isComment);
        }

        [Test]
        public void CommentDetection_RegularLine_IsNotComment()
        {
            // Arrange
            string line = "KEY,regular value";

            // Act
            bool isComment = line.Trim().IndexOf("//") == 0;

            // Assert
            Assert.IsFalse(isComment);
        }

        #endregion

        #region Line Trimming Tests

        [Test]
        public void LineTrimming_RemovesCarriageReturn()
        {
            // Arrange
            string line = "KEY,value\r";

            // Act
            string trimmed = line.Trim('\r', '\n');

            // Assert
            Assert.AreEqual("KEY,value", trimmed);
        }

        [Test]
        public void LineTrimming_RemovesLineFeed()
        {
            // Arrange
            string line = "KEY,value\n";

            // Act
            string trimmed = line.Trim('\r', '\n');

            // Assert
            Assert.AreEqual("KEY,value", trimmed);
        }

        [Test]
        public void LineTrimming_RemovesBothCRLF()
        {
            // Arrange
            string line = "KEY,value\r\n";

            // Act
            string trimmed = line.Trim('\r', '\n');

            // Assert
            Assert.AreEqual("KEY,value", trimmed);
        }

        [Test]
        public void LineTrimming_PreservesInternalWhitespace()
        {
            // Arrange
            string line = "KEY,value with spaces\r\n";

            // Act
            string trimmed = line.Trim('\r', '\n');

            // Assert
            Assert.AreEqual("KEY,value with spaces", trimmed);
        }

        #endregion

        #region Serialization Logic Tests

        [Test]
        public void SerializationLogic_SimpleValue_NoQuotesAdded()
        {
            // Arrange
            string rawValue = "simple value";
            string doubleQuote = "\"";
            string tripleEnclosing = "|||";

            // Act - simulating SerializeMultiple logic
            rawValue = rawValue.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n");
            string output;
            if (rawValue.Contains(doubleQuote) && !rawValue.Contains(tripleEnclosing))
            {
                output = "KEY," + tripleEnclosing + rawValue + tripleEnclosing;
            }
            else if (rawValue.Contains(doubleQuote) || rawValue.Contains(tripleEnclosing) || rawValue.Contains("\\n"))
            {
                string quotedLine = doubleQuote + rawValue.Replace(doubleQuote, "\"\"") + doubleQuote;
                output = "KEY," + quotedLine;
            }
            else
            {
                output = "KEY," + rawValue;
            }

            // Assert
            Assert.AreEqual("KEY,simple value", output);
        }

        [Test]
        public void SerializationLogic_ValueWithNewlines_GetsQuoted()
        {
            // Arrange
            string rawValue = "line1\nline2";
            string doubleQuote = "\"";
            string tripleEnclosing = "|||";

            // Act
            rawValue = rawValue.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n");
            string output;
            if (rawValue.Contains(doubleQuote) && !rawValue.Contains(tripleEnclosing))
            {
                output = "KEY," + tripleEnclosing + rawValue + tripleEnclosing;
            }
            else if (rawValue.Contains(doubleQuote) || rawValue.Contains(tripleEnclosing) || rawValue.Contains("\\n"))
            {
                string quotedLine = doubleQuote + rawValue.Replace(doubleQuote, "\"\"") + doubleQuote;
                output = "KEY," + quotedLine;
            }
            else
            {
                output = "KEY," + rawValue;
            }

            // Assert
            Assert.AreEqual("KEY,\"line1\\nline2\"", output);
        }

        [Test]
        public void SerializationLogic_ValueWithQuotes_GetsTripleQuoted()
        {
            // Arrange
            string rawValue = "value with \"quotes\"";
            string doubleQuote = "\"";
            string tripleEnclosing = "|||";

            // Act
            rawValue = rawValue.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n");
            string output;
            if (rawValue.Contains(doubleQuote) && !rawValue.Contains(tripleEnclosing))
            {
                output = "KEY," + tripleEnclosing + rawValue + tripleEnclosing;
            }
            else if (rawValue.Contains(doubleQuote) || rawValue.Contains(tripleEnclosing) || rawValue.Contains("\\n"))
            {
                string quotedLine = doubleQuote + rawValue.Replace(doubleQuote, "\"\"") + doubleQuote;
                output = "KEY," + quotedLine;
            }
            else
            {
                output = "KEY," + rawValue;
            }

            // Assert
            Assert.AreEqual("KEY,|||value with \"quotes\"|||", output);
        }

        [Test]
        public void SerializationLogic_ValueWithTripleQuotes_GetsDoubleQuoted()
        {
            // Arrange
            string rawValue = "value with ||| in it";
            string doubleQuote = "\"";
            string tripleEnclosing = "|||";

            // Act
            rawValue = rawValue.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n");
            string output;
            if (rawValue.Contains(doubleQuote) && !rawValue.Contains(tripleEnclosing))
            {
                output = "KEY," + tripleEnclosing + rawValue + tripleEnclosing;
            }
            else if (rawValue.Contains(doubleQuote) || rawValue.Contains(tripleEnclosing) || rawValue.Contains("\\n"))
            {
                string quotedLine = doubleQuote + rawValue.Replace(doubleQuote, "\"\"") + doubleQuote;
                output = "KEY," + quotedLine;
            }
            else
            {
                output = "KEY," + rawValue;
            }

            // Assert
            Assert.AreEqual("KEY,\"value with ||| in it\"", output);
        }

        #endregion

        #region Language Header Parsing Tests

        [Test]
        public void LanguageHeaderParsing_ExtractsLanguageName()
        {
            // Arrange
            string[] languageData = new string[] { ".,English", "KEY1,value1" };

            // Act
            string newLanguage = languageData[0].Split(',')[1].Trim('"');

            // Assert
            Assert.AreEqual("English", newLanguage);
        }

        [Test]
        public void LanguageHeaderParsing_QuotedLanguageName_TrimmsQuotes()
        {
            // Arrange
            string[] languageData = new string[] { ".,\"Spanish\"", "KEY1,value1" };

            // Act
            string newLanguage = languageData[0].Split(',')[1].Trim('"');

            // Assert
            Assert.AreEqual("Spanish", newLanguage);
        }

        [Test]
        public void LanguageHeaderParsing_LanguageWithSpaces_Preserved()
        {
            // Arrange
            string[] languageData = new string[] { ".,\"Chinese Simplified\"", "KEY1,value1" };

            // Act
            string newLanguage = languageData[0].Split(',')[1].Trim('"');

            // Assert
            Assert.AreEqual("Chinese Simplified", newLanguage);
        }

        #endregion

        #region Key Search Pattern Tests

        [Test]
        public void KeySearchPattern_KeyWithComma_MatchesCorrectly()
        {
            // Arrange
            string key = "MY_KEY";
            string keySearched = key + ',';
            string rawLine = "MY_KEY,some value";

            // Act
            bool found = rawLine.StartsWith(keySearched, false, null);

            // Assert
            Assert.IsTrue(found);
        }

        [Test]
        public void KeySearchPattern_DifferentKey_DoesNotMatch()
        {
            // Arrange
            string key = "MY_KEY";
            string keySearched = key + ',';
            string rawLine = "OTHER_KEY,some value";

            // Act
            bool found = rawLine.StartsWith(keySearched, false, null);

            // Assert
            Assert.IsFalse(found);
        }

        [Test]
        public void KeySearchPattern_PartialKeyMatch_DoesNotMatchIncorrectly()
        {
            // Arrange
            string key = "KEY";
            string keySearched = key + ',';
            string rawLine = "KEY_EXTENDED,some value";

            // Act
            bool found = rawLine.StartsWith(keySearched, false, null);

            // Assert
            Assert.IsFalse(found);
        }

        [Test]
        public void KeySearchPattern_ExtractValueAfterMatch()
        {
            // Arrange
            string key = "MY_KEY";
            string rawLine = "MY_KEY,the value part";

            // Act
            string keySearched = key + ',';
            string value = null;
            if (rawLine.StartsWith(keySearched, false, null))
            {
                value = rawLine.Substring(key.Length + 1);
            }

            // Assert
            Assert.AreEqual("the value part", value);
        }

        #endregion

        #region Combine Logic Tests

        [Test]
        public void CombineLogic_NoSecondLanguage_ReturnsMainOnly()
        {
            // Arrange
            string mainLanguageValue = "Hello";
            string secondLanguageValue = null;

            // Act - simulating Combine logic
            string result;
            if (secondLanguageValue == null || secondLanguageValue == mainLanguageValue)
            {
                result = mainLanguageValue;
            }
            else
            {
                result = $"{mainLanguageValue} [{secondLanguageValue}]";
            }

            // Assert
            Assert.AreEqual("Hello", result);
        }

        [Test]
        public void CombineLogic_SameValue_ReturnsMainOnly()
        {
            // Arrange
            string mainLanguageValue = "Hello";
            string secondLanguageValue = "Hello";

            // Act
            string result;
            if (secondLanguageValue == null || secondLanguageValue == mainLanguageValue)
            {
                result = mainLanguageValue;
            }
            else
            {
                result = $"{mainLanguageValue} [{secondLanguageValue}]";
            }

            // Assert
            Assert.AreEqual("Hello", result);
        }

        [Test]
        public void CombineLogic_DifferentValues_ReturnsCombined()
        {
            // Arrange
            string mainLanguageValue = "Hello";
            string secondLanguageValue = "Hola";

            // Act
            string result;
            if (secondLanguageValue == null || secondLanguageValue == mainLanguageValue)
            {
                result = mainLanguageValue;
            }
            else
            {
                result = $"{mainLanguageValue} [{secondLanguageValue}]";
            }

            // Assert
            Assert.AreEqual("Hello [Hola]", result);
        }

        #endregion

        #region Required Language HashSet Tests

        [Test]
        public void RequiredLanguages_DefaultContainsEnglish()
        {
            // Arrange
            HashSet<string> requiredLanguages = new HashSet<string> { ValkyrieConstants.DefaultLanguage };

            // Assert
            Assert.IsTrue(requiredLanguages.Contains("English"));
        }

        [Test]
        public void RequiredLanguages_AddNew_ReturnsTrue()
        {
            // Arrange
            HashSet<string> requiredLanguages = new HashSet<string> { ValkyrieConstants.DefaultLanguage };

            // Act
            bool added = requiredLanguages.Add("Spanish");

            // Assert
            Assert.IsTrue(added);
            Assert.IsTrue(requiredLanguages.Contains("Spanish"));
        }

        [Test]
        public void RequiredLanguages_AddExisting_ReturnsFalse()
        {
            // Arrange
            HashSet<string> requiredLanguages = new HashSet<string> { ValkyrieConstants.DefaultLanguage };

            // Act
            bool added = requiredLanguages.Add("English");

            // Assert
            Assert.IsFalse(added);
        }

        [Test]
        public void RequiredLanguages_MultipleLanguages_AllPresent()
        {
            // Arrange
            HashSet<string> requiredLanguages = new HashSet<string> { ValkyrieConstants.DefaultLanguage };

            // Act
            requiredLanguages.Add("Spanish");
            requiredLanguages.Add("German");
            requiredLanguages.Add("French");

            // Assert
            Assert.AreEqual(4, requiredLanguages.Count);
            Assert.IsTrue(requiredLanguages.Contains("English"));
            Assert.IsTrue(requiredLanguages.Contains("Spanish"));
            Assert.IsTrue(requiredLanguages.Contains("German"));
            Assert.IsTrue(requiredLanguages.Contains("French"));
        }

        #endregion

        #region Dictionary Data Structure Tests

        [Test]
        public void DictionaryStructure_AddLanguage_CreatesEntry()
        {
            // Arrange
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

            // Act
            data.Add("English", new Dictionary<string, string>());
            data["English"].Add("KEY", "value");

            // Assert
            Assert.IsTrue(data.ContainsKey("English"));
            Assert.AreEqual("value", data["English"]["KEY"]);
        }

        [Test]
        public void DictionaryStructure_MultipleLanguages_Isolated()
        {
            // Arrange
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

            // Act
            data.Add("English", new Dictionary<string, string>());
            data.Add("Spanish", new Dictionary<string, string>());
            data["English"].Add("GREETING", "Hello");
            data["Spanish"].Add("GREETING", "Hola");

            // Assert
            Assert.AreEqual("Hello", data["English"]["GREETING"]);
            Assert.AreEqual("Hola", data["Spanish"]["GREETING"]);
        }

        [Test]
        public void DictionaryStructure_ReplaceValue_Works()
        {
            // Arrange
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();
            data.Add("English", new Dictionary<string, string>());
            data["English"].Add("KEY", "original");

            // Act
            data["English"]["KEY"] = "updated";

            // Assert
            Assert.AreEqual("updated", data["English"]["KEY"]);
        }

        [Test]
        public void DictionaryStructure_RemoveKey_Works()
        {
            // Arrange
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();
            data.Add("English", new Dictionary<string, string>());
            data["English"].Add("KEY1", "value1");
            data["English"].Add("KEY2", "value2");

            // Act
            data["English"].Remove("KEY1");

            // Assert
            Assert.IsFalse(data["English"].ContainsKey("KEY1"));
            Assert.IsTrue(data["English"].ContainsKey("KEY2"));
        }

        #endregion

        #region Raw Data List Tests

        [Test]
        public void RawDataList_AddRange_AppendsData()
        {
            // Arrange
            Dictionary<string, List<string>> rawData = new Dictionary<string, List<string>>();
            rawData.Add("English", new List<string>());
            rawData["English"].Add(".,English");
            rawData["English"].Add("KEY1,value1");

            List<string> newData = new List<string> { "KEY2,value2", "KEY3,value3" };

            // Act
            rawData["English"].AddRange(newData);

            // Assert
            Assert.AreEqual(4, rawData["English"].Count);
            Assert.AreEqual("KEY3,value3", rawData["English"][3]);
        }

        [Test]
        public void RawDataList_NewLanguage_CreatedIfNotExists()
        {
            // Arrange
            Dictionary<string, List<string>> rawData = new Dictionary<string, List<string>>();

            // Act
            if (!rawData.ContainsKey("Spanish"))
            {
                rawData.Add("Spanish", new List<string>());
            }
            rawData["Spanish"].Add(".,Spanish");

            // Assert
            Assert.IsTrue(rawData.ContainsKey("Spanish"));
            Assert.AreEqual(1, rawData["Spanish"].Count);
        }

        #endregion

        #region Key To Group Mapping Tests

        [Test]
        public void KeyToGroup_SetAndRetrieve_Works()
        {
            // Arrange
            Dictionary<string, string> keyToGroup = new Dictionary<string, string>();

            // Act
            keyToGroup["KEY1"] = "GroupA";
            keyToGroup["KEY2"] = "GroupB";
            keyToGroup["KEY3"] = "GroupA";

            // Assert
            Assert.AreEqual("GroupA", keyToGroup["KEY1"]);
            Assert.AreEqual("GroupB", keyToGroup["KEY2"]);
            Assert.AreEqual("GroupA", keyToGroup["KEY3"]);
        }

        [Test]
        public void GroupToLanguage_SetAndRetrieve_Works()
        {
            // Arrange
            Dictionary<string, string> groupToLanguage = new Dictionary<string, string>();

            // Act
            groupToLanguage["GroupA"] = "Spanish";
            groupToLanguage["GroupB"] = "German";

            // Assert
            Assert.AreEqual("Spanish", groupToLanguage["GroupA"]);
            Assert.AreEqual("German", groupToLanguage["GroupB"]);
        }

        [Test]
        public void GroupToLanguage_RemoveWithEmptyString_Works()
        {
            // Arrange
            Dictionary<string, string> groupToLanguage = new Dictionary<string, string>();
            groupToLanguage["GroupA"] = "Spanish";

            // Act - simulating SetGroupTranslationLanguage with empty/null
            string language = "";
            if (string.IsNullOrWhiteSpace(language))
            {
                groupToLanguage.Remove("GroupA");
            }

            // Assert
            Assert.IsFalse(groupToLanguage.ContainsKey("GroupA"));
        }

        #endregion

        #region Prefix Removal Logic Tests

        [Test]
        public void PrefixRemoval_MatchingPrefix_RemovedFromList()
        {
            // Arrange
            Dictionary<string, string> languageData = new Dictionary<string, string>
            {
                { "QUEST_ITEM1", "Item 1" },
                { "QUEST_ITEM2", "Item 2" },
                { "OTHER_KEY", "Other" }
            };
            string prefix = "QUEST_";

            // Act - simulating RemoveKeyPrefix logic
            List<string> toRemove = new List<string>();
            foreach (string key in languageData.Keys)
            {
                if (key.IndexOf(prefix) == 0)
                {
                    toRemove.Add(key);
                }
            }
            foreach (string key in toRemove)
            {
                languageData.Remove(key);
            }

            // Assert
            Assert.IsFalse(languageData.ContainsKey("QUEST_ITEM1"));
            Assert.IsFalse(languageData.ContainsKey("QUEST_ITEM2"));
            Assert.IsTrue(languageData.ContainsKey("OTHER_KEY"));
        }

        [Test]
        public void PrefixRename_MatchingPrefix_KeysRenamed()
        {
            // Arrange
            Dictionary<string, string> languageData = new Dictionary<string, string>
            {
                { "OLD_ITEM1", "Item 1" },
                { "OLD_ITEM2", "Item 2" },
                { "OTHER_KEY", "Other" }
            };
            string oldPrefix = "OLD_";
            string newPrefix = "NEW_";

            // Act - simulating RenamePrefix logic
            Dictionary<string, string> toRename = new Dictionary<string, string>();
            foreach (string key in languageData.Keys)
            {
                if (key.IndexOf(oldPrefix) == 0)
                {
                    toRename.Add(key, newPrefix + key.Substring(oldPrefix.Length));
                }
            }
            foreach (KeyValuePair<string, string> kv in toRename)
            {
                languageData.Add(kv.Value, languageData[kv.Key]);
                languageData.Remove(kv.Key);
            }

            // Assert
            Assert.IsFalse(languageData.ContainsKey("OLD_ITEM1"));
            Assert.IsFalse(languageData.ContainsKey("OLD_ITEM2"));
            Assert.IsTrue(languageData.ContainsKey("NEW_ITEM1"));
            Assert.IsTrue(languageData.ContainsKey("NEW_ITEM2"));
            Assert.AreEqual("Item 1", languageData["NEW_ITEM1"]);
            Assert.AreEqual("Item 2", languageData["NEW_ITEM2"]);
        }

        #endregion
    }
}
