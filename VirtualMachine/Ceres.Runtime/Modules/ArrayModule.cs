using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public class ArrayModule : RuntimeModule
    {
        public override string FullyQualifiedName => "arr";


        [RuntimeModuleFunction("indof", ReturnType = DynamicValueType.Number)]
        private static DynamicValue IndexOf(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectAnyAt(1, out var value);

            return array.IndexOf(value);
        }
        
        [RuntimeModuleFunction("fill", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue Fill(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectAnyAt(1, out var value);

            array.Fill(value);
            
            return DynamicValue.Nil;
        }
    }
}