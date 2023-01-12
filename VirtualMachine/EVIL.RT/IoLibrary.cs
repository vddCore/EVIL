using System;
using System.Text;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;

namespace EVIL.RT
{
    [ClrLibrary("io")]
    public class IoLibrary
    {
        [ClrFunction("print")]
        public static DynamicValue Print(EVM evm, params DynamicValue[] args)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < args.Length; i++)
            {
                sb.Append(args[i].AsString());

                if (i < args.Length - 1)
                    sb.Append("    ");
            }
            Console.Write(sb.ToString());
            return new(sb.Length);
        }

        [ClrFunction("println")]
        public static DynamicValue PrintLine(EVM evm, params DynamicValue[] args)
        {
            var output = Print(evm, args);
            Console.WriteLine();
            return output;
        }

        [ClrFunction("readln")]
        public static DynamicValue ReadLine(EVM evm, params DynamicValue[] args)
            => new(Console.ReadLine() ?? string.Empty);

        [ClrFunction("read")]
        public static DynamicValue Read(EVM evm, params DynamicValue[] args)
            => new(Console.Read());

        [ClrFunction("readkey")]
        public static DynamicValue ReadKey(EVM evm, params DynamicValue[] args)
            => new((int)Console.ReadKey().Key);
    }
}