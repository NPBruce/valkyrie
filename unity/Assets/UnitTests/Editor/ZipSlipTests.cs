using System;
using System.IO;
using Ionic.Zip;
using NUnit.Framework;

namespace Valkyrie.UnitTests
{
    [TestFixture]
    public class ZipSlipTests
    {
        private string tempDir;
        private string zipPath;
        private string targetDir;

        [SetUp]
        public void Setup()
        {
            ValkyrieTools.ValkyrieDebug.enabled = false;
            
            tempDir = Path.Combine(Path.GetTempPath(), "ValkyrieZipSlipTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            zipPath = Path.Combine(tempDir, "test.zip");
            targetDir = Path.Combine(tempDir, "extract");
            Directory.CreateDirectory(targetDir);

            // Create malicious zip using Ionic.Zip
            using (Ionic.Zip.ZipFile archive = new Ionic.Zip.ZipFile())
            {
                archive.AddEntry("good.txt", "good");

                // Malicious entry that tries to write to the parent directory of targetDir
                Ionic.Zip.ZipEntry entry2 = archive.AddEntry("../evil.txt", "evil");
                // In case AddEntry strips the path, force it:
                entry2.FileName = "../evil.txt";
                
                archive.Save(zipPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            
            ValkyrieTools.ValkyrieDebug.enabled = true;
        }

        [Test]
        public void ZipManager_ExtractFull_MitigatesZipSlip()
        {
            // Act
            ZipManager.Extract(targetDir, zipPath, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_FULL);

            // Assert
            Assert.IsTrue(File.Exists(Path.Combine(targetDir, "good.txt")), "Good file should be extracted.");
            Assert.IsFalse(File.Exists(Path.Combine(tempDir, "evil.txt")), "Evil file should NOT be extracted outside target directory!");
        }
    }
}
