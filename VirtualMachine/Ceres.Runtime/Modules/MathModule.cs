using Ceres.ExecutionEngine.Collections;
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

            Set("rand", new PropertyTable
            {
                { "i32", (_) => Random.Shared.Next() },
                { "i64", (_) => Random.Shared.NextInt64() },
                { "f32", (_) => Random.Shared.NextSingle() },
                { "f64", (_) => Random.Shared.NextDouble() }
            });
        }

        [RuntimeModuleFunction("abs")]
        private static DynamicValue Abs(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Abs(value);
        }
        
        [RuntimeModuleFunction("acos")]
        private static DynamicValue Acos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Acos(value);
        }
        
        [RuntimeModuleFunction("acosh")]
        private static DynamicValue Acosh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Acosh(value);
        }

        [RuntimeModuleFunction("asin")]
        private static DynamicValue Asin(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Asin(value);
        }
        
        [RuntimeModuleFunction("asinh")]
        private static DynamicValue Asinh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Asinh(value);
        }
        
        [RuntimeModuleFunction("atan")]
        private static DynamicValue Atan(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Atan(value);
        }
        
        [RuntimeModuleFunction("atanh")]
        private static DynamicValue Atanh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Atanh(value);
        }
        
        [RuntimeModuleFunction("atan2")]
        private static DynamicValue Atan2(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var y)
                .ExpectNumberAt(1, out var x);
            
            return Math.Atan2(y, x);
        }
        
        [RuntimeModuleFunction("cbrt")]
        private static DynamicValue Cbrt(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Cbrt(value);
        }
        
        [RuntimeModuleFunction("ceil")]
        private static DynamicValue Ceiling(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Ceiling(value);
        }
        
        [RuntimeModuleFunction("clamp")]
        private static DynamicValue Clamp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value)
                .ExpectNumberAt(1, out var min)
                .ExpectNumberAt(2, out var max);

            return Math.Clamp(value, min, max);
        }
        
        [RuntimeModuleFunction("cos")]
        private static DynamicValue Cos(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Cos(value);
        }
        
        [RuntimeModuleFunction("cosh")]
        private static DynamicValue Cosh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Cosh(value);
        }
        
        [RuntimeModuleFunction("exp")]
        private static DynamicValue Exp(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Exp(value);
        }
        
        [RuntimeModuleFunction("floor")]
        private static DynamicValue Floor(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Floor(value);
        }

        [RuntimeModuleFunction("log")]
        private static DynamicValue Log(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value)
                .ExpectNumberAt(1, out var @base);
            
            return Math.Log(value, @base);
        }
        
        [RuntimeModuleFunction("log2")]
        private static DynamicValue Log2(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Log2(value);
        }
        
        [RuntimeModuleFunction("log10")]
        private static DynamicValue Log10(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Log10(value);
        }
        
        [RuntimeModuleFunction("max")]
        private static DynamicValue Max(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b);
            
            return Math.Max(a, b);
        }
        
        [RuntimeModuleFunction("min")]
        private static DynamicValue Min(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var a)
                .ExpectNumberAt(1, out var b);
            
            return Math.Min(a, b);
        }
        
        [RuntimeModuleFunction("pow")]
        private static DynamicValue Pow(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var x)
                .ExpectNumberAt(1, out var y);
            
            return Math.Pow(x, y);
        }
        
        [RuntimeModuleFunction("round")]
        private static DynamicValue Round(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var x)
                .ExpectNumberAt(1, out var decimals);

            return Math.Round(x, (int)decimals);
        }
        
        [RuntimeModuleFunction("sign")]
        private static DynamicValue Sign(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var x);
            return Math.Sign(x);
        }

        [RuntimeModuleFunction("sin")]
        private static DynamicValue Sin(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Sin(value);
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
        
        [RuntimeModuleFunction("sinh")]
        private static DynamicValue Sinh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Sinh(value);
        }
        
        [RuntimeModuleFunction("sqrt")]
        private static DynamicValue Sqrt(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Sqrt(value);
        }
        
        [RuntimeModuleFunction("tan")]
        private static DynamicValue Tan(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Tan(value);
        }
        
        [RuntimeModuleFunction("tanh")]
        private static DynamicValue Tanh(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Tanh(value); 
        }
        
        [RuntimeModuleFunction("trunc")]
        private static DynamicValue Trunc(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var value);
            return Math.Truncate(value);
        }
    }
}