namespace EVIL.Grammar;

using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private RetryStatement RetryStatement()
    {
        var (line, col) = Match(Token.Retry);
        Match(Token.Semicolon);
        
        return new RetryStatement { Line = line, Column = col };
    }
}