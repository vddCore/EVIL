﻿using System;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("str")]
    public class StringLibrary
    {
        [ClrFunction("len")]
        public static DynValue Length(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            return new DynValue(args[0].String.Length);
        }

        [ClrFunction("chr")]
        public static DynValue ToChar(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectIntegerAtIndex(0);

            return new DynValue(((char)args[0].Number).ToString());
        }

        [ClrFunction("code")]
        public static DynValue ToCharCode(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            if (str.Length != 1)
                throw new ClrFunctionException("Expected a single character.");

            return new DynValue(str[0]);
        }

        [ClrFunction("at")]
        public static DynValue CharAt(Execution.Interpreter interpreter, FunctionArguments args)
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

        [ClrFunction("sub")]
        public static DynValue Substring(Execution.Interpreter interpreter, FunctionArguments args)
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

        [ClrFunction("s2n")]
        public static DynValue ToInteger(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            if (!double.TryParse(args[0].String, out var result))
                throw new ClrFunctionException("The number was in an invalid format.");

            return new DynValue(result);
        }

        [ClrFunction("n2s")]
        public static DynValue NumberToString(Execution.Interpreter interpreter, FunctionArguments args)
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

        [ClrFunction("spl")]
        public static DynValue Split(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectAtLeast(2)
                .ExpectAtMost(3)
                .ExpectTypeAtIndex(0, DynValueType.String)
                .ExpectTypeAtIndex(1, DynValueType.String);

            var splitOptions = StringSplitOptions.None;
            
            if (args.Count == 3)
            {
                args.ExpectTypeAtIndex(2, DynValueType.String);
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
                        default: throw new ClrFunctionException($"Unexpected split mode '{flags[i]}'.");
                    }
                }
            }

            var str = args[0].String;
            var with = args[1].String;

            var splitStuff = str.Split(with, splitOptions);
            var retTable = new Table();

            for (var i = 0; i < splitStuff.Length; i++)
            {
                retTable[i] = new DynValue(splitStuff[i]);
            }

            return new DynValue(retTable);
        }

        [ClrFunction("uc")]
        public static DynValue UpperCase(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            return new DynValue(str.ToUpperInvariant());
        }

        [ClrFunction("lc")]
        public static DynValue LowerCase(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var str = args[0].String;

            return new DynValue(str.ToLowerInvariant());
        }
    }
}