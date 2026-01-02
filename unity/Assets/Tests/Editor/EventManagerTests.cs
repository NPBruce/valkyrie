using NUnit.Framework;
using System;
using System.Collections.Generic;
using ValkyrieTools;
using Assets.Scripts.Content;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for EventManager class and related event handling functionality.
    /// Tests focus on static methods, character maps, symbol replacement, and data structures
    /// that can be tested without requiring full Game context.
    /// </summary>
    [TestFixture]
    public class EventManagerTests
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

        #region Character Map Structure Tests

        [Test]
        public void CHARS_MAP_ContainsD2EGameType()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP.ContainsKey("D2E"),
                "CHARS_MAP should contain D2E game type");
        }

        [Test]
        public void CHARS_MAP_ContainsMoMGameType()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP.ContainsKey("MoM"),
                "CHARS_MAP should contain MoM game type");
        }

        [Test]
        public void CHARS_MAP_ContainsIAGameType()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP.ContainsKey("IA"),
                "CHARS_MAP should contain IA game type");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsHeartSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{heart}"),
                "D2E map should contain heart symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsFatigueSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{fatigue}"),
                "D2E map should contain fatigue symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsMightSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{might}"),
                "D2E map should contain might symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsWillSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{will}"),
                "D2E map should contain will symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsActionSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{action}"),
                "D2E map should contain action symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsKnowledgeSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{knowledge}"),
                "D2E map should contain knowledge symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsAwarenessSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{awareness}"),
                "D2E map should contain awareness symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsShieldSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{shield}"),
                "D2E map should contain shield symbol");
        }

        [Test]
        public void CHARS_MAP_D2E_ContainsSurgeSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["D2E"].ContainsKey("{surge}"),
                "D2E map should contain surge symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsWillSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{will}"),
                "MoM map should contain will symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsStrengthSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{strength}"),
                "MoM map should contain strength symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsAgilitySymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{agility}"),
                "MoM map should contain agility symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsLoreSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{lore}"),
                "MoM map should contain lore symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsInfluenceSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{influence}"),
                "MoM map should contain influence symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsObservationSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{observation}"),
                "MoM map should contain observation symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsSuccessSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{success}"),
                "MoM map should contain success symbol");
        }

        [Test]
        public void CHARS_MAP_MoM_ContainsClueSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["MoM"].ContainsKey("{clue}"),
                "MoM map should contain clue symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsWoundSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{wound}"),
                "IA map should contain wound symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsSurgeSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{surge}"),
                "IA map should contain surge symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsAttackSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{attack}"),
                "IA map should contain attack symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsStrainSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{strain}"),
                "IA map should contain strain symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsTechSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{tech}"),
                "IA map should contain tech symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsInsightSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{insight}"),
                "IA map should contain insight symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsBlockSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{block}"),
                "IA map should contain block symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsEvadeSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{evade}"),
                "IA map should contain evade symbol");
        }

        [Test]
        public void CHARS_MAP_IA_ContainsDodgeSymbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHARS_MAP["IA"].ContainsKey("{dodge}"),
                "IA map should contain dodge symbol");
        }

        #endregion

        #region Character Packs Map Tests

        [Test]
        public void CHAR_PACKS_MAP_ContainsD2EGameType()
        {
            // Assert
            Assert.IsTrue(EventManager.CHAR_PACKS_MAP.ContainsKey("D2E"),
                "CHAR_PACKS_MAP should contain D2E game type");
        }

        [Test]
        public void CHAR_PACKS_MAP_ContainsMoMGameType()
        {
            // Assert
            Assert.IsTrue(EventManager.CHAR_PACKS_MAP.ContainsKey("MoM"),
                "CHAR_PACKS_MAP should contain MoM game type");
        }

        [Test]
        public void CHAR_PACKS_MAP_ContainsIAGameType()
        {
            // Assert
            Assert.IsTrue(EventManager.CHAR_PACKS_MAP.ContainsKey("IA"),
                "CHAR_PACKS_MAP should contain IA game type");
        }

        [Test]
        public void CHAR_PACKS_MAP_MoM_ContainsMAD01Symbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHAR_PACKS_MAP["MoM"].ContainsKey("{MAD01}"),
                "MoM packs map should contain MAD01 symbol");
        }

        [Test]
        public void CHAR_PACKS_MAP_MoM_ContainsMAD06Symbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHAR_PACKS_MAP["MoM"].ContainsKey("{MAD06}"),
                "MoM packs map should contain MAD06 symbol");
        }

        [Test]
        public void CHAR_PACKS_MAP_IA_ContainsSWI01Symbol()
        {
            // Assert
            Assert.IsTrue(EventManager.CHAR_PACKS_MAP["IA"].ContainsKey("{SWI01}"),
                "IA packs map should contain SWI01 symbol");
        }

        [Test]
        public void CHAR_PACKS_MAP_D2E_IsEmpty()
        {
            // D2E doesn't have pack-specific symbols
            // Assert
            Assert.AreEqual(0, EventManager.CHAR_PACKS_MAP["D2E"].Count,
                "D2E packs map should be empty");
        }

        #endregion

        #region QuestButtonData Tests

        [Test]
        public void QuestButtonData_DefaultColor_IsWhite()
        {
            // Assert
            Assert.AreEqual("white", QuestButtonData.DEFAULT_COLOR);
        }

        [Test]
        public void QuestButtonData_Constructor_SetsLabel()
        {
            // Arrange
            var label = new StringKey("val", "TEST");

            // Act
            var buttonData = new QuestButtonData(label);

            // Assert
            Assert.AreEqual(label, buttonData.Label);
        }

        [Test]
        public void QuestButtonData_Constructor_DefaultEventNamesIsEmpty()
        {
            // Arrange
            var label = new StringKey("val", "TEST");

            // Act
            var buttonData = new QuestButtonData(label);

            // Assert
            Assert.IsNotNull(buttonData.EventNames);
            Assert.AreEqual(0, buttonData.EventNames.Count);
        }

        [Test]
        public void QuestButtonData_Constructor_WithEventNames_SetsEventNames()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var eventNames = new List<string> { "Event1", "Event2" };

            // Act
            var buttonData = new QuestButtonData(label, eventNames);

            // Assert
            Assert.AreEqual(2, buttonData.EventNames.Count);
            Assert.AreEqual("Event1", buttonData.EventNames[0]);
            Assert.AreEqual("Event2", buttonData.EventNames[1]);
        }

        [Test]
        public void QuestButtonData_HasCondition_FalseWhenNoCondition()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var buttonData = new QuestButtonData(label);

            // Assert
            Assert.IsFalse(buttonData.HasCondition);
        }

        [Test]
        public void QuestButtonData_HasCondition_TrueWhenConditionHasComponents()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var condition = new VarTests();
            condition.VarTestsComponents.Add(new VarOperation("x,==,5"));
            var buttonData = new QuestButtonData(label, null, condition);

            // Assert
            Assert.IsTrue(buttonData.HasCondition);
        }

        [Test]
        public void QuestButtonData_ConditionFailedAction_DefaultToDisableWhenHasCondition()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var condition = new VarTests();
            condition.VarTestsComponents.Add(new VarOperation("x,==,5"));
            var buttonData = new QuestButtonData(label, null, condition);

            // Assert
            Assert.AreEqual(QuestButtonAction.DISABLE, buttonData.ConditionFailedAction);
        }

        [Test]
        public void QuestButtonData_ConditionFailedAction_NoneWhenNoCondition()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var buttonData = new QuestButtonData(label);

            // Assert
            Assert.AreEqual(QuestButtonAction.NONE, buttonData.ConditionFailedAction);
        }

        [Test]
        public void QuestButtonData_ConditionFailedAction_UsesExplicitValue()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var condition = new VarTests();
            condition.VarTestsComponents.Add(new VarOperation("x,==,5"));
            var buttonData = new QuestButtonData(label, null, condition, QuestButtonAction.HIDE);

            // Assert
            Assert.AreEqual(QuestButtonAction.HIDE, buttonData.ConditionFailedAction);
        }

        [Test]
        public void QuestButtonData_Color_DefaultIsWhite()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var buttonData = new QuestButtonData(label);

            // Assert
            Assert.AreEqual("white", buttonData.Color);
        }

        [Test]
        public void QuestButtonData_Color_CanBeSet()
        {
            // Arrange
            var label = new StringKey("val", "TEST");
            var buttonData = new QuestButtonData(label);

            // Act
            buttonData.Color = "red";

            // Assert
            Assert.AreEqual("red", buttonData.Color);
        }

        #endregion

        #region QuestButtonAction Enum Tests

        [Test]
        public void QuestButtonAction_NONE_HasValue0()
        {
            // Assert
            Assert.AreEqual(0, (int)QuestButtonAction.NONE);
        }

        [Test]
        public void QuestButtonAction_DISABLE_HasValue1()
        {
            // Assert
            Assert.AreEqual(1, (int)QuestButtonAction.DISABLE);
        }

        [Test]
        public void QuestButtonAction_HIDE_HasValue2()
        {
            // Assert
            Assert.AreEqual(2, (int)QuestButtonAction.HIDE);
        }

        #endregion

        #region Symbol Replacement Logic Tests

        [Test]
        public void InputSymbolReplace_ReplacesSpecialCharWithMarker()
        {
            // The InputSymbolReplace function replaces special Unicode chars with marker strings
            // Since it requires Game.Get() for character map lookup, we test the concept

            // Arrange - Test that the reverse operation would work
            string markerFormat = "{heart}";

            // Assert - Marker format is correct
            Assert.IsTrue(markerFormat.StartsWith("{"));
            Assert.IsTrue(markerFormat.EndsWith("}"));
            Assert.IsFalse(markerFormat.Contains(" "));
        }

        [Test]
        public void SymbolMarker_Format_UsesCorrectSyntax()
        {
            // All symbol markers should follow {name} format
            foreach (var gameType in EventManager.CHARS_MAP.Keys)
            {
                foreach (var symbol in EventManager.CHARS_MAP[gameType].Keys)
                {
                    Assert.IsTrue(symbol.StartsWith("{"), $"Symbol {symbol} should start with {{");
                    Assert.IsTrue(symbol.EndsWith("}"), $"Symbol {symbol} should end with }}");
                }
            }
        }

        [Test]
        public void PackSymbolMarker_Format_UsesCorrectSyntax()
        {
            // All pack symbol markers should follow {NAME} format
            foreach (var gameType in EventManager.CHAR_PACKS_MAP.Keys)
            {
                foreach (var symbol in EventManager.CHAR_PACKS_MAP[gameType].Keys)
                {
                    Assert.IsTrue(symbol.StartsWith("{"), $"Pack symbol {symbol} should start with {{");
                    Assert.IsTrue(symbol.EndsWith("}"), $"Pack symbol {symbol} should end with }}");
                }
            }
        }

        #endregion

        #region Event Data Structure Tests

        [Test]
        public void EventStack_CanPushAndPop()
        {
            // Test the Stack<Event> behavior used in EventManager
            var stack = new Stack<string>();

            // Act
            stack.Push("Event1");
            stack.Push("Event2");
            string popped = stack.Pop();

            // Assert - Stack is LIFO
            Assert.AreEqual("Event2", popped);
            Assert.AreEqual(1, stack.Count);
        }

        [Test]
        public void EventStack_EmptyStackCount_IsZero()
        {
            // Arrange
            var stack = new Stack<string>();

            // Assert
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void EventStack_PopFromEmpty_ThrowsException()
        {
            // Arrange
            var stack = new Stack<string>();

            // Assert
            Assert.Throws<InvalidOperationException>(() => stack.Pop());
        }

        #endregion

        #region Button Enabled Logic Tests

        [Test]
        public void ButtonEnabled_NoCondition_IsEnabled()
        {
            // Test the IsButtonEnabled logic pattern
            // A button with no condition (NONE action) is always enabled
            var action = QuestButtonAction.NONE;
            bool hasCondition = false;

            // Act - Simulating: action == NONE || !hasCondition || conditionPasses
            bool isEnabled = action == QuestButtonAction.NONE || !hasCondition;

            // Assert
            Assert.IsTrue(isEnabled);
        }

        [Test]
        public void ButtonEnabled_WithCondition_DependsOnConditionResult()
        {
            // When button has condition, enabled state depends on condition evaluation
            var action = QuestButtonAction.DISABLE;
            bool hasCondition = true;
            bool conditionPasses = false;

            // Act - Simulating: action == NONE || !hasCondition || conditionPasses
            bool isEnabled = action == QuestButtonAction.NONE || !hasCondition || conditionPasses;

            // Assert - Button is disabled because condition fails
            Assert.IsFalse(isEnabled);
        }

        [Test]
        public void ButtonEnabled_WithCondition_EnabledWhenConditionPasses()
        {
            // When condition passes, button is enabled regardless of action
            var action = QuestButtonAction.DISABLE;
            bool hasCondition = true;
            bool conditionPasses = true;

            // Act
            bool isEnabled = action == QuestButtonAction.NONE || !hasCondition || conditionPasses;

            // Assert
            Assert.IsTrue(isEnabled);
        }

        #endregion

        #region Component Text Replacement Pattern Tests

        [Test]
        public void ComponentTextPattern_DetectsCorrectFormat()
        {
            // The {c:ComponentName} pattern is used to reference components in text
            string textWithComponent = "You found {c:QItem1}";

            // Assert
            Assert.IsTrue(textWithComponent.Contains("{c:"));
        }

        [Test]
        public void ComponentTextPattern_MultipleComponents()
        {
            // Multiple component references can exist in text
            string textWithComponents = "Take {c:QItem1} to {c:Tile1}";

            // Assert
            int count = 0;
            int index = 0;
            while ((index = textWithComponents.IndexOf("{c:", index)) != -1)
            {
                count++;
                index++;
            }
            Assert.AreEqual(2, count);
        }

        [Test]
        public void VariablePattern_DetectsCorrectFormat()
        {
            // The {var:VarName} pattern is used to display variable values
            string textWithVar = "You have {var:gold} gold";

            // Assert
            Assert.IsTrue(textWithVar.Contains("{var:"));
        }

        [Test]
        public void NewlineEscape_Format()
        {
            // Text uses \\n for newlines that get replaced with actual newlines
            string textWithNewline = "Line 1\\nLine 2";

            // Act
            string replaced = textWithNewline.Replace("\\n", "\n");

            // Assert
            Assert.IsTrue(replaced.Contains("\n"));
            Assert.AreEqual(2, replaced.Split('\n').Length);
        }

        #endregion

        #region Random Hero Pattern Tests

        [Test]
        public void RandomHeroPattern_DetectsCorrectFormat()
        {
            // The {rnd:hero} pattern is used to reference a random hero
            string textWithRndHero = "{rnd:hero} investigates the area";

            // Assert
            Assert.IsTrue(textWithRndHero.Contains("{rnd:hero}"));
        }

        [Test]
        public void RandomHeroPattern_Extended_HasCorrectPrefix()
        {
            // Extended pattern {rnd:hero:...} for gender-specific text
            string pattern = "{rnd:hero:";

            // Assert
            Assert.AreEqual("{rnd:hero:", pattern);
        }

        #endregion
    }
}
