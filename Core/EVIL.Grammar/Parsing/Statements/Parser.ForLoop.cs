using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ForStatement ForLoop()
        {
            var line = Match(Token.For);
            
            List<Statement> assignments;
            Expression condition;
            List<Expression> iterationExpressions;

            Match(Token.LParenthesis);
            {
                assignments = ForDeclarationList();
                
                Match(Token.Semicolon);
                
                condition = AssignmentExpression();
                
                Match(Token.Semicolon);
                
                iterationExpressions = ForExpressionList();
            }
            Match(Token.RParenthesis);

            var statements = LoopDescent(() => Statement());
            
            return new ForStatement(
                assignments,
                condition,
                iterationExpressions,
                statements
            ) {Line = line};
        }

        private List<Statement> ForDeclarationList()
        {
            var nodes = new List<Statement> {ForDeclaration()};
            
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
            else throw new ParserException("Expected a variable definition or an expression.", Lexer.State);
        }

        private List<Expression> ForExpressionList()
        {
            var list = new List<Expression> {AssignmentExpression()};

            while (CurrentToken.Type == TokenType.Comma)
            {
                Match(Token.Comma);
                list.Add(AssignmentExpression());
            }

            return list;
        }
    }
}