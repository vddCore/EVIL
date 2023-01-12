using System;
using System.Linq;
using EVIL.Abstraction;
using EVIL.Execution;

namespace EVIL.Runtime.Library
{
    public class IoLibrary
    {

        [ClrFunction("io.print")]
        public static DynValue Print(Interpreter interpreter, ClrFunctionArguments args)
        {
            var output = string.Join(' ', args.Select(x => x.AsString().String));
            Console.Write(output);

            return new DynValue(output.Length);
        }

        [ClrFunction("io.println")]
        public static DynValue PrintLine(Interpreter interpreter, ClrFunctionArguments args)
        {
            var output = Print(interpreter, args);
            Console.WriteLine();

            return output;
        }

        [ClrFunction("io.readln")]
        public static DynValue ReadLine(Interpreter interpreter, ClrFunctionArguments args)
            => new DynValue(Console.ReadLine() ?? string.Empty);

        [ClrFunction("io.read")]
        public static DynValue Read(Interpreter interpreter, ClrFunctionArguments args)
            => new DynValue(Console.Read());

        [ClrFunction("io.readkey")]
        public static DynValue ReadKey(Interpreter interpreter, ClrFunctionArguments args)
            => new DynValue((int)Console.ReadKey().Key);
    }
}
