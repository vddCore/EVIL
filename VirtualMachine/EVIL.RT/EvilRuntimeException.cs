using System;

namespace EVIL.RT
{
    public class EvilRuntimeException : Exception
    {
        public EvilRuntimeException(string message) 
            : base(message)
        {
        }
    }
}