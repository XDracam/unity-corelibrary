using UnityEngine;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel <br/><br/>
    /// 
    /// Inheriting from this class instead of <see cref="BaseBehaviour"/>
    /// causes a <see cref="Queryable"/> component to be added so that the
    /// inheriting class can be queried in a scene using the static methods
    /// on the <see cref="Query"/> class.
    /// </summary>
    [RequireComponent(typeof(Queryable))]
    public abstract class QueryableBaseBehaviour : BaseBehaviour
    {
        private Queryable _queryable;
        internal Queryable Queryable
        {
            get
            {
                if (_queryable != null) return _queryable;
                AssignComponent(out _queryable);
                return _queryable;
            }
        }
    }
}