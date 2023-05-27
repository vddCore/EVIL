using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;

namespace Ceres.Runtime.Modules
{
    public class MathModule : RuntimeModule
    {
        public override string FullyQualifiedName => "math";

        public MathModule()
        {
            AddGetter("pi", (_) => Math.PI);
            AddGetter("e", (_) => Math.E);
            AddGetter("tau", (_) => Math.Tau);
        }

        [RuntimeModuleFunction("abs")]
        private static DynamicValue Abs(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Abs(value);
        }

        [RuntimeModuleFunction("sqrt")]
        private static DynamicValue Sqrt(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Sqrt(value);
        }

        [RuntimeModuleFunction("sin")]
        private static DynamicValue Sin(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Sin(value);
        }

        [RuntimeModuleFunction("cos")]
        private static DynamicValue Cos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Cos(value);
        }

        [RuntimeModuleFunction("sincos")]
        private static DynamicValue SinCos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            var result = Math.SinCos(value);

            return new Table
            {
                { "sin", result.Sin },
                { "cos", result.Cos }
            };
        }
    }
}