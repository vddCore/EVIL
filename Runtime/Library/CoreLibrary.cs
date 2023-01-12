using EVIL.Abstraction;
using EVIL.Execution;

namespace EVIL.Runtime.Library
{
    public class CoreLibrary
    {
        [ClrFunction("type")]
        public static DynValue Type(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1);
            return new DynValue(args[0].Type.ToString().ToLower());
        }

        [ClrFunction("strace")]
        public static DynValue Strace(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectNone();

            var trace = interpreter.StackTrace();
            var tbl = new Table();

            for (var i = 0; i < trace.Count; i++)
                tbl[i] = new DynValue($"{trace.Count - i - 1}: {trace[i].FunctionName}");

            return new DynValue(tbl);
        }

        [ClrFunction("isglobal")]
        public static DynValue IsGlobal(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            if (interpreter.Environment.Globals.ContainsKey(name))
                return new DynValue(1);

            return new DynValue(0);
        }

        [ClrFunction("islocal")]
        public static DynValue IsLocal(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            if (interpreter.CallStack.Count - 1 <= 0)
                return new DynValue(0);

            var stackTrace = interpreter.StackTrace();

            if (stackTrace[stackTrace.Count - 1].LocalVariableScope.ContainsKey(name))
                return new DynValue(1);

            return new DynValue(0);
        }

        [ClrFunction("isparam")]
        public static DynValue IsParam(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            if (interpreter.CallStack.Count == 0)
                return new DynValue(0);

            var stackTrace = interpreter.StackTrace();

            if (stackTrace[stackTrace.Count - 1].ParameterScope.ContainsKey(name))
                return new DynValue(1);

            return new DynValue(0);
        }

        [ClrFunction("isfunc")]
        public static DynValue IsFunction(Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            if (interpreter.Environment.Functions.ContainsKey(name))
                return new DynValue(1);

            return new DynValue(0);
        }
    }
}
