namespace Ceres.TranslationEngine
{
    public record Symbol(string Name, int Id, Symbol.SymbolType Type, bool ReadWrite)
    {
        public enum SymbolType
        {
            Local,
            Parameter
        }
    }
}