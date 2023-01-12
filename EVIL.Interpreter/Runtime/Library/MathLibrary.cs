using System;
using System.Linq;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Internal;

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
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            var max = args[0].Decimal;

            if (args.Count == 2)
            {
                args.ExpectTypeAtIndex(1, DynValueType.Decimal);

                var min = args[1].Decimal;
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
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Sin(args[0].Decimal));
        }

        [ClrFunction("cos")]
        public static DynValue Cos(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Cos(args[0].Decimal));
        }

        [ClrFunction("tan")]
        public static DynValue Tan(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Tan(args[0].Decimal));
        }

        [ClrFunction("cot")]
        public static DynValue Cot(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(1 / DecimalMath.Tan(args[0].Decimal));
        }

        [ClrFunction("atan")]
        public static DynValue Atan(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Atan(args[0].Decimal));
        }

        [ClrFunction("atan2")]
        public static DynValue Atan2(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Decimal)
                .ExpectTypeAtIndex(1, DynValueType.Decimal);

            return new DynValue(DecimalMath.Atan2(args[0].Decimal, args[1].Decimal));
        }

        [ClrFunction("floor")]
        public static DynValue Floor(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(decimal.Floor(args[0].Decimal));
        }

        [ClrFunction("ceil")]
        public static DynValue Ceil(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(decimal.Ceiling(args[0].Decimal));
        }

        [ClrFunction("abs")]
        public static DynValue Abs(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Abs(args[0].Decimal));
        }

        [ClrFunction("sign")]
        public static DynValue Sign(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Sign(args[0].Decimal));
        }

        [ClrFunction("pow")]
        public static DynValue Pow(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Decimal)
                .ExpectTypeAtIndex(1, DynValueType.Decimal);

            return new DynValue(DecimalMath.Pow(args[0].Decimal, args[1].Decimal));
        }

        [ClrFunction("sqrt")]
        public static DynValue Sqrt(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Sqrt(args[0].Decimal));
        }

        [ClrFunction("log")]
        public static DynValue Log(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Decimal)
                .ExpectTypeAtIndex(1, DynValueType.Decimal);

            return new DynValue((decimal)Math.Log((double)args[0].Decimal, (double)args[1].Decimal));
        }

        [ClrFunction("ln")]
        public static DynValue Ln(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            return new DynValue(DecimalMath.Log10(args[0].Decimal));
        }

        [ClrFunction("bits")]
        public static DynValue Bits(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Decimal);

            var bits = decimal.GetBits(args[0].Decimal);

            var table = new Table();
            for (var i = 0; i < bits.Length; i++)
            {
                table[i] = new DynValue(bits[i]);
            }

            return new DynValue(table);
        }

        [ClrFunction("frombits")]
        public static DynValue FromBits(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTableAtIndex(0, 4, DynValueType.Decimal);

            return new DynValue(
                new decimal(
                    args[0].Table
                        .Values
                        .Select(x => (int)x.Decimal)
                        .ToArray()
                )
            );
        }
    }
}