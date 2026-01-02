using NUnit.Framework;
using System.Collections.Generic;
using ValkyrieTools;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for VarTests, VarTestsLogicalOperator, VarTestsParenthesis, and VarOperation classes.
    /// Tests cover parsing, parenthesis matching, movement operations, and component management.
    /// </summary>
    [TestFixture]
    public class VarTestsTests
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

        #region VarTests Constructor Tests

        [Test]
        public void Constructor_Default_CreatesEmptyComponentsList()
        {
            // Arrange & Act
            var varTests = new VarTests();

            // Assert
            Assert.IsNotNull(varTests.VarTestsComponents);
            Assert.AreEqual(0, varTests.VarTestsComponents.Count);
        }

        [Test]
        public void Constructor_WithList_AssignsComponentsList()
        {
            // Arrange
            var components = new List<VarTestsComponent>
            {
                new VarOperation("x,==,5")
            };

            // Act
            var varTests = new VarTests(components);

            // Assert
            Assert.AreSame(components, varTests.VarTestsComponents);
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
        }

        #endregion

        #region VarTests.Add Tests

        [Test]
        public void Add_StringWithVarOperation_ParsesAndAddsComponent()
        {
            // Arrange
            var varTests = new VarTests();

            // Act
            varTests.Add("VarOperation:x,==,5");

            // Assert
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarOperation", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        [Test]
        public void Add_StringWithLogicalOperator_ParsesAndAddsComponent()
        {
            // Arrange
            var varTests = new VarTests();

            // Act
            varTests.Add("VarTestsLogicalOperator:AND");

            // Assert
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarTestsLogicalOperator", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        [Test]
        public void Add_StringWithParenthesis_ParsesAndAddsComponent()
        {
            // Arrange
            var varTests = new VarTests();

            // Act
            varTests.Add("VarTestsParenthesis:(");

            // Assert
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarTestsParenthesis", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        [Test]
        public void Add_ParenthesisComponent_InsertsAtBeginning()
        {
            // Arrange
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            var parenthesis = new VarTestsParenthesis("(");

            // Act
            varTests.Add(parenthesis);

            // Assert
            Assert.AreEqual(2, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarTestsParenthesis", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        [Test]
        public void Add_NonParenthesisComponent_AppendsToEnd()
        {
            // Arrange
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            var operation = new VarOperation("x,==,5");

            // Act
            varTests.Add(operation);

            // Assert
            Assert.AreEqual(2, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarOperation", varTests.VarTestsComponents[1].GetClassVarTestsComponentType());
        }

        #endregion

        #region VarTests.FindClosingParenthesis Tests

        [Test]
        public void FindClosingParenthesis_SimpleCase_ReturnsCorrectIndex()
        {
            // Arrange: ( x==5 )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act
            int result = varTests.FindClosingParenthesis(0);

            // Assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public void FindClosingParenthesis_NestedParentheses_ReturnsOuterClosing()
        {
            // Arrange: ( ( x==5 ) )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act
            int result = varTests.FindClosingParenthesis(0);

            // Assert
            Assert.AreEqual(4, result);
        }

        [Test]
        public void FindClosingParenthesis_InnerParentheses_ReturnsInnerClosing()
        {
            // Arrange: ( ( x==5 ) )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act
            int result = varTests.FindClosingParenthesis(1);

            // Assert
            Assert.AreEqual(3, result);
        }

        [Test]
        public void FindClosingParenthesis_NoMatchingParenthesis_ReturnsMinusOne()
        {
            // Arrange: ( x==5 - missing closing
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));

            // Act
            int result = varTests.FindClosingParenthesis(0);

            // Assert
            Assert.AreEqual(-1, result);
        }

        #endregion

        #region VarTests.FindOpeningParenthesis Tests

        [Test]
        public void FindOpeningParenthesis_SimpleCase_ReturnsCorrectIndex()
        {
            // Arrange: ( x==5 )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act
            int result = varTests.FindOpeningParenthesis(2);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindOpeningParenthesis_NestedParentheses_ReturnsOuterOpening()
        {
            // Arrange: ( ( x==5 ) )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act
            int result = varTests.FindOpeningParenthesis(4);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindOpeningParenthesis_InnerParentheses_ReturnsInnerOpening()
        {
            // Arrange: ( ( x==5 ) )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act
            int result = varTests.FindOpeningParenthesis(3);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void FindOpeningParenthesis_NoMatchingParenthesis_ReturnsMinusOne()
        {
            // Arrange: x==5 ) - missing opening
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act
            int result = varTests.FindOpeningParenthesis(1);

            // Assert
            Assert.AreEqual(-1, result);
        }

        #endregion

        #region VarTests.FindNextValidPosition Tests

        [Test]
        public void FindNextValidPosition_LogicalOperator_ReturnsMinusOne()
        {
            // Arrange: x==5 AND y==3
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));
            varTests.VarTestsComponents.Add(new VarOperation("y,==,3"));

            // Act - LogicalOperator cannot be moved
            int result = varTests.FindNextValidPosition(1, true);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void FindNextValidPosition_VarOperationUp_FindsNextVarOperation()
        {
            // Arrange: x==5 AND y==3
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));
            varTests.VarTestsComponents.Add(new VarOperation("y,==,3"));

            // Act - Move VarOperation at index 2 up
            int result = varTests.FindNextValidPosition(2, true);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindNextValidPosition_VarOperationDown_FindsNextVarOperation()
        {
            // Arrange: x==5 AND y==3
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));
            varTests.VarTestsComponents.Add(new VarOperation("y,==,3"));

            // Act - Move VarOperation at index 0 down
            int result = varTests.FindNextValidPosition(0, false);

            // Assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public void FindNextValidPosition_VarOperationNoTarget_ReturnsMinusOne()
        {
            // Arrange: x==5 only
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));

            // Act - No other VarOperation to swap with
            int result = varTests.FindNextValidPosition(0, true);

            // Assert
            Assert.AreEqual(-1, result);
        }

        #endregion

        #region VarTests.Remove Tests

        [Test]
        public void Remove_SingleVarOperation_RemovesIt()
        {
            // Arrange
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));

            // Act
            varTests.Remove(0);

            // Assert
            Assert.AreEqual(0, varTests.VarTestsComponents.Count);
        }

        [Test]
        public void Remove_VarOperationWithPrecedingOperator_RemovesBoth()
        {
            // Arrange: x==5 AND y==3
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));
            varTests.VarTestsComponents.Add(new VarOperation("y,==,3"));

            // Act - Remove y==3 (index 2)
            varTests.Remove(2);

            // Assert - Should remove AND and y==3
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarOperation", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        [Test]
        public void Remove_VarOperationWithFollowingOperator_RemovesBoth()
        {
            // Arrange: x==5 AND y==3
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));
            varTests.VarTestsComponents.Add(new VarOperation("y,==,3"));

            // Act - Remove x==5 (index 0)
            varTests.Remove(0);

            // Assert - Should remove x==5 and AND
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarOperation", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        [Test]
        public void Remove_OpeningParenthesis_RemovesBoth()
        {
            // Arrange: ( x==5 )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act - Remove opening parenthesis
            varTests.Remove(0);

            // Assert - Both parentheses should be removed
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarOperation", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        [Test]
        public void Remove_ClosingParenthesis_RemovesBoth()
        {
            // Arrange: ( x==5 )
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarTestsParenthesis("("));
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsParenthesis(")"));

            // Act - Remove closing parenthesis
            varTests.Remove(2);

            // Assert - Both parentheses should be removed
            Assert.AreEqual(1, varTests.VarTestsComponents.Count);
            Assert.AreEqual("VarOperation", varTests.VarTestsComponents[0].GetClassVarTestsComponentType());
        }

        #endregion

        #region VarTests.ToString Tests

        [Test]
        public void ToString_EmptyList_ReturnsEmptyString()
        {
            // Arrange
            var varTests = new VarTests();

            // Act
            string result = varTests.ToString();

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void ToString_WithComponents_ReturnsFormattedString()
        {
            // Arrange
            var varTests = new VarTests();
            varTests.VarTestsComponents.Add(new VarOperation("x,==,5"));
            varTests.VarTestsComponents.Add(new VarTestsLogicalOperator("AND"));

            // Act
            string result = varTests.ToString();

            // Assert
            Assert.IsTrue(result.Contains("VarOperation:x,==,5"));
            Assert.IsTrue(result.Contains("VarTestsLogicalOperator:AND"));
        }

        #endregion

        #region VarTestsLogicalOperator Tests

        [Test]
        public void VarTestsLogicalOperator_DefaultConstructor_SetsAND()
        {
            // Arrange & Act
            var op = new VarTestsLogicalOperator();

            // Assert
            Assert.AreEqual("AND", op.op);
        }

        [Test]
        public void VarTestsLogicalOperator_ParameterizedConstructor_SetsValue()
        {
            // Arrange & Act
            var op = new VarTestsLogicalOperator("OR");

            // Assert
            Assert.AreEqual("OR", op.op);
        }

        [Test]
        public void VarTestsLogicalOperator_NextLogicalOperator_TogglesANDtoOR()
        {
            // Arrange
            var op = new VarTestsLogicalOperator("AND");

            // Act
            op.NextLogicalOperator();

            // Assert
            Assert.AreEqual("OR", op.op);
        }

        [Test]
        public void VarTestsLogicalOperator_NextLogicalOperator_TogglesORtoAND()
        {
            // Arrange
            var op = new VarTestsLogicalOperator("OR");

            // Act
            op.NextLogicalOperator();

            // Assert
            Assert.AreEqual("AND", op.op);
        }

        [Test]
        public void VarTestsLogicalOperator_GetVarTestsComponentType_ReturnsCorrectType()
        {
            // Arrange & Act
            string result = VarTestsLogicalOperator.GetVarTestsComponentType();

            // Assert
            Assert.AreEqual("VarTestsLogicalOperator", result);
        }

        [Test]
        public void VarTestsLogicalOperator_ToString_ReturnsOperator()
        {
            // Arrange
            var op = new VarTestsLogicalOperator("OR");

            // Act
            string result = op.ToString();

            // Assert
            Assert.AreEqual("OR", result);
        }

        #endregion

        #region VarTestsParenthesis Tests

        [Test]
        public void VarTestsParenthesis_ParameterizedConstructor_SetsValue()
        {
            // Arrange & Act
            var paren = new VarTestsParenthesis("(");

            // Assert
            Assert.AreEqual("(", paren.parenthesis);
        }

        [Test]
        public void VarTestsParenthesis_GetVarTestsComponentType_ReturnsCorrectType()
        {
            // Arrange & Act
            string result = VarTestsParenthesis.GetVarTestsComponentType();

            // Assert
            Assert.AreEqual("VarTestsParenthesis", result);
        }

        [Test]
        public void VarTestsParenthesis_ToString_ReturnsParenthesis()
        {
            // Arrange
            var paren = new VarTestsParenthesis(")");

            // Act
            string result = paren.ToString();

            // Assert
            Assert.AreEqual(")", result);
        }

        #endregion

        #region VarOperation Tests

        [Test]
        public void VarOperation_ParameterizedConstructor_ParsesCorrectly()
        {
            // Arrange & Act
            var op = new VarOperation("health,>=,10");

            // Assert
            Assert.AreEqual("health", op.var);
            Assert.AreEqual(">=", op.operation);
            Assert.AreEqual("10", op.value);
        }

        [Test]
        public void VarOperation_GetVarTestsComponentType_ReturnsCorrectType()
        {
            // Arrange & Act
            string result = VarOperation.GetVarTestsComponentType();

            // Assert
            Assert.AreEqual("VarOperation", result);
        }

        [Test]
        public void VarOperation_ToString_ReturnsFormattedString()
        {
            // Arrange
            var op = new VarOperation("health,>=,10");

            // Act
            string result = op.ToString();

            // Assert
            Assert.AreEqual("health,>=,10", result);
        }

        [Test]
        public void VarOperation_UpdateVarName_ConvertsFireVariable()
        {
            // Arrange & Act - #fire should be converted to $fire
            var op = new VarOperation("#fire,==,1");

            // Assert
            Assert.AreEqual("$fire", op.var);
        }

        [Test]
        public void VarOperation_UpdateVarName_ConvertsFireInValue()
        {
            // Arrange & Act - #fire in value should also be converted
            var op = new VarOperation("x,==,#fire");

            // Assert
            Assert.AreEqual("$fire", op.value);
        }

        #endregion
    }
}
