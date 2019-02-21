using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CoreLibrary.Tests
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// </summary>
    public class TestUtilityExtensions 
    {
        [Test]
        public void TestGetChildren()
        {
            var go = new GameObject();
            var c1 = new GameObject();
            var c2 = new GameObject();
            var c3 = new GameObject();
            c1.transform.SetParent(go.transform);
            c2.transform.SetParent(go.transform);
            c3.transform.SetParent(go.transform);
            CollectionAssert.AreEqual(go.transform.GetChildren(), 
                new []{c1.transform, c2.transform, c3.transform});
        }

        [Test]
        public void TestIsNull()
        {
            string s = null;
            Assert.IsTrue(s.IsNull());
            Assert.IsFalse("foo".IsNull());
            var go = new GameObject();
            Object.DestroyImmediate(go);
            Assert.IsTrue(go.IsNull());
        }

        [Test]
        public void TestSetPerceivable()
        { // TODO: test 2D colliders as well
            
            var go = new GameObject();
            var c1 = new GameObject();
            var c2 = new GameObject();
            var c11 = new GameObject();
            c1.transform.SetParent(go.transform);
            var rend11 = c1.AddComponent<MeshRenderer>();
            c2.transform.SetParent(go.transform);
            var col21 = c2.AddComponent<SphereCollider>();
            var col22 = c2.AddComponent<BoxCollider>();
            c11.transform.SetParent(go.transform);
            var rend111 = c11.AddComponent<MeshRenderer>();
            var col111 = c11.AddComponent<SphereCollider>();

            var collider = new List<Collider>{col21, col22, col111};
            var renderer = new List<Renderer>{rend11, rend111};
            var objects = new List<GameObject> {go, c1, c2, c11};
            
            go.SetPerceivable(false);

            foreach (var obj in objects) 
                Assert.IsTrue(obj.activeSelf && obj.activeInHierarchy);
            
            foreach (var col in collider)
                Assert.IsFalse(col.enabled);

            foreach (var rend in renderer)
                Assert.IsFalse(rend.enabled);
            
            go.SetPerceivable(true);
            
            foreach (var obj in objects) 
                Assert.IsTrue(obj.activeSelf && obj.activeInHierarchy);
            
            foreach (var col in collider)
                Assert.IsTrue(col.enabled);

            foreach (var rend in renderer)
                Assert.IsTrue(rend.enabled);
        }
        
        /* Vector extension tests omitted due to simplicity of code */

        [Test]
        public void TestPositionExtensions()
        {
            var go = new GameObject();
            var vec3 = new Vector3(17, 19, -42);
            go.transform.position = vec3;
            Assert.AreEqual(go.transform.position, go.Pos());
            Assert.AreEqual(go.transform.position, go.transform.Pos());
            
            var newVec = new Vector3(1, 2, 3);
            go.SetPos(newVec);
            Assert.AreEqual(newVec, go.transform.position);
            go.transform.SetPos(vec3);
            Assert.AreEqual(vec3, go.transform.position);
            
            go.SetPos(v => v + vec3);
            Assert.AreEqual(vec3 + vec3, go.transform.position);
            go.transform.SetPos(v => v + newVec);
            Assert.AreEqual(vec3 + vec3 + newVec, go.transform.position);
            
            var vec2 = new Vector2(12, 3);
            go.SetPos(vec2);
            Assert.AreEqual((Vector3) vec2, go.transform.position);
            
            go.SetPos(v => v + vec2);
            Assert.AreEqual((Vector3) (vec2 + vec2), go.transform.position);
            go.transform.SetPos(v => v + vec2);
            Assert.AreEqual((Vector3) (vec2 + vec2 + vec2), go.transform.position);
        }

        [Test]
        public void TestForEach()
        {
            var sum = 0;
            var orig = new List<int>{3,5,7,11};
            var uut = new List<int>{3,5,7,11};
            uut.ForEach(v => sum += v);
            Assert.AreEqual(sum, 3 + 5 + 7 + 11);
            CollectionAssert.AreEqual(orig, uut);
        }
        
        [Test]
        public void TestAndAlso()
        {
            var l1 = new List<Collider> {new MeshCollider(), new CapsuleCollider()};
            var l2 = new List<Collider> {new BoxCollider(), new SphereCollider()};

            var res = l1.AndAlso(l2).ToList();
            CollectionAssert.IsSupersetOf(res, l1);
            CollectionAssert.IsSupersetOf(res, l2);
        }

        [Test]
        public void TestShuffled()
        {
            var l1 = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var orig = new List<int>(l1);
            
            // does not modify original
            var shuffled = l1.Shuffled();
            CollectionAssert.AreEqual(l1, orig);
            
            // just modifies the order
            CollectionAssert.AreEquivalent(l1, shuffled);
            
            // is deterministic for same seed
            var rand1 = new System.Random((int)0xDEAD900L);
            var rand2 = new System.Random((int)0xDEAD900L);
            CollectionAssert.AreEqual(l1.Shuffled(rand1), l1.Shuffled(rand2));
            
            // shuffles at all
            CollectionAssert.AreNotEqual(l1, l1.Shuffled(rand1));
        }
        
    }
}
