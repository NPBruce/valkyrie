using NUnit.Framework;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for VersionManager class
    /// </summary>
    [TestFixture]
    public class VersionManagerTests
    {
        [Test]
        public void IsBeta_NormalVersion_ReturnsFalse()
        {
            Assert.IsFalse(VersionManager.IsBeta("3.12"));
            Assert.IsFalse(VersionManager.IsBeta("1.0"));
            Assert.IsFalse(VersionManager.IsBeta("0.1"));
        }

        [Test]
        public void IsBeta_BetaVersion_ReturnsTrue()
        {
            Assert.IsTrue(VersionManager.IsBeta("3.12.0"));
            Assert.IsTrue(VersionManager.IsBeta("3.12.1"));
            Assert.IsTrue(VersionManager.IsBeta("3.12.1.5"));
            Assert.IsTrue(VersionManager.IsBeta("0.0.1"));
            Assert.IsTrue(VersionManager.IsBeta("1.0.0.0"));
        }

        [Test]
        public void IsBeta_StringWithBeta_ReturnsTrue()
        {
            Assert.IsTrue(VersionManager.IsBeta("3.12 BETA"));
            Assert.IsTrue(VersionManager.IsBeta("3.12\nBETA"));
            Assert.IsTrue(VersionManager.IsBeta("3.14  beta"));
        }

        [Test]
        public void IsBeta_StringWithMajor_ReturnsFalse()
        {
            Assert.IsFalse(VersionManager.IsBeta("3.12 MAJOR"));
            Assert.IsFalse(VersionManager.IsBeta("3.12\nMAJOR"));
            Assert.IsFalse(VersionManager.IsBeta("3.14  major"));
        }

        [Test]
        public void IsBeta_EmptyVersion_ReturnsFalse()
        {
            Assert.IsFalse(VersionManager.IsBeta(""));
        }

        [Test]
        public void VersionNewer_BasicComparison_ReturnsCorrectResult()
        {
            Assert.IsTrue(VersionManager.VersionNewer("3.19", "3.20"));
            Assert.IsFalse(VersionManager.VersionNewer("3.21", "3.20"));
            Assert.IsFalse(VersionManager.VersionNewer("3.20", "3.20"));
        }

        [Test]
        public void VersionNewer_BetaToStableUpdate_ReturnsTrue()
        {
            // Case where local is Beta and online is the final Stable release
            Assert.IsTrue(VersionManager.VersionNewer("3.20 BETA", "3.20"));
            Assert.IsTrue(VersionManager.VersionNewer("3.20.0", "3.20")); // 3.20.0 is Beta, 3.20 is Stable
            Assert.IsTrue(VersionManager.VersionNewer("3.20-beta", "3.20"));
        }

        [Test]
        public void VersionNewer_StableToMajorUpdate_ReturnsTrue()
        {
            // Case where stable is released and then a MAJOR version is released
            Assert.IsTrue(VersionManager.VersionNewer("3.20", "3.20 MAJOR"));
            Assert.IsTrue(VersionManager.VersionNewer("3.20.0", "3.20 MAJOR"));
        }

        [Test]
        public void VersionNewer_NumericPrecedenceOverSuffix_ReturnsCorrectResult()
        {
            // 3.20 BETA is numerically newer than 3.15 MAJOR, so it should be true (3.15 MAJOR -> 3.20 BETA)
            Assert.IsTrue(VersionManager.VersionNewer("3.15 MAJOR", "3.20 BETA"));
            
            // Conversely, 3.20 BETA is NOT newer than 3.21 (stable)
            Assert.IsTrue(VersionManager.VersionNewer("3.20 BETA", "3.21"));
            
            // 3.20 MAJOR is NOT newer than 3.21 (even if 3.21 is stable)
            Assert.IsTrue(VersionManager.VersionNewer("3.20 MAJOR", "3.21"));
            
            // 3.21 is NOT newer than 3.20 MAJOR
            Assert.IsFalse(VersionManager.VersionNewer("3.21", "3.20 MAJOR"));
        }

        [Test]
        public void VersionNewer_StableToNewerBeta_ReturnsTrue()
        {
            // Case where stable is installed but a newer version (in Beta) is available
            Assert.IsTrue(VersionManager.VersionNewer("3.20", "3.21 BETA"));
            Assert.IsTrue(VersionManager.VersionNewer("3.20", "3.20.1")); // 3.20.1 is a newer beta
        }

        [Test]
        public void VersionNewer_EqualVersions_ReturnsFalse()
        {
            Assert.IsFalse(VersionManager.VersionNewer("3.20", "3.20"));
            Assert.IsFalse(VersionManager.VersionNewer("3.20 BETA", "3.20 BETA"));
        }

        [Test]
        public void VersionNewerOrEqual_BasicComparison_ReturnsCorrectResult()
        {
            Assert.IsTrue(VersionManager.VersionNewerOrEqual("3.19", "3.20"));
            Assert.IsTrue(VersionManager.VersionNewerOrEqual("3.20", "3.20"));
            Assert.IsFalse(VersionManager.VersionNewerOrEqual("3.21", "3.20"));
        }

        [Test]
        public void VersionNewerOrEqual_BetaToStable_ReturnsTrue()
        {
            Assert.IsTrue(VersionManager.VersionNewerOrEqual("3.20 BETA", "3.20"));
        }

        [Test]
        public void VersionNewer_ComplexSuffixes_HandledCorrectly()
        {
            // VersionNewer removes non-digits within components
            Assert.IsFalse(VersionManager.VersionNewer("3.20a", "3.20")); 
            Assert.IsFalse(VersionManager.VersionNewer("3.20", "3.20a"));
        }

        [Test]
        public void IsBeta_NullVersion_ThrowsException()
        {
            Assert.Throws<System.NullReferenceException>(() => VersionManager.IsBeta(null));
        }
    }
}
