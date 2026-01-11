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
        public void IsBeta_EmptyVersion_ReturnsFalse()
        {
            Assert.IsFalse(VersionManager.IsBeta(""));
        }

        [Test]
        public void IsBeta_NullVersion_ThrowsException()
        {
            Assert.Throws<System.NullReferenceException>(() => VersionManager.IsBeta(null));
        }
    }
}
