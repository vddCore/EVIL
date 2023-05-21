using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableDefinition VariableDefinition()
        {
            var (line, col) = Match(Token.Var);

            var definitions = new Dictionary<string, Expression?>();

            while (true)
            {
                var identifier = CurrentToken.Value;
                Match(Token.Identifier);

                Expression? initializer = null;
                if (CurrentToken.Type == TokenType.Assign)
                {
                    Match(Token.Assign);
                    initializer = AssignmentExpression();
                }

                definitions.Add(identifier!, initializer);

                if (CurrentToken.Type == TokenType.Comma)
                {
                    Match(Token.Comma);
                    continue;
                }

                break;
            }

            return new VariableDefinition(definitions)
                { Line = line, Column = col };
        }
    }
}