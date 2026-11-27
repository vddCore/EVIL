namespace EVIL.Assessment.Tests.VM.Language;

using EVIL.VM.ExecutionEngine.Marshaling;

public class NativeTypeProviderObject : INativeTypeProvider
{
    public string ProvideType()
        => "SomeNativeTypeLol";
}