using System;
using EVIL.Abstraction;
using EVIL.Execution;
using EVIL.RuntimeLibrary.Base;

namespace EVIL.RuntimeLibrary
{
    public class StringLibrary : ClrPackage
    {
        private DynValue Length(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            return new DynValue(args[0].String.Length);
        }

        private DynValue ToChar(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectIntegerAtIndex(0);

            return new DynValue(((char)args[0].Number).ToString());
        }

        private DynValue ToCharCode(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            if (str.Length != 1)
                throw new ClrFunctionException("Expected a single character.");

            return new DynValue(str[0]);
        }

        private DynValue CharAt(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.String)
                .ExpectIntegerAtIndex(1);

            var str = args[0].String;
            var index = (int)args[1].Number;

            if (index >= str.Length)
                throw new ClrFunctionException("Requested index is outside the provided string.");

            return new DynValue(str[index].ToString());
        }

        public DynValue Substring(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectAtLeast(2)
                .ExpectAtMost(3);

            if (args.Count == 2)
            {
                args.ExpectTypeAtIndex(0, DynValueType.String)
                    .ExpectIntegerAtIndex(1);

                var str = args[0].String;
                var startIndex = (int)args[1].Number;

                if (startIndex < 0 || startIndex >= str.Length)
                    throw new ClrFunctionException("The starting index is outside the provided string.");

                return new DynValue(str.Substring(startIndex));
            }
            else
            {
                args.ExpectTypeAtIndex(0, DynValueType.String)
                    .ExpectIntegerAtIndex(1)
                    .ExpectIntegerAtIndex(2);

                var str = args[0].String;
                var startIndex = (int)args[1].Number;

                if (startIndex < 0 || startIndex >= str.Length)
                    throw new ClrFunctionException("The starting index is outside the provided string.");

                var length = (int)args[2].Number;
                if (length < 0 || length + startIndex > str.Length)
                    throw new ClrFunctionException("The provided length of substring exceeds the base string bounds.");

                return new DynValue(str.Substring(startIndex, length));
            }
        }

        public DynValue ToCharTable(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;
            var table = new Table();

            for (var i = 0; i < str.Length; i++)
                table[i] = new DynValue(str[i].ToString());

            return new DynValue(table);
        }

        public DynValue ToNumber(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            if (!double.TryParse(args[0].String, out var result))
                throw new ClrFunctionException("The number was in an invalid format.");

            return new DynValue(result);
        }

        public DynValue NumberToString(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectAtLeast(1)
                .ExpectIntegerAtIndex(0);

            var number = (int)args[0].Number;
            var toBase = 10;

            if (args.Count == 2)
            {
                args.ExpectIntegerAtIndex(1);
                toBase = (int)args[1].Number;
            }

            return new DynValue(Convert.ToString(number, toBase));
        }

        public DynValue Split(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.String)
                .ExpectTypeAtIndex(1, DynValueType.String);

            var str = args[0].String;
            var with = args[1].String;

            if (with.Length > 1)
                throw new ClrFunctionException("Expected a single character at index 1");

            var splitStuff = str.Split(with[0]);
            var retTable = new Table();

            for (var i = 0; i < splitStuff.Length; i++)
            {
                retTable[i] = new DynValue(splitStuff[i]);
            }

            return new DynValue(retTable);
        }

        public DynValue UpperCase(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            return new DynValue(str.ToUpperInvariant());
        }
        
        public DynValue LowerCase(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            return new DynValue(str.ToLowerInvariant());
        }
        
        public override void Register(Environment env, Interpreter interpreter)
        {
            env.RegisterBuiltIn("str.len", Length);
            env.RegisterBuiltIn("str.chr", ToChar);
            env.RegisterBuiltIn("str.code", ToCharCode);
            env.RegisterBuiltIn("str.at", CharAt);
            env.RegisterBuiltIn("str.sub", Substring);
            env.RegisterBuiltIn("str.tbl", ToCharTable);
            env.RegisterBuiltIn("str.s2n", ToNumber);
            env.RegisterBuiltIn("str.n2s", NumberToString);
            env.RegisterBuiltIn("str.spl", Split);
            env.RegisterBuiltIn("str.uc", UpperCase);
            env.RegisterBuiltIn("str.lc", LowerCase);
        }
    }
}
