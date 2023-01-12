﻿using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FunctionCallExpression FunctionCall(Expression callee)
        {
            var line = Match(Token.LParenthesis);
            var arguments = new List<Expression>();

            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                if (CurrentToken.Type == TokenType.EOF)
                {
                    throw new ParserException(
                        $"Unexpected EOF in argument list on line {line}.",
                        Lexer.State
                    );
                }

                arguments.Add(AssignmentExpression());

                if (CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(Token.Comma);
            }
            Match(Token.RParenthesis);
            
            return new FunctionCallExpression(callee, arguments) { Line = line };
        }
    }
}
