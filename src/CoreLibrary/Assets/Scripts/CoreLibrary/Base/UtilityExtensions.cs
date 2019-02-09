using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// The class holding all nonspecific extension methods in the core library.
    /// </summary>
    public static class UtilityExtensions
    {
        //==============================
        //===== GENERAL EXTENSIONS =====
        //==============================

        /// <summary>
        /// An IEnumerable containing all the children of
        /// this Transform in order. <b>Iterable only once</b>.
        /// </summary>
        [Pure] public static IEnumerable<Transform> GetChildren(this Transform t)
        {
            for (var i = 0; i < t.childCount; ++i)
            {
                yield return t.GetChild(i);
            }
        }

        /// <summary>
        /// A game object is not percievable if it has no active collider and renderer.
        /// Used instead of deactivation to enable coroutines and sounds to continue to play.
        /// </summary>
        public static void SetPerceivable(this GameObject gameObject, bool state)
        {
            if (!(gameObject.activeSelf && gameObject.activeInHierarchy))
                return;
            var allRenderer = gameObject.All<Renderer>(Search.InChildren);
            var allCollider = gameObject.All<Collider>(Search.InChildren);
            foreach(var rend in allRenderer) rend.enabled = state;
            foreach(var col in allCollider) col.enabled = state;
        }

        //=============================
        //===== VECTOR EXTENSIONS =====
        //=============================
        
        /// <returns>The same vector with it's x coordinate set to the specified value.</returns>
        [Pure] public static Vector2 WithX(this Vector2 vec, float x)
        {
            return new Vector3(x, vec.y);
        }

        /// <returns>The same vector with it's y coordinate set to the specified value.</returns>
        [Pure] public static Vector2 WithY(this Vector2 vec, float y)
        {
            return new Vector3(vec.x, y);
        }
        
        /// <returns>The same vector with it's x coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector2 WithX(this Vector2 vec, [NotNull] Func<float, float> fx)
        {
            return new Vector2(fx(vec.x), vec.y);
        }

        /// <returns>The same vector with it's y coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector2 WithY(this Vector2 vec, [NotNull] Func<float, float> fy)
        {
            return new Vector2(vec.x, fy(vec.y));
        }

        /// <returns>The same vector with it's x coordinate set to the specified value.</returns>
        [Pure] public static Vector3 WithX(this Vector3 vec, float x)
        {
            return new Vector3(x, vec.y, vec.z);
        }

        /// <returns>The same vector with it's y coordinate set to the specified value.</returns>
        [Pure] public static Vector3 WithY(this Vector3 vec, float y)
        {
            return new Vector3(vec.x, y, vec.z);
        }

        /// <returns>The same vector with it's z coordinate set to the specified value.</returns>
        [Pure] public static Vector3 WithZ(this Vector3 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }

        /// <returns>The same vector with it's x coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector3 WithX(this Vector3 vec, [NotNull] Func<float, float> fx)
        {
            return new Vector3(fx(vec.x), vec.y, vec.z);
        }

        /// <returns>The same vector with it's y coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector3 WithY(this Vector3 vec, [NotNull] Func<float, float> fy)
        {
            return new Vector3(vec.x, fy(vec.y), vec.z);
        }

        /// <returns>The same vector with it's z coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector3 WithZ(this Vector3 vec, [NotNull] Func<float, float> fz)
        {
            return new Vector3(vec.x, vec.y, fz(vec.z));
        }

        //=================================
        //===== LINQ STYLE EXTENSIONS =====
        //=================================

        /// <summary>
        /// Executes the specified action with side effects for each element in this sequence,
        /// thereby consuming the sequence if it was only iterable once.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var item in sequence) action(item);
        }

        /// <summary>
        /// Equal to calling <code>.Select(mapping).Where(v => v != null)</code>
        /// <br/>
        /// Nice for calling functions that may return no result such as
        /// <code>.Collect(v => v.As&lt;Whatever&gt;())</code>
        /// </summary>
        public static IEnumerable<TRes> Collect<T, TRes>(this IEnumerable<T> sequence, Func<T, TRes> mapping)
        {
            return sequence.Select(mapping).Where(v => v != null);
        }
        
        /// <summary>
        /// Equal to calling <code>.SelectMany(mapping).Where(v => v != null)</code>
        /// <br/>
        /// Basically the flattening equivalent to <see cref="Collect{T,TRes}"/>
        /// </summary>
        public static IEnumerable<TRes> CollectMany<T, TRes>(
            this IEnumerable<T> sequence, Func<T, IEnumerable<TRes>> mapping)
        {
            return sequence.SelectMany(mapping).Where(v => v != null);
        }

        /// <summary>
        /// Merges two sequences in a LINQ call chain without having to drop out of it.
        /// When the concrete types of the two sequences differ, then one must specify
        /// the desired common supertype explicitly, as seen in the example.
        /// </summary>
        /// <example><code>
        /// List&lt;Component&gt; foo = gameObject
        ///           .All&lt;T&gt;()
        ///           .AndAlso&lt;Component&gt;(gameObject.All&lt;U&gt;())
        ///           .ToList();
        /// </code></example>
        public static IEnumerable<T> AndAlso<T>(this IEnumerable<T> sequence, IEnumerable<T> other)
        {
            foreach (var x in sequence) yield return x;
            foreach (var x in other) yield return x;
        }

        private static readonly System.Random Rng = new System.Random();
        /// <summary>
        /// Shuffles this sequence, yielding a <b>new</b> IEnumerable with all elements in random order.
        /// Uses the <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Fisher–Yates algorithm</a>.
        /// <br/>If the passed IEnumerable is only iterable once it is consumed in the process.
        /// </summary>
        public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> l, System.Random random = null)
        {
            if (random == null) random = Rng;
            var list = new List<T>(l);
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}