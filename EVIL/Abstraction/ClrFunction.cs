using System;
using EVIL.Abstraction.Base;
using EVIL.Execution;

namespace EVIL.Abstraction
{
    public class ClrFunction : IFunction
    {
        public Func<Interpreter, ClrFunctionArguments, DynValue> Invokable { get; }

        public ClrFunction(Func<Interpreter, ClrFunctionArguments, DynValue> invokable)
        {
            Invokable = invokable;
        }
    }
}