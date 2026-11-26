namespace EVIL.Assessment.Tests.VM.Language;

using EVIL.VirtualMachine.ExecutionEngine.Marshaling;

public class NativeTypeProviderObject : INativeTypeProvider
{
    public string ProvideType()
        => "SomeNativeTypeLol";
}