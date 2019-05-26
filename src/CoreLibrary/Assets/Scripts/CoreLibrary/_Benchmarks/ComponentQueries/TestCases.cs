using System.Collections.Generic;
using UnityEngine;

namespace CoreLibrary.Benchmarks.ComponentQueries
{
    public sealed class TestComponent : MonoBehaviour
    {
    }

    public static class TestCases
    {
        // inclusive bounds
        private static Mapper AddComponentAtLevels<T>(int min, int max, float? chance = null) where T : Component
        {
            return (obj, i) =>
            {
                if ((chance == null || Random.value < chance) && i >= min && i <= max)
                {
                    obj.AddComponent<T>();
                }
            };
        }

        public static readonly List<Config> All = new List<Config>
        {
            LinearLastComponent(1000), 
            LinearLastComponent(10000), 
            LinearLastComponent(100000),
            LinearLastComponent(1000, true), 
            LinearLastComponent(10000, true), 
            LinearLastComponent(100000, true),
            LinearRandomComponents(1000, .2f),
            LinearRandomComponents(1000, .5f),
            LinearRandomComponents(1000, .8f),
            LinearRandomComponents(10000, .2f),
            LinearRandomComponents(10000, .5f),
            LinearRandomComponents(10000, .8f),
            LinearRandomComponents(100000, .2f),
            LinearRandomComponents(100000, .5f),
            LinearRandomComponents(100000, .8f),
            SpreadRandomComponents(100, 1000, .2f),
            SpreadRandomComponents(100, 1000, .5f),
            SpreadRandomComponents(100, 1000, .8f),
            SpreadRandomComponents(1000, 10000, .2f),
            SpreadRandomComponents(1000, 10000, .5f),
            SpreadRandomComponents(1000, 10000, .8f),
        };

        public static Config LinearLastComponent(int depth, bool childrenOnly = false)
        {
            return new Config
            {
                Name = depth + " parents and non-branching children with component at the last one each" + 
                    (childrenOnly ? ", component only on children" : ""),
                MinChildDepth = depth,
                MaxChildDepth = depth,
                MinParentDepth = depth,
                MaxParentDepth = depth,
                MinNumberChildren = 1,
                MaxNumberChildren = 1,
                NoChildrenChance = 0,
                ChildMapper = AddComponentAtLevels<TestComponent>(depth, depth),
                ParentMapper = childrenOnly ? (go, i) => {} : AddComponentAtLevels<TestComponent>(depth, depth),
            };
        }

        public static Config LinearRandomComponents(int depth, float chance)
        {
            return new Config
            {
                Name = depth + " parents and non-branching children with " + (chance * 100) + "% chance for component",
                MinChildDepth = depth,
                MaxChildDepth = depth,
                MinParentDepth = depth,
                MaxParentDepth = depth,
                MinNumberChildren = 1,
                MaxNumberChildren = 1,
                NoChildrenChance = 0,
                ChildMapper = AddComponentAtLevels<TestComponent>(1, depth, chance),
                ParentMapper = AddComponentAtLevels<TestComponent>(1, depth, chance),
            };
        }

        public static Config SpreadRandomComponents(int minDepth, int maxDepth, float componentChance)
        {
            return new Config
            {
                Name = "Between " + minDepth + " and " + maxDepth + " random parents and branched children with " +
                       (componentChance * 100) + "% chance for component",
                MinChildDepth = minDepth,
                MaxChildDepth = maxDepth,
                MinParentDepth = minDepth,
                MaxParentDepth = maxDepth,
                MinNumberChildren = 1,
                MaxNumberChildren = 100,
                NoChildrenChance = .5f,
                ChildMapper = AddComponentAtLevels<TestComponent>(1, maxDepth, componentChance),
                ParentMapper = AddComponentAtLevels<TestComponent>(1, maxDepth, componentChance),
            };
        }
    }
}