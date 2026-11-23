namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private readonly List<TokenType> _postfixOperators =
    [
        TokenType.LParenthesis,
        TokenType.ElvisInvocation,
        TokenType.LBracket,
        TokenType.Dot,
        TokenType.Elvis,
        TokenType.ElvisArray,
        TokenType.Colon,
        TokenType.SafeColon,
        TokenType.Increment,
        TokenType.Decrement
    ];

    private Expression PostfixExpression()
    {
        var node = PrimaryExpression();
        var token = CurrentToken;
            
        __incdec:
        switch (token.Type)
        {
            case TokenType.Increment:
            {
                var (line, col) = Match(Token.Increment);
                
                return new IncrementationExpression(node, false)
                    { Line = line, Column = col };
            }
            case TokenType.Decrement:
            {
                var (line, col) = Match(Token.Decrement);
                
                return new DecrementationExpression(node, false) 
                    { Line = line, Column = col};
            }
        }

        while (_postfixOperators.Contains(token.Type))
        {
            switch (token.Type)
            {
                case TokenType.Increment or TokenType.Decrement:
                {
                    goto __incdec;
                }

                case TokenType.LParenthesis or TokenType.ElvisInvocation:
                {
                    node = InvocationExpression(node);
                    break;
                }

                case TokenType.LBracket:
                case TokenType.Dot:
                case TokenType.Elvis:
                case TokenType.ElvisArray:
                {
                    node = IndexerExpression(node, false);
                    break;
                }

                case TokenType.Colon or TokenType.SafeColon:
                {
                    if (!IsSelfInvocation())
                    {
                        goto __exit;
                    }

                    node = SelfInvocationExpression(node);
                    break;
                }
            }

            token = CurrentToken;
        }

    __exit:
        return node;
    }

    private bool IsSelfInvocation()
    {
        if (CurrentToken.Type is not (TokenType.Colon or TokenType.SafeColon))
        {
            return false;
        }

        if (_lexer.PeekToken(1).Type != TokenType.Identifier)
        {
            return false;
        }

        if (_lexer.PeekToken(2).Type != TokenType.LParenthesis)
        {
            return false;
        }

        if (CurrentToken.Type == TokenType.SafeColon || _ternaryDepth == 0)
        {
            return true;
        }

        return HasSubsequentTernaryColon();
    }

    private bool HasSubsequentTernaryColon()
    {
        var k = 3; // start right after '('
        var parenDepth = 1;
        var bracketDepth = 0;
        var braceDepth = 0;

        while (true)
        {
            var tok = _lexer.PeekToken(k);
            if (tok.Type == TokenType.EOF)
            {
                return false;
            }

            switch (tok.Type)
            {
                case TokenType.LParenthesis:
                    parenDepth++;
                    break;
                case TokenType.RParenthesis:
                    parenDepth--;
                    break;
                case TokenType.LBracket:
                    bracketDepth++;
                    break;
                case TokenType.RBracket:
                    bracketDepth--;
                    break;
                case TokenType.LBrace:
                    braceDepth++;
                    break;
                case TokenType.RBrace:
                    braceDepth--;
                    break;
            }

            k++;

            if (parenDepth == 0 && bracketDepth == 0 && braceDepth == 0)
            {
                break;
            }
        }

        var colonsFound = 0;
        var innerQuestionMarks = 0;

        while (true)
        {
            var tok = _lexer.PeekToken(k);
            if (tok.Type is TokenType.EOF or TokenType.Semicolon)
            {
                break;
            }

            switch (tok.Type)
            {
                case TokenType.LParenthesis:
                    parenDepth++;
                    break;
                case TokenType.RParenthesis:
                    if (parenDepth == 0)
                        goto __done;
                    parenDepth--;
                    break;
                case TokenType.LBracket:
                    bracketDepth++;
                    break;
                case TokenType.RBracket:
                    if (bracketDepth == 0)
                        goto __done;
                    bracketDepth--;
                    break;
                case TokenType.LBrace:
                    braceDepth++;
                    break;
                case TokenType.RBrace:
                    if (braceDepth == 0)
                        goto __done;
                    braceDepth--;
                    break;
                case TokenType.Comma when parenDepth == 0 && bracketDepth == 0 && braceDepth == 0:
                    goto __done;
                case TokenType.QuestionMark when parenDepth == 0 && bracketDepth == 0 && braceDepth == 0:
                    innerQuestionMarks++;
                    break;
                case TokenType.Colon when parenDepth == 0 && bracketDepth == 0 && braceDepth == 0:
                    if (innerQuestionMarks > 0)
                    {
                        innerQuestionMarks--;
                    }
                    else
                    {
                        colonsFound++;
                        if (colonsFound >= _ternaryDepth)
                        {
                            return true;
                        }
                    }
                    break;
            }

            k++;
        }

    __done:
        return colonsFound >= _ternaryDepth;
    }
}