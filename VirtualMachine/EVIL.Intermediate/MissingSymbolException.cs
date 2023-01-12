namespace EVIL.Intermediate
{
    public class MissingSymbolException : CompilerException
    {
        public MissingSymbolException(string identifier, int line, int column) 
            : base($"Symbol '{identifier}' was never defined.", line, column)
        {
        }
    }
}