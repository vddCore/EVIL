using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

namespace EVIL.Ceres.Runtime.Extensions
{
    public static partial class FunctionArguments
    {
        public static Table ToTable(this DynamicValue[] args)
        {
            var ret = new Table();

            for (var i = 0; i < args.Length; i++)
                ret[i] = args[i];
            
            return ret;
        }

        public static Array ToArray(this DynamicValue[] args)
        {
            var ret = new Array(args.Length);

            for (var i = 0; i < args.Length; i++)
                ret[i] = args[i];

            return ret;
        }
    }
}