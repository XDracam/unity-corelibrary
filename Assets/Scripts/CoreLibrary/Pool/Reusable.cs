using CoreLibrary;

namespace CoreLibrary.Pool
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// GameObjects with a Reusable component attached are marked
    /// as reusable for use with <see cref="GenericPool"/> and
    /// potential other utilities. <br/>
    /// Calling <see cref="FreeForReuse"/> enables other objects
    /// to directly reuse this object. Calling <see cref="LockForReuse"/>
    /// should be used after an object has just been reused in order
    /// to prevent immediate reuse. 
    /// </summary>
    public abstract class Reusable : BaseBehaviour
    {
        /// <summary>
        /// This is called whenever an object is attempted to be reused.
        /// When this is called, the state of the entire GameObject should
        /// be reset to it's original state. When the pool doesn't do what
        /// it should, check your implementation of this first.
        /// </summary>
        public abstract void ResetForReuse();
        
        /// <summary>
        /// This method is guaranteed to be called after the object has been
        /// reused, just before returning the reused object to the requester.
        /// The difference to <see cref="ResetForReuse"/> is that this happens
        /// after reuse, while resetting happens just beforehand. 
        /// </summary>
        public abstract void AfterReuse();

        /// <summary>
        /// This method is called when a reuse is requested. The implementing
        /// class has the option to either call <see cref="FreeForReuse"/> or
        /// ignore the request entirely. If the item accepts and frees itself
        /// for reuse <i>in the very same frame</i> it is immediately reused.
        /// </summary>
        public abstract void ReuseRequested();

        private bool _canBeReused = true;

        /// <summary>
        /// Flag that tells utilities such as <see cref="GenericPool"/>
        /// whether this object is ready to be reused. When this is
        /// true and the object is inactive it may be reused on demand.
        /// </summary>
        public bool CanBeReused {
            get { return _canBeReused; }
            private set { _canBeReused = value; } 
        }
        
        /// <summary>
        /// Marks this object for reuse so that it may be used 
        /// by utilities such as <see cref="GenericPool"/>.
        /// </summary>
        public void FreeForReuse()
        {
            CanBeReused = true;
        }

        /// <summary>
        /// Locks this object so that it cannot be reused
        /// by utilities such as <see cref="GenericPool"/>
        /// until <see cref="FreeForReuse"/> is called.
        /// </summary>
        public void LockForReuse()
        {
            CanBeReused = false;
        }
    }
}