namespace EVIL.VM.ExecutionEngine.Marshaling;

using EVIL.VM.ExecutionEngine.TypeSystem;

public interface IDynamicValueProvider
{
    DynamicValue ToDynamicValue();
}