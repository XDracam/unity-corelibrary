using System.Collections;
using System.Collections.Generic;
using CoreLibrary.Exceptions;
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

        // ReSharper disable all Unity.InefficientPropertyAccess
        [Test]
        public void TestPositionProxy()
        {
            var go = new GameObject();
            var uut = go.AddComponent<BaseBehaviour>();
            var tr = uut.transform;
            
            var vec = new Vector3(3,5,7);
            uut.Position = vec;
            Assert.AreEqual(vec, tr.position);
            Vector3 pos = uut.Position;
            Assert.AreEqual(vec, pos);

            var newX = 17;
            uut.Position.x = newX;
            Assert.AreEqual(newX, tr.position.x);            
            Assert.AreEqual(newX, uut.Position.x);  
            
            var newY = 18;
            uut.Position.y = newY;
            Assert.AreEqual(newY, tr.position.y);            
            Assert.AreEqual(newY, uut.Position.y);  
            
            var newZ = 19;
            uut.Position.z = newZ;
            Assert.AreEqual(newZ, tr.position.z);            
            Assert.AreEqual(newZ, uut.Position.z);

            var constVec = new Vector3(11, 88, 0);
            Util.VectorProxy proxy = constVec;
            Assert.AreEqual(constVec, (Vector3) proxy);
            Assert.AreEqual(constVec.x, proxy.x);
            Assert.AreEqual(constVec.y, proxy.y);
            Assert.AreEqual(constVec.z, proxy.z);

            proxy.x = 5;
            Assert.AreEqual(5, proxy.x);
            proxy.y = 6;
            Assert.AreEqual(6, proxy.y);
            proxy.z = 7;
            Assert.AreEqual(7, proxy.z);
        }
    }
}