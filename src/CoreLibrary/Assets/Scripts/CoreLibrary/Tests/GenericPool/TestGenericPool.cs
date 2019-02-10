using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CoreLibrary.Tests.GenericPool
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// </summary>
    public class TestReusable : Reusable
    {
        public int TimesReused = 0;
        public int TimesAfterReuseCalled = 0;
        public int TimesReuseRequested = 0;
        public override void ResetForReuse()
        {
            TimesReused += 1;
        }

        public override void AfterReuse()
        {
            TimesAfterReuseCalled += 1;
        }

        public override void ReuseRequested()
        {
            TimesReuseRequested += 1;
        }
    }
    
    /// <summary>
    /// Author: Cameron Reuschel
    /// </summary>
    public class TestGenericPool
    {
        private static CoreLibrary.GenericPool NewPool(int capacity, Type toAdd = null)
        {
            var go = new GameObject();
            var pool = go.AddComponent<CoreLibrary.GenericPool>();
            pool.Capacity = capacity;
            pool.GrowRate = 1;
            var templateGo = new GameObject {name = "Template Object"};
            pool.Template = (TestReusable) templateGo.AddComponent(toAdd ?? typeof(TestReusable));
            return pool;
        }

        [Test]
        public void TestInit()
        {
            var go = new GameObject();
            var pool = go.AddComponent<CoreLibrary.GenericPool>();
            Assert.Throws<NoTemplateException>(() => pool.Init());
            var templateGo = new GameObject {name = "Template Object"};
            pool.Template = templateGo.AddComponent<TestReusable>();
            pool.Capacity = -1;
            Assert.DoesNotThrow(() => pool.Init());
            Assert.AreEqual(0, pool.Capacity);
        }
        
        [Test]
        public void TestProperReuse()
        {
            var uut = NewPool(1);
            uut.Init();
            // check that reuse is happening instead of growing
            var fst = uut.RequestItem().As<TestReusable>();
            Assert.AreEqual(fst.TimesReused, 1);
            Assert.AreEqual(fst.TimesAfterReuseCalled, 1);
            Assert.AreEqual(fst.TimesReuseRequested, 0);
            fst.FreeForReuse();
            var snd = uut.RequestItem().As<TestReusable>();
            Assert.AreSame(fst, snd);
            Assert.AreEqual(snd.TimesReused, 2);
            Assert.AreEqual(snd.TimesAfterReuseCalled, 2);
            Assert.AreEqual(snd.TimesReuseRequested, 0);
            // but only when object is marked as reusable
            var trd = uut.RequestItem().As<TestReusable>();
            Assert.AreNotSame(trd, snd);
            Assert.AreEqual(trd.TimesReused, 1);
            Assert.AreEqual(trd.TimesAfterReuseCalled, 1);
            Assert.AreEqual(fst.TimesReuseRequested, 1);
            // further ensure that inactive objects are prioritized
            snd.FreeForReuse();
            trd.FreeForReuse();
            trd.gameObject.SetActive(false);
            var fth = uut.RequestItem().As<TestReusable>();
            Assert.AreEqual(uut.Capacity, 2);
            Assert.AreSame(fth, trd);
        }

        private static int GetBufferSize(CoreLibrary.GenericPool pool)
        {
            return ((List<Reusable>) 
                typeof(CoreLibrary.GenericPool)
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .First(f => f.Name == "_buffer")
                    .GetValue(pool)).Count;
        }
            
        [UnityTest]
        public IEnumerator TestProperGrowth()
        {
            var uut = NewPool(0);
            uut.Init();
            uut.RequestItem();
            Assert.AreEqual(uut.Capacity, 1);
            Assert.AreEqual(GetBufferSize(uut), 1);
            uut.RequestItem();
            Assert.AreEqual(uut.Capacity, 2);
            Assert.AreEqual(GetBufferSize(uut), 2);
            uut.RequestItem();
            Assert.AreEqual(uut.Capacity, 4);
            Assert.AreEqual(GetBufferSize(uut), 3);
            yield return null;
            Assert.AreEqual(GetBufferSize(uut), 4);
        }

        [UnityTest]
        public IEnumerator TestRefill()
        {
            var uut = NewPool(10);
            uut.Init();
            uut.GrowRate = 0.3f;
            for (var i = 0; i < 11; ++i) uut.RequestItem();
            Assert.AreEqual(13, uut.Capacity);
            Assert.AreEqual(11, GetBufferSize(uut));
            yield return null;
            Assert.AreEqual(12, GetBufferSize(uut));
            yield return null;
            Assert.AreEqual(13, GetBufferSize(uut));
        }

        [Test]
        public void TestFailOnZeroGrowRate()
        {
            var uut = NewPool(0);
            uut.GrowRate = 0;
            uut.Init();
            Assert.Throws<PoolOutOfItemsException>(() => uut.RequestItem());
        }

        private class AlwaysReusable : TestReusable
        {
            public override void ReuseRequested()
            {
                base.ReuseRequested();
                FreeForReuse();
            }
        }

        [Test]
        public void TestReuseRequesting()
        {
            var uut = NewPool(1, typeof(AlwaysReusable));
            uut.GrowRate = 1;
            uut.Init();
            var fst = uut.RequestItem().As<TestReusable>();
            Assert.AreEqual(fst.TimesReused, 1);
            Assert.AreEqual(fst.TimesAfterReuseCalled, 1);
            Assert.AreEqual(fst.TimesReuseRequested, 0);
            var snd = uut.RequestItem().As<TestReusable>();
            Assert.AreSame(fst, snd);
            Assert.AreEqual(fst.TimesReused, 2);
            Assert.AreEqual(fst.TimesAfterReuseCalled, 2);
            Assert.AreEqual(fst.TimesReuseRequested, 1);
            Assert.AreEqual(1, GetBufferSize(uut));
            var trd = uut.RequestItem().As<TestReusable>();
            Assert.AreSame(snd, trd);
            Assert.AreEqual(fst.TimesReused, 3);
            Assert.AreEqual(fst.TimesAfterReuseCalled, 3);
            Assert.AreEqual(fst.TimesReuseRequested, 2);
            Assert.AreEqual(1, GetBufferSize(uut));
        }
    }
}