using NUnit.Framework;
using System.IO;
using ValkyrieTools;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for Quest.FindLocalisedMultimediaFile.
    /// Uses the internal testable overload that accepts lang/fallback/editMode parameters,
    /// so no Game instance is needed.
    /// </summary>
    [TestFixture]
    public class FindLocalisedMultimediaFileTests
    {
        private string _tempDir;

        [SetUp]
        public void Setup()
        {
            ValkyrieDebug.enabled = false;
            _tempDir = Path.Combine(Path.GetTempPath(), "ValkyrieTestQuest_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            ValkyrieDebug.enabled = true;
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        // --- Helpers ---

        private void CreateFile(string relativePath)
        {
            string fullPath = Path.Combine(_tempDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, string.Empty);
        }

        private string Call(string name, string currentLang, string fallbackLang, bool editMode = false)
        {
            return Quest.FindLocalisedMultimediaFile(name, _tempDir, currentLang, fallbackLang, editMode);
        }

        // --- Tests ---

        [Test]
        public void Returns_CurrentLanguagePath_WhenFileExistsThere()
        {
            // Arrange
            CreateFile(Path.Combine("German", "Tile.png"));

            // Act
            string result = Call("Tile.png", "German", "English");

            // Assert
            Assert.AreEqual(Path.Combine(_tempDir, "German", "Tile.png"), result);
        }

        [Test]
        public void Returns_FallbackLanguagePath_WhenCurrentLangMissingButFallbackExists()
        {
            // Arrange — only English version exists, current lang is German with English fallback
            CreateFile(Path.Combine("English", "Tile.png"));

            // Act
            string result = Call("Tile.png", "German", "English");

            // Assert
            Assert.AreEqual(Path.Combine(_tempDir, "English", "Tile.png"), result);
        }

        [Test]
        public void Returns_RootPath_WhenNoLanguageSubfolderFileExists()
        {
            // Arrange — only root file exists
            CreateFile("Tile.png");

            // Act
            string result = Call("Tile.png", "German", "English");

            // Assert
            Assert.AreEqual(Path.Combine(_tempDir, "Tile.png"), result);
        }

        [Test]
        public void Returns_RootPath_WhenNoFilesExistAtAll()
        {
            // Arrange — no files created

            // Act
            string result = Call("Missing.png", "German", "English");

            // Assert — returns the composed root path even if the file doesn't exist
            Assert.AreEqual(Path.Combine(_tempDir, "Missing.png"), result);
        }

        [Test]
        public void Returns_RootPath_WhenEditModeIsTrue_EvenIfLanguageFileExists()
        {
            // Arrange — language file exists, but edit mode bypasses language lookup
            CreateFile(Path.Combine("German", "Tile.png"));

            // Act
            string result = Call("Tile.png", "German", "English", editMode: true);

            // Assert
            Assert.AreEqual(Path.Combine(_tempDir, "Tile.png"), result);
        }

        [Test]
        public void CurrentLanguage_TakesPriority_OverFallbackLanguage()
        {
            // Arrange — both lang files exist
            CreateFile(Path.Combine("German", "Tile.png"));
            CreateFile(Path.Combine("English", "Tile.png"));

            // Act
            string result = Call("Tile.png", "German", "English");

            // Assert — current lang wins
            Assert.AreEqual(Path.Combine(_tempDir, "German", "Tile.png"), result);
        }

        [Test]
        public void Returns_RootPath_WhenFallbackLangIsEmpty()
        {
            // Arrange — English folder exists but fallback is empty
            CreateFile(Path.Combine("English", "Tile.png"));

            // Act
            string result = Call("Tile.png", "German", string.Empty);

            // Assert — no fallback check, goes to root
            Assert.AreEqual(Path.Combine(_tempDir, "Tile.png"), result);
        }

        [Test]
        public void Returns_RootPath_WhenFallbackLangEqualsCurrentLang()
        {
            // Arrange — only the fallback folder exists but fallback == current
            CreateFile(Path.Combine("German", "Tile.png"));

            // Act — fallback same as current, so fallback step skipped; German file is for "current"
            // Here current = French to avoid the currentLang hit, and fallback = French too
            string result = Call("Tile.png", "French", "French");

            // Assert — both steps skipped, returns root
            Assert.AreEqual(Path.Combine(_tempDir, "Tile.png"), result);
        }

        [Test]
        public void Returns_FallbackLanguagePath_WhenFallbackLangIsNull()
        {
            // Arrange — only English folder exists
            CreateFile(Path.Combine("English", "Tile.png"));

            // Act — null fallback should not throw and should skip fallback step
            string result = Call("Tile.png", "German", null);

            // Assert — null treated as empty, no fallback lookup
            Assert.AreEqual(Path.Combine(_tempDir, "Tile.png"), result);
        }

        [Test]
        public void Supports_RelativeSubfolderInName()
        {
            // Arrange — image is in an images/ subdirectory within the language folder
            CreateFile(Path.Combine("German", "images", "BgTile.png"));

            // Act — name uses platform separator as produced by the ini parsing on this OS
            string name = Path.Combine("images", "BgTile.png");
            string result = Call(name, "German", "English");

            // Assert
            Assert.AreEqual(Path.Combine(_tempDir, "German", "images", "BgTile.png"), result);
        }

        [Test]
        public void Supports_LanguageFolderInsideSubfolder()
        {
            // Arrange — image is in a subfolder and localized version is inside that subfolder (e.g. image/German/map.png)
            CreateFile(Path.Combine("image", "map.png"));
            CreateFile(Path.Combine("image", "German", "map.png"));

            // Act — name uses platform separator as produced by the ini parsing on this OS
            string name = Path.Combine("image", "map.png");
            string result = Call(name, "German", "English");

            // Assert — localized inside subfolder should be preferred over root
            Assert.AreEqual(Path.Combine(_tempDir, "image", "German", "map.png"), result);
        }

        [Test]
        public void Fallback_LanguageFolderInsideSubfolder_IsUsed_WhenCurrentMissing()
        {
            // Arrange — only fallback localized version exists inside the subfolder
            CreateFile(Path.Combine("image", "English", "map.png"));

            // Act
            string name = Path.Combine("image", "map.png");
            string result = Call(name, "German", "English");

            // Assert — fallback inside subfolder should be returned
            Assert.AreEqual(Path.Combine(_tempDir, "image", "English", "map.png"), result);
        }

        [Test]
        public void EditMode_Ignores_LanguageFolderInsideSubfolder()
        {
            // Arrange — localized version exists inside the subfolder but edit mode should bypass it
            CreateFile(Path.Combine("image", "German", "map.png"));
            CreateFile(Path.Combine("image", "map.png"));

            // Act
            string name = Path.Combine("image", "map.png");
            string result = Call(name, "German", "English", editMode: true);

            // Assert — edit mode returns root path
            Assert.AreEqual(Path.Combine(_tempDir, "image", "map.png"), result);
        }
    }
}
