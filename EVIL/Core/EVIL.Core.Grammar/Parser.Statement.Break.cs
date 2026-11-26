namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private BreakStatement BreakStatement()
    {
        if (_loopDescent == 0)
        {
            throw new ParserException(
                "Unexpected 'break' outside of a loop.", 
                (_lexer.State.Line, _lexer.State.Column)
            );
        }
            
        var (line, col) = Match(Token.Break);

        return new BreakStatement 
            { Line = line, Column = col };
    }
}