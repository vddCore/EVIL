using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Modules
{
    public sealed class MathModule : RuntimeModule
    {
        public override string FullyQualifiedName => "math";

        public MathModule()
        {
            AddGetter("pi", (_) => Math.PI);
            AddGetter("e", (_) => Math.E);
            AddGetter("tau", (_) => Math.Tau);

            Set("rand", new PropertyTable
            {
                { "i32", (_) => Random.Shared.Next() },
                { "i64", (_) => Random.Shared.NextInt64() },
                { "f32", (_) => Random.Shared.NextSingle() },
                { "f64", (_) => Random.Shared.NextDouble() }
            });
        }

        [RuntimeModuleFunction("abs", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Abs(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Abs(value);
        }

        [RuntimeModuleFunction("acos", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Acos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Acos(value);
        }

        [RuntimeModuleFunction("acosh", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Acosh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Acosh(value);
        }

        [RuntimeModuleFunction("asin", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Asin(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Asin(value);
        }

        [RuntimeModuleFunction("asinh", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Asinh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Asinh(value);
        }

        [RuntimeModuleFunction("atan", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Atan(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Atan(value);
        }

        [RuntimeModuleFunction("atanh", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Atanh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Atanh(value);
        }

        [RuntimeModuleFunction("atan2", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Atan2(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var y)
                .ExpectNumberAt(1, out var x);

            return Math.Atan2(y, x);
        }

        [RuntimeModuleFunction("cbrt", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Cbrt(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Cbrt(value);
        }

        [RuntimeModuleFunction("ceil", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Ceiling(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Ceiling(value);
        }

        [RuntimeModuleFunction("clamp", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Clamp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectNumberAt(0, out var value)
                .ExpectNumberAt(1, out var min)
                .ExpectNumberAt(2, out var max);

            return Math.Clamp(value, min, max);
        }

        [RuntimeModuleFunction("cos", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Cos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Cos(value);
        }

        [RuntimeModuleFunction("cosh", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Cosh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Cosh(value);
        }

        [RuntimeModuleFunction("exp", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Exp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Exp(value);
        }

        [RuntimeModuleFunction("floor", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Floor(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Floor(value);
        }

        [RuntimeModuleFunction("lerp", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Lerp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b)
                .ExpectNumberAt(2, out var t);

            t = Math.Clamp(t, 0.0, 1.0);
            return a * (1 - t) + b * t;
        }

        [RuntimeModuleFunction("log", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Log(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var value)
                .ExpectNumberAt(1, out var @base);

            return Math.Log(value, @base);
        }

        [RuntimeModuleFunction("log2", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Log2(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Log2(value);
        }

        [RuntimeModuleFunction("log10", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Log10(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);

            return Math.Log10(value);
        }

        [RuntimeModuleFunction("max", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Max(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b);

            return Math.Max(a, b);
        }

        [RuntimeModuleFunction("min", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Min(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b);

            return Math.Min(a, b);
        }

        [RuntimeModuleFunction("pow", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Pow(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var x)
                .ExpectNumberAt(1, out var y);

            return Math.Pow(x, y);
        }

        [RuntimeModuleFunction("round", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Round(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectNumberAt(0, out var x)
                .ExpectNumberAt(1, out var decimals);

            return Math.Round(x, (int)decimals);
        }

        [RuntimeModuleFunction("sign", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Sign(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sign(value);
        }

        [RuntimeModuleFunction("sin", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Sin(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sin(value);
        }

        [RuntimeModuleFunction("sincos", ReturnType = DynamicValueType.Table)]
        private static DynamicValue SinCos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            var result = Math.SinCos(value);

            return new Table
            {
                { "sin", result.Sin },
                { "cos", result.Cos }
            };
        }

        [RuntimeModuleFunction("sinh", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Sinh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sinh(value);
        }

        [RuntimeModuleFunction("sqrt", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Sqrt(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Sqrt(value);
        }

        [RuntimeModuleFunction("tan", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Tan(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Tan(value);
        }

        [RuntimeModuleFunction("tanh", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Tanh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Tanh(value);
        }

        [RuntimeModuleFunction("trunc", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Trunc(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var value);
            
            return Math.Truncate(value);
        }

        [RuntimeModuleFunction("rad2deg", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Rad2Deg(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var radians);
            
            return radians * (180.0 / Math.PI);
        }

        [RuntimeModuleFunction("deg2rad", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Deg2Rad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectNumberAt(0, out var degrees);
            
            return degrees * (Math.PI / 180.0);
        }
    }
}