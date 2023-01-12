using System.Linq;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;
using EVIL.RT;

namespace EVIL.Interpreter.Runtime.Library
{
    [ClrLibrary("tbl")]
    public class TableLibrary
    {
        [ClrFunction("ins")]
        public static DynamicValue Insert(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectTypeAtIndex(0, DynamicValueType.Table);

            var tbl = args[0].Table;
            var ret = 0;

            if (!tbl.IsSet(args[1]))
            {
                ret = 1;
                tbl.Set(args[1], args[2]);
            }

            return new DynamicValue(ret);
        }

        [ClrFunction("rm")]
        public static DynamicValue Remove(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.Table);

            var tbl = args[0].Table;
            var ret = 0;

            if (tbl.IsSet(args[1]))
            {
                ret = 1;
                tbl.Unset(args[1]);
            }

            return new DynamicValue(ret);
        }
        
        [ClrFunction("at")]
        public static DynamicValue At(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.Table)
                .ExpectIntegerAtIndex(1);

            var tbl = args[0].Table;
            var index = (int)args[1].Number;

            return tbl.Entries.Values.ElementAt(index);
        }
    }
}