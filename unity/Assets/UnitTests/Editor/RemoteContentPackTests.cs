using System.Collections.Generic;
using Assets.Scripts.Content;
using NUnit.Framework;

namespace UnitTests.Editor
{
    public class RemoteContentPackTests
    {
        [Test]
        public void GetTitle_ReturnsUserLanguage_WhenAvailable()
        {
            var rcp = new RemoteContentPack("test", new Dictionary<string, string>());
            rcp.languages_name = new Dictionary<string, string>
            {
                { "English", "Test Title" },
                { "German", "Test Titel" }
            };

            Assert.AreEqual("Test Titel", rcp.GetTitle("German"));
        }

        [Test]
        public void GetTitle_ReturnsDefaultLanguage_WhenUserLanguageMissing()
        {
            var rcp = new RemoteContentPack("test", new Dictionary<string, string>());
            rcp.languages_name = new Dictionary<string, string>
            {
                { "English", "Test Title" }
            };

            Assert.AreEqual("Test Title", rcp.GetTitle("German"));
        }

        [Test]
        public void GetTitle_ReturnsFirstAvailable_WhenUserAndDefaultMissing()
        {
            var rcp = new RemoteContentPack("test", new Dictionary<string, string>());
            rcp.languages_name = new Dictionary<string, string>
            {
                { "Spanish", "Titulo de prueba" }
            };

            Assert.AreEqual("Titulo de prueba", rcp.GetTitle("German"));
        }

        [Test]
        public void GetTitle_ReturnsEmpty_WhenNoLanguages()
        {
            var rcp = new RemoteContentPack("test", new Dictionary<string, string>());
            rcp.languages_name = new Dictionary<string, string>();

            Assert.AreEqual("", rcp.GetTitle("German"));
        }

        [Test]
        public void GetDescription_ReturnsUserLanguage_WhenAvailable()
        {
            var rcp = new RemoteContentPack("test", new Dictionary<string, string>());
            rcp.languages_description = new Dictionary<string, string>
            {
                { "English", "Description" },
                { "German", "Beschreibung" }
            };

            Assert.AreEqual("Beschreibung", rcp.GetDescription("German"));
        }
    }
}
