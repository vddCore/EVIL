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

        [RuntimeModuleFunction("freeze", ReturnType = DynamicValue.DynamicValueType.Table)]
        private static DynamicValue Freeze(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            table.Freeze();
            
            return table;
        }
        
        [RuntimeModuleFunction("unfreeze", ReturnType = DynamicValue.DynamicValueType.Table)]
        private static DynamicValue Unfreeze(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            table.Unfreeze();

            return table;
        }
        
        [RuntimeModuleFunction("is_frozen", ReturnType = DynamicValue.DynamicValueType.Boolean)]
        private static DynamicValue /*IsFrozen is already a symbol...*/ _IsFrozen(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            return table.IsFrozen;  
        }

        [RuntimeModuleFunction("keys", ReturnType = DynamicValue.DynamicValueType.Table)]
        private static DynamicValue Keys(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            return table.GetKeys();
        }

        [RuntimeModuleFunction("values")]
        private static DynamicValue Values(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            return table.GetValues();
        }

        [RuntimeModuleFunction("cpy")]
        private static DynamicValue Copy(Fiber _, params DynamicValue[] args)
        {
            args.ExpectTableAt(0, out var table)
                .OptionalBooleanAt(1, false, out var deep);

            return deep ? table.DeepCopy() : table.ShallowCopy();
        }
    }
}