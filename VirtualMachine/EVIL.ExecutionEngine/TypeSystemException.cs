using System;

namespace EVIL.ExecutionEngine
{
    public class TypeSystemException : Exception
    {
        public TypeSystemException(string message)
            : base(message)
        {
        }
    }
}