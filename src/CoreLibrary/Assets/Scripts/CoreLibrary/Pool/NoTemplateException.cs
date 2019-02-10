using System;

namespace CoreLibrary
{
    public class NoTemplateException : Exception
    {
        public NoTemplateException(GenericPool pool) 
            : base(pool + ": No template has been set before Init() was called.") {}
    }
}