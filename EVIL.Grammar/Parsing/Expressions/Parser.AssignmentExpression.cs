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

        private AstNode AssignmentExpression(AstNode left = null)
        {
            var node = left ?? ConditionalExpression();
            var token = CurrentToken;
            
            while (_assignmentOperators.Contains(token.Type))
            {
                switch (token.Type)
                {
                    case TokenType.Assign:
                    {
                        var line = Match(TokenType.Assign);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.Direct)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignAdd:
                    {
                        var line = Match(TokenType.AssignAdd);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.Add) {Line = line};
                        break;
                    }
                    case TokenType.AssignSubtract:
                    {
                        var line = Match(TokenType.AssignSubtract);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.Subtract)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignMultiply:
                    {
                        var line = Match(TokenType.AssignMultiply);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.Multiply)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignDivide:
                    {
                        var line = Match(TokenType.AssignDivide);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.Divide)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignModulo:
                    {
                        var line = Match(TokenType.AssignModulo);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.Modulo)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignBitwiseAnd:
                    {
                        var line = Match(TokenType.AssignBitwiseAnd);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.BitwiseAnd)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignBitwiseOr:
                    {
                        var line = Match(TokenType.AssignBitwiseOr);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.BitwiseOr)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignBitwiseXor:
                    {
                        var line = Match(TokenType.AssignBitwiseXor);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.BitwiseXor)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignShiftRight:
                    {
                        var line = Match(TokenType.AssignShiftRight);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.ShiftRight)
                            {Line = line};
                        break;
                    }
                    case TokenType.AssignShiftLeft:
                    {
                        var line = Match(TokenType.AssignShiftLeft);
                        node = new AssignmentNode(node, ConditionalExpression(), AssignmentOperationType.ShiftLeft)
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