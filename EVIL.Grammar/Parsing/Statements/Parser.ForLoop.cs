using System.Collections.Generic;
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
            List<AstNode> iterationStatements;
            
            AstNode condition;

            Match(TokenType.LParenthesis);
            {
                assignments = ForDeclarationList();
                
                Match(TokenType.Semicolon);
                
                condition = AssignmentExpression();
                
                Match(TokenType.Semicolon);
                
                iterationStatements = ForExpressionList();
            }
            Match(TokenType.RParenthesis);

            Match(TokenType.LBrace);
            var statementList = LoopStatementList();
            Match(TokenType.RBrace);

            return new ForLoopNode(
                assignments,
                condition,
                iterationStatements,
                statementList
            ) {Line = line};
        }

        private List<AstNode> ForDeclarationList()
        {
            var nodes = new List<AstNode> {ForDeclaration()};
            
            while (Scanner.State.CurrentToken.Type == TokenType.Comma)
            {
                Match(TokenType.Comma);
                nodes.Add(ForDeclaration());
            }

            return nodes;
        }

        private AstNode ForDeclaration()
        {
            var token = Scanner.State.CurrentToken;

            if (token.Type == TokenType.Var)
            {
                return VariableDefinition();
            }
            else if (token.Type == TokenType.Identifier)
            {
                return AssignmentExpression();
            }
            else throw new ParserException("Expected a variable definition or an expression.", Scanner.State);
        }

        private List<AstNode> ForExpressionList()
        {
            var list = new List<AstNode> {AssignmentExpression()};

            while (Scanner.State.CurrentToken.Type == TokenType.Comma)
            {
                Match(TokenType.Comma);
                list.Add(AssignmentExpression());
            }

            return list;
        }
    }
}