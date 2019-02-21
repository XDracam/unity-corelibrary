using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// The class holding all extension methods specific to positions in the core library.
    /// </summary>
    public static class VectorExtensions
    {
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
    }
}