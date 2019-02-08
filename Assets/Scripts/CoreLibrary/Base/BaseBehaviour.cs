using JetBrains.Annotations;
using UnityEngine;

namespace CoreLibrary
{
	/// <summary>
	/// Author: Cameron Reuschel
	/// <br/><br/>
	/// Base class for all behaviours that depend on CoreLibrary utilities.
	/// Adds no additional overhead compared to extending from MonoBehaviour directly.
	/// </summary>
	public class BaseBehaviour : MonoBehaviour 
	{
		
		/// <summary>
		/// A game object is not perceivable if it has no active collider and renderer.
		/// Used instead of deactivation to enable coroutines and sounds to continue to play.
		/// Redirects to <see cref="UtilityExtensions.SetPerceivable(GameObject, bool)"/>
		/// </summary>
		public void SetPerceivable(bool state)
		{
			gameObject.SetPerceivable(state);
		}

		/// <summary>
		/// Mathematically correct modulus. Always positive, even for negative arguments.
		/// </summary>
		[Pure] public static int Mod(int x, int m)
		{
			return (x % m + m) % m;
		}

		/// <inheritdoc cref="ComponentQueryExtensions.AssignComponent{T}(GameObject, out T, Search)"/>
		/// <seealso cref="ComponentQueryExtensions.AssignComponent{T}(GameObject, out T, Search)"/>
		protected void AssignComponent<T>(out T variable, Search where = Search.InObjectOnly) where T:Component
		{
			gameObject.AssignComponent(out variable, where);
		}

		/// <inheritdoc cref="ComponentQueryExtensions.AssignIfAbsent{T}(GameObject, ref T, Search)"/>
		/// <seealso cref="ComponentQueryExtensions.AssignIfAbsent{T}(GameObject, ref T, Search)"/>
		protected bool AssignIfAbsent<T>(ref T variable, Search where = Search.InObjectOnly) where T:Component
		{
			return gameObject.AssignIfAbsent(ref variable, where);
		}
	
	}
}
