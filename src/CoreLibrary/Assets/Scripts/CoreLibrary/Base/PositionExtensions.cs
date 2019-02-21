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
    public static class PositionExtensions
    {
        /// <summary>
        /// Equal to
        /// <code>obj.transform.position</code>
        /// </summary>
        [Pure] public static Vector3 Pos(this GameObject obj)
        {
            return obj.transform.position;
        }

        /// <summary>
        /// Equal to
        /// <code>comp.transform.position</code>
        /// </summary>
        [Pure] public static Vector3 Pos(this Component comp)
        {
            return comp.transform.position;
        }

        /// <summary>
        /// Equal to
        /// <code>obj.transform.position = newPosition;</code>
        /// </summary>
        /// <seealso cref="SetPos(UnityEngine.Component,UnityEngine.Vector3)"/>
        public static void SetPos(this GameObject obj, Vector3 newPosition)
        {
            obj.transform.position = newPosition;
        }

        /// <summary>
        /// Equal to
        /// <code>comp.transform.position = newPosition;</code>
        /// </summary>
        /// <seealso cref="SetPos(UnityEngine.GameObject,UnityEngine.Vector3)"/>
        public static void SetPos(this Component comp, Vector3 newPosition)
        {
            comp.gameObject.SetPos(newPosition);
        }
        
        /// <summary>
        /// Equal to
        /// <code>obj.transform.position = newPositionMapping(obj.transform.position);</code>
        /// </summary>
        /// <seealso cref="SetPos(UnityEngine.Component,Func{Vector3, Vector3})"/>
        /// <example><code>
        /// rocket.gameObject.SetPos(pos => pos.WithY(y => y + Time.deltaTime * Speed));
        /// </code></example>
        public static void SetPos(this GameObject obj, [NotNull] Func<Vector3, Vector3> newPositionMapping)
        {
            obj.transform.position = newPositionMapping(obj.transform.position);
        }

        /// <summary>
        /// Equal to
        /// <code>comp.transform.position = newPositionMapping(comp.transform.position);</code>
        /// </summary>
        /// <seealso cref="SetPos(UnityEngine.GameObject,Func{Vector3, Vector3})"/>
        /// <example><code>
        /// rocket.SetPos(pos => pos.WithY(y => y + Time.deltaTime * Speed));
        /// </code></example>
        public static void SetPos(this Component comp, [NotNull] Func<Vector3, Vector3> newPositionMapping)
        {
            comp.gameObject.SetPos(newPositionMapping);
        }
        
        /// <summary>
        /// Vector2 version. Requires explicit typing of lambda parameters for ambiguity reasons. Equal to
        /// <code>obj.transform.position = newPositionMapping(obj.transform.position);</code>
        /// </summary>
        /// <seealso cref="SetPos(UnityEngine.Component,Func{Vector3, Vector3})"/>
        /// <example><code>
        /// rocket.gameObject.SetPos(pos => pos.WithY(y => y + Time.deltaTime * Speed));
        /// </code></example>
        public static void SetPos(this GameObject obj, [NotNull] Func<Vector2, Vector2> newPositionMapping)
        {
            obj.transform.position = newPositionMapping(obj.transform.position);
        }

        /// <summary>
        /// Vector2 version. Requires explicit typing of lambda parameters for ambiguity reasons. Equal to
        /// <code>comp.transform.position = newPositionMapping(comp.transform.position);</code>
        /// </summary>
        /// <seealso cref="SetPos(UnityEngine.GameObject,Func{Vector3, Vector3})"/>
        /// <example><code>
        /// rocket.SetPos(pos => pos.WithY(y => y + Time.deltaTime * Speed));
        /// </code></example>
        public static void SetPos(this Component comp, [NotNull] Func<Vector2, Vector2> newPositionMapping)
        {
            comp.gameObject.SetPos(newPositionMapping);
        }
    }
}