using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private SkipStatement Skip()
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
}