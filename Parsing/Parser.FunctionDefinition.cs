using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionDefinition()
        {
            var line = Match(TokenType.Fn);
            var procName = (string)Scanner.State.CurrentToken.Value;
            Match(TokenType.Identifier);

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

            return new FunctionDefinitionNode(procName, statementList, parameterList) { Line = line };
        }
    }
}
