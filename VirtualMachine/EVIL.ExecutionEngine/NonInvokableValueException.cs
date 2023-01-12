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
                $"Unable to invoke a non-invokable value of type {value.Type}."
                + (value.Type == DynamicValueType.Number && value.Number == 0
                    ? " Perhaps it is undefined?"
                    : string.Empty)
            )
        {
            Value = value;
        }
    }
}