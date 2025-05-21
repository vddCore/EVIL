namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private Expression UnaryExpression()
    {
        var token = CurrentToken;

        switch (token.Type)
        {
            case TokenType.AsString:
            {
                var (line, col) = Match(Token.AsString);

                return new UnaryExpression(UnaryExpression(), UnaryOperationType.ToString)
                    { Line = line, Column = col };
            }
            
            case TokenType.Plus:
            {
                var (line, col) = Match(Token.Plus);

                return new UnaryExpression(UnaryExpression(), UnaryOperationType.Plus)
                    { Line = line, Column = col };
            }
            
            case TokenType.Minus:
            {
                var (line, col) = Match(Token.Minus);

                return new UnaryExpression(UnaryExpression(), UnaryOperationType.Minus)
                    { Line = line, Column = col };
            }
            
            case TokenType.LogicalNot:
            {
                var (line, col) = Match(Token.LogicalNot);

                return new UnaryExpression(UnaryExpression(), UnaryOperationType.LogicalNot)
                    { Line = line, Column = col };
            }
            
            case TokenType.BitwiseNot:
            {
                var (line, col) = Match(Token.BitwiseNot);

                return new UnaryExpression(UnaryExpression(), UnaryOperationType.BitwiseNot)
                    { Line = line, Column = col };
            }
            
            case TokenType.AsNumber:
            {
                var (line, col) = Match(Token.AsNumber);

                return new UnaryExpression(UnaryExpression(), UnaryOperationType.ToNumber)
                    { Line = line, Column = col };
            }
            
            case TokenType.Length:
            {
                var (line, col) = Match(Token.Length);
                
                return new UnaryExpression(UnaryExpression(), UnaryOperationType.Length) 
                    { Line = line, Column = col };
            }
            
            default: return PostfixExpression();
        }
    }
}