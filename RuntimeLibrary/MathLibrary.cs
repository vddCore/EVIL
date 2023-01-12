using System;
using EVIL.Abstraction;
using EVIL.Execution;
using EVIL.RuntimeLibrary.Base;

namespace EVIL.RuntimeLibrary
{
    public class MathLibrary : ClrPackage
    {
        internal static Random Random { get; } = new();

        private DynValue Rnd(Interpreter interpreter, ClrFunctionArguments args)
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

        private DynValue Sin(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sin(args[0].Number));
        }

        private DynValue Cos(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Cos(args[0].Number));
        }

        private DynValue Tan(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Tan(args[0].Number));
        }

        private DynValue Cot(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(1 / Math.Tan(args[0].Number));
        }

        private DynValue Atan(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Atan(args[0].Number));
        }

        private DynValue Atan2(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Atan2(args[0].Number, args[1].Number));
        }

        private DynValue Floor(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Floor(args[0].Number));
        }

        private DynValue Ceil(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Ceiling(args[0].Number));
        }

        private DynValue Abs(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Abs(args[0].Number));
        }

        private DynValue Sign(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sign(args[0].Number));
        }

        private DynValue Pow(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Pow(args[0].Number, args[1].Number));
        }

        private DynValue Sqrt(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Sqrt(args[0].Number));
        }

        private DynValue Log(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Number)
                .ExpectTypeAtIndex(1, DynValueType.Number);

            return new DynValue(Math.Log(args[0].Number, args[1].Number));
        }

        private DynValue Ln(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.Number);

            return new DynValue(Math.Log10(args[0].Number));
        }

        public override void Register(Environment env, Interpreter interpreter)
        {
            env.RegisterBuiltIn("math.sin", Sin);
            env.RegisterBuiltIn("math.cos", Cos);
            env.RegisterBuiltIn("math.tan", Tan);
            env.RegisterBuiltIn("math.cot", Cot);
            env.RegisterBuiltIn("math.atan", Atan);
            env.RegisterBuiltIn("math.atan2", Atan2);
            env.RegisterBuiltIn("math.floor", Floor);
            env.RegisterBuiltIn("math.ceil", Ceil);
            env.RegisterBuiltIn("math.abs", Abs);
            env.RegisterBuiltIn("math.sign", Sign);
            env.RegisterBuiltIn("math.pow", Pow);
            env.RegisterBuiltIn("math.sqrt", Sqrt);
            env.RegisterBuiltIn("math.log", Log);
            env.RegisterBuiltIn("math.ln", Ln);
            env.RegisterBuiltIn("math.rnd", Rnd);
        }
    }
}
