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
            var isAnonymous = false;
            string procName = null;
            
            if (Scanner.State.CurrentToken.Type == TokenType.Identifier)
            {
                if (IsInsideFunctionDefinition)
                {
                    throw new ParserException("Nested named functions are not supported.");
                }
                
                IsInsideFunctionDefinition = true;

                procName = (string)Scanner.State.CurrentToken.Value;
                
                Match(TokenType.Identifier);
            }
            else
            {
                isAnonymous = true;
            }

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

            if (!isAnonymous)
            {
                IsInsideFunctionDefinition = false;
            }

            return new FunctionDefinitionNode(procName, statementList, parameterList) { Line = line };
        }
    }
}
