namespace EVIL.Ceres.TranslationEngine.Scoping;

using EVIL.Ceres.ExecutionEngine.Diagnostics;

internal record Symbol(
    string Name, 
    int Id, 
    Symbol.SymbolType Type,
    bool ReadWrite,
    int DefinedOnLine,
    int DefinedOnColumn,
    ClosureInfo? ClosureInfo)
{
    public string TypeName => Type.ToString().ToLower();
        
    public enum SymbolType
    {
        Local,
        Parameter,
        Closure
    }
}