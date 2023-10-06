namespace Ceres.TranslationEngine.Scoping
{
    internal record Symbol(
        string Name, 
        int Id, 
        Symbol.SymbolType Type,
        bool ReadWrite,
        int DefinedOnLine,
        int DefinedOnColumn)
    {
        public string TypeName => Type.ToString().ToLower();
        
        public enum SymbolType
        {
            Local,
            Parameter
        }
    }
}