using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
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
            TokenType.AssignBitwiseXor
        };

        private AstNode Assignment(AstNode left = null)
        {
            var node = left ?? LogicalExpression();
            var token = Scanner.State.CurrentToken;
            
            while (_assignmentOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.Assign)
                {
                    var line = Match(TokenType.Assign);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.Direct) { Line = line };
                }
                else if (token.Type == TokenType.AssignAdd)
                {
                    var line = Match(TokenType.AssignAdd);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.Add) { Line = line };
                }
                else if (token.Type == TokenType.AssignSubtract)
                {
                    var line = Match(TokenType.AssignSubtract);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.Subtract) { Line = line };
                }
                else if (token.Type == TokenType.AssignMultiply)
                {
                    var line = Match(TokenType.AssignMultiply);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.Multiply) { Line = line };
                }
                else if (token.Type == TokenType.AssignDivide)
                {
                    var line = Match(TokenType.AssignDivide);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.Divide) { Line = line };
                }
                else if (token.Type == TokenType.AssignModulo)
                {
                    var line = Match(TokenType.AssignModulo);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.Modulo) { Line = line };
                }
                else if (token.Type == TokenType.AssignBitwiseAnd)
                {
                    var line = Match(TokenType.AssignBitwiseAnd);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.BitwiseAnd) { Line = line };
                }
                else if (token.Type == TokenType.AssignBitwiseOr)
                {
                    var line = Match(TokenType.AssignBitwiseOr);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.BitwiseOr) { Line = line };
                }
                else if (token.Type == TokenType.AssignBitwiseXor)
                {
                    var line = Match(TokenType.AssignBitwiseXor);
                    node = new AssignmentNode(node, LogicalExpression(), AssignmentOperationType.BitwiseXor) { Line = line };
                }
            }

            return node;
        }
    }
}