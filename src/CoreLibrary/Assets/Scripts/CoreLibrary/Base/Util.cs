using System;
using CoreLibrary.Exceptions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// This class serves as a namespace for every
    /// non-specific utility function, such as 
    /// <see cref="Mod"/> and <see cref="IsNull{T}"/>,
    /// as well as <see cref="VectorProxy"/>.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Mathematically correct modulus.
        /// Always positive for any x as long as m > 0.
        /// </summary>
        [Pure] public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
        
        /// <summary>
        /// A null check that works for any generic type T. This works
        /// works for unity components as well as every other type.
        /// <br/>
        /// See <a href="https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/">
        /// this blog post</a> for more details about Unity's custom null handling. 
        /// </summary>
        [Pure] public static bool IsNull<T>(T value)
        {
            return value == null || value is Object && value as Object == null;
        }
        
        /// <summary>
        /// Author: Cameron Reuschel
        /// <br/><br/>
        /// A proxy that enables setting x, y and z coordinates of something holding a vector by reference.
        /// Provides implicit conversions from and to <see cref="Vector3"/>. 
        /// </summary>
        /// <example><code>
        /// var proxy = new VectorProxy(() => transform.position, vec => transform.position = vec);
        /// </code></example>
        // ReSharper disable all InconsistentNaming
        public class VectorProxy
        {
            private Func<Vector3> _getter;
            private Action<Vector3> _setter;
            private bool _wrapsVectorDirectly = false;
            
            public VectorProxy([NotNull] Func<Vector3> getter, [NotNull] Action<Vector3> setter)
            {
                _getter = getter;
                _setter = setter;
            }

            private Vector3 _vectorRef;
            public VectorProxy(Vector3 vectorRef = default(Vector3))
            {
                _vectorRef = vectorRef;
                _wrapsVectorDirectly = true;
            }
			
            /// <summary>
            /// The x coordinate of this position.
            /// </summary>
            public float x
            {
                get
                {
                    return _wrapsVectorDirectly ? _vectorRef.x : _getter().x; }
                set
                {
                    if (_wrapsVectorDirectly) _vectorRef.x = value;
                    else _setter(_getter().WithX(value));
                }
            }
			
            /// <summary>
            /// The y coordinate of this position.
            /// </summary>
            public float y
            {
                get
                {
                    return _wrapsVectorDirectly ? _vectorRef.y : _getter().y; }
                set
                {
                    if (_wrapsVectorDirectly) _vectorRef.y = value;
                    else _setter(_getter().WithY(value));
                }
            }
			
            /// <summary>
            /// The z coordinate of this position.
            /// </summary>
            public float z
            {
                get
                {
                    return _wrapsVectorDirectly ? _vectorRef.z : _getter().z; }
                set
                {
                    if (_wrapsVectorDirectly) _vectorRef.z = value;
                    else _setter(_getter().WithZ(value));
                }
            }
			
            public static implicit operator Vector3(VectorProxy proxy)
            {
                return proxy._wrapsVectorDirectly ? proxy._vectorRef : proxy._getter();
            }
            
            public static implicit operator VectorProxy(Vector3 vec)
            {
                return new VectorProxy(vec);
            }
        }
    }
}