using System.Collections;
using UnityEngine;

namespace CoreLibrary.Tests.GenericPool
{
	/// <summary>
	/// Author: Cameron Reuschel
	/// </summary>
	public class Dispenser : BaseBehaviour
	{

		private Pool.GenericPool _pool;
		IEnumerator Start() 
		{
			AssignComponent(out _pool);
			while (true)
			{
				_pool.RequestItem(transform.position + _pool.transform.TransformDirection(Vector3.down));
				yield return new WaitForSeconds(.25f);
			}
		}
	}
}
