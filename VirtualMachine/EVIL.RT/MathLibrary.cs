using System;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.ExecutionEngine.Interop;
using EVIL.RT;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("math")]
    public class MathLibrary
    {
        internal static Random Random { get; } = new();

        [ClrFunction("rnd", RuntimeAlias = "math.rnd")]
        public static DynamicValue Rnd(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectAtMost(2)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            var max = args[0].Number;

            if (args.Length == 2)
            {
                args.ExpectTypeAtIndex(1, DynamicValueType.Number);

                var min = args[1].Number;
                return new DynamicValue(Random.Next((int)min, (int)max));
            }
            else
            {
                return new DynamicValue(Random.Next((int)max));
            }
        }

        [ClrFunction("sin", RuntimeAlias = "math.sin")]
        public static DynamicValue Sin(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Sin(args[0].Number));
        }

        [ClrFunction("cos", RuntimeAlias = "math.cos")]
        public static DynamicValue Cos(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Cos(args[0].Number));
        }

        [ClrFunction("tan", RuntimeAlias = "math.tan")]
        public static DynamicValue Tan(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Tan(args[0].Number));
        }

        [ClrFunction("cot", RuntimeAlias = "math.cot")]
        public static DynamicValue Cot(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(1 / Math.Tan(args[0].Number));
        }

        [ClrFunction("atan", RuntimeAlias = "math.atan")]
        public static DynamicValue Atan(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Atan(args[0].Number));
        }

        [ClrFunction("atan2", RuntimeAlias = "math.atan2")]
        public static DynamicValue Atan2(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.Number)
                .ExpectTypeAtIndex(1, DynamicValueType.Number);

            return new DynamicValue(Math.Atan2(args[0].Number, args[1].Number));
        }

        [ClrFunction("floor", RuntimeAlias = "math.floor")]
        public static DynamicValue Floor(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Floor(args[0].Number));
        }

        [ClrFunction("ceil", RuntimeAlias = "math.ceil")]
        public static DynamicValue Ceil(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Ceiling(args[0].Number));
        }

        [ClrFunction("abs", RuntimeAlias = "math.abs")]
        public static DynamicValue Abs(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Abs(args[0].Number));
        }

        [ClrFunction("sign", RuntimeAlias = "math.sign")]
        public static DynamicValue Sign(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Sign(args[0].Number));
        }

        [ClrFunction("pow", RuntimeAlias = "math.pow")]
        public static DynamicValue Pow(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.Number)
                .ExpectTypeAtIndex(1, DynamicValueType.Number);

            return new DynamicValue(Math.Pow(args[0].Number, args[1].Number));
        }

        [ClrFunction("sqrt", RuntimeAlias = "math.sqrt")]
        public static DynamicValue Sqrt(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Sqrt(args[0].Number));
        }

        [ClrFunction("log", RuntimeAlias = "math.log")]
        public static DynamicValue Log(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.Number)
                .ExpectTypeAtIndex(1, DynamicValueType.Number);

            return new DynamicValue(Math.Log(args[0].Number, args[1].Number));
        }

        [ClrFunction("ln", RuntimeAlias = "math.ln")]
        public static DynamicValue Ln(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new DynamicValue(Math.Log10(args[0].Number));
        }
    }
}