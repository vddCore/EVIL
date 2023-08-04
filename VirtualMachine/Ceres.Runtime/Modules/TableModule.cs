using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;

namespace Ceres.Runtime.Modules
{
    public class TableModule : RuntimeModule
    {
        public override string FullyQualifiedName => "tbl";

        [RuntimeModuleFunction("clear", ReturnType = DynamicValue.DynamicValueType.Nil)]
        private static DynamicValue Clear(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);
            
            table.Clear();
           
            return DynamicValue.Nil;
        }
    }
}