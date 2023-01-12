using System;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;

namespace EVIL.RT
{
    [ClrLibrary("str")]
    public class StringLibrary
    {
        [ClrFunction("len")]
        public static DynamicValue Length(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            return new DynamicValue(args[0].String.Length);
        }

        [ClrFunction("chr", RuntimeAlias = "str.chr")]
        public static DynamicValue ToChar(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectIntegerAtIndex(0);

            return new DynamicValue(((char)args[0].Number).ToString());
        }

        [ClrFunction("code", RuntimeAlias = "str.code")]
        public static DynamicValue ToCharCode(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            var str = args[0].String;

            if (str.Length != 1)
                throw new EvilRuntimeException("Expected a single character.");

            return new DynamicValue(str[0]);
        }

        [ClrFunction("at", RuntimeAlias = "str.at")]
        public static DynamicValue CharAt(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.String)
                .ExpectIntegerAtIndex(1);

            var str = args[0].String;
            var index = (int)args[1].Number;

            if (index >= str.Length)
                throw new EvilRuntimeException("Requested index is outside the provided string.");

            return new DynamicValue(str[index].ToString());
        }

        [ClrFunction("sub", RuntimeAlias = "str.sub")]
        public static DynamicValue Substring(EVM evm, params DynamicValue[] args)
        {
            args.ExpectAtLeast(2)
                .ExpectAtMost(3);

            if (args.Length == 2)
            {
                args.ExpectTypeAtIndex(0, DynamicValueType.String)
                    .ExpectIntegerAtIndex(1);

                var str = args[0].String;
                var startIndex = (int)args[1].Number;

                if (startIndex < 0 || startIndex >= str.Length)
                    throw new EvilRuntimeException("The starting index is outside the provided string.");

                return new DynamicValue(str.Substring(startIndex));
            }
            else
            {
                args.ExpectTypeAtIndex(0, DynamicValueType.String)
                    .ExpectIntegerAtIndex(1)
                    .ExpectIntegerAtIndex(2);

                var str = args[0].String;
                var startIndex = (int)args[1].Number;

                if (startIndex < 0 || startIndex >= str.Length)
                    throw new EvilRuntimeException("The starting index is outside the provided string.");

                var length = (int)args[2].Number;
                if (length < 0 || length + startIndex > str.Length)
                    throw new EvilRuntimeException("The provided length of substring exceeds the base string bounds.");

                return new DynamicValue(str.Substring(startIndex, length));
            }
        }

        [ClrFunction("s2n", RuntimeAlias = "str.s2n")]
        public static DynamicValue ToInteger(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            if (!double.TryParse(args[0].String, out var result))
                throw new EvilRuntimeException("The number was in an invalid format.");

            return new DynamicValue(result);
        }

        [ClrFunction("n2s", RuntimeAlias = "str.n2s")]
        public static DynamicValue NumberToString(EVM evm, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectIntegerAtIndex(0);

            var number = (int)args[0].Number;
            var toBase = 10;

            if (args.Length == 2)
            {
                args.ExpectIntegerAtIndex(1);
                toBase = (int)args[1].Number;
            }

            return new DynamicValue(Convert.ToString(number, toBase));
        }

        [ClrFunction("spl", RuntimeAlias = "str.spl")]
        public static DynamicValue Split(EVM evm, params DynamicValue[] args)
        {
            args.ExpectAtLeast(2)
                .ExpectAtMost(3)
                .ExpectTypeAtIndex(0, DynamicValueType.String)
                .ExpectTypeAtIndex(1, DynamicValueType.String);

            var splitOptions = StringSplitOptions.None;

            if (args.Length == 3)
            {
                args.ExpectTypeAtIndex(2, DynamicValueType.String);
                var flags = args[2].String;

                for (var i = 0; i < flags.Length; i++)
                {
                    switch (flags[i])
                    {
                        case 't':
                            splitOptions |= StringSplitOptions.TrimEntries;
                            break;
                        case 'r':
                            splitOptions |= StringSplitOptions.RemoveEmptyEntries;
                            break;
                        default: throw new EvilRuntimeException($"Unexpected split mode '{flags[i]}'.");
                    }
                }
            }

            var str = args[0].String;
            var with = args[1].String;

            var splitStuff = str.Split(with, splitOptions);
            var retTable = new Table();

            for (var i = 0; i < splitStuff.Length; i++)
            {
                retTable.SetByNumber(i, new DynamicValue(splitStuff[i]));
            }

            return new DynamicValue(retTable);
        }

        [ClrFunction("uc", RuntimeAlias = "str.uc")]
        public static DynamicValue UpperCase(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            var str = args[0].String;

            return new DynamicValue(str.ToUpperInvariant());
        }

        [ClrFunction("lc", RuntimeAlias = "str.lc")]
        public static DynamicValue LowerCase(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            var str = args[0].String;

            return new DynamicValue(str.ToLowerInvariant());
        }
    }
}