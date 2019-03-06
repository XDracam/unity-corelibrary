#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
#endif
using UnityEngine;

namespace CoreLibrary{
	/// <summary>
	/// Author: Daniel Götz
	/// <br/><br/>
	/// This class provides useful extension methods for unity's Objects
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// This will always destroy the Object in the correct way, so that Unity will not throw an error
		/// Note: This cannot be called in the OnValidateMethod
		/// If your write code that is both executed in the editor while not playing and when playing and want to use Destroy, this method becomes useful:
		/// Normally you have to check a preprocessor directive and if the application is playing to determine if you should use Destroy or DestroyImmediate
		/// </summary>
		public static void SafeDestroy(this Object o){
	#if UNITY_EDITOR
			if (EditorApplication.isPlaying)
			{
				Object.Destroy(o);
			}
			else
			{
				Object.DestroyImmediate(o);
			}
	#else
			Destroy(o);
	#endif
		}  
	}
}
