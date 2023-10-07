using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Statements.TopLevel
{
    public sealed class IncludeStatement : TopLevelStatement
    {
        public string Path { get; }

        public IncludeStatement(string path)
        {
            Path = path;
        }
    }
}