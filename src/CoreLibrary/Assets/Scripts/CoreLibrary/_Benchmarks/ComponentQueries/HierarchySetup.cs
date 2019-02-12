using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CoreLibrary.Benchmarks.ComponentQueries
{
	public delegate void Mapper(GameObject obj, int depth);
	
	public struct Config
	{
		public string Name;

		public int MinParentDepth;
		public int MaxParentDepth;

		public int MinChildDepth;
		public int MaxChildDepth;

		public int MinNumberChildren;
		public int MaxNumberChildren;

		public float NoChildrenChance;

		public Mapper ChildMapper;
		public Mapper ParentMapper;
	}

	public static class HierarchySetup
	{


		private static int RandomBetween(int min, int max)
		{
			return (int) (UnityEngine.Random.value * (max - min) + min);
		}

		private static readonly List<GameObject> Objects = new List<GameObject>();

		public static GameObject SetupNewHierarchy(GameObject root, Config conf)
		{
			Objects.ForEach(Object.Destroy);
			Objects.Clear();
			
			Objects.Add(root);

			// add parents
			var parentDepth = RandomBetween(conf.MinParentDepth, conf.MaxParentDepth);
			var curr = root;
			for (var i = 0; i < parentDepth; ++i)
			{
				var parent = new GameObject {name = "Parent #" + (i + 1)};
				curr.transform.parent = parent.transform;
				conf.ParentMapper(parent, i + 1);
				curr = parent;
				Objects.Add(parent);
			}

			PopulateChildren(root, conf, conf.MinChildDepth, conf.MaxChildDepth);

			return root;
		}

		private static void PopulateChildren(GameObject root, Config conf, int minLevelsToGo, int maxLevelsToGo)
		{
			if (maxLevelsToGo == 0) return;

			var numChildren = RandomBetween(conf.MinNumberChildren, conf.MaxNumberChildren);
			if (minLevelsToGo > 0 || UnityEngine.Random.value > conf.NoChildrenChance)
				for (var i = 0; i < numChildren; ++i)
				{
					var child = new GameObject("Child #" + (i + 1));
					child.transform.parent = root.transform.parent;
					PopulateChildren(child, conf, minLevelsToGo - 1, maxLevelsToGo - 1);
					conf.ChildMapper(child, conf.MaxChildDepth - maxLevelsToGo + 1);
					Objects.Add(child);
				}
		}
	}
}
