using System;

namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Thrown when a <see cref="GenericPool"/>
    /// is initialized without a Template. 
    /// </summary>
    public class NoTemplateException : Exception
    {
        public NoTemplateException(GenericPool pool) 
            : base(pool + ": No template has been set before Init() was called.") {}
    }
}