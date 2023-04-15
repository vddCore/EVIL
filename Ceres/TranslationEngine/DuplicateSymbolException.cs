namespace Ceres.TranslationEngine
{
    public class DuplicateSymbolException : CompilerException
    {
        public string SymbolName { get; }

        public DuplicateSymbolException(string symbolName, string message) 
            : base(message)
        {
            SymbolName = symbolName;
        }
    }
}