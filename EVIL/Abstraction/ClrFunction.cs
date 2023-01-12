using System;
using EVIL.Execution;

namespace EVIL.Abstraction
{
    public class ClrFunction
    {
        public Func<Interpreter, ClrFunctionArguments, DynValue> Invokable { get; }

        public ClrFunction(Func<Interpreter, ClrFunctionArguments, DynValue> invokable)
        {
            Invokable = invokable;
        }
    }
}