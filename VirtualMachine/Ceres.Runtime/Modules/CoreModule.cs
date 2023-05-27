using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public sealed class CoreModule : RuntimeModule
    {
        public override string FullyQualifiedName => "core";
        
        [RuntimeModuleFunction("print")]
        private static DynamicValue Print(Fiber _, params DynamicValue[] args)
        {
            var str = string.Join(
                "\t",
                args.Select(x => x.ConvertToString().String)
            );

            Console.Write(str);
            return str.Length;
        }
    }
}