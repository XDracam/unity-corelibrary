using System;
using System.Collections.Generic;
using CoreLibrary.Exceptions;
using NUnit.Framework;
using UnityEngine;

namespace CoreLibrary.Tests
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// </summary>
    public class TestComponentQueryExtensions 
    {
        [Test]
        public void TestSimpleIs()
        {
            var uut = new GameObject();
            uut.AddComponent<Rigidbody>();
            Assert.IsTrue(uut.Is<Rigidbody>());
            Assert.IsFalse(uut.Is<MeshRenderer>());
        }

        [Test]
        public void TestSimpleAs()
        {
            var uut = new GameObject();
            var rb = uut.AddComponent<Rigidbody>();
            Assert.AreSame(rb, uut.As<Rigidbody>());
            Assert.IsNull(uut.As<MeshRenderer>());
        }

        [Test]
        public void TestSimpleAssignComponent()
        {
            var uut = new GameObject();
            var rb = uut.AddComponent<Rigidbody>();
            
            Rigidbody expected;
            uut.AssignComponent(out expected);
            Assert.AreEqual(rb, expected);

            MeshRenderer rend;
            Assert.Throws<ComponentNotFoundException>(() => uut.AssignComponent(out rend));
        }
        
        [Test]
        public void TestSimpleAssignComponentOrAdd()
        {
            var uut = new GameObject();
            var rb = uut.AddComponent<Rigidbody>();
            
            Rigidbody expected;
            uut.AssignComponentOrAdd(out expected);
            Assert.AreEqual(rb, expected);

            MeshRenderer rend;
            uut.AssignComponentOrAdd(out rend);
            Assert.That(Util.IsNull(rend), Is.False);
        }

        [Test]
        public void TestSimpleAssignIfAbsent()
        {
            var uut = new GameObject();
            var rb = uut.AddComponent<Rigidbody>();
            var expected = default(Rigidbody);
            
            Assert.IsTrue(uut.AssignIfAbsent(ref expected));
            Assert.AreEqual(rb, expected);

            var oldExpected = expected;
            Assert.IsFalse(uut.AssignIfAbsent(ref expected));
            Assert.AreSame(oldExpected, expected);
            
            MeshRenderer rend = null;
            Assert.Throws<ComponentNotFoundException>(() => uut.AssignIfAbsent(ref rend));
        }
        
        [Test]
        public void TestSimpleAssignIfAbsentOrAdd()
        {
            var uut = new GameObject();
            var rb = uut.AddComponent<Rigidbody>();
            var expected = default(Rigidbody);
            
            Assert.IsTrue(uut.AssignIfAbsentOrAdd(ref expected));
            Assert.AreEqual(rb, expected);

            var oldExpected = expected;
            Assert.IsFalse(uut.AssignIfAbsentOrAdd(ref expected));
            Assert.AreSame(oldExpected, expected);
            
            MeshRenderer rend = null;
            Assert.That(uut.AssignIfAbsentOrAdd(ref rend), Is.True);
            Assert.That(Util.IsNull(rend), Is.False);
        }

        private static GameObject NewGameObject(GameObject parent, string name = null)
        {
            var res = name != null ? new GameObject(name) : new GameObject();
            res.transform.parent = parent.transform;
            return res;
        }

        // reusable for inWholeHierarchy
        private static void AssertParentsFound(Search scope)
        {
            Assert.AreNotEqual(scope, Search.InChildren);
            Assert.AreNotEqual(scope, Search.InObjectOnly);
            
            var parent3 = new GameObject("p3");
            var parent2 = NewGameObject(parent3, "p2");
            var parent1 = NewGameObject(parent2, "p1");
            var uut = NewGameObject(parent1, "uut");
            
            // test whole hierarchy traversed
            var col1 = parent3.AddComponent<BoxCollider>();
            Assert.AreSame(col1, uut.Find(obj => obj.As<Collider>(), scope));
            
            // test return first found
            var col2 = parent1.AddComponent<SphereCollider>();
            Assert.AreSame(col2, uut.Find(obj => obj.As<Collider>(), scope));
        }
        
        [Test]
        public void TestFindParents()
        {
            var uut = new GameObject("uut");
            var c1 = NewGameObject(uut, "c1");
            
            // test if children are ignored
            c1.AddComponent<BoxCollider>();
            Assert.IsNull(uut.Find(obj => obj.As<Collider>(), Search.InParents));
            
            AssertParentsFound(Search.InParents);
        }

        private static void AssertChildrenFound(Search scope)
        {
            Assert.AreNotEqual(scope, Search.InParents);
            Assert.AreNotEqual(scope, Search.InObjectOnly);
            
            var uut = new GameObject("uut");
            
            var c1 = NewGameObject(uut, "c1");
            var c2 = NewGameObject(uut, "c2");
            var c11 = NewGameObject(c1, "c11");
            var c12 = NewGameObject(c1, "c12");
            var c21 = NewGameObject(c2, "c21");
            var c211 = NewGameObject(c21, "c211");
            
            // test whole hierarchy traversed
            var col1 = c211.AddComponent<MeshCollider>();
            Assert.AreSame(col1, uut.Find(obj => obj.As<Collider>(), scope));
           
            // test eager stop
            var col2 = c12.AddComponent<BoxCollider>();
            Assert.AreSame(col2, uut.Find(obj => obj.As<Collider>(), scope));
            
            // test traversal of children in order
            var col3 = c11.AddComponent<SphereCollider>();
            Assert.AreSame(col3, uut.Find(obj => obj.As<Collider>(), scope));
        }

        [Test]
        public void TestFindChildren()
        {
            var p1 = new GameObject();
            var uut = NewGameObject(p1);
            
            // test if parents are ignored
            p1.AddComponent<BoxCollider>();
            Assert.IsNull(uut.Find(obj => obj.As<Collider>(), Search.InChildren));
            
            AssertChildrenFound(Search.InChildren);
        }

        [Test]
        public void TestFindWholeHierarchy()
        {
            AssertParentsFound(Search.InWholeHierarchy);
            AssertChildrenFound(Search.InWholeHierarchy);
        }

        public void TestFindSiblings()
        {
            var p1 = new GameObject();
            var uut = NewGameObject(p1);
            var s1 = NewGameObject(p1);
            var s2 = NewGameObject(p1);
            var c1 = NewGameObject(uut);
            var c2 = NewGameObject(s1);
    
            // test that parent and children are ignored
            p1.AddComponent<BoxCollider>();
            Assert.That(uut.Find(obj => obj.As<Collider>(), Search.InSiblings), Is.Null);

            c1.AddComponent<BoxCollider>();
            c2.AddComponent<BoxCollider>();
            Assert.That(uut.Find(obj => obj.As<Collider>(), Search.InSiblings), Is.Null);
            
            // test early stop
            var toFind = s1.AddComponent<BoxCollider>();
            s2.AddComponent<BoxCollider>();
            Assert.That(uut.Find(obj => obj.As<Collider>(), Search.InSiblings), Is.SameAs(toFind));
            
            // test object itself searched if no parent
            uut.transform.SetParent(null);
            var selfCollider = uut.AddComponent<BoxCollider>();
            Assert.That(uut.Find(obj => obj.As<Collider>(), Search.InSiblings), Is.EqualTo(selfCollider));
        }

        [Test]
        public void TestAllQuery()
        {
            var parent2 = new GameObject();
            var col1 = parent2.AddComponent<BoxCollider>();
            var col2 = parent2.AddComponent<SphereCollider>();
            var parent1 = NewGameObject(parent2);
            
            var uut = NewGameObject(parent1);
            
            var parentCollider = new List<Collider> {col1, col2};
            CollectionAssert.AreEquivalent(parentCollider, uut.All<Collider>(Search.InParents));

            var c1 = NewGameObject(uut);
            var c2 = NewGameObject(uut);
            var col3 = c2.AddComponent<BoxCollider>();
            var c11 = NewGameObject(c1);
            var col4 = c11.AddComponent<SphereCollider>();
                      NewGameObject(c1);
            var c21 = NewGameObject(c2);
            var c211 = NewGameObject(c21);
            var col5 = c211.AddComponent<MeshCollider>();

            var childCollider = new List<Collider> {col3, col4, col5};
            CollectionAssert.AreEquivalent(childCollider, uut.All<Collider>(Search.InChildren));
            
            var collider = new List<Collider> {col1, col2, col3, col4, col5};
            CollectionAssert.AreEquivalent(collider, uut.All<Collider>(Search.InWholeHierarchy));

            var selfCol = uut.AddComponent<BoxCollider>();
            CollectionAssert.AreEquivalent(new List<Collider>{selfCol}, uut.All<Collider>());     
        }
        
    
    }
}
