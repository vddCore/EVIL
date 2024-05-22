using System;
using System.Linq;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

namespace EVIL.Ceres.Runtime.Modules
{
    public class IoModule : RuntimeModule
    {
        public override string FullyQualifiedName => "io";

        [RuntimeModuleFunction("print")]
        [EvilDocFunction(
            "Writes string representations of the provided values, separated by the tabulator character, to the standard output stream.",
            Returns = "Length of the printed string, including the tabulator characters, if any.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument(
            "...",
            "__At least 1__ value to be printed.",
            CanBeNil = true
        )]
        private static DynamicValue Print(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1);

            var str = string.Join(
                "\t",
                args.Select(x => x.ConvertToString().String)
            );

            Console.Write(str);
            return str.Length;
        }

        [RuntimeModuleFunction("println")]
        [EvilDocFunction(
            "Writes string representations of the provided values, separated by the tabulator character, followed by the platform's new line sequence, to the standard output stream.",
            Returns =
                "Length of the printed string, including the tabulator characters, if any, and the new line sequence.",
            ReturnType = DynamicValueType.Number
        )]
        [EvilDocArgument(
            "...",
            "An arbitrary (can be none) amount of values to be printed.",
            CanBeNil = true
        )]
        private static DynamicValue PrintLine(Fiber _, params DynamicValue[] args)
        {
            var str = string.Join(
                "\t",
                args.Select(x => x.ConvertToString().String)
            );

            Console.Write(str);
            Console.Write(Environment.NewLine);

            return str.Length + Environment.NewLine.Length;
        }

        [RuntimeModuleFunction("readkey")]
        [EvilDocFunction(
            "Waits until the user presses a key.",
            Returns = "A table representing information about the pressed key.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument(
            "echo_to_stdout",
            "Whether to echo the pressed key to the standard output stream.",
            DynamicValueType.Boolean,
            CanBeNil = false,
            DefaultValue = "true"
        )]
        private static DynamicValue ReadKey(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, true, out var echoToStdout);

            var key = Console.ReadKey(!echoToStdout);

            return new Table
            {
                { "keycode", (int)key.Key },
                { "char", key.KeyChar.ToString() },
                { "modifiers", (int)key.Modifiers }
            };
        }

        [RuntimeModuleFunction("readln")]
        [EvilDocFunction(
            "Waits until the user inputs a string and accepts it with Return key",
            Returns = "A string containing the input entered by the user, or `nil` if no more lines are available..",
            ReturnType = DynamicValueType.String
        )]
        private static DynamicValue ReadLine(Fiber _, params DynamicValue[] __)
            => Console.ReadLine() ?? DynamicValue.Nil;
    }
}