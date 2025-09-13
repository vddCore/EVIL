namespace EVIL.Ceres.LanguageTests;

using EVIL.Ceres.ExecutionEngine.Marshaling;

public class NativeTypeProviderObject : INativeTypeProvider
{
    public string ProvideType()
        => "SomeNativeTypeLol";
}