using JetBrains.Annotations;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// This class serves as a namespace for every
    /// non-specific utility function, such as 
    /// <see cref="Mod"/> and <see cref="IsNull{T}"/>.
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
    }
}