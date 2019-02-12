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
    public class TestLazySingleton {

        public class UniqueComponent : LazySingleton<UniqueComponent>
        {
            public static int AwakeCount = 0;
            public static int StartCount = 0;
            private void Awake() { AwakeCount++; }
            private void Start() { StartCount++; }
        }

        [UnityTest]
        public IEnumerator TestInstantiatesOnNone()
        {
            CollectionAssert.IsEmpty(Object.FindObjectsOfType<UniqueComponent>());
            var _ = UniqueComponent.Instance;
            Assert.NotNull(_);
            yield return null;
            Assert.AreEqual(1, UniqueComponent.AwakeCount);
            Assert.AreEqual(1, UniqueComponent.StartCount);
            Object.Destroy(_);
        }

        [UnityTest]
        public IEnumerator TestWorksWithOne()
        {
            var _ = new GameObject();
            var comp = _.AddComponent<UniqueComponent>();
            yield return null;
            Assert.AreSame(comp, UniqueComponent.Instance);
            Object.Destroy(_);
        }

        [UnityTest]
        public IEnumerator TestThrowsOnMultiple()
        {
            var _1 = new GameObject();
            _1.AddComponent<UniqueComponent>();
            var _2 = new GameObject();
            _2.AddComponent<UniqueComponent>();
            yield return null;
            UniqueComponent _;
            Assert.Throws<WrongSingletonUsageException>(() => _ = UniqueComponent.Instance);
            Object.Destroy(_1);
            Object.Destroy(_2);
        }
    }
}
