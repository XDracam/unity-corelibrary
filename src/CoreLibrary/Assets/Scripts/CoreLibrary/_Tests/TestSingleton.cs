using System;
using System.Collections;
using CoreLibrary.Exceptions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CoreLibrary.Tests
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// </summary>
    public class TestSingleton {

        public class UniqueComponent : Singleton<UniqueComponent> { }

        [Test]
        public void TestThrowsOnNone()
        {
            UniqueComponent _;
            Assert.Throws<WrongSingletonUsageException>(() => _ = UniqueComponent.Instance);
        }

        [UnityTest]
        public IEnumerator TestWorksWithOne()
        {
            var go = new GameObject();
            var comp = go.AddComponent<UniqueComponent>();
            yield return null;
            Assert.AreSame(comp, UniqueComponent.Instance);
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator TestThrowsOnMultiple()
        {
            var go1 = new GameObject();
            go1.AddComponent<UniqueComponent>();
            var go2 = new GameObject();
            go2.AddComponent<UniqueComponent>();
            yield return null;
            UniqueComponent _;
            Assert.Throws<WrongSingletonUsageException>(() => _ = UniqueComponent.Instance);
            Object.Destroy(go1);
            Object.Destroy(go2);
        }
    }
}
