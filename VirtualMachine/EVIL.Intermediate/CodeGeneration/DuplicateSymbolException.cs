namespace EVIL.Intermediate.CodeGeneration
{
    public class DuplicateSymbolException : CompilerException
    {
        public string SymbolName { get; }

        public DuplicateSymbolException(string symbolName, SymbolInfo prev, int line, int col)
            : base($"'{symbolName}' ({prev.Type.ToString().ToLowerInvariant()}) was already defined in the current scope.", line, col)
        {
            SymbolName = symbolName;
        }
    }
}