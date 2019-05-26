using System;
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
		private Util.VectorProxy _positionProxy;

		/// <summary>
		/// Returns the position of this component as a <see cref="Util.VectorProxy"/>.
		/// This is implicitly convertible to and from a <see cref="Vector3"/>.
		/// You can treat this exactly like <code>transform.position</code>,
		/// except that you can now directly write <code>Position.y += 5</code>.
		/// </summary>
		public Util.VectorProxy Position
		{
			get
			{
				if (_positionProxy != null) 
					return _positionProxy;
				_positionProxy = new Util.VectorProxy(
					() => transform.position, 
					pos => transform.position = pos);
				return _positionProxy;
			}
			set { transform.position = value; }
		}
		
		
		/// <summary>
		/// A game object is not perceivable if it has no active collider and renderer.
		/// Used instead of deactivation to enable coroutines and sounds to continue to play.
		/// Redirects to <see cref="UtilityExtensions.SetPerceivable(GameObject, bool)"/>
		/// </summary>
		public void SetPerceivable(bool state)
		{
			gameObject.SetPerceivable(state);
		}

		/// <inheritdoc cref="ComponentQueryExtensions.AssignComponent{T}(GameObject, out T, Search)"/>
		/// <seealso cref="ComponentQueryExtensions.AssignComponent{T}(GameObject, out T, Search)"/>
		protected void AssignComponent<T>(out T variable, Search where = Search.InObjectOnly) where T : class
		{
			gameObject.AssignComponent(out variable, where);
		}
		
		/// <inheritdoc cref="ComponentQueryExtensions.AssignIfAbsent{T}(GameObject, ref T, Search)"/>
		/// <seealso cref="ComponentQueryExtensions.AssignIfAbsent{T}(GameObject, ref T, Search)"/>
		protected bool AssignIfAbsent<T>(ref T variable, Search where = Search.InObjectOnly) where T : class
		{
			return gameObject.AssignIfAbsent(ref variable, where);
		}

		/// <inheritdoc cref="Util.IfAbsentCompute{T}(ref T, Func{T})"/>
		/// <seealso cref="Util.IfAbsentCompute{T}(ref T, Func{T})"/>
		protected bool IfAbsentCompute<T>(ref T field, Func<T> getter)
		{
			return Util.IfAbsentCompute(ref field, getter);
		}
	}
}
