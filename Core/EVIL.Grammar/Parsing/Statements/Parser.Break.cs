using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private BreakStatement Break()
        {
            if (_loopDescent == 0)
            {
                throw new ParserException(
                    "Unexpected 'break' outside of a loop.", 
                    (_lexer.State.Column, _lexer.State.Line)
                );
            }
            
            var (line, col) = Match(Token.Break);
            
            return new BreakStatement 
                { Line = line, Column = col };
        }
    }
}
