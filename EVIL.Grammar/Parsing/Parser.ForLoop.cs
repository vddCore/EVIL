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
            var assignments = new List<AstNode>();
            var iterationStatements = new List<AstNode>();
            AstNode condition;
            
            Match(TokenType.LParenthesis);
            {
                assignments.Add(Statement());
                while (Scanner.State.CurrentToken.Type == TokenType.Comma)
                {
                    Match(TokenType.Comma);
                    assignments.Add(Statement());
                }
                Match(TokenType.Semicolon);

                condition = Assignment();
                Match(TokenType.Semicolon);

                iterationStatements.Add(Statement());
                while (Scanner.State.CurrentToken.Type == TokenType.Comma)
                {
                    Match(TokenType.Comma);
                    iterationStatements.Add(Statement());
                }
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
            ) { Line = line };
        }
    }
}
