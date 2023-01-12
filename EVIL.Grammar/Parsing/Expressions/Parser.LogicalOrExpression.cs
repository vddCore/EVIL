using System.Linq;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode LogicalOrExpression()
        {
            var node = LogicalAndExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.LogicalOr)
            {
                var line = Match(TokenType.LogicalOr);
                node = new BinaryOperationNode(node, LogicalAndExpression(), BinaryOperationType.LogicalOr) {Line = line};
                
                token = CurrentToken;
            }

            return node;
        }
    }
}