﻿using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ForStatement ForLoop()
        {
            var (line, col) = Match(Token.For);

            List<Statement> assignments;
            Expression condition;
            List<Statement> iterationStatements;

            Match(Token.LParenthesis);
            {
                assignments = ForDeclarationList();

                Match(Token.Semicolon);

                condition = AssignmentExpression();

                Match(Token.Semicolon);

                iterationStatements = ForStatementList();
            }
            Match(Token.RParenthesis);

            var statements = LoopDescent(() => Statement());

            return new ForStatement(
                assignments,
                condition,
                iterationStatements,
                statements
            ) { Line = line, Column = col };
        }

        private List<Statement> ForDeclarationList()
        {
            var nodes = new List<Statement> { ForDeclaration() };

            while (CurrentToken.Type == TokenType.Comma)
            {
                Match(Token.Comma);
                nodes.Add(ForDeclaration());
            }

            return nodes;
        }

        private Statement ForDeclaration()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.Var)
            {
                return VariableDefinition();
            }
            else if (token.Type == TokenType.Identifier)
            {
                return new ExpressionStatement(AssignmentExpression());
            }
            else throw new ParserException(
                "Expected a variable definition or an expression.", 
                (Lexer.State.Line, Lexer.State.Column)
            );
        }

        private List<Statement> ForStatementList()
        {
            var list = new List<Statement> { new ExpressionStatement(AssignmentExpression()) };

            while (CurrentToken.Type == TokenType.Comma)
            {
                Match(Token.Comma);
                list.Add(new ExpressionStatement(AssignmentExpression()));
            }

            return list;
        }
    }
}