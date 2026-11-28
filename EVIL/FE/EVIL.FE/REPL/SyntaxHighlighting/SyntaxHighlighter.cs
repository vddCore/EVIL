namespace EVIL.FE.REPL.SyntaxHighlighting;

using System;
using System.Collections.Generic;
using System.Text;
using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.StringInterpolation;
using EVIL.Core.Lexical;

public sealed class SyntaxHighlighter
{
    private const string Reset = "\u001b[0m";

    private readonly List<(Func<Token, bool> Matcher, TokenStyle Style)> _tokenStyleRules = [];

    public IDictionary<TokenStyle, string> TokenStyleColors { get; } = new Dictionary<TokenStyle, string>
    {
        { TokenStyle.None, string.Empty },
        { TokenStyle.Keyword, "\u001b[35m" },
        { TokenStyle.Type, "\u001b[36m" },
        { TokenStyle.Literal, "\u001b[32m" },
        { TokenStyle.Number, "\u001b[36m" },
        { TokenStyle.String, "\u001b[32m" },
        { TokenStyle.Operator, "\u001b[33m" },
        { TokenStyle.Comment, "\u001b[90m" },
    };

    public void AddTokenStyleRule(Func<Token, bool> matcher, TokenStyle style)
    {
        ArgumentNullException.ThrowIfNull(matcher);
        ArgumentNullException.ThrowIfNull(style);

        _tokenStyleRules.Insert(0, (matcher, style));
    }

    public string Render(string source, int start, int length)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var end = start + length;
        if (end > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var styles = new TokenStyle[source.Length];
        Array.Fill(styles, TokenStyle.None);
        HighlightCode(source, 0, styles);

        var rendered = new StringBuilder();
        var currentStyle = TokenStyle.None;
        var currentColor = string.Empty;
        
        for (var index = start; index < end; index++)
        {
            var style = styles[index];
            if (style != currentStyle)
            {
                if (!string.IsNullOrEmpty(currentColor))
                {
                    rendered.Append(Reset);
                }

                currentColor = string.Empty;
                if (style != TokenStyle.None
                    && TokenStyleColors.TryGetValue(style, out var color)
                    && !string.IsNullOrEmpty(color))
                {
                    rendered.Append(color);
                    currentColor = color;
                }

                currentStyle = style;
            }

            rendered.Append(source[index]);
        }

        if (!string.IsNullOrEmpty(currentColor))
        {
            rendered.Append(Reset);
        }

        return rendered.ToString();
    }

    private void HighlightCode(string source, int sourceOffset, TokenStyle[] styles)
    {
        var lexer = new Lexer();
        try
        {
            lexer.LoadSource(source);
        }
        catch (LexerException)
        {
            return;
        }

        var previousEnd = 0;
        while (true)
        {
            var token = lexer.State.CurrentToken;
            
            var tokenStart = Math.Clamp(
                token.Column - 1,
                previousEnd,
                source.Length
            );
            
            HighlightComments(
                source,
                previousEnd,
                tokenStart,
                sourceOffset,
                styles
            );

            if (token.Type == TokenType.EOF)
            {
                HighlightComments(
                    source,
                    previousEnd,
                    source.Length,
                    sourceOffset,
                    styles
                );
                
                return;
            }

            var tokenEnd = GetTokenEnd(
                source,
                tokenStart,
                token
            );
            
            if (tokenEnd <= tokenStart)
            {
                tokenEnd = Math.Min(tokenStart + 1, source.Length);
            }

            if (token.Type is TokenType.PlainString or TokenType.InterpolatedString)
            {
                SetStyle(styles, sourceOffset + tokenStart, sourceOffset + tokenEnd, TokenStyle.String);
                if (token.Type == TokenType.InterpolatedString)
                {
                    HighlightInterpolatedExpressions(
                        source,
                        tokenStart,
                        tokenEnd,
                        sourceOffset,
                        styles
                    );
                }
            }
            else
            {
                SetStyle(
                    styles,
                    sourceOffset + tokenStart,
                    sourceOffset + tokenEnd,
                    GetStyle(token)
                );
            }

            previousEnd = tokenEnd;
            try
            {
                lexer.NextToken();
            }
            catch (LexerException)
            {
                return;
            }
        }
    }

    private int GetTokenEnd(string source, int start, Token token)
    {
        if (token.Type is TokenType.PlainString or TokenType.InterpolatedString)
        {
            return FindStringEnd(source, start);
        }

        if (token.Type == TokenType.HexInteger)
        {
            var end = Math.Min(start + 2, source.Length);
            while (end < source.Length && Uri.IsHexDigit(source[end]))
            {
                end++;
            }

            return end;
        }

        return Math.Min(start + token.Value.Length, source.Length);
    }

    private int FindStringEnd(string source, int start)
    {
        var delimiter = source[start];
        for (var index = start + 1; index < source.Length; index++)
        {
            if (source[index] == '\\')
            {
                index++;
            }
            else if (source[index] == delimiter)
            {
                return index + 1;
            }
        }

        return source.Length;
    }

