using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private MemoryGetNode MemoryGet()
        {
            var line = Match(TokenType.LBracket);

            var size = 0;

            if (Scanner.State.CurrentToken.Type == TokenType.Colon)
            {
                Match(TokenType.Colon);
                size++;
            }

            var expression = LogicalExpression();

            if (Scanner.State.CurrentToken.Type == TokenType.Colon)
            {
                Match(TokenType.Colon);
                size++;
            }

            Match(TokenType.RBracket);
            return new MemoryGetNode(expression, (MemoryGetNode.OperandSize)size) { Line = line };
        }
    }
}
