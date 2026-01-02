using NUnit.Framework;
using System.Collections.Generic;
using ValkyrieTools;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for VarManager class - Quest variable management functionality
    /// </summary>
    [TestFixture]
    public class VarManagerTests
    {
        private VarManager varManager;

        [SetUp]
        public void Setup()
        {
            // Disable ValkyrieDebug to prevent Unity logging during tests
            ValkyrieDebug.enabled = false;
            varManager = new VarManager();
        }

        [TearDown]
        public void TearDown()
        {
            ValkyrieDebug.enabled = true;
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Default_CreatesEmptyVarsDictionary()
        {
            // Arrange & Act
            VarManager vm = new VarManager();

            // Assert
            Assert.IsNotNull(vm.vars);
            Assert.AreEqual(0, vm.vars.Count);
        }

        [Test]
        public void Constructor_WithDictionaryData_ParsesValuesCorrectly()
        {
            // Arrange
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "health", "100" },
                { "mana", "50.5" },
                { "strength", "25" }
            };

            // Act
            VarManager vm = new VarManager(data);

            // Assert
            Assert.AreEqual(100f, vm.vars["health"]);
            Assert.AreEqual(50.5f, vm.vars["mana"]);
            Assert.AreEqual(25f, vm.vars["strength"]);
        }

        [Test]
        public void Constructor_WithDictionaryData_HandlesInvalidFloatValues()
        {
            // Arrange
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "valid", "100" },
                { "invalid", "not_a_number" }
            };

            // Act
            VarManager vm = new VarManager(data);

            // Assert
            Assert.AreEqual(100f, vm.vars["valid"]);
            Assert.AreEqual(0f, vm.vars["invalid"]); // Should default to 0
        }

        [Test]
        public void Constructor_WithDictionaryData_StripsBackslashFromHashKeys()
        {
            // Arrange - Keys starting with \ followed by # are stored without the \
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "\\#specialVar", "42" }
            };

            // Act
            VarManager vm = new VarManager(data);

            // Assert
            Assert.IsTrue(vm.vars.ContainsKey("#specialVar"));
            Assert.AreEqual(42f, vm.vars["#specialVar"]);
        }

        #endregion

        #region GetValue Tests

        [Test]
        public void GetValue_ExistingVar_ReturnsValue()
        {
            // Arrange
            varManager.vars["testVar"] = 42.5f;

            // Act
            float result = varManager.GetValue("testVar");

            // Assert
            Assert.AreEqual(42.5f, result);
        }

        [Test]
        public void GetValue_NonExistingVar_ReturnsZero()
        {
            // Act
            float result = varManager.GetValue("nonExistent");

            // Assert
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void GetValue_NegativeValue_ReturnsCorrectValue()
        {
            // Arrange
            varManager.vars["negative"] = -15.75f;

            // Act
            float result = varManager.GetValue("negative");

            // Assert
            Assert.AreEqual(-15.75f, result);
        }

        #endregion

        #region GetPrefixVars Tests

        [Test]
        public void GetPrefixVars_MatchingPrefix_ReturnsFilteredDictionary()
        {
            // Arrange
            varManager.vars["monster_health"] = 100f;
            varManager.vars["monster_damage"] = 25f;
            varManager.vars["player_health"] = 200f;

            // Act
            Dictionary<string, float> result = varManager.GetPrefixVars("monster_");

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("monster_health"));
            Assert.IsTrue(result.ContainsKey("monster_damage"));
            Assert.IsFalse(result.ContainsKey("player_health"));
        }

        [Test]
        public void GetPrefixVars_NoMatchingPrefix_ReturnsEmptyDictionary()
        {
            // Arrange
            varManager.vars["monster_health"] = 100f;

            // Act
            Dictionary<string, float> result = varManager.GetPrefixVars("player_");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetPrefixVars_EmptyVars_ReturnsEmptyDictionary()
        {
            // Act
            Dictionary<string, float> result = varManager.GetPrefixVars("any_");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetPrefixVars_ExactMatchOnly_DoesNotReturnPartialMatch()
        {
            // Arrange
            varManager.vars["monster"] = 1f;
            varManager.vars["monster_health"] = 100f;

            // Act
            Dictionary<string, float> result = varManager.GetPrefixVars("monster_");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("monster_health"));
            Assert.IsFalse(result.ContainsKey("monster"));
        }

        #endregion

        #region TrimQuest Tests

        [Test]
        public void TrimQuest_KeepsPercentVars_RemovesOthers()
        {
            // Arrange
            varManager.vars["%persistent"] = 1f;
            varManager.vars["questVar"] = 2f;
            varManager.vars["anotherVar"] = 3f;

            // Act
            varManager.TrimQuest();

            // Assert
            Assert.IsTrue(varManager.vars.ContainsKey("%persistent"));
            Assert.IsFalse(varManager.vars.ContainsKey("questVar"));
            Assert.IsFalse(varManager.vars.ContainsKey("anotherVar"));
        }

        [Test]
        public void TrimQuest_KeepsDollarPercentVars_RemovesOthers()
        {
            // Arrange
            varManager.vars["$%special"] = 10f;
            varManager.vars["$normalDollar"] = 20f;
            varManager.vars["regular"] = 30f;

            // Act
            varManager.TrimQuest();

            // Assert
            Assert.IsTrue(varManager.vars.ContainsKey("$%special"));
            Assert.IsFalse(varManager.vars.ContainsKey("$normalDollar"));
            Assert.IsFalse(varManager.vars.ContainsKey("regular"));
        }

        [Test]
        public void TrimQuest_EmptyVars_RemainsEmpty()
        {
            // Act
            varManager.TrimQuest();

            // Assert
            Assert.AreEqual(0, varManager.vars.Count);
        }

        [Test]
        public void TrimQuest_MixedVars_KeepsOnlyPersistentTypes()
        {
            // Arrange
            varManager.vars["%save1"] = 1f;
            varManager.vars["%save2"] = 2f;
            varManager.vars["$%globalSave"] = 3f;
            varManager.vars["temp1"] = 4f;
            varManager.vars["$temp2"] = 5f;

            // Act
            varManager.TrimQuest();

            // Assert
            Assert.AreEqual(3, varManager.vars.Count);
            Assert.IsTrue(varManager.vars.ContainsKey("%save1"));
            Assert.IsTrue(varManager.vars.ContainsKey("%save2"));
            Assert.IsTrue(varManager.vars.ContainsKey("$%globalSave"));
        }

        #endregion

        #region Test(VarOperation) Comparison Tests

        [Test]
        public void Test_EqualOperator_ReturnsTrueWhenEqual()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", "==", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_EqualOperator_ReturnsFalseWhenNotEqual()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", "==", "50");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_NotEqualOperator_ReturnsTrueWhenDifferent()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", "!=", "50");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_NotEqualOperator_ReturnsFalseWhenEqual()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", "!=", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_GreaterThanOrEqualOperator_ReturnsTrueWhenGreater()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", ">=", "50");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_GreaterThanOrEqualOperator_ReturnsTrueWhenEqual()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", ">=", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_GreaterThanOrEqualOperator_ReturnsFalseWhenLess()
        {
            // Arrange
            varManager.vars["health"] = 50f;
            VarOperation op = CreateVarOperation("health", ">=", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_LessThanOrEqualOperator_ReturnsTrueWhenLess()
        {
            // Arrange
            varManager.vars["health"] = 50f;
            VarOperation op = CreateVarOperation("health", "<=", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_LessThanOrEqualOperator_ReturnsTrueWhenEqual()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", "<=", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_LessThanOrEqualOperator_ReturnsFalseWhenGreater()
        {
            // Arrange
            varManager.vars["health"] = 150f;
            VarOperation op = CreateVarOperation("health", "<=", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_GreaterThanOperator_ReturnsTrueWhenGreater()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", ">", "50");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_GreaterThanOperator_ReturnsFalseWhenEqual()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", ">", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_LessThanOperator_ReturnsTrueWhenLess()
        {
            // Arrange
            varManager.vars["health"] = 50f;
            VarOperation op = CreateVarOperation("health", "<", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_LessThanOperator_ReturnsFalseWhenEqual()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", "<", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_UnknownOperator_ReturnsFalse()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            VarOperation op = CreateVarOperation("health", "??", "100");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_NegativeValues_ComparesCorrectly()
        {
            // Arrange
            varManager.vars["temperature"] = -10f;
            VarOperation op = CreateVarOperation("temperature", "<", "0");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_DecimalValues_ComparesCorrectly()
        {
            // Arrange
            varManager.vars["progress"] = 0.75f;
            VarOperation op = CreateVarOperation("progress", ">=", "0.5");

            // Act
            bool result = varManager.Test(op);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Test(VarTests) Compound Tests

        [Test]
        public void Test_NullVarTests_ReturnsTrue()
        {
            // Act
            bool result = varManager.Test((VarTests)null);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_EmptyVarTests_ReturnsTrue()
        {
            // Arrange
            VarTests tests = new VarTests();

            // Act
            bool result = varManager.Test(tests);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_SingleTrueCondition_ReturnsTrue()
        {
            // Arrange
            varManager.vars["x"] = 10f;
            VarTests tests = new VarTests();
            tests.VarTestsComponents.Add(CreateVarOperation("x", "==", "10"));

            // Act
            bool result = varManager.Test(tests);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_SingleFalseCondition_ReturnsFalse()
        {
            // Arrange
            varManager.vars["x"] = 10f;
            VarTests tests = new VarTests();
            tests.VarTestsComponents.Add(CreateVarOperation("x", "==", "20"));

            // Act
            bool result = varManager.Test(tests);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_TwoConditionsWithAnd_BothTrue_ReturnsTrue()
        {
            // Arrange
            varManager.vars["x"] = 10f;
            varManager.vars["y"] = 20f;
            VarTests tests = new VarTests();
            tests.VarTestsComponents.Add(CreateVarOperation("x", "==", "10"));
            tests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));
            tests.VarTestsComponents.Add(CreateVarOperation("y", "==", "20"));

            // Act
            bool result = varManager.Test(tests);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_TwoConditionsWithAnd_OneFalse_ReturnsFalse()
        {
            // Arrange
            varManager.vars["x"] = 10f;
            varManager.vars["y"] = 20f;
            VarTests tests = new VarTests();
            tests.VarTestsComponents.Add(CreateVarOperation("x", "==", "10"));
            tests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));
            tests.VarTestsComponents.Add(CreateVarOperation("y", "==", "30"));

            // Act
            bool result = varManager.Test(tests);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_TwoConditionsWithOr_OneFalse_ReturnsTrue()
        {
            // Arrange
            varManager.vars["x"] = 10f;
            varManager.vars["y"] = 20f;
            VarTests tests = new VarTests();
            tests.VarTestsComponents.Add(CreateVarOperation("x", "==", "999")); // false
            tests.VarTestsComponents.Add(new VarTestsLogicalOperator("OR"));
            tests.VarTestsComponents.Add(CreateVarOperation("y", "==", "20")); // true

            // Act
            bool result = varManager.Test(tests);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_TwoConditionsWithOr_BothFalse_ReturnsFalse()
        {
            // Arrange
            varManager.vars["x"] = 10f;
            varManager.vars["y"] = 20f;
            VarTests tests = new VarTests();
            tests.VarTestsComponents.Add(CreateVarOperation("x", "==", "999")); // false
            tests.VarTestsComponents.Add(new VarTestsLogicalOperator("OR"));
            tests.VarTestsComponents.Add(CreateVarOperation("y", "==", "999")); // false

            // Act
            bool result = varManager.Test(tests);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region ToString Tests

        [Test]
        public void ToString_EmptyVars_ReturnsHeaderOnly()
        {
            // Act
            string result = varManager.ToString();

            // Assert
            Assert.IsTrue(result.Contains("[Vars]"));
        }

        [Test]
        public void ToString_WithVars_ContainsKeyValuePairs()
        {
            // Arrange
            varManager.vars["health"] = 100f;

            // Act
            string result = varManager.ToString();

            // Assert
            Assert.IsTrue(result.Contains("[Vars]"));
            Assert.IsTrue(result.Contains("health=100"));
        }

        [Test]
        public void ToString_ZeroValueVars_AreNotIncluded()
        {
            // Arrange
            varManager.vars["health"] = 100f;
            varManager.vars["zero"] = 0f;

            // Act
            string result = varManager.ToString();

            // Assert
            Assert.IsTrue(result.Contains("health=100"));
            Assert.IsFalse(result.Contains("zero="));
        }

        [Test]
        public void ToString_HashVars_AreEscapedWithBackslash()
        {
            // Arrange
            varManager.vars["#comment"] = 5f;

            // Act
            string result = varManager.ToString();

            // Assert
            Assert.IsTrue(result.Contains("\\#comment=5"));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a VarOperation with the specified parameters without using the string constructor
        /// which would call ValkyrieDebug on invalid input
        /// </summary>
        private VarOperation CreateVarOperation(string varName, string operation, string value)
        {
            VarOperation op = new VarOperation();
            op.var = varName;
            op.operation = operation;
            op.value = value;
            return op;
        }

        #endregion
    }
}
