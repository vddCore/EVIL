using System;

namespace EVIL.Interpreter.Abstraction
{
    [Serializable]
    public enum DynValueType
    {
        String,
        Number,
        Table,
        Function,
        ClrFunction
    }
}