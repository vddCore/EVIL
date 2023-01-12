using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionDefinition()
        {         
            var line = Match(TokenType.Fn);
            string procName = null;
            
            if (CurrentToken.Type == TokenType.Identifier)
            {
                procName = CurrentToken.Value;                
                Match(TokenType.Identifier);
            }

            Match(TokenType.LParenthesis);
            var parameterList = new List<string>();

            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                parameterList.Add(CurrentToken.Value);
                Match(TokenType.Identifier);

                if (CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(TokenType.Comma);
            }
            Match(TokenType.RParenthesis);

            return new FunctionDefinitionNode(procName, BlockStatement(), parameterList) { Line = line };
        }
    }
}
