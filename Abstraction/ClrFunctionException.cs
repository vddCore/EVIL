using System;

namespace EVIL.Abstraction
{
    public class ClrFunctionException : Exception
    {
        public ClrFunctionException(string message) : base(message)
        {

        }
    }
}
