using NUnit.Framework;
using System.Collections.Generic;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for Puzzle classes - PuzzleSlide, PuzzleTower, and PuzzleImage
    /// Tests cover pure logic methods avoiding Unity-dependent code paths
    /// </summary>
    [TestFixture]
    public class PuzzleTests
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

        #region PuzzleSlide.Block Tests

        [Test]
        public void Block_ConstructFromString_ParsesAllFieldsCorrectly()
        {
            // Arrange
            string blockData = "True,2,1,3,4,False";

            // Act
            var block = new PuzzleSlide.Block(blockData);

            // Assert
            Assert.IsTrue(block.rotation);
            Assert.AreEqual(2, block.xlen);
            Assert.AreEqual(1, block.ylen);
            Assert.AreEqual(3, block.xpos);
            Assert.AreEqual(4, block.ypos);
            Assert.IsFalse(block.target);
        }

        [Test]
        public void Block_ConstructFromString_TargetBlockParsesCorrectly()
        {
            // Arrange
            string blockData = "False,1,0,0,2,True";

            // Act
            var block = new PuzzleSlide.Block(blockData);

            // Assert
            Assert.IsFalse(block.rotation);
            Assert.AreEqual(1, block.xlen);
            Assert.AreEqual(0, block.ylen);
            Assert.AreEqual(0, block.xpos);
            Assert.AreEqual(2, block.ypos);
            Assert.IsTrue(block.target);
        }

        [Test]
        public void Block_ToString_ProducesCorrectFormat()
        {
            // Arrange
            var block = new PuzzleSlide.Block("True,2,1,3,4,False");

            // Act
            string result = block.ToString();

            // Assert
            Assert.AreEqual("True,2,1,3,4,False", result);
        }

        [Test]
        public void Block_CopyConstructor_CreatesIndependentCopy()
        {
            // Arrange
            var original = new PuzzleSlide.Block("True,2,1,3,4,False");

            // Act
            var copy = new PuzzleSlide.Block(original);
            copy.xpos = 99;

            // Assert
            Assert.AreEqual(3, original.xpos);
            Assert.AreEqual(99, copy.xpos);
            Assert.AreEqual(original.rotation, copy.rotation);
            Assert.AreEqual(original.xlen, copy.xlen);
            Assert.AreEqual(original.ylen, copy.ylen);
        }

        [Test]
        public void Block_Blocks_SingleCell_ReturnsTrueForOverlappingPosition()
        {
            // Arrange - 1x1 block at position (2, 2)
            var block = new PuzzleSlide.Block("False,0,0,2,2,False");

            // Act & Assert
            Assert.IsTrue(block.Blocks(2, 2));
            Assert.IsFalse(block.Blocks(1, 2));
            Assert.IsFalse(block.Blocks(3, 2));
            Assert.IsFalse(block.Blocks(2, 1));
            Assert.IsFalse(block.Blocks(2, 3));
        }

        [Test]
        public void Block_Blocks_HorizontalBlock_ReturnsTrueForAllCoveredPositions()
        {
            // Arrange - 3x1 horizontal block (xlen=2) at position (1, 3)
            var block = new PuzzleSlide.Block("False,2,0,1,3,False");

            // Act & Assert
            Assert.IsTrue(block.Blocks(1, 3));
            Assert.IsTrue(block.Blocks(2, 3));
            Assert.IsTrue(block.Blocks(3, 3));
            Assert.IsFalse(block.Blocks(0, 3));
            Assert.IsFalse(block.Blocks(4, 3));
            Assert.IsFalse(block.Blocks(2, 2));
        }

        [Test]
        public void Block_Blocks_VerticalBlock_ReturnsTrueForAllCoveredPositions()
        {
            // Arrange - 1x3 vertical block (ylen=2) at position (4, 1)
            var block = new PuzzleSlide.Block("True,0,2,4,1,False");

            // Act & Assert
            Assert.IsTrue(block.Blocks(4, 1));
            Assert.IsTrue(block.Blocks(4, 2));
            Assert.IsTrue(block.Blocks(4, 3));
            Assert.IsFalse(block.Blocks(4, 0));
            Assert.IsFalse(block.Blocks(4, 4));
            Assert.IsFalse(block.Blocks(3, 2));
        }

        [Test]
        public void Block_BlocksOtherBlock_ReturnsTrueWhenOverlapping()
        {
            // Arrange
            var block1 = new PuzzleSlide.Block("False,2,0,1,2,False"); // Horizontal at (1,2) spanning to (3,2)
            var block2 = new PuzzleSlide.Block("True,0,2,2,1,False");  // Vertical at (2,1) spanning to (2,3)

            // Act & Assert - they overlap at (2,2)
            Assert.IsTrue(block1.Blocks(block2));
            Assert.IsTrue(block2.Blocks(block1));
        }

        [Test]
        public void Block_BlocksOtherBlock_ReturnsFalseWhenNotOverlapping()
        {
            // Arrange
            var block1 = new PuzzleSlide.Block("False,1,0,0,0,False"); // Horizontal at (0,0) to (1,0)
            var block2 = new PuzzleSlide.Block("False,1,0,3,3,False"); // Horizontal at (3,3) to (4,3)

            // Act & Assert
            Assert.IsFalse(block1.Blocks(block2));
            Assert.IsFalse(block2.Blocks(block1));
        }

        [Test]
        public void Block_BlocksList_ReturnsTrueIfAnyBlockOverlaps()
        {
            // Arrange
            var testBlock = new PuzzleSlide.Block("False,1,0,2,2,False"); // At (2,2) to (3,2)
            var blockList = new List<PuzzleSlide.Block>
            {
                new PuzzleSlide.Block("False,0,0,0,0,False"), // At (0,0)
                new PuzzleSlide.Block("False,0,0,3,2,False"), // At (3,2) - overlaps!
                new PuzzleSlide.Block("False,0,0,5,5,False")  // At (5,5)
            };

            // Act & Assert
            Assert.IsTrue(testBlock.Blocks(blockList));
        }

        [Test]
        public void Block_BlocksList_ReturnsFalseIfNoBlockOverlaps()
        {
            // Arrange
            var testBlock = new PuzzleSlide.Block("False,0,0,2,2,False"); // At (2,2)
            var blockList = new List<PuzzleSlide.Block>
            {
                new PuzzleSlide.Block("False,0,0,0,0,False"), // At (0,0)
                new PuzzleSlide.Block("False,0,0,4,4,False"), // At (4,4)
                new PuzzleSlide.Block("False,0,0,5,5,False")  // At (5,5)
            };

            // Act & Assert
            Assert.IsFalse(testBlock.Blocks(blockList));
        }

        #endregion

        #region PuzzleSlide Static Methods Tests

        [Test]
        public void PuzzleSlide_Empty_ReturnsFalseForNegativeCoordinates()
        {
            // Arrange
            var emptyState = new List<PuzzleSlide.Block>();

            // Act & Assert
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, -1, 0));
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, 0, -1));
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, -1, -1));
        }

        [Test]
        public void PuzzleSlide_Empty_ReturnsFalseForOutOfBoundsY()
        {
            // Arrange
            var emptyState = new List<PuzzleSlide.Block>();

            // Act & Assert
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, 0, 6)); // y > 5
            Assert.IsTrue(PuzzleSlide.Empty(emptyState, 0, 5));  // y = 5 is valid
        }

        [Test]
        public void PuzzleSlide_Empty_AllowsExitOnlyOnRow2()
        {
            // Arrange
            var emptyState = new List<PuzzleSlide.Block>();

            // Act & Assert - x > 5 only allowed when y == 2 (exit row)
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, 6, 0)); // y != 2
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, 6, 1)); // y != 2
            Assert.IsTrue(PuzzleSlide.Empty(emptyState, 6, 2));  // y == 2, exit allowed
            Assert.IsTrue(PuzzleSlide.Empty(emptyState, 7, 2));  // y == 2, still in exit
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, 8, 2)); // x > 7, beyond exit
            Assert.IsFalse(PuzzleSlide.Empty(emptyState, 6, 3)); // y != 2
        }

        [Test]
        public void PuzzleSlide_Empty_ReturnsFalseWhenBlockOccupiesPosition()
        {
            // Arrange
            var state = new List<PuzzleSlide.Block>
            {
                new PuzzleSlide.Block("False,1,0,2,2,True") // Block at (2,2) to (3,2)
            };

            // Act & Assert
            Assert.IsFalse(PuzzleSlide.Empty(state, 2, 2));
            Assert.IsFalse(PuzzleSlide.Empty(state, 3, 2));
            Assert.IsTrue(PuzzleSlide.Empty(state, 1, 2));
            Assert.IsTrue(PuzzleSlide.Empty(state, 4, 2));
        }

        [Test]
        public void PuzzleSlide_HardCodedPuzzle_ReturnsValidPuzzleData()
        {
            // Arrange & Act
            var puzzleData = PuzzleSlide.HardCodedPuzzle();

            // Assert
            Assert.IsNotNull(puzzleData);
            Assert.IsTrue(puzzleData.ContainsKey("moves"));
            Assert.IsTrue(puzzleData.ContainsKey("block0"));
            Assert.AreEqual("0", puzzleData["moves"]);
            Assert.AreEqual("False,1,0,0,2,True", puzzleData["block0"]); // Target block
        }

        #endregion

        #region PuzzleSlide Instance Tests

        [Test]
        public void PuzzleSlide_Loadpuzzle_LoadsBlocksAndMoves()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "moves", "5" },
                { "block0", "False,1,0,0,2,True" },
                { "block1", "True,0,1,3,0,False" }
            };

            // Act
            var puzzle = new PuzzleSlide(data);

            // Assert
            Assert.AreEqual(5, puzzle.moves);
            Assert.AreEqual(2, puzzle.puzzle.Count);
        }

        [Test]
        public void PuzzleSlide_Solved_ReturnsTrueWhenTargetAtExit()
        {
            // Arrange - Target block at x=6 (exit position)
            var data = new Dictionary<string, string>
            {
                { "moves", "0" },
                { "block0", "False,1,0,6,2,True" } // Target at exit
            };

            // Act
            var puzzle = new PuzzleSlide(data);

            // Assert
            Assert.IsTrue(puzzle.Solved());
        }

        [Test]
        public void PuzzleSlide_Solved_ReturnsFalseWhenTargetNotAtExit()
        {
            // Arrange - Target block not at exit
            var data = new Dictionary<string, string>
            {
                { "moves", "0" },
                { "block0", "False,1,0,0,2,True" } // Target at x=0
            };

            // Act
            var puzzle = new PuzzleSlide(data);

            // Assert
            Assert.IsFalse(puzzle.Solved());
        }

        [Test]
        public void PuzzleSlide_ToString_ProducesValidSaveFormat()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "moves", "3" },
                { "block0", "False,1,0,0,2,True" }
            };
            var puzzle = new PuzzleSlide(data);

            // Act
            string result = puzzle.ToString("TestPuzzle");

            // Assert
            Assert.IsTrue(result.Contains("[PuzzleSlideTestPuzzle]"));
            Assert.IsTrue(result.Contains("moves=3"));
            Assert.IsTrue(result.Contains("block0="));
        }

        #endregion

        #region PuzzleTower Tests

        [Test]
        public void PuzzleTower_MoveOK_ReturnsFalseForInvalidFromTower()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3, 2, 1 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.MoveOK(-1, 1));
            Assert.IsFalse(puzzle.MoveOK(3, 1));
            Assert.IsFalse(puzzle.MoveOK(10, 1));
        }

        [Test]
        public void PuzzleTower_MoveOK_ReturnsFalseForInvalidToTower()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3, 2, 1 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.MoveOK(0, -1));
            Assert.IsFalse(puzzle.MoveOK(0, 3));
            Assert.IsFalse(puzzle.MoveOK(0, 10));
        }

        [Test]
        public void PuzzleTower_MoveOK_ReturnsFalseForEmptyFromTower()
        {
            // Arrange
            var state = CreateTowerState(new int[] { }, new int[] { 3, 2, 1 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.MoveOK(0, 1)); // Tower 0 is empty
        }

        [Test]
        public void PuzzleTower_MoveOK_ReturnsTrueForMoveToEmptyTower()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3, 2, 1 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsTrue(puzzle.MoveOK(0, 1)); // Move from tower with discs to empty tower
            Assert.IsTrue(puzzle.MoveOK(0, 2));
        }

        [Test]
        public void PuzzleTower_MoveOK_ReturnsTrueForSmallerOntoLarger()
        {
            // Arrange - Tower 0 has disc 1 (small), Tower 1 has disc 3 (large)
            var state = CreateTowerState(new int[] { 1 }, new int[] { 3 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsTrue(puzzle.MoveOK(0, 1)); // Small (1) onto large (3)
        }

        [Test]
        public void PuzzleTower_MoveOK_ReturnsFalseForLargerOntoSmaller()
        {
            // Arrange - Tower 0 has disc 3 (large), Tower 1 has disc 1 (small)
            var state = CreateTowerState(new int[] { 3 }, new int[] { 1 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.MoveOK(0, 1)); // Large (3) onto small (1)
        }

        [Test]
        public void PuzzleTower_Move_MovesDiscCorrectly()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3, 2, 1 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act
            puzzle.Move(0, 1);

            // Assert
            Assert.AreEqual(2, puzzle.puzzle[0].Count);
            Assert.AreEqual(1, puzzle.puzzle[1].Count);
            Assert.AreEqual(1, puzzle.puzzle[1][0]); // Disc 1 moved to tower 1
        }

        [Test]
        public void PuzzleTower_Move_DoesNothingForInvalidMove()
        {
            // Arrange - Tower 0 has large disc, Tower 1 has small disc
            var state = CreateTowerState(new int[] { 3 }, new int[] { 1 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act
            puzzle.Move(0, 1); // Invalid: larger onto smaller

            // Assert - state unchanged
            Assert.AreEqual(1, puzzle.puzzle[0].Count);
            Assert.AreEqual(3, puzzle.puzzle[0][0]);
            Assert.AreEqual(1, puzzle.puzzle[1].Count);
            Assert.AreEqual(1, puzzle.puzzle[1][0]);
        }

        [Test]
        public void PuzzleTower_Solved_ReturnsTrueWhenAllDiscsOnOneTowerInOrder()
        {
            // Arrange - All 8 discs on tower 0, largest to smallest
            var state = CreateTowerState(new int[] { 7, 6, 5, 4, 3, 2, 1, 0 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsTrue(puzzle.Solved());
        }

        [Test]
        public void PuzzleTower_Solved_ReturnsFalseWhenDiscsSpreadAcrossTowers()
        {
            // Arrange - Discs spread across towers
            var state = CreateTowerState(new int[] { 7, 6, 5, 4 }, new int[] { 3, 2, 1, 0 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.Solved());
        }

        [Test]
        public void PuzzleTower_Solved_ReturnsFalseWhenDiscsOutOfOrder()
        {
            // Arrange - Discs on one tower but out of order (larger on smaller)
            var state = CreateTowerState(new int[] { 1, 2, 3 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.Solved());
        }

        [Test]
        public void PuzzleTower_CopyState_CreatesIndependentCopy()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3, 2, 1 }, new int[] { 5, 4 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act
            var copy = puzzle.CopyState(puzzle.puzzle);
            copy[0].Add(99);
            copy[1].RemoveAt(0);

            // Assert - original unchanged
            Assert.AreEqual(3, puzzle.puzzle[0].Count);
            Assert.AreEqual(2, puzzle.puzzle[1].Count);
        }

        [Test]
        public void PuzzleTower_ReverseMoveOK_ReturnsFalseForInvalidTower()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3, 2, 1 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.ReverseMoveOK(-1, puzzle.puzzle));
            Assert.IsFalse(puzzle.ReverseMoveOK(3, puzzle.puzzle));
        }

        [Test]
        public void PuzzleTower_ReverseMoveOK_ReturnsFalseForEmptyTower()
        {
            // Arrange
            var state = CreateTowerState(new int[] { }, new int[] { 3, 2, 1 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsFalse(puzzle.ReverseMoveOK(0, puzzle.puzzle));
        }

        [Test]
        public void PuzzleTower_ReverseMoveOK_ReturnsTrueForSingleDisc()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsTrue(puzzle.ReverseMoveOK(0, puzzle.puzzle));
        }

        [Test]
        public void PuzzleTower_ReverseMoveOK_ReturnsTrueWhenTopSmallerThanBase()
        {
            // Arrange - Properly stacked: 3 (bottom), 1 (top)
            var state = CreateTowerState(new int[] { 3, 1 }, new int[] { }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);

            // Act & Assert
            Assert.IsTrue(puzzle.ReverseMoveOK(0, puzzle.puzzle));
        }

        [Test]
        public void PuzzleTower_ToString_ProducesValidSaveFormat()
        {
            // Arrange
            var state = CreateTowerState(new int[] { 3, 2, 1 }, new int[] { 5, 4 }, new int[] { });
            var puzzle = CreatePuzzleTowerFromState(state);
            puzzle.moves = 7;

            // Act
            string result = puzzle.ToString("TestTower");

            // Assert
            Assert.IsTrue(result.Contains("[PuzzleTowerTestTower]"));
            Assert.IsTrue(result.Contains("moves=7"));
            Assert.IsTrue(result.Contains("0=3 2 1"));
            Assert.IsTrue(result.Contains("1=5 4"));
        }

        [Test]
        public void PuzzleTower_Loadpuzzle_RestoresStateCorrectly()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "moves", "15" },
                { "0", "7 6 5" },
                { "1", "4 3" },
                { "2", "2 1 0" }
            };

            // Act
            var puzzle = new PuzzleTower(data);

            // Assert
            Assert.AreEqual(15, puzzle.moves);
            Assert.AreEqual(3, puzzle.puzzle[0].Count);
            Assert.AreEqual(7, puzzle.puzzle[0][0]);
            Assert.AreEqual(2, puzzle.puzzle[1].Count);
            Assert.AreEqual(3, puzzle.puzzle[2].Count);
        }

        #endregion

        #region PuzzleImage.TilePosition Tests

        [Test]
        public void TilePosition_ConstructFromInts_SetsCoordinatesCorrectly()
        {
            // Arrange & Act
            var pos = new PuzzleImage.TilePosition(3, 5);

            // Assert
            Assert.AreEqual(3, pos.x);
            Assert.AreEqual(5, pos.y);
        }

        [Test]
        public void TilePosition_ConstructFromString_ParsesCorrectly()
        {
            // Arrange & Act
            var pos = new PuzzleImage.TilePosition("4 7");

            // Assert
            Assert.AreEqual(4, pos.x);
            Assert.AreEqual(7, pos.y);
        }

        [Test]
        public void TilePosition_ToString_ProducesCorrectFormat()
        {
            // Arrange
            var pos = new PuzzleImage.TilePosition(2, 8);

            // Act
            string result = pos.ToString();

            // Assert
            Assert.AreEqual("2 8", result);
        }

        [Test]
        public void TilePosition_RoundTrip_PreservesValues()
        {
            // Arrange
            var original = new PuzzleImage.TilePosition(6, 3);

            // Act
            string serialized = original.ToString();
            var restored = new PuzzleImage.TilePosition(serialized);

            // Assert
            Assert.AreEqual(original.x, restored.x);
            Assert.AreEqual(original.y, restored.y);
        }

        #endregion

        #region PuzzleImage Tests

        [Test]
        public void PuzzleImage_Solved_ReturnsTrueWhenAllTilesInPlace()
        {
            // Arrange - Create a solved puzzle state manually
            var data = new Dictionary<string, string>
            {
                { "moves", "0" },
                { "state", "0 0,0 0:0 1,0 1:1 0,1 0:1 1,1 1" }
            };

            // Act
            var puzzle = new PuzzleImage(data);

            // Assert
            Assert.IsTrue(puzzle.Solved());
        }

        [Test]
        public void PuzzleImage_Solved_ReturnsFalseWhenTilesSwapped()
        {
            // Arrange - Create puzzle with swapped tiles
            var data = new Dictionary<string, string>
            {
                { "moves", "5" },
                { "state", "0 0,0 1:0 1,0 0:1 0,1 0:1 1,1 1" } // First two tiles swapped
            };

            // Act
            var puzzle = new PuzzleImage(data);

            // Assert
            Assert.IsFalse(puzzle.Solved());
        }

        [Test]
        public void PuzzleImage_LoadFromDictionary_RestoresMoves()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "moves", "42" },
                { "state", "0 0,0 0" }
            };

            // Act
            var puzzle = new PuzzleImage(data);

            // Assert
            Assert.AreEqual(42, puzzle.moves);
        }

        [Test]
        public void PuzzleImage_ToString_ProducesValidSaveFormat()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "moves", "10" },
                { "state", "0 0,0 0:0 1,0 1" }
            };
            var puzzle = new PuzzleImage(data);

            // Act
            string result = puzzle.ToString("TestImage");

            // Assert
            Assert.IsTrue(result.Contains("[PuzzleImageTestImage]"));
            Assert.IsTrue(result.Contains("moves=10"));
            Assert.IsTrue(result.Contains("state="));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a tower state from arrays of disc sizes
        /// </summary>
        private List<List<int>> CreateTowerState(int[] tower0, int[] tower1, int[] tower2)
        {
            var state = new List<List<int>>
            {
                new List<int>(tower0),
                new List<int>(tower1),
                new List<int>(tower2)
            };
            return state;
        }

        /// <summary>
        /// Creates a PuzzleTower from a given state using the dictionary constructor
        /// </summary>
        private PuzzleTower CreatePuzzleTowerFromState(List<List<int>> state)
        {
            var data = new Dictionary<string, string>
            {
                { "moves", "0" }
            };

            for (int i = 0; i < state.Count; i++)
            {
                if (state[i].Count > 0)
                {
                    data[i.ToString()] = string.Join(" ", state[i]);
                }
                else
                {
                    data[i.ToString()] = "";
                }
            }

            return new PuzzleTower(data);
        }

        #endregion
    }
}
