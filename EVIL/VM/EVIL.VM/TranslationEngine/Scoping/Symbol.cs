namespace EVIL.VM.TranslationEngine.Scoping;

using EVIL.VM.ExecutionEngine.Diagnostics;

internal record Symbol(
    string Name, 
    int Id, 
    Symbol.SymbolType Type,
    bool ReadWrite,
    int DefinedOnLine,
    int DefinedOnColumn,
    ClosureInfo? ClosureInfo)
{
    public string TypeName => Type
        .ToString()
        .ToLower();
        
    public enum SymbolType
    {
        Local,
        Parameter,
        Closure,
        Global
    }
}