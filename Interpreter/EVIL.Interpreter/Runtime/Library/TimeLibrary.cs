using System;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("time")]
    public class TimeLibrary
    {
        [ClrFunction("stamp")]
        public static DynValue Stamp(Execution.Interpreter interpreter, FunctionArguments args)
        {
            return new(
                DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
            );
        }

        [ClrFunction("ticks")]
        public static DynValue Ticks(Execution.Interpreter interpreter, FunctionArguments args)
        {
            return new(
                DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks
            );
        }
    }
}