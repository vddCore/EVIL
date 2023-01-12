using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode LogicalAndExpression()
        {
            var node = InclusiveOrExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.LogicalAnd)
            {
                var line = Match(TokenType.LogicalAnd);
                node = new BinaryOperationNode(node, InclusiveOrExpression(), BinaryOperationType.And) {Line = line};
                
                token = CurrentToken;
            }

            return node;
        }
    }
}