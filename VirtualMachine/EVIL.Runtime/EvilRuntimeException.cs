using System;

namespace EVIL.Runtime
{
    public class EvilRuntimeException : Exception
    {
        public EvilRuntimeException(string message) 
            : base(message)
        {
        }
    }
}