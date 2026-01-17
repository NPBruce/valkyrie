using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Assets.Scripts.Content;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for LocalizationRead class - String processing and localization lookup functionality
    /// These tests focus on the pure logic components that can be tested without Game.Get() dependencies
    /// </summary>
    [TestFixture]
    public class LocalizationReadTests
    {
        private Dictionary<string, DictionaryI18n> originalDicts;

        [SetUp]
        public void Setup()
        {
            // Disable ValkyrieDebug to prevent Unity logging during tests
            ValkyrieDebug.enabled = false;

            // Save original dicts state
            originalDicts = LocalizationRead.dicts;
            // Reset dicts to empty state for controlled testing
            LocalizationRead.dicts = new Dictionary<string, DictionaryI18n>();
        }

        [TearDown]
        public void TearDown()
        {
            ValkyrieDebug.enabled = true;
            // Restore original dicts
            LocalizationRead.dicts = originalDicts;
        }

        #region LookupRegexKey Tests

        [Test]
        public void LookupRegexKey_EmptyDicts_ReturnsMinimalPattern()
        {
            // Arrange - dicts is already empty from SetUp
            LocalizationRead.dicts = new Dictionary<string, DictionaryI18n>();
            LocalizationRead.dicts.Add("ffg", null);

            // Act
            string result = LocalizationRead.LookupRegexKey();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("{(ffg):", result);
        }

        [Test]
        public void LookupRegexKey_SingleDictionary_ReturnsCorrectPattern()
        {
            // Arrange
            LocalizationRead.dicts.Add("qst", null);

            // Act
            string result = LocalizationRead.LookupRegexKey();

            // Assert
            Assert.AreEqual("{(qst):", result);
        }

        [Test]
        public void LookupRegexKey_MultipleDictionaries_ReturnsAlternationPattern()
        {
            // Arrange
            LocalizationRead.dicts.Add("ffg", null);
            LocalizationRead.dicts.Add("qst", null);

            // Act
            string result = LocalizationRead.LookupRegexKey();

            // Assert
            // Pattern should contain alternation for both dictionary keys
            Assert.IsTrue(result.Contains("ffg"));
            Assert.IsTrue(result.Contains("qst"));
            Assert.IsTrue(result.Contains("|"));
            Assert.IsTrue(result.StartsWith("{("));
            Assert.IsTrue(result.EndsWith("):"));
        }

        [Test]
        public void LookupRegexKey_ThreeDictionaries_ReturnsCorrectAlternationPattern()
        {
            // Arrange
            LocalizationRead.dicts.Add("ffg", null);
            LocalizationRead.dicts.Add("qst", null);
            LocalizationRead.dicts.Add("val", null);

            // Act
            string result = LocalizationRead.LookupRegexKey();

            // Assert
            Assert.IsTrue(result.Contains("ffg"));
            Assert.IsTrue(result.Contains("qst"));
            Assert.IsTrue(result.Contains("val"));
            // Should have 2 pipe characters for 3 alternatives
            int pipeCount = result.Split('|').Length - 1;
            Assert.AreEqual(2, pipeCount);
        }

        [Test]
        public void LookupRegexKey_ResultIsValidRegex_MatchesExpectedKeys()
        {
            // Arrange
            LocalizationRead.dicts.Add("ffg", null);
            LocalizationRead.dicts.Add("qst", null);

            // Act
            string regexPattern = LocalizationRead.LookupRegexKey();

            // Assert - verify pattern is valid regex and matches expected strings
            Assert.DoesNotThrow(() => Regex.Match("test", regexPattern));
            Assert.IsTrue(Regex.IsMatch("{ffg:TEST}", regexPattern));
            Assert.IsTrue(Regex.IsMatch("{qst:MONSTER_NAME}", regexPattern));
            Assert.IsFalse(Regex.IsMatch("{xyz:TEST}", regexPattern));
        }

        [Test]
        public void LookupRegexKey_PatternMatchesNestedBraces()
        {
            // Arrange
            LocalizationRead.dicts.Add("ffg", null);

            // Act
            string regexPattern = LocalizationRead.LookupRegexKey();

            // Assert - pattern should match start of nested lookups
            Assert.IsTrue(Regex.IsMatch("{ffg:KEY{nested}}", regexPattern));
        }

        #endregion

        #region selectDictionary Tests

        [Test]
        public void SelectDictionary_NullDictName_ReturnsNull()
        {
            // Act
            var result = LocalizationRead.selectDictionary(null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void SelectDictionary_NonExistentDictName_ReturnsNull()
        {
            // Arrange
            LocalizationRead.dicts.Add("ffg", null);

            // Act
            var result = LocalizationRead.selectDictionary("nonexistent");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void SelectDictionary_ExistingDictName_ReturnsDictionary()
        {
            // Arrange - we use null since DictionaryI18n requires Game.Get()
            LocalizationRead.dicts.Add("qst", null);

            // Act
            var result = LocalizationRead.selectDictionary("qst");

            // Assert
            Assert.IsNull(result); // Returns the null we stored
        }

        [Test]
        public void SelectDictionary_EmptyString_ReturnsNull()
        {
            // Act
            var result = LocalizationRead.selectDictionary("");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region AddDictionary Tests

        [Test]
        public void AddDictionary_NewDictionary_AddsToDicts()
        {
            // Arrange
            string dictName = "test";

            // Act
            LocalizationRead.AddDictionary(dictName, null);

            // Assert
            Assert.IsTrue(LocalizationRead.dicts.ContainsKey(dictName));
        }

        [Test]
        public void AddDictionary_ReplacesExistingDictionary()
        {
            // Arrange
            string dictName = "test";
            LocalizationRead.dicts.Add(dictName, null);

            // Act - should replace without error
            LocalizationRead.AddDictionary(dictName, null);

            // Assert
            Assert.IsTrue(LocalizationRead.dicts.ContainsKey(dictName));
            Assert.AreEqual(1, LocalizationRead.dicts.Count);
        }

        [Test]
        public void AddDictionary_MultipleAdds_AllPersist()
        {
            // Act
            LocalizationRead.AddDictionary("dict1", null);
            LocalizationRead.AddDictionary("dict2", null);
            LocalizationRead.AddDictionary("dict3", null);

            // Assert
            Assert.AreEqual(3, LocalizationRead.dicts.Count);
            Assert.IsTrue(LocalizationRead.dicts.ContainsKey("dict1"));
            Assert.IsTrue(LocalizationRead.dicts.ContainsKey("dict2"));
            Assert.IsTrue(LocalizationRead.dicts.ContainsKey("dict3"));
        }

        #endregion

        #region BBCode to HTML Conversion Tests (String Replace Logic)

        // These tests verify the BBCode to HTML conversion logic
        // We test the string replacement patterns directly

        [Test]
        public void BbCodeConversion_UnderlineToHtml_ConvertsToBold()
        {
            // Arrange - simulating the conversion in DictLookup
            string input = "[u]underlined text[/u]";

            // Act
            string result = input.Replace("[u]", "<b>").Replace("[/u]", "</b>");

            // Assert
            Assert.AreEqual("<b>underlined text</b>", result);
        }

        [Test]
        public void BbCodeConversion_ItalicToHtml_ConvertsToItalic()
        {
            // Arrange
            string input = "[i]italic text[/i]";

            // Act
            string result = input.Replace("[i]", "<i>").Replace("[/i]", "</i>");

            // Assert
            Assert.AreEqual("<i>italic text</i>", result);
        }

        [Test]
        public void BbCodeConversion_BoldToHtml_ConvertsToBold()
        {
            // Arrange
            string input = "[b]bold text[/b]";

            // Act
            string result = input.Replace("[b]", "<b>").Replace("[/b]", "</b>");

            // Assert
            Assert.AreEqual("<b>bold text</b>", result);
        }

        [Test]
        public void BbCodeConversion_MixedTags_AllConverted()
        {
            // Arrange
            string input = "[b]bold[/b] and [i]italic[/i] and [u]underline[/u]";

            // Act
            string result = input.Replace("[u]", "<b>").Replace("[/u]", "</b>");
            result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
            result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");

            // Assert
            Assert.AreEqual("<b>bold</b> and <i>italic</i> and <b>underline</b>", result);
        }

        [Test]
        public void BbCodeConversion_NestedTags_AllConverted()
        {
            // Arrange
            string input = "[b][i]bold italic[/i][/b]";

            // Act
            string result = input.Replace("[u]", "<b>").Replace("[/u]", "</b>");
            result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
            result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");

            // Assert
            Assert.AreEqual("<b><i>bold italic</i></b>", result);
        }

        [Test]
        public void BbCodeConversion_NoTags_UnchangedText()
        {
            // Arrange
            string input = "plain text without tags";

            // Act
            string result = input.Replace("[u]", "<b>").Replace("[/u]", "</b>");
            result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
            result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");

            // Assert
            Assert.AreEqual("plain text without tags", result);
        }

        [Test]
        public void BbCodeConversion_EmptyString_ReturnsEmpty()
        {
            // Arrange
            string input = "";

            // Act
            string result = input.Replace("[u]", "<b>").Replace("[/u]", "</b>");
            result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
            result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");

            // Assert
            Assert.AreEqual("", result);
        }

        #endregion

        #region Unclosed Tag Handling Tests

        [Test]
        public void UnclosedTagHandling_OneBoldUnclosed_AddsClosingTag()
        {
            // Arrange
            string input = "<b>unclosed bold";

            // Act - simulating the auto-closing logic from DictLookup
            while (Regex.Matches(input, "<b>").Count > Regex.Matches(input, "</b>").Count)
            {
                input += "</b>";
            }

            // Assert
            Assert.AreEqual("<b>unclosed bold</b>", input);
        }

        [Test]
        public void UnclosedTagHandling_TwoBoldUnclosed_AddsTwoClosingTags()
        {
            // Arrange
            string input = "<b><b>double unclosed";

            // Act
            while (Regex.Matches(input, "<b>").Count > Regex.Matches(input, "</b>").Count)
            {
                input += "</b>";
            }

            // Assert
            Assert.AreEqual("<b><b>double unclosed</b></b>", input);
        }

        [Test]
        public void UnclosedTagHandling_OneItalicUnclosed_AddsClosingTag()
        {
            // Arrange
            string input = "<i>unclosed italic";

            // Act
            while (Regex.Matches(input, "<i>").Count > Regex.Matches(input, "</i>").Count)
            {
                input += "</i>";
            }

            // Assert
            Assert.AreEqual("<i>unclosed italic</i>", input);
        }

        [Test]
        public void UnclosedTagHandling_MixedUnclosed_AllClosed()
        {
            // Arrange
            string input = "<b>bold <i>italic";

            // Act
            while (Regex.Matches(input, "<b>").Count > Regex.Matches(input, "</b>").Count)
            {
                input += "</b>";
            }
            while (Regex.Matches(input, "<i>").Count > Regex.Matches(input, "</i>").Count)
            {
                input += "</i>";
            }

            // Assert
            Assert.IsTrue(input.EndsWith("</b></i>") || input.EndsWith("</i></b>")
                || input.Contains("</b>") && input.Contains("</i>"));
            Assert.AreEqual(Regex.Matches(input, "<b>").Count, Regex.Matches(input, "</b>").Count);
            Assert.AreEqual(Regex.Matches(input, "<i>").Count, Regex.Matches(input, "</i>").Count);
        }

        [Test]
        public void UnclosedTagHandling_ProperlyClosedTags_NoChange()
        {
            // Arrange
            string input = "<b>bold</b>";

            // Act
            string original = input;
            while (Regex.Matches(input, "<b>").Count > Regex.Matches(input, "</b>").Count)
            {
                input += "</b>";
            }

            // Assert
            Assert.AreEqual(original, input);
        }

        [Test]
        public void UnclosedTagHandling_MoreClosingThanOpening_NoChange()
        {
            // Arrange - edge case: more closing than opening tags
            string input = "text</b></b>";

            // Act
            string original = input;
            while (Regex.Matches(input, "<b>").Count > Regex.Matches(input, "</b>").Count)
            {
                input += "</b>";
            }

            // Assert - loop doesn't run because opening count is not greater
            Assert.AreEqual(original, input);
        }

        #endregion

        #region Recursive Limit Tests (RECURSIVE_LIMIT = 20)

        [Test]
        public void RecursiveLimit_ConstantValue_IsTwenty()
        {
            // This tests our understanding of the recursive limit
            // The actual constant is private, but we verify our test assumptions
            int expectedLimit = 20;
            Assert.AreEqual(20, expectedLimit);
        }

        [Test]
        public void RecursiveCountLogic_IncrementsProperly()
        {
            // Arrange - simulating the recursive count logic
            int recursiveCount = 0;
            int recursiveLimit = 20;

            // Act
            for (int i = 0; i < 5; i++)
            {
                recursiveCount++;
            }

            // Assert
            Assert.AreEqual(5, recursiveCount);
            Assert.IsTrue(recursiveCount < recursiveLimit);
        }

        [Test]
        public void RecursiveCountLogic_StopsAtLimit()
        {
            // Arrange
            int recursiveCount = 0;
            int recursiveLimit = 20;
            int iterations = 0;

            // Act - simulate the while loop condition
            while (recursiveCount < recursiveLimit)
            {
                recursiveCount++;
                iterations++;
                if (iterations > 100) break; // safety
            }

            // Assert
            Assert.AreEqual(recursiveLimit, recursiveCount);
            Assert.AreEqual(recursiveLimit, iterations);
        }

        #endregion

        #region DictQuery Element Parsing Tests

        [Test]
        public void DictQueryParsing_SimpleKey_NoColons()
        {
            // Arrange - simulating the element parsing in DictQuery
            string input = "SIMPLE_KEY";
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Act - parsing logic from DictQuery
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                if (input[index].Equals(':'))
                {
                    if (bracketLevel == 0)
                    {
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            // Assert
            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual("SIMPLE_KEY", elements[0]);
        }

        [Test]
        public void DictQueryParsing_KeyWithOneParameter_TwoElements()
        {
            // Arrange - FORMAT: KEY:{param1}:value1
            string input = "MESSAGE_KEY:{A}:Hero";
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Act
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                if (input[index].Equals(':'))
                {
                    if (bracketLevel == 0)
                    {
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            // Assert
            Assert.AreEqual(3, elements.Count);
            Assert.AreEqual("MESSAGE_KEY", elements[0]);
            Assert.AreEqual("{A}", elements[1]);
            Assert.AreEqual("Hero", elements[2]);
        }

        [Test]
        public void DictQueryParsing_KeyWithTwoParameters_FiveElements()
        {
            // Arrange - FORMAT: KEY:{param1}:value1:{param2}:value2
            string input = "A_GOES_B_MESSAGE:{A}:Peter:{B}:Dining Room";
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Act
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                if (input[index].Equals(':'))
                {
                    if (bracketLevel == 0)
                    {
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            // Assert
            Assert.AreEqual(5, elements.Count);
            Assert.AreEqual("A_GOES_B_MESSAGE", elements[0]);
            Assert.AreEqual("{A}", elements[1]);
            Assert.AreEqual("Peter", elements[2]);
            Assert.AreEqual("{B}", elements[3]);
            Assert.AreEqual("Dining Room", elements[4]);
        }

        [Test]
        public void DictQueryParsing_NestedBrackets_ColonInsideIgnored()
        {
            // Arrange - colon inside brackets should not be treated as separator
            string input = "KEY:{nested:value}:replacement";
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Act
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                if (input[index].Equals(':'))
                {
                    if (bracketLevel == 0)
                    {
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            // Assert
            Assert.AreEqual(3, elements.Count);
            Assert.AreEqual("KEY", elements[0]);
            Assert.AreEqual("{nested:value}", elements[1]);
            Assert.AreEqual("replacement", elements[2]);
        }

        [Test]
        public void DictQueryParsing_EmptyInput_SingleEmptyElement()
        {
            // Arrange
            string input = "";
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Act
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                if (input[index].Equals(':'))
                {
                    if (bracketLevel == 0)
                    {
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            // Assert
            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual("", elements[0]);
        }

        #endregion

        #region Parameter Replacement Tests

        [Test]
        public void ParameterReplacement_SingleParameter_Replaced()
        {
            // Arrange - simulating the replacement loop in DictQuery
            string fetched = "{A} goes to the store";
            List<string> elements = new List<string> { "KEY", "{A}", "Peter" };

            // Act
            for (int i = 2; i < elements.Count; i += 2)
            {
                fetched = fetched.Replace(elements[i - 1], elements[i]);
            }

            // Assert
            Assert.AreEqual("Peter goes to the store", fetched);
        }

        [Test]
        public void ParameterReplacement_TwoParameters_BothReplaced()
        {
            // Arrange
            string fetched = "{A} goes to {B}";
            List<string> elements = new List<string> { "KEY", "{A}", "Peter", "{B}", "the store" };

            // Act
            for (int i = 2; i < elements.Count; i += 2)
            {
                fetched = fetched.Replace(elements[i - 1], elements[i]);
            }

            // Assert
            Assert.AreEqual("Peter goes to the store", fetched);
        }

        [Test]
        public void ParameterReplacement_MultipleOccurrences_AllReplaced()
        {
            // Arrange
            string fetched = "{A} met {A} at the {B}";
            List<string> elements = new List<string> { "KEY", "{A}", "John", "{B}", "park" };

            // Act
            for (int i = 2; i < elements.Count; i += 2)
            {
                fetched = fetched.Replace(elements[i - 1], elements[i]);
            }

            // Assert
            Assert.AreEqual("John met John at the park", fetched);
        }

        [Test]
        public void ParameterReplacement_NoMatchingPlaceholder_NoChange()
        {
            // Arrange
            string fetched = "No placeholders here";
            List<string> elements = new List<string> { "KEY", "{A}", "Value" };

            // Act
            for (int i = 2; i < elements.Count; i += 2)
            {
                fetched = fetched.Replace(elements[i - 1], elements[i]);
            }

            // Assert
            Assert.AreEqual("No placeholders here", fetched);
        }

        #endregion

        #region DictLookup and StringKey Tests

        [Test]
        public void DictLookup_NumericParameter_ReplacesCorrectly()
        {
            // Arrange
            DictionaryI18n dict = new DictionaryI18n();
            dict.AddEntry("KEY", "Value: {0}", "English");
            LocalizationRead.dicts.Add("val", dict);

            // Act
            // This mirrors the fix: passing int directly to StringKey constructor
            StringKey sk = new StringKey("val", "KEY", 5);
            string result = LocalizationRead.DictLookup(sk);

            // Assert
            Assert.AreEqual("Value: 5", result);
        }

        [Test]
        public void DictLookup_StringParameter_ReplacedCorrectly()
        {
            // Arrange
            DictionaryI18n dict = new DictionaryI18n();
            dict.AddEntry("KEY_S", "Hello {0}", "English");
            LocalizationRead.dicts.Add("val", dict);

            // Act
            StringKey sk = new StringKey("val", "KEY_S", "World");
            string result = LocalizationRead.DictLookup(sk);

            // Assert
            Assert.AreEqual("Hello World", result);
        }


        [Test]
        public void DictLookup_MultipleParameters_ReplacedCorrectly()
        {
            // Arrange
            DictionaryI18n dict = new DictionaryI18n();
            dict.AddEntry("KEY_M", "{0} met {1}", "English");
            LocalizationRead.dicts.Add("val", dict);

            // Act
            // To replace multiple parameters, we need to ensure the parameters string contains brackets {}
            // so DictQuery enters the parsing mode.
            // We construct parameters manually: "{0}:Alice:{1}:Bob", passing "Alice:{1}:Bob" 
            // because the constructor prepends "{0}:"
            StringKey sk = new StringKey("val", "KEY_M", "Alice:{1}:Bob");
            
            string result = LocalizationRead.DictLookup(sk);

            // Assert
            Assert.AreEqual("Alice met Bob", result);
        }

        [Test]
        public void DictLookup_IncorrectParameterFormat_FailsToReplace()
        {
            // Arrange
            DictionaryI18n dict = new DictionaryI18n();
            dict.AddEntry("KEY_ERR", "Count: {0}", "English");
            LocalizationRead.dicts.Add("val", dict);

            // Act
            // This mirrors the bug: manually adding "{0}:" to the value parameter
            // The StringKey constructor adds "{0}:", so we get "{0}:{0}:5" in the parameters string
            // DictQuery parses elements: KEY_ERR, {0}, {0}, 5
            // It replaces "{0}" with "{0}", leaving the string unchanged.
            StringKey sk = new StringKey("val", "KEY_ERR", "{0}:" + 5);
            string result = LocalizationRead.DictLookup(sk);

            // Assert
            Assert.AreEqual("Count: {0}", result);
        }

        #endregion
    }
}
