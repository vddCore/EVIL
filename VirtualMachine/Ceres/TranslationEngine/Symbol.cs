namespace Ceres.TranslationEngine
{
    public record Symbol(
        string Name, 
        int Id, 
        Symbol.SymbolType Type,
        bool ReadWrite,
        int DefinedOnLine)
    {
        public enum SymbolType
        {
            Local,
            Parameter
        }
    }
}