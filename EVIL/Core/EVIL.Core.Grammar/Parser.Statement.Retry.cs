namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private RetryStatement RetryStatement()
    {
        var (line, col) = Match(Token.Retry);
        Match(Token.Semicolon);
        
        return new RetryStatement { Line = line, Column = col };
    }
}