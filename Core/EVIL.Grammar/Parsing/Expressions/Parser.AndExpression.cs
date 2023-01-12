using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression AndExpression()
        {
            var node = EqualityExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.BitwiseAnd)
            {
                var line = Match(Token.BitwiseAnd);
                node = new BinaryExpression(node, EqualityExpression(), BinaryOperationType.BitwiseAnd) {Line = line};
                
                token = CurrentToken;
            }

            return node;
        }
    }
}