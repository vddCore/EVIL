using System;
using EVIL.Interpreter.Execution;

namespace EVIL.Interpreter.Abstraction
{
    public class ClrFunction
    {
        public Func<Execution.Interpreter, ClrFunctionArguments, DynValue> Invokable { get; }

        public ClrFunction(Func<Execution.Interpreter, ClrFunctionArguments, DynValue> invokable)
        {
            Invokable = invokable;
        }
    }
}