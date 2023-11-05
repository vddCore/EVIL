using System.Linq;
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
        private static DynamicValue IndexOf(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectAnyAt(1, out var value);

            return array.IndexOf(value);
        }
        
        [RuntimeModuleFunction("fill", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue Fill(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectAnyAt(1, out var value);

            array.Fill(value);
            
            return DynamicValue.Nil;
        }
        
        [RuntimeModuleFunction("resize", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Resize(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectIntegerAt(1, out var size);

            return array.Resize((int)size);
        }
        
        [RuntimeModuleFunction("push", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Push(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array);

            var values = args.Skip(1).ToArray();
            return array.Push(values);
        }
        
        [RuntimeModuleFunction("insert", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Insert(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectIntegerAt(1, out var index);

            var values = args.Skip(2).ToArray();
            return array.Insert((int)index, values);
        }
        
        [RuntimeModuleFunction("rsh")]
        private static DynamicValue RightShift(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array);
            return array.RightShift();
        }
        
        [RuntimeModuleFunction("lsh")]
        private static DynamicValue Shift(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array);
            return array.LeftShift();
        }
    }
}