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
        /// <summary>
        /// Returns a new vector with specific coordinates changed to new values.
        /// This method is intended to be used with named parameters.
        /// </summary>
        /// <example>
        /// var foo = transform.position.With(x: 2);
        /// </example>
        /// <returns>A new vector with it's coordinates set to everything specified that is not null.</returns>
        [Pure] public static Vector2 With(this Vector2 vec, float? x = null, float? y = null, float? z = null)
        {
            if (x != null) vec.x = x.Value;
            if (y != null) vec.y = y.Value;
            return vec; // a copy, since Vector2 is a struct.
        }
        
        /// <summary>
        /// Returns a new vector with specific coordinates changed to new values.
        /// This method is intended to be used with named parameters.
        /// </summary>
        /// <example>
        /// var foo = transform.position.With(x: 2, z: 3);
        /// </example>
        /// <returns>A new vector with it's coordinates set to everything specified that is not null.</returns>
        [Pure] public static Vector3 With(this Vector3 vec, float? x = null, float? y = null, float? z = null)
        {
            if (x != null) vec.x = x.Value;
            if (y != null) vec.y = y.Value;
            if (z != null) vec.z = z.Value;
            return vec; // a copy, since Vector3 is a struct.
        }
        
        /// <returns>A new vector with it's x coordinate set to the specified value.</returns>
        [Pure] public static Vector2 WithX(this Vector2 vec, float x)
        {
            return new Vector3(x, vec.y);
        }

        /// <returns>A new vector with it's y coordinate set to the specified value.</returns>
        [Pure] public static Vector2 WithY(this Vector2 vec, float y)
        {
            return new Vector3(vec.x, y);
        }
        
        /// <returns>A new vector with it's x coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector2 WithX(this Vector2 vec, [NotNull] Func<float, float> fx)
        {
            return new Vector2(fx(vec.x), vec.y);
        }

        /// <returns>A new vector with it's y coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector2 WithY(this Vector2 vec, [NotNull] Func<float, float> fy)
        {
            return new Vector2(vec.x, fy(vec.y));
        }

        /// <returns>A new vector with it's x coordinate set to the specified value.</returns>
        [Pure] public static Vector3 WithX(this Vector3 vec, float x)
        {
            return new Vector3(x, vec.y, vec.z);
        }

        /// <returns>A new vector with it's y coordinate set to the specified value.</returns>
        [Pure] public static Vector3 WithY(this Vector3 vec, float y)
        {
            return new Vector3(vec.x, y, vec.z);
        }

        /// <returns>A new vector with it's z coordinate set to the specified value.</returns>
        [Pure] public static Vector3 WithZ(this Vector3 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }

        /// <returns>A new vector with it's x coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector3 WithX(this Vector3 vec, [NotNull] Func<float, float> fx)
        {
            return new Vector3(fx(vec.x), vec.y, vec.z);
        }

        /// <returns>A new vector with it's y coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector3 WithY(this Vector3 vec, [NotNull] Func<float, float> fy)
        {
            return new Vector3(vec.x, fy(vec.y), vec.z);
        }

        /// <returns>A new vector with it's z coordinate set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Vector3 WithZ(this Vector3 vec, [NotNull] Func<float, float> fz)
        {
            return new Vector3(vec.x, vec.y, fz(vec.z));
        }
    }
}