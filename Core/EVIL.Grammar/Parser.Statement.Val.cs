﻿namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private ValStatement ValStatement(bool readWrite)
    {
        var (line, col) = Match(Token.Val);

        var definitions = new Dictionary<IdentifierNode, Expression?>();

        while (true)
        {
            var identifier = Identifier();

            Expression? initializer = null;
            if (CurrentToken.Type == TokenType.Assign)
            {
                Match(Token.Assign);
                initializer = AssignmentExpression();
            }

            definitions.Add(identifier, initializer);

            if (CurrentToken.Type == TokenType.Comma)
            {
                Match(Token.Comma);
                continue;
            }

            break;
        }

        return new ValStatement(definitions, readWrite)
            { Line = line, Column = col };
    }
}