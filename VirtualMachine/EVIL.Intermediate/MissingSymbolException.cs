namespace EVIL.Intermediate
{
    public class MissingSymbolException : CompilerException
    {
        public MissingSymbolException(string identifier, int line = -1) 
            : base($"Symbol '{identifier}' was never defined.", line)
        {
        }
    }
}