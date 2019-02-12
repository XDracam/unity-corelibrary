using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLibrary.Benchmarks.ComponentQueries
{
    public class TestCaseRunner : MonoBehaviour
    {
        private const int WarmUpRuns = 100;
        private const int BenchmarkRuns = 10000;
        
        public static void RunTestCase<T>(Config conf, Func<GameObject, T> fn, Action<T> evaluator)
        {
            RunTestCaseOnce(conf, fn, evaluator);
            
            var root = new GameObject();
            
            HierarchySetup.SetupNewHierarchy(root, conf);
            for (var i = 0; i < WarmUpRuns; ++i)
            {
                var res = fn(root);
                evaluator(res);
            }

            var start = DateTime.Now;
            for (var i = 0; i < BenchmarkRuns; ++i)
            {
                var res = fn(root);
                evaluator(res);
            }
            var end = DateTime.Now;
            
            Debug.Log(conf.Name + 
                      "\nAverage time for " + BenchmarkRuns + " runs: " + 
                      ((end - start).TotalSeconds / BenchmarkRuns) + 
                      "s ; Total time: " + (end - start));
        }
        
        public static void RunTestCaseOnce<T>(Config conf, Func<GameObject, T> fn, Action<T> evaluator)
        {
            var root = new GameObject();
            HierarchySetup.SetupNewHierarchy(root, conf);

            var start = DateTime.Now;
            var res = fn(root);
            evaluator(res);
            var end = DateTime.Now;
            
            Debug.Log(conf.Name + "\nTime for one run: " + (end - start));
        }
    }
}