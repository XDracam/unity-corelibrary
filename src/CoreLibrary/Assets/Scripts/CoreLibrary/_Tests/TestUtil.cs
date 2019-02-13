using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CoreLibrary.Tests
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// </summary>
    public class TestUtil
    {
        [Test]
        public void TestMod()
        {
            // equality to % for positive values
            Assert.AreEqual(13 % 4, Util.Mod(13, 4));
            Assert.AreEqual(37 % 3, Util.Mod(37, 3));
            
            // yields positive values for negative numbers
            Assert.AreEqual(2, Util.Mod(-37, 3)); // -1 + 3
            Assert.AreEqual(3, Util.Mod(-13, 4)); // -1 + 4
        }

        [UnityTest]
        public IEnumerator TestIsNull()
        {
            Assert.IsFalse(Util.IsNull(0.5f));
            Assert.IsFalse(Util.IsNull(0));
            Assert.IsFalse(Util.IsNull(Vector3.zero));
            
            Assert.IsTrue(Util.IsNull<int?>(null));
            Assert.IsTrue(Util.IsNull<Component>(null));
            Assert.IsTrue(Util.IsNull<List<int>>(null));
            
            var go = new GameObject();
            Assert.IsTrue(Util.IsNull(go.As<Collider>()));
            
            Object.Destroy(go);
            yield return null;
            Assert.IsTrue(Util.IsNull(go));
        }
    }
}