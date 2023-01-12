using System;
using EVIL.Abstraction;
using EVIL.Execution;

namespace EVIL.Runtime.Library
{
    public class TimeLibrary
    {
        [ClrFunction("time.stamp")]
        public static DynValue Stamp(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectNone();

            var stamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

            return new DynValue(stamp);
        }
    }
}
