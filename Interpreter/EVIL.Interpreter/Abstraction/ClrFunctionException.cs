using System;

namespace EVIL.Interpreter.Abstraction
{
    public class ClrFunctionException : Exception
    {
        public ClrFunctionException(string message) : base(message)
        {

        }
    }
}
