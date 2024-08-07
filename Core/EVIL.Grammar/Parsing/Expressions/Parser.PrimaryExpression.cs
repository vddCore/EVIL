﻿namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private Expression PrimaryExpression()
    {
        var token = CurrentToken;

        if (token.Type == TokenType.LParenthesis)
        {
            var (line, col) = Match(Token.LParenthesis);

            var node = AssignmentExpression();
            node.Line = line;
            node.Column = col;

            Match(Token.RParenthesis);

            return node;
        }
        else if (token.Type == TokenType.LBrace)
        {
            return TableExpression();
        }
        else if (token.Type == TokenType.Array)
        {
            return ArrayExpression();
        }
        else if (token.Type == TokenType.Error)
        {
            return ErrorExpression();
        }
        else if (token.Type == TokenType.Ellipsis)
        {
            var (line, col) = Match(Token.Ellipsis);
                
            return new ExtraArgumentsExpression(false) 
                { Line = line, Column = col };
        }
        else if (token.Type == TokenType.Fn)
        {
            return FnExpression();
        }
        else if (token.Type == TokenType.Identifier)
        {
            return SymbolReferenceExpression();
        }
        else if (token.Type == TokenType.Self)
        {
            return SelfExpression();
        }

        return ConstantExpression();
    }
}