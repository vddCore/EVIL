namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

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
                if (CurrentToken.Type == TokenType.LBracket 
                    || _lexer.PeekToken(1) == Token.Associate 
                    || CurrentToken.Type == TokenType.Identifier 
                    && _lexer.PeekToken(1) == Token.Colon 
                    || IsSpreadToken())
                {
                    keyed = true;
                }
            }

            if (keyed)
            {
                if (IsSpreadToken())
                {
                    initializers.Add(TableSpreadExpression());
                }
                else
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
                    }
                    else
                    {
                        if (CurrentToken.Type == TokenType.Identifier)
                        {
                            var keyIdentifier = Identifier();

                            if (CurrentToken == Token.Colon)
                            {
                                Match(Token.Colon);
                                key = new StringConstant(keyIdentifier.Name, false)
                                    { Line = keyIdentifier.Line, Column = keyIdentifier.Column };
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
                            key = ConstantExpression();
                            Match(Token.Associate);
                        }

                        if (CurrentToken == Token.Self && _lexer.PeekToken(1) == Token.DoubleColon)
                        {
                            value = SelfFnExpression();
                        }
                        else
                        {
                            value = AssignmentExpression();
                        }
                    }

                    initializers.Add(new KeyValuePairExpression(key, value));
                }
            }
            else
            {
                if (IsSpreadToken())
                {
                    initializers.Add(TableSpreadExpression());
                }
                else if (CurrentToken == Token.Self)
                {
                    initializers.Add(SelfFnExpression());
                }
                else
                {
                    initializers.Add(AssignmentExpression());
                }
            }

            if (CurrentToken.Type == TokenType.RBrace)
                break;

            Match(Token.Comma);
        }

        Match(Token.RBrace);
        return new TableExpression(initializers, keyed)
            { Line = line, Column = col };
    }

    private bool IsSpreadToken()
    {
        if (CurrentToken.Type == TokenType.SafeSpread)
        {
            return true;
        }

        if (CurrentToken.Type == TokenType.Ellipsis)
        {
            var nextType = _lexer.PeekToken(1).Type;
            
            return nextType != TokenType.RBrace
                && nextType != TokenType.Comma
                && nextType != TokenType.LBracket;
        }

        return false;
    }

    private TableSpreadExpression TableSpreadExpression()
    {
        var isNilTolerant = CurrentToken.Type == TokenType.SafeSpread;
        
        var (line, col) = Match(CurrentToken);
        var target = AssignmentExpression();

        return new TableSpreadExpression(target, isNilTolerant)
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