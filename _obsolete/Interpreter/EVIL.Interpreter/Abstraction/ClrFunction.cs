using System;

namespace EVIL.Interpreter.Abstraction
{
    public class ClrFunction
    {
        public Func<Execution.Interpreter, FunctionArguments, DynValue> Invokable { get; }

        public ClrFunction(Func<Execution.Interpreter, FunctionArguments, DynValue> invokable)
        {
            Invokable = invokable;
        }
    }
}