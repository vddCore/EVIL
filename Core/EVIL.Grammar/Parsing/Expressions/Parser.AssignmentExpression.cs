using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
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
            TokenType.AssignShiftRight
        };

        private Expression AssignmentExpression()
        {
            var node = ConditionalExpression();
            var token = CurrentToken;

            while (_assignmentOperators.Contains(token.Type))
            {
                if (node is AssignmentExpression leftAssignment)
                {
                    if (leftAssignment.Right.IsConstant)
                    {
                        throw new ParserException(
                            "Left-hand side of an assignment must be an assignable entity.",
                            Lexer.State
                        );
                    }
                }
                
                switch (token.Type)
                {
                    case TokenType.Assign:
                    {
                        var line = Match(Token.Assign);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.Direct)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignAdd:
                    {
                        var line = Match(Token.AssignAdd);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.Add)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignSubtract:
                    {
                        var line = Match(Token.AssignSubtract);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.Subtract)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignMultiply:
                    {
                        var line = Match(Token.AssignMultiply);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.Multiply)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignDivide:
                    {
                        var line = Match(Token.AssignDivide);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.Divide)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignModulo:
                    {
                        var line = Match(Token.AssignModulo);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.Modulo)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignBitwiseAnd:
                    {
                        var line = Match(Token.AssignBitwiseAnd);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.BitwiseAnd)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignBitwiseOr:
                    {
                        var line = Match(Token.AssignBitwiseOr);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.BitwiseOr)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignBitwiseXor:
                    {
                        var line = Match(Token.AssignBitwiseXor);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.BitwiseXor)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignShiftRight:
                    {
                        var line = Match(Token.AssignShiftRight);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.ShiftRight)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignShiftLeft:
                    {
                        var line = Match(Token.AssignShiftLeft);
                        node = new AssignmentExpression(node, ConditionalExpression(), AssignmentOperationType.ShiftLeft)
                            {Line = line};
                        break;
                    }
                }

                token = CurrentToken;
            }

            return node;
        }
    }
}