using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    public class CoreLibrary
    {
        [ClrFunction("type")]
        public static DynValue Type(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1);
            return new(args[0].Type.ToString().ToLower());
        }

        [ClrFunction("strace")]
        public static DynValue Strace(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectNone();

            var trace = interpreter.Environment.StackTrace();
            var tbl = new Table();

            for (var i = 0; i < trace.Count; i++)
                tbl[i] = new DynValue($"{trace.Count - i - 1}: {trace[i].FunctionName}");

            return new(tbl);
        }

        [ClrFunction("isdef")]
        public static DynValue IsDefined(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectAtLeast(1)
                .ExpectAtMost(2)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            var scope = "any";

            if (args.Count > 1)
            {
                if (args[1].Type == DynValueType.String)
                {
                    scope = args[1].String;
                }
            }

            return scope switch
            {
                "local" => new(interpreter.Environment.LocalScope.HasMember(name)),
                "global" => new(interpreter.Environment.GlobalScope.HasMember(name)),
                "any" => new(interpreter.Environment.LocalScope.FindInScopeChain(name) != null),
                _ => throw new ClrFunctionException("Unsupported scope type.")
            };
        }
    }
}