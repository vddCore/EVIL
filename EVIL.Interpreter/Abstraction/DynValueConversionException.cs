using System;

namespace EVIL.Interpreter.Abstraction
{
    public class DynValueConversionException : Exception
    {
        public DynValueConversionException(string message) : base(message)
        {
        }
    }
}