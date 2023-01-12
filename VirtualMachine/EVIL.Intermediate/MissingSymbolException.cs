using System;

namespace EVIL.Intermediate
{
    public class MissingSymbolException : Exception
    {
        public MissingSymbolException(string identifier) 
            : base($"Symbol '{identifier}' was never defined.")
        {
        }
    }
}