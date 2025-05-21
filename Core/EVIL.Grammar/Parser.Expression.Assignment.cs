namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private static readonly List<TokenType> _assignmentOperators = new()
    {
        TokenType.Assign,
        TokenType.AssignAdd,
        TokenType.AssignSubtract,
        TokenType.AssignMultiply,
        TokenType.AssignDivide,
        TokenType.AssignModulo,
        TokenType.AssignBitwiseAnd,
        TokenType.AssignBitwiseOr,
        TokenType.AssignBitwiseXor,
        TokenType.AssignShiftLeft,
        TokenType.AssignShiftRight,
        TokenType.AssignCoalesce
    };

    private Expression AssignmentExpression()
    {
        var node = ConditionalExpression();
        var token = CurrentToken;
            
        while (_assignmentOperators.Contains(token.Type))
        {
            if (node.IsConstant)
            {
                throw new ParserException(
                    "Left-hand side of an assignment must be an assignable entity.",
                    (_lexer.State.Line, _lexer.State.Column)
                );
            }

            switch (token.Type)
            {
                case TokenType.Assign:
                {
                    var (line, col) = Match(Token.Assign);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.Direct)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignAdd:
                {
                    var (line, col) = Match(Token.AssignAdd);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.Add)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignSubtract:
                {
                    var (line, col) = Match(Token.AssignSubtract);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.Subtract)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignMultiply:
                {
                    var (line, col) = Match(Token.AssignMultiply);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.Multiply)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignDivide:
                {
                    var (line, col) = Match(Token.AssignDivide);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.Divide)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignModulo:
                {
                    var (line, col) = Match(Token.AssignModulo);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.Modulo)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignBitwiseAnd:
                {
                    var (line, col) = Match(Token.AssignBitwiseAnd);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.BitwiseAnd)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignBitwiseOr:
                {
                    var (line, col) = Match(Token.AssignBitwiseOr);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.BitwiseOr)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignBitwiseXor:
                {
                    var (line, col) = Match(Token.AssignBitwiseXor);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.BitwiseXor)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignShiftRight:
                {
                    var (line, col) = Match(Token.AssignShiftRight);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.ShiftRight)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignShiftLeft:
                {
                    var (line, col) = Match(Token.AssignShiftLeft);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.ShiftLeft)
                        { Line = line, Column = col };
                    break;
                }
                case TokenType.AssignCoalesce:
                {
                    var (line, col) = Match(Token.AssignCoalesce);
                        
                    node = new AssignmentExpression(node, AssignmentExpression(), AssignmentOperationType.Coalesce)
                        { Line = line, Column = col };
                    break;
                }
            }

            token = CurrentToken;
        }

        return node;
    }
}