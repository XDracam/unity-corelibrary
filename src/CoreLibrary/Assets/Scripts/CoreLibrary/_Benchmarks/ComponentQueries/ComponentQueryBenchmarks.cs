using System;
using System.Collections;
using System.Collections.Generic;
using CoreLibrary.Benchmarks.ComponentQueries;
using UnityEngine;

namespace CoreLibrary.Benchmarks.ComponentQueries
{
	public class ComponentQueryBenchmarks : MonoBehaviour
	{

		private readonly List<Config> _linearSingle = new List<Config>
		{
			TestCases.LinearLastComponent(200),
//			TestCases.LinearLastComponent(10000),
//			TestCases.LinearLastComponent(100000)
		};
		
		private readonly List<Config> _linearSingleChildrenOnly = new List<Config>
		{
			TestCases.LinearLastComponent(200),
//			TestCases.LinearLastComponent(10000),
//			TestCases.LinearLastComponent(100000)
		};

		private readonly List<Config> _linearRandom = new List<Config>
		{
			TestCases.LinearRandomComponents(200, .2f),
			TestCases.LinearRandomComponents(200, .5f),
			TestCases.LinearRandomComponents(200, .8f),
//			TestCases.LinearRandomComponents(10000, .2f),
//			TestCases.LinearRandomComponents(10000, .5f),
//			TestCases.LinearRandomComponents(10000, .8f),
//			TestCases.LinearRandomComponents(100000, .2f),
//			TestCases.LinearRandomComponents(100000, .5f),
//			TestCases.LinearRandomComponents(100000, .8f),
		};

		private readonly List<Config> _spreadRandom = new List<Config>
		{
//			TestCases.SpreadRandomComponents(10, 100, .2f),
//			TestCases.SpreadRandomComponents(10, 100, .5f),
//			TestCases.SpreadRandomComponents(10, 100, .8f),
//			TestCases.SpreadRandomComponents(1000, 10000, .2f),
//			TestCases.SpreadRandomComponents(1000, 10000, .5f),
//			TestCases.SpreadRandomComponents(1000, 10000, .8f),
		};

		private static readonly List<TestComponent> ForceEvaluationList = new List<TestComponent>();
		private Action<TestComponent> _singleEvaluator = _ => { };
		private Action<IEnumerable<TestComponent>> _multiEvaluator = it =>
		{
			ForceEvaluationList.AddRange(it);
			ForceEvaluationList.ForEach(_ => {});
			ForceEvaluationList.Clear();
		};

		private IEnumerator Start() {
			
			Debug.LogWarning("Times for parent hierarchy - single component");
			
			Debug.Log("Regular Unity");
			
			_linearSingle.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.GetComponentInParent<TestComponent>(), _singleEvaluator));

			yield return null;
			Debug.Log("CoreLibrary");
			
			_linearSingle.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.As<TestComponent>(Search.InParents), _singleEvaluator));
			
			Debug.LogWarning("Times for child hierarchy - single component");
			
			yield return null;
			Debug.Log("Regular Unity");
			
			_linearSingle.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.GetComponentInChildren<TestComponent>(), _singleEvaluator));
			
			yield return null;
			Debug.Log("CoreLibrary");
			
			_linearSingle.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.As<TestComponent>(Search.InChildren), _singleEvaluator));
			
			Debug.LogWarning("Times for whole hierarchy - single component");
			
			yield return null;
			Debug.Log("Regular Unity");
			
			_linearSingleChildrenOnly.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go =>
				{
					var inParent = go.GetComponentInParent<TestComponent>();
					if (inParent != null) return inParent;
					return go.GetComponentInChildren<TestComponent>();
				}, _singleEvaluator));
			
			yield return null;
			Debug.Log("CoreLibrary");
			
			_linearSingleChildrenOnly.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.As<TestComponent>(Search.InWholeHierarchy), _singleEvaluator));
			
			Debug.LogWarning("Times for parent hierarchy - all components");

			yield return null;
			Debug.Log("Regular Unity");
			
			_linearRandom.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.GetComponentsInParent<TestComponent>(), _multiEvaluator));
			
			yield return null;
			Debug.Log("CoreLibrary");
			
			_linearRandom.ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.All<TestComponent>(Search.InParents), _multiEvaluator));
			
			Debug.LogWarning("Times for child hierarchy - all components");

			yield return null;
			Debug.Log("Regular Unity");
			
			_linearRandom.AndAlso(_spreadRandom).ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.GetComponentsInChildren<TestComponent>(), _multiEvaluator));
			
			yield return null;
			Debug.Log("CoreLibrary");
			
			_linearRandom.AndAlso(_spreadRandom).ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.All<TestComponent>(Search.InChildren), _multiEvaluator));
			
			Debug.LogWarning("Times for whole hierarchy - all components");

			yield return null;
			Debug.Log("Regular Unity");{}
			
			_linearRandom.AndAlso(_spreadRandom).ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go =>
				{
					var inCh = go.GetComponentsInChildren<TestComponent>();
					var inPs = go.GetComponentsInParent<TestComponent>();
					return inCh.AndAlso(inPs);
				}, _multiEvaluator));
			
			yield return null;
			Debug.Log("CoreLibrary");
			
			_linearRandom.AndAlso(_spreadRandom).ForEach(
				tc => TestCaseRunner.RunTestCase(tc, go => go.All<TestComponent>(Search.InWholeHierarchy), _multiEvaluator));

		}
	}
}
