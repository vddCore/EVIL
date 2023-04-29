using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public sealed class CoreModule : EvilRuntimeModule
    {
        [EvilRuntimeMember("print", AllowRedefinition = true)]
        private static DynamicValue Print(Fiber context, params DynamicValue[] args)
        {
            foreach (var arg in args)
            {
                var value = arg;

                if (arg.Type != DynamicValue.DynamicValueType.String)
                    value = arg.ConvertToString();
                
                Console.Write(value.ConvertToString().String);
                Console.Write("\t");
            }
            
            Console.WriteLine("\n");

            return new(args.Length);
        }
    }
}