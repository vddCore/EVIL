using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;

namespace EVIL.ExecutionEngine
{
    public class NonInvokableValueException : VirtualMachineException
    {
        public DynamicValue Value { get; }

        public NonInvokableValueException(ExecutionContext ctx, DynamicValue value)
            : base(
                ctx,
                $"Unable to invoke a {value.Type.Alias()} value."
                + (value.Type == DynamicValueType.Null
                    ? " Perhaps it is undefined?"
                    : string.Empty)
            )
        {
            Value = value;
        }
    }
}