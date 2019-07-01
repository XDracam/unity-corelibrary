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
        /// An IEnumerable containing all the children of this Transform in order.
        /// It is safe to modify the transforms children during iteration.
        /// <b>Iterable only once</b>.
        /// </summary>
        [Pure] public static IEnumerable<Transform> GetChildren(this Transform transform)
        {
            foreach (Transform child in transform) yield return child;
        }

        /// <inheritdoc cref="Util.IsNull{T}"/>
        [Pure]
        public static bool IsNull<T>(this T value) where T : class
        {
            return Util.IsNull(value);
        }

        /// <summary>
        /// A game object is not perceivable if it has no active collider and renderer.
        /// Used instead of deactivation to enable coroutines and sounds to continue to play.
        /// </summary>
        public static void SetPerceivable(this GameObject gameObject, bool state)
        {
            if (!(gameObject.activeSelf && gameObject.activeInHierarchy))
                return;
            var allRenderer = gameObject.All<Renderer>(Search.InChildren);
            var allCollider = gameObject.All<Collider>(Search.InChildren);
            var allCollider2D = gameObject.All<Collider2D>(Search.InChildren);
            foreach(var rend in allRenderer) rend.enabled = state;
            foreach(var col in allCollider) col.enabled = state;
            foreach(var col in allCollider2D) col.enabled = state;

            var queryables = gameObject.All<QueryableBaseBehaviour>(Search.InChildren);
            foreach (var q in queryables) q.Queryable.enabled = state;
        }
        
        /// <summary>
        /// Destroys the object in the recommended way.
        /// Safe to use in code that is shared between editor and runtime.
        /// <br/>
        /// In Unity, it is recommended to always use Destroy(obj). However,
        /// when used in editor code, object destruction is delayed forever.
        /// For this reason, DestroyImmediate(obj) must be used in editor code.
        /// This function encapsulates the preprocessor code necessary 
        /// to determine whether the code is being run in editor mode.
        /// <br/>
        /// Note that transforms cannot be destroyed. When a user still
        /// attempts to destroy a transform, a warning is logged and the
        /// transforms game object is destroyed instead.
        /// </summary>
        /// <param name="obj"></param>
        public static void SafeDestroy(this UnityEngine.Object obj) {
            var transform = obj as Transform;
            if (transform != null) {
                transform.gameObject.SafeDestroy();
                Debug.LogWarning(
                    "Calling SafeDestroy() on a transform. " +
                    "Destroying the GameObject instead, since transforms cannot be destroyed.");
                return;
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(obj);
                return;
            }
#endif
            UnityEngine.Object.Destroy(obj);
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
        
        /// <returns>True if the specified sequence contains no elements, false otherwise.</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> sequence) { return !sequence.Any(); }
        
        /// <returns>False if the specified sequence contains no elements, true otherwise.</returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> sequence) { return sequence.Any(); }
        
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