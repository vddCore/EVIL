namespace EVIL.Grammar;

using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private SkipStatement SkipStatement()
    {
        if (_loopDescent == 0)
        {
            throw new ParserException(
                "Unexpected 'skip' outside of a loop.",
                (_lexer.State.Line, _lexer.State.Column)
            );
        }

        var (line, col) = Match(Token.Skip);

        return new SkipStatement
            { Line = line, Column = col };
    }
}