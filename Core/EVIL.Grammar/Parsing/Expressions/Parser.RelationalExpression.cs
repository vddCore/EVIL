namespace EVIL.Grammar.Parsing;

using System.Linq;
using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private static readonly TokenType[] _comparisonOperators =
    [
        TokenType.LessThan,
        TokenType.GreaterThan,
        TokenType.LessThanOrEqual,
        TokenType.GreaterThanOrEqual,
        TokenType.In,
        TokenType.NotIn,
        TokenType.Is,
        TokenType.IsNot
    ];

    private Expression RelationalExpression()
    {
        var node = ShiftExpression();
        var token = CurrentToken;

        while (_comparisonOperators.Contains(token.Type))
        {
            switch (token.Type)
            {
                case TokenType.LessThan:
                {
                    var (line, col) = Match(Token.LessThan);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.Less)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.GreaterThan:
                {
                    var (line, col) = Match(Token.GreaterThan);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.Greater)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.LessThanOrEqual:
                {
                    var (line, col) = Match(Token.LessThanOrEqual);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.LessOrEqual)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.GreaterThanOrEqual:
                {
                    var (line, col) = Match(Token.GreaterThanOrEqual);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.GreaterOrEqual)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.In:
                {
                    var (line, col) = Match(Token.In);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.ExistsIn)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.NotIn:
                {
                    var (line, col) = Match(Token.NotIn);
                    
                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.DoesNotExistIn)
                        { Line = line, Column = col } ;
                    break;
                }
                
                case TokenType.Is or TokenType.IsNot:
                {
                    var (line, col) = (-1, -1);

                    var invert = false;

                    if (token.Type == TokenType.Is)
                    {
                        (line, col) = Match(Token.Is);
                    }
                    else
                    {
                        (line, col) = Match(Token.IsNot);
                        invert = true;
                    }

                    var right = ConstantExpression();

                    if (right is not TypeCodeConstant typeCodeConstant)
                    {
                        throw new ParserException(
                            "Expected a type code constant.",
                            (right.Line, right.Column)
                        );
                    }

                    StringConstant? nativeTypeConstant = null;
                    if (typeCodeConstant.Value == DynamicValueType.NativeObject)
                    {
                        if (CurrentToken.Type == TokenType.PlainString || CurrentToken is { Type: TokenType.InterpolatedString })
                        {
                            var constant = ConstantExpression();
                        
                            if (constant is not StringConstant stringConstant)
                            {
                                throw new ParserException(
                                    "Expected a string or an interpolated string.",
                                    (constant.Line, constant.Column)
                                );
                            }
                        
                            nativeTypeConstant = stringConstant;
                        }
                    }

                    node = new IsExpression(node, typeCodeConstant, nativeTypeConstant, invert)
                        { Line = line, Column = col };
                    break;
                }
            }

            token = CurrentToken;
        }

        return node;
    }
}