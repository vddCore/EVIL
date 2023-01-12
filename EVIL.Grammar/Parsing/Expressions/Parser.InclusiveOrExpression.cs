using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode InclusiveOrExpression()
        {
            var node = ExclusiveOrExpression();
            var token = Scanner.State.CurrentToken;

            while (token.Type == TokenType.BitwiseOr)
            {
                var line = Match(TokenType.BitwiseOr);
                node = new BinaryOperationNode(node, ExclusiveOrExpression(), BinaryOperationType.BitwiseOr) {Line = line};
                
                token = Scanner.State.CurrentToken;
            }

            return node;
        }
    }
}