namespace Ceres.TranslationEngine
{
    public record Symbol(string Name, int Id, Symbol.SymbolType Type)
    {
        public enum SymbolType
        {
            Local,
            Parameter
        }
    }
}