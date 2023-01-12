using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Break()
        {
            if (_loopDescent == 0)
            {
                throw new ParserException("Unexpected 'break' outside of a loop.", Lexer.State);
            }
            
            var line = Match(TokenType.Break);
            return new BreakNode { Line = line };
        }
    }
}
