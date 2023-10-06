using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression ExclusiveOrExpression()
        {
            var node = AndExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.BitwiseXor)
            {
                var (line, col) = Match(Token.BitwiseXor);

                node = new BinaryExpression(node, AndExpression(), BinaryOperationType.BitwiseXor)
                    { Line = line, Column = col };

                token = CurrentToken;
            }

            return node;
        }
    }
}