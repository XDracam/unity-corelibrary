using System;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Any BaseBehaviour that is a lazy singleton should only be found at most once per scene.
    /// A lazy singleton creates an instance of itself in the scene if there is none.
    /// Enables static access to this single object by using <code>Classname.Instance</code>.
    /// </summary>
    /// <typeparam name="T">The implementing type itself</typeparam>
    public abstract class LazySingleton<T> : BaseBehaviour where T : LazySingleton<T>
    {
        private static T _instance;

        /// <summary>
        /// Returns the instance of this singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                var tmp = FindObjectsOfType<T>();

                if (tmp == null || tmp.Length == 0)
                {
                    var newInstance = new GameObject();
                    _instance = newInstance.AddComponent<T>();
                    newInstance.name = typeof(T).Name + " - Lazy Singleton";
                    Debug.Log("Creating new instance of lazy singleton " + typeof(T).Name);
                }
                else _instance = tmp[0];
                
                if (tmp != null && tmp.Length > 1)
                    throw new Exception("Singleton: There is more than one instance of " +
                                        typeof(T) + " in the scene.");
                
                return _instance;
            }
        }
    }
}