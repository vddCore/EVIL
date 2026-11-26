namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private Expression AndExpression()
    {
        var node = EqualityExpression();
        var token = CurrentToken;

        while (token.Type == TokenType.BitwiseAnd)
        {
            var (line, col) = Match(Token.BitwiseAnd);
            node = new BinaryExpression(node, EqualityExpression(), BinaryOperationType.BitwiseAnd)
                { Line = line, Column = col };

            token = CurrentToken;
        }

        return node;
    }
}