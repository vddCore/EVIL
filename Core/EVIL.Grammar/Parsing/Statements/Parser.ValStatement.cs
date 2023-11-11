using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ValStatement ValStatement(bool readWrite)
        {
            var nilAccepting = false;
            
            var (line, col) = Match(Token.Val);
            if (CurrentToken == Token.QuestionMark)
            {
                Match(Token.QuestionMark);
                nilAccepting = true;
            }
            
            var definitions = new Dictionary<IdentifierNode, Expression?>();

            while (true)
            {
                var identifier = Identifier();

                Expression? initializer = null;
                if (CurrentToken.Type == TokenType.Assign)
                {
                    Match(Token.Assign);
                    initializer = AssignmentExpression();

                    if (!nilAccepting && initializer.Reduce() is NilConstant)
                    {
                        throw new ParserException(
                            $"Nil-rejecting `val' declarator requires a non-nil initializer.",
                            (CurrentState.Line, CurrentState.Column)
                        );
                    }
                }
                else
                {
                    if (!nilAccepting)
                    {
                        throw new ParserException(
                            $"Nil-rejecting `val' declarator requires an initializer for all its identifiers.",
                            (CurrentState.Line, CurrentState.Column)
                        );
                    }
                }

                definitions.Add(identifier, initializer);

                if (CurrentToken.Type == TokenType.Comma)
                {
                    Match(Token.Comma);
                    continue;
                }

                break;
            }

            return new ValStatement(definitions, readWrite, nilAccepting)
                { Line = line, Column = col };
        }
    }
}