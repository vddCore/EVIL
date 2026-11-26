namespace EVIL.Core.Grammar;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

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