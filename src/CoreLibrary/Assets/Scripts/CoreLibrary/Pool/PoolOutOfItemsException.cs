using System;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Thrown only when a <see cref="GenericPool"/>
    /// has its GrowRate set to 0 and is out of items.
    /// </summary>
    public class PoolOutOfItemsException : Exception
    {
        public PoolOutOfItemsException(string message) : base(message) {}
    }
}