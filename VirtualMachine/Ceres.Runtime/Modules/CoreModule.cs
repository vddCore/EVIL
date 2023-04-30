using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public sealed class CoreModule : EvilRuntimeModule
    {
        [EvilRuntimeMember("print")]
        private static DynamicValue Print(Fiber _, params DynamicValue[] args)
        {
            foreach (var arg in args)
            {
                var value = arg;

                if (arg.Type != DynamicValue.DynamicValueType.String)
                    value = arg.ConvertToString();
                
                Console.Write(value.String);
                Console.Write("\t");
            }
            
            Console.WriteLine("\n");

            return new(args.Length);
        }
    }
}