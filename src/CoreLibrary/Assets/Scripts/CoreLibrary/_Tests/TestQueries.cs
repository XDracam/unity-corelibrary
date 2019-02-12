using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CoreLibrary.Tests
{
	/// <summary>
	/// Author: Cameron Reuschel
	/// </summary>
	public class TestQueries
	{
		private Dictionary<Type, HashSet<QueryableBaseBehaviour>> _registered
		{
			get
			{
				var field = typeof(Query)
					.GetField("_registered", BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null)
					return (Dictionary<Type, HashSet<QueryableBaseBehaviour>>) field.GetValue(Query.Instance);
				else return null;
			}
		}


		private HashSet<Queryable> _enabled
		{
			get
			{
				var field = typeof(Query)
					.GetField("_enabled", BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null) return (HashSet<Queryable>) field.GetValue(Query.Instance);
				else return null;
			}	
			
		}

		private class TestBehaviour : QueryableBaseBehaviour {}

		private GameObject go;
		private TestBehaviour tb;
		private Queryable q;
		private IEnumerator Setup()
		{
			TearDown();
			yield return null;
			go = new GameObject {name = "go"};
			tb = go.AddComponent<TestBehaviour>();
			q = go.GetComponent<Queryable>();
			yield return null; // wait for init
		}

		private GameObject go2;
		private TestBehaviour tb2;
		private Queryable q2;
		private IEnumerator Setup2()
		{
			TearDown();
			yield return null;
			go = new GameObject {name = "go1"};
			tb = go.AddComponent<TestBehaviour>();
			q = go.GetComponent<Queryable>();
			go2 = new GameObject {name = "go2"};
			tb2 = go2.AddComponent<TestBehaviour>();
			q2 = go2.GetComponent<Queryable>();
			yield return null;
		}

		private void TearDown()
		{
			Object.Destroy(go);
			if (go2 != null) Object.Destroy(go2);
			_registered.Clear();
			_enabled.Clear();
		}
		
		[UnityTest]
		public IEnumerator TestSingleRegistrationAndEnabling()
		{
			yield return Setup();
			Assert.Contains(typeof(TestBehaviour), _registered.Keys);
			Assert.Contains(tb, _registered[typeof(TestBehaviour)].ToList());
			Assert.Contains(q, _enabled.ToList());
			TearDown();
		}
		
		[UnityTest]
		public IEnumerator TestMultipleRegistrationAndEnabling()
		{
			yield return Setup2();
			Assert.Contains(typeof(TestBehaviour), _registered.Keys);
			Assert.Contains(tb, _registered[typeof(TestBehaviour)].ToList());
			Assert.Contains(q, _enabled.ToList());
			Assert.Contains(tb2, _registered[typeof(TestBehaviour)].ToList());
			Assert.Contains(q2, _enabled.ToList());
			TearDown();
		}

		[UnityTest]
		public IEnumerator TestDisablingAndReenabling()
		{
			yield return Setup2();
			
			go.SetActive(false);
			yield return null;
			Assert.Contains(typeof(TestBehaviour), _registered.Keys);
			Assert.Contains(tb, _registered[typeof(TestBehaviour)].ToList());
			CollectionAssert.AreEquivalent(new List<Queryable>{q2}, _enabled.ToList());
			
			go.SetActive(true);
			yield return null;
			Assert.Contains(typeof(TestBehaviour), _registered.Keys);
			Assert.Contains(tb, _registered[typeof(TestBehaviour)].ToList());
			Assert.Contains(q, _enabled.ToList());
			
			TearDown();
		}

		[UnityTest]
		public IEnumerator TestDeregistration()
		{
			yield return Setup2();
			
			Object.Destroy(go);
			yield return null;
			CollectionAssert.AreEquivalent(new List<QueryableBaseBehaviour> {tb2}, _registered[typeof(TestBehaviour)].ToList());
			CollectionAssert.AreEquivalent(new List<Queryable>{q2}, _enabled.ToList());
			
			TearDown();
		}

		[UnityTest]
		public IEnumerator TestAll()
		{
			yield return Setup();

			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb}, Query.All<TestBehaviour>().ToList());
			
			go.SetActive(false);
			yield return null;
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb}, Query.All<TestBehaviour>().ToList());
			
			TearDown();

			yield return Setup2();
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb, tb2}, Query.All<TestBehaviour>().ToList());
			
			go.SetActive(false);
			yield return null;
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb, tb2}, Query.All<TestBehaviour>().ToList());
			
			go2.SetActive(false);
			yield return null;
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb, tb2}, Query.All<TestBehaviour>().ToList());
			
			Object.Destroy(go);
			yield return null;
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb2}, Query.All<TestBehaviour>().ToList());
			
			Object.Destroy(go2);
			yield return null;
			Assert.IsEmpty(Query.All<TestBehaviour>().ToList());
			
			TearDown();
		}
		
		[UnityTest]
		public IEnumerator TestAllActive()
		{
			yield return Setup();

			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb}, Query.AllActive<TestBehaviour>().ToList());
			
			go.SetActive(false);
			yield return null;
			Assert.IsEmpty(Query.AllActive<TestBehaviour>().ToList());
			
			TearDown();

			yield return Setup2();
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb, tb2}, Query.AllActive<TestBehaviour>().ToList());
			
			go.SetActive(false);
			yield return null;
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb2}, Query.AllActive<TestBehaviour>().ToList());
			
			go2.SetActive(false);
			yield return null;
			Assert.IsEmpty(Query.AllActive<TestBehaviour>().ToList());
			
			go.SetActive(true);
			go2.SetActive(true);
			
			Object.Destroy(go);
			yield return null;
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tb2}, Query.AllActive<TestBehaviour>().ToList());
			
			Object.Destroy(go2);
			yield return null;
			Assert.IsEmpty(Query.AllActive<TestBehaviour>().ToList());
			
			TearDown();
		}

		private GameObject gow1;
		private GameObject gow2;
		private GameObject gow3;
		private TestBehaviour tbw1;
		private TestBehaviour tbw2;
		private TestBehaviour tbw3;
		
		private class TestA : QueryableBaseBehaviour {}
		private class TestB : QueryableBaseBehaviour {}
		private class TestC : QueryableBaseBehaviour {}

		private IEnumerator SetupWith()
		{
			TearDownWith();
			yield return null;
			
			gow1 = new GameObject {name = "gow1"};
			gow2 = new GameObject {name = "gow2"};
			gow3 = new GameObject {name = "gow3"};

			tbw1 = gow1.AddComponent<TestBehaviour>();
			tbw2 = gow2.AddComponent<TestBehaviour>();
			tbw3 = gow3.AddComponent<TestBehaviour>();
			
			// 1 A   C
			// 2 A B
			// 3 A B C

			gow1.AddComponent<TestA>();
			gow1.AddComponent<TestC>();
			gow2.AddComponent<TestA>();
			gow2.AddComponent<TestB>();
			gow3.AddComponent<TestA>();
			gow3.AddComponent<TestB>();
			gow3.AddComponent<TestC>();

			yield return null;
			yield return null;
		}

		private void TearDownWith()
		{
			new List<GameObject>{gow1, gow2, gow3}.ForEach(Object.Destroy);
		}

		[UnityTest]
		public IEnumerator TestAllWith()
		{
			yield return SetupWith();
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1, tbw2, tbw3}, Query.AllWith<TestBehaviour>(typeof(TestA)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw2, tbw3}, Query.AllWith<TestBehaviour>(typeof(TestB)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1, tbw3}, Query.AllWith<TestBehaviour>(typeof(TestC)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw3}, Query.AllWith<TestBehaviour>(typeof(TestA), typeof(TestB), typeof(TestC)).ToList());
			
			gow1.SetActive(false);
			gow2.SetActive(false);
			yield return null;
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1, tbw2, tbw3}, Query.AllWith<TestBehaviour>(typeof(TestA)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw2, tbw3}, Query.AllWith<TestBehaviour>(typeof(TestB)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1, tbw3}, Query.AllWith<TestBehaviour>(typeof(TestC)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw3}, Query.AllWith<TestBehaviour>(typeof(TestA), typeof(TestB), typeof(TestC)).ToList());

			Object.Destroy(gow3);
			yield return null;
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1, tbw2}, Query.AllWith<TestBehaviour>(typeof(TestA)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw2}, Query.AllWith<TestBehaviour>(typeof(TestB)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1}, Query.AllWith<TestBehaviour>(typeof(TestC)).ToList());
			Assert.IsEmpty(Query.AllWith<TestBehaviour>(typeof(TestA), typeof(TestB), typeof(TestC)).ToList());

			TearDownWith();
		}
		
		[UnityTest]
		public IEnumerator TestAllActiveWith()
		{
			yield return SetupWith();
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1, tbw2, tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestA)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw2, tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestB)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw1, tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestC)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestA), typeof(TestB), typeof(TestC)).ToList());
			
			gow1.SetActive(false);
			gow2.SetActive(false);
			yield return null;
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestA)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestB)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestC)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw3}, Query.AllActiveWith<TestBehaviour>(typeof(TestA), typeof(TestB), typeof(TestC)).ToList());

			gow3.SetActive(true);
			gow2.SetActive(true);
			
			gow1.SetActive(false);

			yield return null;
			
			Object.Destroy(gow3);
			yield return null;
			
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw2}, Query.AllActiveWith<TestBehaviour>(typeof(TestA)).ToList());
			CollectionAssert.AreEquivalent(new List<TestBehaviour>{tbw2}, Query.AllActiveWith<TestBehaviour>(typeof(TestB)).ToList());
			Assert.IsEmpty(Query.AllActiveWith<TestBehaviour>(typeof(TestC)).ToList());
			Assert.IsEmpty(Query.AllActiveWith<TestBehaviour>(typeof(TestA), typeof(TestB), typeof(TestC)).ToList());

			TearDownWith();
		}
		
		[UnityTest]
		public IEnumerator TestMultiRegistration()
		{
			TearDown();
			TearDownWith();
			
			var goA = new GameObject {name = "goA"};
			var goBC = new GameObject {name = "goBC"};
			
			var ta = goA.AddComponent<TestA>();
			var tb = goBC.AddComponent<TestB>();
			var tc = goBC.AddComponent<TestC>();

			var qa = goA.GetComponent<Queryable>();
			var qbc = goBC.GetComponent<Queryable>();
			
			yield return null;
			
			Assert.Contains(ta, _registered[typeof(TestA)].ToList());
			Assert.Contains(tb, _registered[typeof(TestB)].ToList());
			Assert.Contains(tc, _registered[typeof(TestC)].ToList());
			
			CollectionAssert.AreEquivalent(new List<Queryable>{qa, qbc}, _enabled.ToList());
			
			Object.Destroy(goA);
			Object.Destroy(goBC);
		}

		[UnityTest]
		public IEnumerator TestMultiQueries()
		{
			var goA = new GameObject {name = "goA"};
			var goBC = new GameObject {name = "goBC"};

			var ta = goA.AddComponent<TestA>();
			goBC.AddComponent<TestB>();
			var tc = goBC.AddComponent<TestC>();

			yield return null;
			
			CollectionAssert.AreEquivalent(new List<TestA> {ta}, Query.All<TestA>().ToList());
			CollectionAssert.AreEquivalent(new List<TestC> {tc}, Query.AllWith<TestC>(typeof(TestB)).ToList());

			Object.Destroy(goA);
			Object.Destroy(goBC);
		}
		
		abstract class BaseTestBehaviour : QueryableBaseBehaviour {}
		class TestClassA : BaseTestBehaviour {}
		class TestClassB : BaseTestBehaviour {}
		abstract class BonusBase : QueryableBaseBehaviour {}
		class TestClassC : BonusBase {}

		[UnityTest]
		public IEnumerator TestQuerySuperclasses()
		{
			var goA = new GameObject {name = "goA"};
			var goB = new GameObject {name = "goB"};

			var ta = goA.AddComponent<TestClassA>();
			var tb = goB.AddComponent<TestClassB>();
			goA.AddComponent<TestClassC>();

			yield return null;
			
			CollectionAssert.AreEquivalent(new List<BaseTestBehaviour> {ta, tb}, Query.All<BaseTestBehaviour>().ToList());

			CollectionAssert.AreEquivalent(new List<BaseTestBehaviour> {ta},
				Query.AllWith<BaseTestBehaviour>(typeof(BonusBase)).ToList());
			
			Object.Destroy(goA);
			Object.Destroy(goB);
		}
		
	}
}
