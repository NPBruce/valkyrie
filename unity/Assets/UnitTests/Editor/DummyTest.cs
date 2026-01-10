using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Valkyrie.UnitTests
{
    public class DummyTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void DummyGenericTest()
        {
            // Verify we can access game code
            // D2EGameType is defined in Assets/Scripts/GameType.cs
            // This ensures Assembly-CSharp-Editor can see Assembly-CSharp
            var gameType = new D2EGameType();
            Assert.IsNotNull(gameType);
            Assert.AreEqual("D2E", gameType.TypeName());
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator DummyEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
            Assert.IsTrue(true);
        }
    }
}
