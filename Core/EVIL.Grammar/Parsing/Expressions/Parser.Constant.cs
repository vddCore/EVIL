﻿using System.Globalization;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Constants;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression Constant()
        {
            var token = CurrentToken;

            switch (token.Type)
            {
                case TokenType.Number:
                {
                    var (line, col) = Match(Token.Number);

                    return new NumberConstant(double.Parse(token.Value!))
                        { Line = line, Column = col };
                }
                case TokenType.HexInteger:
                {
                    var (line, col) = Match(Token.HexInteger);

                    return new NumberConstant(long.Parse(token.Value!, NumberStyles.HexNumber))
                        { Line = line, Column = col };
                }
                case TokenType.True:
                {
                    var (line, col) = Match(Token.True);

                    return new BooleanConstant(true)
                        { Line = line, Column = col };
                }
                case TokenType.False:
                {
                    var (line, col) = Match(Token.False);

                    return new BooleanConstant(false)
                        { Line = line, Column = col };
                }
                case TokenType.String:
                {
                    var (line, col) = Match(Token.String);

                    return new StringConstant(token.Value!)
                        { Line = line, Column = col };
                }
                case TokenType.Nil:
                {
                    var (line, col) = Match(Token.Nil);

                    return new NilConstant
                        { Line = line, Column = col };
                }
                default:
                {
                    throw new ParserException(
                        $"Unexpected token {token}",
                        (Lexer.State.Line, Lexer.State.Column)
                    );
                }
            }
        }
    }
}