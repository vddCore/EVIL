using System;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("io")]
    public class IoLibrary
    {
        [ClrFunction("print")]
        public static DynValue Print(Execution.Interpreter interpreter, FunctionArguments args)
        {
            var output = string.Empty;
            if (args.Count > 0)
            {
                output = args[0].AsString().String;
            }
            
            Console.Write(output);

            return new(output.Length);
        }

        [ClrFunction("println")]
        public static DynValue PrintLine(Execution.Interpreter interpreter, FunctionArguments args)
        {
            var output = Print(interpreter, args);
            Console.WriteLine();

            return output;
        }

        [ClrFunction("readln")]
        public static DynValue ReadLine(Execution.Interpreter interpreter, FunctionArguments args)
            => new(Console.ReadLine() ?? string.Empty);

        [ClrFunction("read")]
        public static DynValue Read(Execution.Interpreter interpreter, FunctionArguments args)
            => new(Console.Read());

        [ClrFunction("readkey")]
        public static DynValue ReadKey(Execution.Interpreter interpreter, FunctionArguments args)
            => new((int)Console.ReadKey().Key);
    }
}
