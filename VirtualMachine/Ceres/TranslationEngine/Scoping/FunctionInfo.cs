namespace Ceres.TranslationEngine.Scoping
{
    internal class FunctionInfo
    {
        public string Identifier { get; }
        public bool NilReturning { get; }

        public FunctionInfo(string identifier, bool nilReturning)
        {
            Identifier = identifier;
            NilReturning = nilReturning;
        }
    }
}