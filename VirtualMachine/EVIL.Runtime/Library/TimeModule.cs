﻿using System;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.ExecutionEngine.Interop;

namespace EVIL.Runtime.Library
{
    [ClrLibrary("time")]
    public class TimeModule
    {
        [ClrFunction("stamp", RuntimeAlias = "time.stamp")]
        public static DynamicValue Stamp(ExecutionContext ctx, params DynamicValue[] args)
        {
            return new(
                DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
            );
        }

        [ClrFunction("ticks", RuntimeAlias = "ticks")]
        public static DynamicValue Ticks(ExecutionContext ctx, params DynamicValue[] args)
        {
            return new(
                DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks
            );
        }
    }
}