namespace EVIL.Intermediate
{
    public class MissingSymbolException : CompilerException
    {
        public MissingSymbolException(string identifier, int line, int col) 
            : base($"Symbol '{identifier}' was never defined.", line, col)
        {
        }
    }
}