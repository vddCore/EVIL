using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("tbl")]
    public class TableLibrary
    {
        [ClrFunction("ins")]
        public static DynValue Insert(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(3)
                .ExpectTypeAtIndex(0, DynValueType.Table);

            var tbl = args[0].Table;

            if (tbl.ContainsKey(args[1]))
                throw new ClrFunctionException("Cannot insert a value into a table when the same key already exists.");

            tbl[args[1]] = args[2];

            return DynValue.Zero;
        }

        [ClrFunction("rm")]
        public static DynValue Remove(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Table);

            var tbl = args[0].Table;
            var key = tbl[args[1]];

            if (key == null)
                throw new ClrFunctionException("The requested key was not found in the table.");

            tbl.Remove(key);

            return DynValue.Zero;
        }
        
        [ClrFunction("at")]
        public static DynValue At(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.Table)
                .ExpectIntegerAtIndex(1);

            var tbl = args[0].Table;
            var index = (int)args[1].Number;

            return tbl.ElementAt(index);
        }
    }
}