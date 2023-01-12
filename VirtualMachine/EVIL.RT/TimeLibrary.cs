using System;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("time")]
    public class TimeLibrary
    {
        [ClrFunction("stamp")]
        public static DynamicValue Stamp(EVM evm, params DynamicValue[] args)
        {
            return new(
                DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
            );
        }

        [ClrFunction("ticks")]
        public static DynamicValue Ticks(EVM evm, params DynamicValue[] args)
        {
            return new(
                DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks
            );
        }
    }
}