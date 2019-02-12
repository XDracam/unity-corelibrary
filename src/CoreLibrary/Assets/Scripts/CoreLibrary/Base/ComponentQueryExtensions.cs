using System;
using System.Collections.Generic;
using CoreLibrary.Exceptions;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
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
        private static TRes SearchParents<T, TRes>(GameObject go, Func<T, TRes> fn, TRes onFailure) where T : Component
        {
            while (true)
            {
                var res = go.GetComponent<T>();
                if (res) return fn(res);
                if (go.transform.parent == null) return onFailure;
                go = go.transform.parent.gameObject;
            }
        }

        private static TRes SearchChildren<T, TRes>(GameObject go, Func<T, TRes> fn, TRes onFailure) where T : Component
        {
            var res = go.GetComponent<T>();
            if (res) return fn(res);
            foreach (var child in go.transform.GetChildren())
            {
                var recres = SearchChildren(child.gameObject, fn, onFailure);
                if (recres != null && !recres.Equals(onFailure)) return recres;
            }

            return onFailure;
        }

        private static T SearchFor<T>(GameObject go, Search where) where T : Component
        {
            switch (where)
            {
                case Search.InObjectOnly:
                    return go.GetComponent<T>();
                case Search.InChildren:
                    return go.GetComponentInChildren<T>();
                case Search.InParents:
                    return go.GetComponentInParent<T>();
                case Search.InWholeHierarchy:
                    var parentSearch = go.GetComponentInParent<T>();
                    if (parentSearch != null)
                        return parentSearch;
                    return go.GetComponentInChildren<T>();
                default:
                    throw new UnsupportedSearchException("Unsupported search type: " + where);
            }
        }

        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>true if any object in the specified search scope has a component of type T.</returns>
        public static bool Is<T>(this GameObject go, Search where = Search.InObjectOnly) where T : Component
        {
            var res = SearchFor<T>(go, where);
            return res != null;
        }

        /// <inheritdoc cref="Is{T}(GameObject, Search)"/>
        public static bool Is<T>(this Transform tr, Search where = Search.InObjectOnly) where T : Component
        {
            return tr.gameObject.Is<T>(where);
        }

        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>The first component of type T found in the search scope or null if not found.</returns>
        public static T As<T>(this GameObject go, Search where = Search.InObjectOnly) where T : Component
        {
            return SearchFor<T>(go, where);
        }

        /// <inheritdoc cref="As{T}(GameObject, Search)"/>
        public static T As<T>(this Transform tr, Search where = Search.InObjectOnly) where T : Component
        {
            return tr.gameObject.As<T>(where);
        }
        
        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <returns>A lazily generated IEnumerable of all components of type T found in the search scope. Might be empty.</returns>
        public static IEnumerable<T> All<T>(this GameObject go, Search where = Search.InObjectOnly) where T : Component
        {
            switch (where)
            {
                case Search.InObjectOnly:
                    return go.GetComponents<T>();
                case Search.InParents:
                    return go.GetComponentsInParent<T>();
                case Search.InChildren:
                    return go.GetComponentsInChildren<T>();
                case Search.InWholeHierarchy:
                    return go.GetComponentsInParent<T>().AndAlso(go.GetComponentsInChildren<T>());
                default:
                    throw new UnsupportedSearchException("Unsupported search type: " + where);
            }
        }

        /// <inheritdoc cref="All{T}(GameObject, Search)"/>
        public static IEnumerable<T> All<T>(this Transform tr, Search where = Search.InObjectOnly) where T : Component
        {
            return tr.gameObject.All<T>(where);
        }

        /// <summary>
        /// Searches through the specified search scope until a component of type T is found
        /// and assigns it to the passed variable reference. Throws an exception if nothing could be found.
        /// </summary>
        /// <param name="variable">A reference to the variable to be set.</param>
        /// <param name="where">Optional search scope if the object itself does not have the component.</param>
        /// <typeparam name="T">The type of the component to find.</typeparam>
        /// <exception cref="Exception">If there was no component to be found in the specified search scope.</exception>
        public static void AssignComponent<T>(this GameObject go, out T variable, Search where = Search.InObjectOnly)
            where T : Component
        {
            T found = go.As<T>();
            if (found == null)
                throw new ComponentNotFoundException(
                    "Failed to assign component of type " + typeof(T) + " to " + go + ".");

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
            where T : Component
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
    }
}