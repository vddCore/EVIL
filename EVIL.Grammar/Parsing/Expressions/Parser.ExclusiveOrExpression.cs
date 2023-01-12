using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode ExclusiveOrExpression()
        {
            var node = AndExpression();
            var token = Scanner.State.CurrentToken;

            while (token.Type == TokenType.BitwiseXor)
            {
                var line = Match(TokenType.BitwiseXor);
                node = new BinaryOperationNode(node, AndExpression(), BinaryOperationType.BitwiseXor) {Line = line};
                
                token = Scanner.State.CurrentToken;
            }

            return node;
        }
    }
}