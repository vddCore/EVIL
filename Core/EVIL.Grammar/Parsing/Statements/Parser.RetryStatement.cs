namespace EVIL.Grammar.Parsing;

using AST.Statements;
using Lexical;

public partial class Parser
{
    private RetryStatement RetryStatement()
    {
        var (line, col) = Match(Token.Retry);
        Match(Token.Semicolon);
        
        return new RetryStatement { Line = line, Column = col };
    }
}