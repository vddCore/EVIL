﻿using EVIL.Grammar.AST;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode PrimaryExpression()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.LParenthesis)
            {
                var line = Match(Token.LParenthesis);

                var node = AssignmentExpression();
                node.Line = line;

                Match(Token.RParenthesis);

                return node;
            }
            else if (token.Type == TokenType.Fn)
            {
                return FunctionDefinitionAnonymous();
            }
            else if (token.Type == TokenType.LBrace)
            {
                return TableCreation();
            }
            else if (token.Type == TokenType.Identifier)
            {
                return Variable();
            }

            return Constant();
        }
    }
}