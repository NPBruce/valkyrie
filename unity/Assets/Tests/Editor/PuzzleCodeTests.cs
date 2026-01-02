using NUnit.Framework;
using System.Collections.Generic;
using ValkyrieTools;

namespace Valkyrie.Tests.Editor
{
    /// <summary>
    /// Unit tests for PuzzleCode, PuzzleCode.Answer, and PuzzleCode.CodeGuess classes.
    /// Tests cover puzzle creation, guess validation, and solution checking.
    /// </summary>
    [TestFixture]
    public class PuzzleCodeTests
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

        #region PuzzleCode.Answer Tests

        [Test]
        public void Answer_StringConstructor_ParsesSingleValue()
        {
            // Arrange & Act
            var answer = new PuzzleCode.Answer("5");

            // Assert
            Assert.AreEqual(1, answer.state.Count);
            Assert.AreEqual(5, answer.state[0]);
        }

        [Test]
        public void Answer_StringConstructor_ParsesMultipleValues()
        {
            // Arrange & Act
            var answer = new PuzzleCode.Answer("1 3 5 4");

            // Assert
            Assert.AreEqual(4, answer.state.Count);
            Assert.AreEqual(1, answer.state[0]);
            Assert.AreEqual(3, answer.state[1]);
            Assert.AreEqual(5, answer.state[2]);
            Assert.AreEqual(4, answer.state[3]);
        }

        [Test]
        public void Answer_StringConstructor_HandlesInvalidValue()
        {
            // Arrange & Act - invalid values should parse to 0
            var answer = new PuzzleCode.Answer("abc");

            // Assert
            Assert.AreEqual(1, answer.state.Count);
            Assert.AreEqual(0, answer.state[0]);
        }

        [Test]
        public void Answer_ToString_ReturnsSpaceSeparatedValues()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");

            // Act
            string result = answer.ToString();

