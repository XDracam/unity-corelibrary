using System;

namespace CoreLibrary.Exceptions
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Thrown when an invalid <see cref="Search"/> value is passed
    /// to a function. Should never happen unless you manually modified
    /// the <see cref="Search"/> enum.
    /// </summary>
    public class UnsupportedSearchException : Exception
    {
        internal UnsupportedSearchException(Search where) : base("Unsupported search type: " + where) {}
    }
}