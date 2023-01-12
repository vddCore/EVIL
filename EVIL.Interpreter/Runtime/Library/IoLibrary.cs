using System;
using System.Linq;
using EVIL.Interpreter.Execution;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    public class IoLibrary
    {

        [ClrFunction("io.print")]
        public static DynValue Print(Execution.Interpreter interpreter, FunctionArguments args)
        {
            var output = string.Join(' ', args.Select(x => x.AsString().String));
            Console.Write(output);

            return new DynValue(output.Length);
        }

        [ClrFunction("io.println")]
        public static DynValue PrintLine(Execution.Interpreter interpreter, FunctionArguments args)
        {
            var output = Print(interpreter, args);
            Console.WriteLine();

            return output;
        }

        [ClrFunction("io.readln")]
        public static DynValue ReadLine(Execution.Interpreter interpreter, FunctionArguments args)
            => new DynValue(Console.ReadLine() ?? string.Empty);

        [ClrFunction("io.read")]
        public static DynValue Read(Execution.Interpreter interpreter, FunctionArguments args)
            => new DynValue(Console.Read());

        [ClrFunction("io.readkey")]
        public static DynValue ReadKey(Execution.Interpreter interpreter, FunctionArguments args)
            => new DynValue((int)Console.ReadKey().Key);
    }
}
