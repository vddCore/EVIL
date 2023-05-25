using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;

namespace Ceres.Runtime.Modules
{
    public class MathModule : RuntimeModule
    {
        public override string FullyQualifiedName => "math";
        
        [RuntimeModuleFunction("sqrt")]
        private static DynamicValue Print(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return new(Math.Sqrt(value));
        }
    }
}