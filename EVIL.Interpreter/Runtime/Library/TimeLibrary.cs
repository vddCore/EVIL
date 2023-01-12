using System;
using EVIL.Interpreter.Execution;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    public class TimeLibrary
    {
        [ClrFunction("time.stamp")]
        public static DynValue Stamp(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectNone();

            var stamp = (decimal)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

            return new DynValue(stamp);
        }
    }
}
