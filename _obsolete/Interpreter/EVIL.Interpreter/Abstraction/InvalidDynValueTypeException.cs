using System;

namespace EVIL.Interpreter.Abstraction
{
    public class InvalidDynValueTypeException : Exception
    {
        public DynValueType RequestedType { get; }
        public DynValueType ActualType { get; }

        public InvalidDynValueTypeException(string message, DynValueType requestedType, DynValueType actualType) : base(message)
        {
            RequestedType = requestedType;
            ActualType = actualType;
        }
    }
}