using System;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("math")]
    public class MathLibrary
    {
        internal static Random Random { get; } = new();

        [ClrFunction("rnd")]
        public static DynValue Rnd(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectAtLeast(1)
                .ExpectAtMost(2)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            var max = args[0].Number;

            if (args.Count == 2)
            {
                args.ExpectTypeAtIndex(1, DynValueType.Number);

                var min = args[1].Number;
                return new DynValue(Random.Next((int)min, (int)max));
            }
            else
            {
                return new DynValue(Random.Next((int)max));
            }
        }

        [ClrFunction("sin")]
        public static DynValue Sin(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sin(args[0].Number));
        }

        [ClrFunction("cos")]
        public static DynValue Cos(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Cos(args[0].Number));
        }

        [ClrFunction("tan")]
        public static DynValue Tan(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Tan(args[0].Number));
        }

        [ClrFunction("cot")]
        public static DynValue Cot(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(1 / Math.Tan(args[0].Number));
        }

        [ClrFunction("atan")]
        public static DynValue Atan(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Atan(args[0].Number));
        }

        [ClrFunction("atan2")]
        public static DynValue Atan2(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Atan2(args[0].Number, args[1].Number));
        }

        [ClrFunction("floor")]
        public static DynValue Floor(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Floor(args[0].Number));
        }

        [ClrFunction("ceil")]
        public static DynValue Ceil(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Ceiling(args[0].Number));
        }

        [ClrFunction("abs")]
        public static DynValue Abs(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Abs(args[0].Number));
        }

        [ClrFunction("sign")]
        public static DynValue Sign(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sign(args[0].Number));
        }

        [ClrFunction("pow")]
        public static DynValue Pow(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Pow(args[0].Number, args[1].Number));
        }

        [ClrFunction("sqrt")]
        public static DynValue Sqrt(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sqrt(args[0].Number));
        }

        [ClrFunction("log")]
        public static DynValue Log(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Log(args[0].Number, args[1].Number));
        }

        [ClrFunction("ln")]
        public static DynValue Ln(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Log10(args[0].Number));
        }
    }
}