using System;
using System.Linq;
using EVIL.Interpreter.Execution;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    public class CoreLibrary
    {
        [ClrFunction("type")]
        public static DynValue Type(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1);
            return new DynValue(args[0].Type.ToString().ToLower());
        }

        [ClrFunction("strace")]
        public static DynValue Strace(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectNone();

            var trace = interpreter.Environment.StackTrace();
            var tbl = new Table();

            for (var i = 0; i < trace.Count; i++)
                tbl[i] = new DynValue($"{trace.Count - i - 1}: {trace[i].FunctionName}");

            return new DynValue(tbl);
        }

        [ClrFunction("isglobal")]
        public static DynValue IsGlobal(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            if (interpreter.Environment.GlobalScope.HasMember(name))
                return new DynValue(1);

            return new DynValue(0);
        }

        [ClrFunction("islocal")]
        public static DynValue IsLocal(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            if (interpreter.Environment.CallStack.Count - 1 <= 0)
                return new DynValue(0);

            if (interpreter.Environment.LocalScope.HasMember(name))
                return new DynValue(1);

            return new DynValue(0);
        }
    }
}