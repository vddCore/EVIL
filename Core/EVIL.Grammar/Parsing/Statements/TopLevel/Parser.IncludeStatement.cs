using EVIL.Grammar.AST.Statements.TopLevel;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private IncludeStatement IncludeStatement()
        {
            var (line, col) = Match(Token.Include);
            var token = CurrentToken;

            Match(Token.String);
            
            return new IncludeStatement(token.Value) 
                { Line = line, Column = col };
        }
    }
}