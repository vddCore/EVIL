using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.TranslationEngine.Scoping
{
    internal record Symbol(
        string Name, 
        int Id, 
        Symbol.SymbolType Type,
        bool ReadWrite,
        bool NilAccepting,
        int DefinedOnLine,
        int DefinedOnColumn,
        ClosureInfo? ClosureInfo,
        FunctionInfo? FunctionInfo)
    {
        public string TypeName => Type.ToString().ToLower();
        
        public enum SymbolType
        {
            GlobalFunction,
            Local,
            Parameter,
            Closure
        }
    }
}