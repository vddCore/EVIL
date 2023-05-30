using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Modules
{
    public sealed class TimeModule : RuntimeModule
    {
        public override string FullyQualifiedName => "time";

        [RuntimeModuleFunction("stamp", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Timestamp(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNone();
            
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}   