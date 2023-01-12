using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;

namespace EVIL.ExecutionEngine
{
    public class GlobalNotFoundException : VirtualMachineException
    {
        public GlobalNotFoundException(ExecutionContext ctx, DynamicValue constKey) 
            : base(ctx, $"Attempt to access an undefined variable '{constKey.AsString()}'.")
        {
        }
    }
}