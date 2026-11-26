namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private Expression InclusiveOrExpression()
    {
        var node = ExclusiveOrExpression();
        var token = CurrentToken;

        while (token.Type == TokenType.BitwiseOr)
        {
            var (line, col) = Match(Token.BitwiseOr);

            node = new BinaryExpression(node, ExclusiveOrExpression(), BinaryOperationType.BitwiseOr)
                { Line = line, Column = col };

            token = CurrentToken;
        }

        return node;
    }
}