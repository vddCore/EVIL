using System;
using EVIL.Abstraction;
using EVIL.Execution;
using EVIL.RuntimeLibrary.Base;

namespace EVIL.RuntimeLibrary
{
    public class TimeLibrary : ClrPackage
    {
        public DynValue Stamp(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectNone();

            var stamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

            return new DynValue(stamp);
        }

        public override void Register(Environment env, Interpreter interpreter)
        {
            env.RegisterBuiltIn("time.stamp", Stamp);
        }
    }
}
