using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode New()
        {
            var line = Match(TokenType.New);
            
            Match(TokenType.LParenthesis);
            var parameterList = new List<string>();

            while (Scanner.State.CurrentToken.Type != TokenType.RParenthesis)
            {
                parameterList.Add((string)Scanner.State.CurrentToken.Value);
                Match(TokenType.Identifier);

                if (Scanner.State.CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(TokenType.Comma);
            }
            Match(TokenType.RParenthesis);

            Match(TokenType.LBrace);
            var statementList = FunctionStatementList();
            Match(TokenType.RBrace);

            var functionDefinitionNode = new FunctionDefinitionNode(
                null,
                statementList,
                parameterList,
                true
            ) { Line = line };

            return new AssignmentNode(new StringNode("new"), functionDefinitionNode, AssignmentOperationType.Direct);
        }
    }
}