            // Assert
            Assert.AreEqual("1 2 3", result);
        }

        #endregion

        #region PuzzleCode.CodeGuess Tests

        [Test]
        public void CodeGuess_ListConstructor_StoresGuess()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guessValues = new List<int> { 1, 2, 3 };

            // Act
            var guess = new PuzzleCode.CodeGuess(answer, guessValues);

            // Assert
            Assert.AreEqual(3, guess.guess.Count);
            Assert.AreEqual(1, guess.guess[0]);
            Assert.AreEqual(2, guess.guess[1]);
            Assert.AreEqual(3, guess.guess[2]);
        }

        [Test]
        public void CodeGuess_StringConstructor_ParsesGuess()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");

            // Act
            var guess = new PuzzleCode.CodeGuess(answer, "4 5 6");

            // Assert
            Assert.AreEqual(3, guess.guess.Count);
            Assert.AreEqual(4, guess.guess[0]);
            Assert.AreEqual(5, guess.guess[1]);
            Assert.AreEqual(6, guess.guess[2]);
        }

        [Test]
        public void CodeGuess_Correct_AllMatch_ReturnsTrue()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 1, 2, 3 });

            // Act
            bool result = guess.Correct();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CodeGuess_Correct_OneMismatch_ReturnsFalse()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 1, 2, 4 });

            // Act
            bool result = guess.Correct();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CodeGuess_Correct_AllMismatch_ReturnsFalse()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 4, 5, 6 });

            // Act
            bool result = guess.Correct();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CodeGuess_CorrectSpot_AllCorrect_ReturnsCount()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 1, 2, 3 });

            // Act
            int result = guess.CorrectSpot();

            // Assert
            Assert.AreEqual(3, result);
        }

        [Test]
        public void CodeGuess_CorrectSpot_NoneCorrect_ReturnsZero()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 4, 5, 6 });

            // Act
            int result = guess.CorrectSpot();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void CodeGuess_CorrectSpot_SomeCorrect_ReturnsPartialCount()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 1, 5, 3 });

            // Act
            int result = guess.CorrectSpot();

            // Assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public void CodeGuess_CorrectType_AllWrongSpotRightType_ReturnsCount()
        {
            // Arrange - answer is 1,2,3 - guess has same values in wrong positions
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 3, 1, 2 });

            // Act
            int result = guess.CorrectType();

            // Assert
            Assert.AreEqual(3, result);
        }

        [Test]
        public void CodeGuess_CorrectType_NoMatchingTypes_ReturnsZero()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 4, 5, 6 });

            // Act
            int result = guess.CorrectType();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void CodeGuess_CorrectType_MixedCorrectSpotAndType_CountsOnlyWrongSpot()
        {
            // Arrange - answer is 1,2,3, guess is 1,3,2
            // 1 is in correct spot (not counted in CorrectType)
            // 3 and 2 are in wrong spots but correct type
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 1, 3, 2 });

            // Act
            int result = guess.CorrectType();

            // Assert
            Assert.AreEqual(2, result);
        }

        [Test]
        public void CodeGuess_CorrectType_DuplicateValuesInGuess_CountsOnce()
        {
            // Arrange - answer is 1,2,3, guess is 2,2,4
            // First 2 is wrong spot but right type, second 2 should not be counted
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 2, 2, 4 });

            // Act
            int result = guess.CorrectType();

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void CodeGuess_ToString_ReturnsSpaceSeparatedValues()
        {
            // Arrange
            var answer = new PuzzleCode.Answer("1 2 3");
            var guess = new PuzzleCode.CodeGuess(answer, new List<int> { 4, 5, 6 });

            // Act
            string result = guess.ToString();

            // Assert
            Assert.AreEqual("4 5 6", result);
        }

        #endregion

        #region PuzzleCode Constructor Tests

        [Test]
        public void PuzzleCode_DictionaryConstructor_ParsesAnswer()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" }
            };

            // Act
            var puzzle = new PuzzleCode(data);

            // Assert
            Assert.IsNotNull(puzzle.answer);
            Assert.AreEqual(3, puzzle.answer.state.Count);
            Assert.AreEqual(1, puzzle.answer.state[0]);
        }

        [Test]
        public void PuzzleCode_DictionaryConstructor_ParsesGuesses()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" },
                { "guess", "4 5 6,1 2 3" }
            };

            // Act
            var puzzle = new PuzzleCode(data);

            // Assert
            Assert.AreEqual(2, puzzle.guess.Count);
        }

        [Test]
        public void PuzzleCode_DictionaryConstructor_EmptyGuess_CreatesEmptyList()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" }
            };

            // Act
            var puzzle = new PuzzleCode(data);

            // Assert
            Assert.IsNotNull(puzzle.guess);
            Assert.AreEqual(0, puzzle.guess.Count);
        }

        #endregion

        #region PuzzleCode Methods Tests

        [Test]
        public void PuzzleCode_Solved_NoGuesses_ReturnsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" }
            };
            var puzzle = new PuzzleCode(data);

            // Act
            bool result = puzzle.Solved();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void PuzzleCode_Solved_LastGuessCorrect_ReturnsTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" },
                { "guess", "4 5 6,1 2 3" }
            };
            var puzzle = new PuzzleCode(data);

            // Act
            bool result = puzzle.Solved();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void PuzzleCode_Solved_LastGuessIncorrect_ReturnsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" },
                { "guess", "1 2 3,4 5 6" }
            };
            var puzzle = new PuzzleCode(data);

            // Act
            bool result = puzzle.Solved();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void PuzzleCode_AddGuess_AddsToGuessList()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" }
            };
            var puzzle = new PuzzleCode(data);

            // Act
            puzzle.AddGuess(new List<int> { 4, 5, 6 });

            // Assert
            Assert.AreEqual(1, puzzle.guess.Count);
        }

        [Test]
        public void PuzzleCode_ToString_ContainsAnswerAndGuess()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "answer", "1 2 3" },
                { "guess", "4 5 6" }
            };
            var puzzle = new PuzzleCode(data);

            // Act
            string result = puzzle.ToString("Test");

            // Assert
            Assert.IsTrue(result.Contains("[PuzzleCodeTest]"));
            Assert.IsTrue(result.Contains("answer=1 2 3"));
            Assert.IsTrue(result.Contains("guess=4 5 6"));
        }

        #endregion
    }
}
