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
            var token = CurrentToken;

            while (token.Type == TokenType.BitwiseXor)
            {
                var line = Match(Token.BitwiseXor);
                node = new BinaryOperationNode(node, AndExpression(), BinaryOperationType.BitwiseXor) {Line = line};
                
                token = CurrentToken;
            }

            return node;
        }
    }
}