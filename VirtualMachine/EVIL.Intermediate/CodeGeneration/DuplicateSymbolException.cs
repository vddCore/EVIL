namespace EVIL.Intermediate.CodeGeneration
{
    public class DuplicateSymbolException : CompilerException
    {
        public string SymbolName { get; }

        public DuplicateSymbolException(string symbolName, int line, int col)
            : base($"Symbol '{symbolName}' was already defined in the current scope.", line, col)
        {
            SymbolName = symbolName;
        }
    }
}