    private void HighlightInterpolatedExpressions(
        string source,
        int tokenStart,
        int tokenEnd,
        int sourceOffset,
        TokenStyle[] styles)
    {
        if (tokenStart >= tokenEnd || source[tokenStart] != '"')
        {
            return;
        }

        var hasClosingDelimiter = tokenEnd > tokenStart + 1 
                               && source[tokenEnd - 1] == '"';
        
        var contentStart = tokenStart + 1;
        
        var contentEnd = hasClosingDelimiter 
            ? tokenEnd - 1 
            : tokenEnd;
        
        var content = source.Substring(
            contentStart,
            contentEnd - contentStart
        );

        try
        {
            InterpolatedStringParser.ParseSegments(
                content,
                1,
                1,
                ParseEmbeddedExpression
            );
        }
        catch (LexerException)
        {
            // incomplete expressions are expected when typing.
        }

        var index = contentStart;
        while (index < contentEnd)
        {
            if (source[index] == '\\' && index + 1 < contentEnd)
            {
                index += 2;
                continue;
            }

            if (source[index] != '$' || index + 1 >= contentEnd)
            {
                index++;
                continue;
            }

            if (source[index + 1] == '{')
            {
                var close = FindInterpolationClose(
                    source,
                    index + 2,
                    contentEnd
                );
                
                var expressionEnd = close < 0 
                    ? contentEnd 
                    : close;
                
                SetStyle(
                    styles, 
                    sourceOffset + index,
                    sourceOffset + index + 2,
                    TokenStyle.Operator
                );
                
                HighlightCode(
                    source.Substring(
                        index + 2,
                        expressionEnd - index - 2
                    ),
                    sourceOffset + index + 2,
                    styles
                );

                if (close >= 0)
                {
                    SetStyle(
                        styles,
                        sourceOffset + close,
                        sourceOffset + close + 1,
                        TokenStyle.Operator
                    );
                    
                    index = close + 1;
                }
                else
                {
                    index = contentEnd;
                }

                continue;
            }

            if (char.IsLetter(source[index + 1]) || source[index + 1] == '_')
            {
                var identifierEnd = index + 2;
                while (identifierEnd < contentEnd
                    && (char.IsLetterOrDigit(source[identifierEnd]) || source[identifierEnd] == '_'))
                {
                    identifierEnd++;
                }

                SetStyle(
                    styles,
                    sourceOffset + index,
                    sourceOffset + index + 1,
                    TokenStyle.Operator
                );
                
                HighlightCode(
                    source.Substring(index + 1, identifierEnd - index - 1),
                    sourceOffset + index + 1,
                    styles
                );
                
                index = identifierEnd;
                continue;
            }

            index++;
        }
    }

    private Expression ParseEmbeddedExpression(string source, int line, int column)
    {
        try
        {
            return new Parser().ParseExpression(
                source
            );
        }
        catch (ParserException)
        {
            return null!;
        }
        catch (LexerException)
        {
            return null!;
        }
    }

    private int FindInterpolationClose(string source, int start, int end)
    {
        var braceDepth = 1;
        for (var index = start; index < end; index++)
        {
            if (source[index] == '\\')
            {
                index++;
                continue;
            }

            if (source[index] == '/' && index + 1 < end && source[index + 1] == '/')
            {
                index += 2;
                while (index < end && source[index] != '\n')
                {
                    index++;
                }

                continue;
            }

            if (source[index] == '/' && index + 1 < end && source[index + 1] == '*')
            {
                index += 2;
                while (index + 1 < end && !(source[index] == '*' && source[index + 1] == '/'))
                {
                    index++;
                }

                index++;
                continue;
            }

            if (source[index] is '"' or '\'')
            {
                var delimiter = source[index++];
                while (index < end && source[index] != delimiter)
                {
                    if (source[index] == '\\')
                    {
                        index++;
                    }

                    index++;
                }

                continue;
            }

            if (source[index] == '{')
            {
                braceDepth++;
            }
            else if (source[index] == '}' && --braceDepth == 0)
            {
                return index;
            }
        }

        return -1;
    }

    private void HighlightComments(
        string source,
        int start,
        int end,
        int sourceOffset,
        TokenStyle[] styles)
    {
        for (var index = start; index + 1 < end; index++)
        {
            if (source[index] != '/')
            {
                continue;
            }

            if (source[index + 1] == '/')
            {
                SetStyle(
                    styles,
                    sourceOffset + index,
                    sourceOffset + end,
                    TokenStyle.Comment
                );
                
                return;
            }

            if (source[index + 1] == '*')
            {
                var commentEnd = source.IndexOf(
                    "*/",
                    index + 2,
                    StringComparison.Ordinal
                );
                
                commentEnd = commentEnd < 0 || commentEnd + 2 > end 
                    ? end 
                    : commentEnd + 2;
                
                SetStyle(
                    styles,
                    sourceOffset + index, 
                    sourceOffset + commentEnd,
                    TokenStyle.Comment
                );
                
                index = commentEnd - 1;
            }
        }
    }

    private TokenStyle GetStyle(Token token)
    {
        foreach (var (matcher, style) in _tokenStyleRules)
        {
            if (matcher(token))
            {
                return style;
            }
        }

        return token.Class switch
        {
            TokenClass.Keyword => TokenStyle.Keyword,
            TokenClass.TypeCode => TokenStyle.Type,
            TokenClass.Alias => TokenStyle.Number,
            TokenClass.Literal when token.Type 
                is TokenType.Number 
                or TokenType.HexInteger => TokenStyle.Number,
            TokenClass.Literal => TokenStyle.Literal,
            TokenClass.Operator => TokenStyle.Operator,
            _ => TokenStyle.None
        };
    }

    private void SetStyle(
        TokenStyle[] styles,
        int start, 
        int end, TokenStyle style)
    {
        start = Math.Clamp(start, 0, styles.Length);
        end = Math.Clamp(end, start, styles.Length);
        
        Array.Fill(
            styles,
            style,
            start,
            end - start
        );
    }
}