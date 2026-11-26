namespace EVIL.VirtualMachine.ExecutionEngine.Marshaling;

using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public interface IDynamicValueProvider
{
    DynamicValue ToDynamicValue();
}