using System;
using EVIL.Abstraction;
using EVIL.Execution;

namespace EVIL.Runtime.Library
{
    public class MathLibrary
    {
        internal static Random Random { get; } = new();

        [ClrFunction("math.rnd")]
        public static DynValue Rnd(Interpreter interpreter, ClrFunctionArguments args)
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

        [ClrFunction("math.sin")]
        public static DynValue Sin(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sin(args[0].Number));
        }

        [ClrFunction("math.cos")]
        public static DynValue Cos(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Cos(args[0].Number));
        }

        [ClrFunction("math.tan")]
        public static DynValue Tan(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Tan(args[0].Number));
        }

        [ClrFunction("math.cot")]
        public static DynValue Cot(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(1 / Math.Tan(args[0].Number));
        }

        [ClrFunction("math.atan")]
        public static DynValue Atan(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Atan(args[0].Number));
        }

        [ClrFunction("math.atan2")]
        public static DynValue Atan2(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Atan2(args[0].Number, args[1].Number));
        }

        [ClrFunction("math.floor")]
        public static DynValue Floor(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Floor(args[0].Number));
        }

        [ClrFunction("math.ceil")]
        public static DynValue Ceil(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Ceiling(args[0].Number));
        }

        [ClrFunction("math.abs")]
        public static DynValue Abs(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Abs(args[0].Number));
        }

        [ClrFunction("math.sign")]
        public static DynValue Sign(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sign(args[0].Number));
        }

        [ClrFunction("math.pow")]
        public static DynValue Pow(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Pow(args[0].Number, args[1].Number));
        }

        [ClrFunction("math.sqrt")]
        public static DynValue Sqrt(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sqrt(args[0].Number));
        }

        [ClrFunction("math.log")]
        public static DynValue Log(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Log(args[0].Number, args[1].Number));
        }

        [ClrFunction("math.ln")]
        public static DynValue Ln(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Log10(args[0].Number));
        }
    }
}
