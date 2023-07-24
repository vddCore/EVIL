using System.Collections.Generic;
using System.Data.Common;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private TableExpression TableExpression()
        {
            var (line, col) = Match(Token.LBrace);
            var initializers = new List<Expression>();

            var keyed = false;

            while (CurrentToken.Type != TokenType.RBrace)
            {
                if (initializers.Count == 0)
                {
                    if (CurrentToken.Type == TokenType.LBracket)
                    {
                        keyed = true;
                    }
                    else if (_lexer.PeekToken(1) == Token.Associate)
                    {
                        keyed = true;
                    }
                    else if (CurrentToken.Type == TokenType.Identifier && _lexer.PeekToken(1) == Token.Colon)
                    {
                        keyed = true;
                    }
                }

                if (keyed)
                {
                    Expression key, value;

                    if (CurrentToken.Type == TokenType.LBracket)
                    {
                        key = ComputedKeyExpression();

                        if (key is NilConstant)
                        {
                            throw new ParserException(
                                "'nil' is not a valid key expression.",
                                (key.Line, key.Column)
                            );
                        }

                        Match(Token.Associate);
                        value = AssignmentExpression();

                        initializers.Add(new KeyValuePairExpression(key, value));
                    }
                    else
                    {
                        if (CurrentToken.Type == TokenType.Identifier)
                        {
                            var keyValue = CurrentToken.Value;
                            var (kline, kcol) = Match(Token.Identifier);

                            if (CurrentToken == Token.Colon)
                            {
                                Match(Token.Colon);
                                key = new StringConstant(keyValue)
                                    { Line = kline, Column = kcol };
                            }
                            else
                            {
                                throw new ParserException(
                                    "Identifier-style keys must be followed by a colon.",
                                    (CurrentToken.Line, CurrentToken.Column)
                                );
                            }
                        }
                        else
                        {
                            key = Constant();
                            Match(Token.Associate);
                        }

                        value = AssignmentExpression();

                        initializers.Add(new KeyValuePairExpression(key, value));
                    }
                }
                else
                {
                    initializers.Add(AssignmentExpression());
                }

                if (CurrentToken.Type == TokenType.RBrace)
                    break;

                Match(Token.Comma);
            }

            Match(Token.RBrace);
            return new TableExpression(initializers, keyed)
                { Line = line, Column = col };
        }

        private Expression ComputedKeyExpression()
        {
            Match(Token.LBracket);
            var computedKey = AssignmentExpression();
            Match(Token.RBracket);

            return computedKey;
        }
    }
}