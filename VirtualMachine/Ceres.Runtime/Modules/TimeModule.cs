using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public sealed class TimeModule : RuntimeModule
    {
        public override string FullyQualifiedName => "time";
        
        [RuntimeModuleFunction("stamp")]
        private static DynamicValue Timestamp(Fiber fiber, params DynamicValue[] args)
        {
            return new DynamicValue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
}   