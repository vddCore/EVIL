using EVIL.Interpreter.Execution;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    public class TableLibrary
    {
        [ClrFunction("tbl.ins")]
        public static DynValue Insert(Execution.Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(3)
                .ExpectTypeAtIndex(0, DynValueType.Table);

            var tbl = args[0].Table;

            if (tbl.ContainsKey(args[1]))
                throw new ClrFunctionException("Cannot insert a value into a table when the same key already exists.");

            tbl[args[1]] = args[2];

            return DynValue.Zero;
        }

        [ClrFunction("tbl.rm")]
        public static DynValue Remove(Execution.Interpreter interpreter, ClrFunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Table);

            var tbl = args[0].Table;
            var key = tbl.GetKeyByDynValue(args[1]);

            if (key == null)
                throw new ClrFunctionException("The requested key was not found in the table.");

            tbl.Remove(key);

            return DynValue.Zero;
        }
    }
}
