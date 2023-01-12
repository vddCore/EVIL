﻿using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode ForLoop()
        {
            var line = Match(TokenType.For);
            
            List<AstNode> assignments;
            AstNode condition;
            List<AstNode> iterationStatements;

            Match(TokenType.LParenthesis);
            {
                assignments = ForDeclarationList();
                
                Match(TokenType.Semicolon);
                
                condition = AssignmentExpression();
                
                Match(TokenType.Semicolon);
                
                iterationStatements = ForExpressionList();
            }
            Match(TokenType.RParenthesis);

            return new ForLoopNode(
                assignments,
                condition,
                iterationStatements,
                BlockStatement()
            ) {Line = line};
        }

        private List<AstNode> ForDeclarationList()
        {
            var nodes = new List<AstNode> {ForDeclaration()};
            
            while (CurrentToken.Type == TokenType.Comma)
            {
                Match(TokenType.Comma);
                nodes.Add(ForDeclaration());
            }

            return nodes;
        }

        private AstNode ForDeclaration()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.Var)
            {
                return VariableDefinition();
            }
            else if (token.Type == TokenType.Identifier)
            {
                return AssignmentExpression();
            }
            else throw new ParserException("Expected a variable definition or an expression.", Lexer.State);
        }

        private List<AstNode> ForExpressionList()
        {
            var list = new List<AstNode> {AssignmentExpression()};

            while (CurrentToken.Type == TokenType.Comma)
            {
                Match(TokenType.Comma);
                list.Add(AssignmentExpression());
            }

            return list;
        }
    }
}