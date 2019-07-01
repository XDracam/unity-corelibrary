using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// The class holding all extension methods specific to colors in the core library.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns a new color with specific values changed to new values.
        /// This method is intended to be used with named parameters.
        /// </summary>
        /// <example>
        /// var foo = color.With(r: 0.3f, a: 0.1f);
        /// </example>
        /// <returns>A new vector with it's coordinates set to everything specified that is not null.</returns>
        [Pure] public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            if (r != null) color.r = r.Value;
            if (g != null) color.g = g.Value;
            if (b != null) color.b = b.Value;
            if (a != null) color.a = a.Value;
            return color; // this is a copy since Color is a struct.
        }
        
        /// <returns>A new color with it's r value set to the specified value.</returns>
        [Pure] public static Color WithR(this Color col, float r) { return new Color(r, col.g, col.b, col.a); }
        
        /// <returns>A new color with it's g value set to the specified value.</returns>
        [Pure] public static Color WithG(this Color col, float g) { return new Color(col.r, g, col.b, col.a); }
        
        /// <returns>A new color with it's b value set to the specified value.</returns>
        [Pure] public static Color WithB(this Color col, float b) { return new Color(col.r, col.g, b, col.a); }
        
        /// <returns>A new color with it's a value set to the specified value.</returns>
        [Pure] public static Color WithA(this Color col, float a) { return new Color(col.r, col.g, col.b, a); }
        
        /// <returns>A new color with it's r value set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Color WithR(this Color col, [NotNull] Func<float, float> fr)
        {
            return new Color(fr(col.r), col.g, col.b, col.a);
        }
        
        /// <returns>A new color with it's g value set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Color WithG(this Color col, [NotNull] Func<float, float> fg)
        {
            return new Color(col.r, fg(col.g), col.b, col.a);
        }
        
        /// <returns>A new color with it's b value set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Color WithB(this Color col, [NotNull] Func<float, float> fb)
        {
            return new Color(col.r, col.g, fb(col.b), col.a);
        }
        
        /// <returns>A new color with it's a value set to the result
        /// of applying the specified function to the original value.</returns>
        [Pure] public static Color WithA(this Color col, [NotNull] Func<float, float> fa)
        {
            return new Color(col.r, col.g, col.b, fa(col.a));
        }
    }
}