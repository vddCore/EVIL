﻿using System.Collections.Generic;
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
                    else
                    {
                        var ahead = _lexer.PeekToken(1);

                        if (ahead.Type == TokenType.Associate)
                        {
                            keyed = true;
                        }
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
                        key = Constant();
                        Match(Token.Associate);
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