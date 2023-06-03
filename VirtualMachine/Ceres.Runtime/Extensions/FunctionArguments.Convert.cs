using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.Runtime.Extensions
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
    }
}