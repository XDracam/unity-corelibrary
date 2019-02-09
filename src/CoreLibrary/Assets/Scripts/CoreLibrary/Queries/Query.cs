using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Singleton that provides static methods to query all components which
    /// inherit from <see cref="QueryableBaseBehaviour"/> in a scene.
    /// <br/><br/>
    /// You can either query all objects in the scene using <see cref="All{T}"/>
    /// or only the currently active ones using <see cref="AllActive{T}"/>.
    /// <br/><br/>
    /// By using <see cref="AllWith{T}"/> and <see cref="AllActiveWith{T}"/>
    /// you can specify an arbitrary number of additional queryable behaviour
    /// types that must also be present on the objects to be queried.
    /// The additional type constraints are passed as <see cref="Type"/>
    /// objects, using `typeof(Foo)` or `bar.GetType()`.
    /// <br/><br/>
    /// Every method returns an `IEnumerable` that can only be traversed <b>once</b>.
    /// </summary>
    public class Query : LazySingleton<Query>
    {
        private readonly Dictionary<Type, HashSet<QueryableBaseBehaviour>> _registered = 
            new Dictionary<Type, HashSet<QueryableBaseBehaviour>>();

        private readonly HashSet<Queryable> _enabled = new HashSet<Queryable>();

        private HashSet<QueryableBaseBehaviour> Registered(QueryableBaseBehaviour obj)
        {
            var key = obj.GetType();
            HashSet<QueryableBaseBehaviour> res;
            if (_registered.TryGetValue(key, out res))
            {
                return res;
            }
            res = new HashSet<QueryableBaseBehaviour>();
            _registered[key] = res;
            return res;
        }

        internal void Register(QueryableBaseBehaviour obj)
        {
            Registered(obj).Add(obj);
        }

        internal void Deregister(QueryableBaseBehaviour obj)
        {
            Registered(obj).Remove(obj);
        }

        internal void SetEnabled(Queryable obj)
        {
            _enabled.Add(obj);
        }

        internal void SetDisabled(Queryable obj)
        {
            _enabled.Remove(obj);
        }


        /// <summary>
        /// Retrieves all queryable components of type T in the scene with active game objects.
        /// </summary>
        /// <typeparam name="T">The type of the components to find.</typeparam>
        /// <returns>An IEnumerable containing all active components of type T in the scene. Can only be iterated once.</returns>
        [NotNull] public static IEnumerable<T> AllActive<T>() where T : QueryableBaseBehaviour
        {
            return Instance.AllActiveImpl<T>();
        }

        private IEnumerable<T> AllActiveImpl<T>() where T : QueryableBaseBehaviour
        {
            var res = QueryAllOf(typeof(T));
            res = res.Where(q => _enabled.Contains(q.Queryable));
            return res.OfType<T>();
        }

        /// <summary>
        /// Retrieves all queryable components of type T in the scene, even inactive ones.
        /// </summary>
        /// <typeparam name="T">The type of the components to find.</typeparam>
        /// <returns>An IEnumerable containing all components of type T in the scene. Can only be iterated once.</returns>
        [NotNull] public static IEnumerable<T> All<T>() where T : QueryableBaseBehaviour
        {
            return Instance.AllImpl<T>();
        }
        
        private IEnumerable<T> AllImpl<T>() where T : QueryableBaseBehaviour
        {
            var res = QueryAllOf(typeof(T));
            return res.OfType<T>();
        }    
            
        /// <summary>
        /// Retrieves all queryable components of type T in the scene with active game objects,
        /// only when the gameObject also has queryable components of all other types specified as arguments.
        /// </summary>
        /// <typeparam name="T">The type of the components to find.</typeparam>
        /// <returns>An IEnumerable containing all components of type T in the scene
        /// with the specified additional Queryables. Can only be iterated once.</returns>
        [NotNull] public static IEnumerable<T> AllActiveWith<T>(Type constraint, params Type[] constraints) where T : QueryableBaseBehaviour
        {
            return Instance.AllWithBounds<T>(constraint, constraints, false);
        }
        
        
        /// <summary>
        /// Retrieves all active queryable components of type T in the scene,
        /// only when the gameObject also has queryable components of all other types specified as arguments.
        /// </summary>
        /// <typeparam name="T">The type of the components to find.</typeparam>
        /// <returns>An IEnumerable containing all components of type T in the scene
        /// with the specified additional Queryables. Can only be iterated once.</returns>
        [NotNull] public static IEnumerable<T> AllWith<T>(Type constraint, params Type[] constraints) where T : QueryableBaseBehaviour
        {
            return Instance.AllWithBounds<T>(constraint, constraints, true);
        }

        private IEnumerable<T> AllWithBounds<T>(Type constraint, Type[] constraints, bool findInactiveObjects)
        {
            var types = new Type[constraints.Length + 1];
            types[0] = constraint;
            Array.Copy(constraints, 0, types, 1, constraints.Length);
            var ts = QueryAllOf(typeof(T));
            var others = types.Select(QueryAllOf);
            var res = ts.Where(t1 => others.All(ots => ots.Select(q => q.Queryable).Contains(t1.Queryable)));
            if (!findInactiveObjects) res = res.Where(q => _enabled.Contains(q.Queryable));
            return res.OfType<T>();
        }
        
        private IEnumerable<QueryableBaseBehaviour> QueryAllOf(Type t)
        {
            HashSet<QueryableBaseBehaviour> queried;
            if (_registered.TryGetValue(t, out queried))
            {
                return queried.AsEnumerable();
            }
            else
            { // check for subtypes  
                return _registered
                    .Where(r => r.Key.IsSubclassOf(t))
                    .SelectMany(r => r.Value.AsEnumerable());
            }
        } 
    }
}