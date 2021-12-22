using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.Exceptions;
using JetBrains.Annotations;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Authors: Cameron Reuschel, Daniel Götz
    /// <br/><br/>
    /// Holds a number of extension methods to find a game object's
    /// components of a certain type in a concise and flexible manner.
    /// <br/><br/>
    /// All methods include a parameter of type <see cref="Search"/>,
    /// which enables you to search through all parents until the scene
    /// root, recursively through all children all both until a component
    /// of the specified type can be found.  
    /// </summary>
    // ReSharper disable all InvalidXmlDocComment
    public static class ComponentQueryExtensions
    {
        private static T SearchParents<T>(GameObject go, Func<GameObject, T> mapper) where T : class
        {
            while (true)
            {
                var res = mapper(go);
                if (!Util.IsNull(res)) return res;
                if (Util.IsNull(go.transform.parent)) return null;
                go = go.transform.parent.gameObject;
            }
        }

        private static T SearchChildren<T>(GameObject go, Func<GameObject, T> mapper) where T : class
        {
            var res = mapper(go);
            if (!Util.IsNull(res)) return res;
            foreach (var child in go.transform.GetChildren())
            {
                var recres = SearchChildren(child.gameObject, mapper);
                if (!Util.IsNull(recres)) return recres;
            }

            return null;
        }

        private static T SearchSiblings<T>(GameObject go, Func<GameObject, T> mapper) where T : class
        {
            if (go.transform.parent == null) return mapper(go);
            foreach (var child in go.transform.parent.GetChildren())
            {
                var res = mapper(child.gameObject);
                if (!Util.IsNull(res)) return res;
            }

            return null;
        }

        /// <summary>
        /// This method enables you to search an object's hierarchy
        /// for a more complex conditions. The passed function <paramref name="fn"/>
        /// is applied to every object in the specified search path until
        /// it returns a value that is not null. This value is returned.
        /// When nothing is found, null is returned.
        /// </summary>
        /// <example>
        /// T GetComponentInParent<T>(GameObject go) =>
        ///     go.Find(obj => obj.As<T>(), Search.InParents);
        /// </example>
        /// <param name="fn">
        /// This function is applied to every game object in the search path
        /// until it returns a value that is not null. This result is returned.
        /// </param>
        /// <param name="where">
        /// The search scope if <paramref name="fn"/> returns null for the object itself.
        /// </param>
        /// <returns
        /// >The first non-null result from applying <paramref name="fn"/>
        /// to each game object in the search path, or null if nothing was found.
        /// </returns>
        [CanBeNull]
        public static T Find<T>(this GameObject go, [NotNull] Func<GameObject, T> fn, Search where)
            where T : class
        {
            switch (where)
            {
                case Search.InObjectOnly:
                    return fn(go);
                case Search.InChildren:
                    return SearchChildren(go, fn);
                case Search.InParents:
                    return SearchParents(go, fn);
                case Search.InSiblings:
                    return SearchSiblings(go, fn);
                case Search.InWholeHierarchy:
                    if (go.transform.parent != null)
                    {
                        var parentSearch = SearchParents(go.transform.parent.gameObject, fn);
                        if (!Util.IsNull(parentSearch)) return parentSearch;
                    }

                    return SearchChildren(go, fn);
                default:
                    throw new UnsupportedSearchException(where);
            }
        }

        /// <inheritdoc cref="Find{T}(UnityEngine.GameObject,System.Func{UnityEngine.GameObject,T},CoreLibrary.Search)"/>
        [CanBeNull]
        public static T Find<T>(this Transform tr, [NotNull] Func<GameObject, T> fn, Search where = Search.InObjectOnly)
            where T : class
        {
            return tr.gameObject.Find<T>(fn, where);
        }
        
        /// <example>
        /// Before C# 7
        /// <code>
        /// Collider result;
        /// if(gameObject.Is&lt;Collider&gt;(out result))
        ///     result.trigger = true;
        /// </code>
        /// With C# 7
        /// <code>
        /// if(gameObject.Is&lt;Collider&gt;(out var result))
        ///     result.trigger = true;
        /// </code>
        /// </example>
        /// <param name="result">A component of type T if found, null otherwise.</param>
        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>true if any object in the specified search scope has a component of type T.</returns>
        public static bool Is<T>(this GameObject go, out T result, Search where = Search.InObjectOnly) where T : class 
        {
            result = go.As<T>(where);
            return !Util.IsNull(result);
        }

        /// <inheritdoc cref="Is{T}(UnityEngine.GameObject,out T,CoreLibrary.Search)" />
        public static bool Is<T>(this Component comp, out T result, Search where = Search.InObjectOnly) where T : class 
        {
            return comp.gameObject.Is<T>(out result, where);
        }

        /// <inheritdoc cref="Is{T}(UnityEngine.GameObject,out T,CoreLibrary.Search)" />
        public static bool Is<T>(this Collision col, out T result, Search where = Search.InObjectOnly) where T : class 
        {
            return col.gameObject.Is<T>(out result, where);
        }
        
        /// <inheritdoc cref="Is{T}(UnityEngine.GameObject,out T,CoreLibrary.Search)" />
        public static bool Is<TSource, TResult>(this TSource obj, out TResult ret, Search where = Search.InObjectOnly) 
            where TResult : class 
        {
            // `obj is TResult t` only works with C# 7.1 or higher.
            if (obj is TResult) {
                ret = obj as TResult;
                return true;
            }
            // Same as above.
            if (obj is Component) {
                return (obj as Component).Is<TResult>(out ret, where);
            }
            ret = null;
            return false;
        }

        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>true if any object in the specified search scope has a component of type T.</returns>
        public static bool Is<T>(this GameObject go, Search where = Search.InObjectOnly) where T : class
        {
            T unused;
            return go.Is<T>(out unused, where);
        }

        /// <inheritdoc cref="Is{T}(GameObject, Search)"/>
        public static bool Is<T>(this Component comp, Search where = Search.InObjectOnly) where T : class
        {
            return comp.gameObject.Is<T>(where);
        }
        
        /// <inheritdoc cref="Is{T}(GameObject, Search)"/>
        public static bool Is<T>(this Collision col, Search where = Search.InObjectOnly) where T : class
        {
            return col.gameObject.Is<T>(where);
        }
        
        /// <inheritdoc cref="Is{T}(GameObject, Search)"/>
        public static bool Is<TSource, TResult>(this TSource obj, Search where = Search.InObjectOnly)
            where TResult : class
        {
            TResult unused;
            return obj.Is<TSource, TResult>(out unused, where);
        }

        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>The first component of type T found in the search scope or null if not found.</returns>
        [CanBeNull]
        public static T As<T>(this GameObject go, Search where = Search.InObjectOnly) where T : class
        {
            switch (where)
            {
                case Search.InObjectOnly:
                    return go.GetComponent<T>();
                case Search.InChildren:
                    return go.GetComponentInChildren<T>();
                case Search.InParents:
                    return go.GetComponentInParent<T>();
                case Search.InSiblings:
                    if (go.transform.parent == null) return go.As<T>();
                    foreach (var child in go.transform.parent.GetChildren())
                    {
                        var component = child.GetComponent<T>();
                        if (component != null) return component;
                    }
                    return null;
                case Search.InWholeHierarchy:
                    if (go.transform.parent != null)
                    {
                        var parentSearch = go.transform.parent.GetComponentInParent<T>();
                        if (parentSearch != null) return parentSearch;
                    }

                    return go.GetComponentInChildren<T>();
                default:
                    throw new UnsupportedSearchException(where);
            }
        }

        /// <inheritdoc cref="As{T}(GameObject, Search)"/>
        [CanBeNull]
        public static T As<T>(this Component comp, Search where = Search.InObjectOnly) where T : class
        {
            return comp.gameObject.As<T>(where);
        }
        
        /// <inheritdoc cref="As{T}(GameObject, Search)"/>
        [CanBeNull]
        public static T As<T>(this Collision col, Search where = Search.InObjectOnly) where T : class
        {
            return col.gameObject.As<T>(where);
        }

        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>A lazily generated IEnumerable of all components of type T found in the search scope. Might be empty.</returns>
        [NotNull]
        public static IEnumerable<T> All<T>(this GameObject go, Search where = Search.InObjectOnly) where T : class
        {
            switch (where)
            {
                case Search.InObjectOnly:
                    return go.GetComponents<T>();
                case Search.InParents:
                    return go.GetComponentsInParent<T>();
                case Search.InChildren:
                    return go.GetComponentsInChildren<T>();
                case Search.InSiblings:
                    if (go.transform.parent == null) return go.All<T>();
                    return go.transform.parent.GetChildren().Collect(c => c.As<T>()).ToArray();
                case Search.InWholeHierarchy:
                    var parentSearch = go.transform.parent != null
                        ? go.transform.parent.gameObject.GetComponentsInParent<T>()
                        : new T[] { };
                    return parentSearch.AndAlso(go.GetComponentsInChildren<T>());
                default:
                    throw new UnsupportedSearchException(where);
            }
        }
        
        /// <inheritdoc cref="All{T}(GameObject, Search)"/>
        [CanBeNull]
        public static IEnumerable<T> All<T>(this Component comp, Search where = Search.InObjectOnly) where T : class
        {
            return comp.gameObject.All<T>(where);
        }
        
        /// <inheritdoc cref="All{T}(GameObject, Search)"/>
        [CanBeNull]
        public static IEnumerable<T> All<T>(this Collision col, Search where = Search.InObjectOnly) where T : class
        {
            return col.gameObject.All<T>(where);
        }

        /// <summary>
        /// Searches through the specified search scope until a component of type T is found
        /// and assigns it to the passed variable reference. Throws an exception if nothing could be found.
        /// </summary>
        /// <param name="variable">A reference to the variable to be set.</param>
        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <exception cref="ComponentNotFoundException">
        /// If there was no component to be found in the specified search scope.
        /// </exception>
        public static void AssignComponent<T>(this GameObject go, out T variable, Search where = Search.InObjectOnly)
            where T : class
        {
            T found = go.As<T>(where);
            if (Util.IsNull(found))
                throw new ComponentNotFoundException(
                    "Failed to assign component of type " + typeof(T) + " to " + go + ".");

            variable = found;
        }

        /// <summary>
        /// Searches through the specified search scope until a component of type T is found
        /// and assigns it to the passed variable reference. When no component could be found
        /// in the specified scope, a new component of type T is added to the game object instead.
        /// </summary>
        /// <param name="variable">A reference to the variable to be set.</param>
        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        public static void AssignComponentOrAdd<T>(this GameObject go, out T variable,
            Search where = Search.InObjectOnly)
            where T : Component
        {
            T found = go.As<T>(where);
            if (found == null) found = go.AddComponent<T>();

            variable = found;
        }

        /// <summary>
        /// Searches through the specified search scope until a component of type T is found
        /// and assigns it to the passed variable reference if and only iff the variable has
        /// nothing assigned to it yet. Throws an exception if nothing could be found.
        /// </summary>
        /// <seealso cref="AssignComponent{T}"/>
        /// <param name="variable">A reference to the variable to be set if unset so far.</param>
        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>true if new value was assigned, false if variable already has a value.</returns>
        /// <exception cref="Exception">If there was no component to be found in the specified search scope.</exception>
        public static bool AssignIfAbsent<T>(this GameObject go, ref T variable, Search where = Search.InObjectOnly)
            where T : class
        {
            if (variable != default(T)) // safer than null
            {
                Debug.Log(
                    "Tried to assign component of type " + typeof(T) + " but field already had value " + variable, go);
                return false;
            }

            go.AssignComponent(out variable, where);
            return true;
        }

        /// <summary>
        /// Searches through the specified search scope until a component of type T is found
        /// and assigns it to the passed variable reference if and only iff the variable has
        /// nothing assigned to it yet. When no component could be found in the specified scope,
        /// a new component of type T is added to the game object instead.
        /// </summary>
        /// <param name="variable">A reference to the variable to be set.</param>
        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>true if new value was assigned, false if variable already has a value.</returns>
        public static bool AssignIfAbsentOrAdd<T>(this GameObject go, ref T variable,
            Search where = Search.InObjectOnly)
            where T : Component
        {
            if (!Util.IsNull(variable))
            {
                Debug.Log(
                    "Tried to assign component of type " + typeof(T) + " but field already had value " + variable, go);
                return false;
            }

            go.AssignComponentOrAdd(out variable, where);
            return true;
        }
    }
}