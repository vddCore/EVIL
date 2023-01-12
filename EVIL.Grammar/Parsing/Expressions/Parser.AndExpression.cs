using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode AndExpression()
        {
            var node = EqualityExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.BitwiseAnd)
            {
                var line = Match(Token.BitwiseAnd);
                node = new BinaryOperationNode(node, EqualityExpression(), BinaryOperationType.BitwiseAnd) {Line = line};
                
                token = CurrentToken;
            }

            return node;
        }
    }
}