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

            while (Scanner.State.CurrentToken.Type == TokenType.Colon)
            {
                if (size + 1 > (int)MemoryGetNode.OperandSize.Dword)
                {
                    throw new ParserException("Maximum supported operand size is DWORD.");
                }

                Match(TokenType.Colon);
                size++;
            }

            var expression = LogicalExpression();

            Match(TokenType.RBracket);
            return new MemoryGetNode(expression, (MemoryGetNode.OperandSize)size) {Line = line};
        }
    }
}