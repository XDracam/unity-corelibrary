using System.Collections;
using UnityEngine;

namespace CoreLibrary.Tests.GenericPool
{
	/// <summary>
	/// Author: Cameron Reuschel
	/// </summary>
	public class Dispenser : BaseBehaviour
	{
		public float ShotImpulse = 100f;

		private CoreLibrary.GenericPool _pool;
		IEnumerator Start() 
		{
			AssignComponent(out _pool);
			while (true)
			{
				var item = _pool.RequestItem(transform.position + _pool.transform.TransformDirection(Vector3.down));
				item.As<Rigidbody>().AddForce(_pool.transform.TransformDirection(Vector3.down) * ShotImpulse);
				yield return new WaitForSeconds(.25f);
			}
		}
	}
}
