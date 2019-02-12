using System;

namespace CoreLibrary.Exceptions
{
    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Thrown when a component could not be found
    /// during an <see cref="ComponentQueryExtensions.AssignComponent"/> call.
    /// </summary>
    public class ComponentNotFoundException : Exception 
    {
        public ComponentNotFoundException(string message) : base(message) {}
    }
}