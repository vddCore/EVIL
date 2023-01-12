using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private LocalDefinition LocalDefinition()
        {
            var (line, col) = Match(Token.Loc);

            var definitions = new Dictionary<string, Expression>();

            while (true)
            {
                var identifier = CurrentToken.Value;
                Match(Token.Identifier);

                Expression initializer = null;
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

            return new LocalDefinition(definitions)
                { Line = line, Column = col };
        }
    }
}