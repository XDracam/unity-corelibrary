using System;
using CoreLibrary.Exceptions;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Any BaseBehaviour that is a singleton should only be found once.
    /// Enables static access to this single object by using <code>Classname.Instance</code>.
    /// </summary>
    /// <typeparam name="T">The implementing type itself</typeparam>
    public abstract class Singleton<T> : BaseBehaviour where T : Singleton<T>
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
                    throw new WrongSingletonUsageException(
                        "Singleton: An instance of " + typeof(T).Name +
                        " is needed in the scene, but there is none.");
                if (tmp.Length > 1)
                    throw new WrongSingletonUsageException(
                        "Singleton: There is more than one instance of " +
                        typeof(T).Name + " in the scene.");
                _instance = tmp[0];

                return _instance;
            }
        }
    }
}