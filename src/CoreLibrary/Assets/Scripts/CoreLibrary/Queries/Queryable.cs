using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel <br/><br/>
    /// Added automatically by inheriting from <see cref="QueryableBaseBehaviour"/>.
    /// <br/><br/>
    /// DO NOT ADD DIRECTLY.
    /// </summary>
    [RequireComponent(typeof(QueryableBaseBehaviour))]
    public sealed class Queryable : BaseBehaviour
    {
        // TODO: figure out a way to register secondary queryables which are added later
        
        private Query _q;
        
        private List<QueryableBaseBehaviour> Underlying { get; set; }

        private void OnEnable()
        {
            _q.SetEnabled(this);
        }

        private void OnDisable()
        {
            _q.SetDisabled(this);
        }

        private void Awake()
        {
            _q = Query.Instance;   
        }

        private void Start()
        {
            Underlying = GetComponents<QueryableBaseBehaviour>().ToList();
            Underlying.ForEach(u =>_q.Register(u));
        }

        private void OnDestroy()
        {
            if (Underlying != null)
            {
                Underlying.ForEach(u =>
                {
                    if (_q != null) _q.Deregister(u);
                });
            }
        }
    }
}