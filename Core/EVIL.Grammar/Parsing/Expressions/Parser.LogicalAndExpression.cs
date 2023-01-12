using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression LogicalAndExpression()
        {
            var node = InclusiveOrExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.LogicalAnd)
            {
                var line = Match(Token.LogicalAnd);
                node = new BinaryExpression(node, InclusiveOrExpression(), BinaryOperationType.LogicalAnd) {Line = line};
                
                token = CurrentToken;
            }

            return node;
        }
    }
}