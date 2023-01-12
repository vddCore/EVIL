using System;
using EVIL.Abstraction;
using EVIL.Execution;

namespace EVIL.Runtime.Library
{
    public class StringLibrary
    {
        [ClrFunction("str.len")]
        public static DynValue Length(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            return new DynValue(args[0].String.Length);
        }

        [ClrFunction("str.chr")]
        public static DynValue ToChar(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectIntegerAtIndex(0);

            return new DynValue(((char)args[0].Number).ToString());
        }
        
        [ClrFunction("str.code")]
        public static DynValue ToCharCode(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            if (str.Length != 1)
                throw new ClrFunctionException("Expected a single character.");

            return new DynValue(str[0]);
        }
        
        [ClrFunction("str.at")]
        public static DynValue CharAt(Interpreter interpreter, ClrFunctionArguments args)
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

        [ClrFunction("str.sub")]
        public static DynValue Substring(Interpreter interpreter, ClrFunctionArguments args)
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
        
        [ClrFunction("str.s2n")]
        public static DynValue ToNumber(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            if (!decimal.TryParse(args[0].String, out var result))
                throw new ClrFunctionException("The number was in an invalid format.");

            return new DynValue(result);
        }

        [ClrFunction("str.n2s")]
        public static DynValue NumberToString(Interpreter interpreter, ClrFunctionArguments args)
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

        [ClrFunction("str.spl")]
        public static DynValue Split(Interpreter interpreter, ClrFunctionArguments args)
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

        [ClrFunction("str.uc")]
        public static DynValue UpperCase(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            return new DynValue(str.ToUpperInvariant());
        }
        
        [ClrFunction("str.lc")]
        public static DynValue LowerCase(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            return new DynValue(str.ToLowerInvariant());
        }
    }
}
