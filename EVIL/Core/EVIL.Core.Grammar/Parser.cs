namespace EVIL.Core.Grammar;

using System;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Lexical;

public partial class Parser
{
    private readonly Lexer _lexer;
        
    private uint _functionDescent;
    private uint _loopDescent;
    private int _ternaryDepth;

    public LexerState CurrentState => _lexer.State;
    public Token CurrentToken => CurrentState.CurrentToken;

    public IDiagnosticSink? DiagnosticSink { get; set; }
    public string? FilePath { get; set; }
    public int MaxErrors { get; set; } = 20;

    public Parser(IDiagnosticSink? diagnosticSink = null, string? filePath = null)
    {
        DiagnosticSink = diagnosticSink;
        FilePath = filePath;
        _lexer = new Lexer(diagnosticSink, filePath);
    }

    public ProgramNode Parse(string source)
    {
        _functionDescent = 0;
        _loopDescent = 0;
        _ternaryDepth = 0;
        _lexer.LoadSource(source);

        var programNode = Program();
        if (DiagnosticSink == null || DiagnosticSink.ErrorCount < MaxErrors)
        {
            Match(Token.EOF);
        }

        return programNode;
    }

    public Expression ParseExpression(string expressionSource)
    {
        _functionDescent = 0;
        _loopDescent = 0;
        _ternaryDepth = 0;
        _lexer.LoadSource(expressionSource);

        var expression = AssignmentExpression();
        Match(Token.EOF);

        return expression;
    }

    public void Synchronize()
    {
        if (CurrentToken.Type != TokenType.EOF)
        {
            _lexer.NextToken();
        }

        while (CurrentToken.Type != TokenType.EOF)
        {
            if (StateIsStatementDelimiter(CurrentToken))
            {
                if (CurrentToken.Type == TokenType.Semicolon)
                {
                    _lexer.NextToken();
                }
                return;
            }

            _lexer.NextToken();
        }
    }

    private static bool StateIsStatementDelimiter(Token token)
    {
        return token.Type switch
        {
            TokenType.Semicolon => true,
            TokenType.Val => true,
            TokenType.Rw => true,
            TokenType.Fn => true,
            TokenType.If => true,
            TokenType.While => true,
            TokenType.Do => true,
            TokenType.For => true,
            TokenType.Each => true,
            TokenType.Ret => true,
            TokenType.Throw => true,
            TokenType.Try => true,
            TokenType.RBrace => true,
            _ => false
        };
    }

    private void ReportParserException(ParserException pe)
    {
        var loc = pe.Location.HasLocation 
            ? pe.Location 
            : new SourceLocation(FilePath, pe.Line, pe.Column);

        DiagnosticSink?.Report(
            CompilerDiagnostics.SyntaxError.Create(loc, pe.Message)
        );
    }

    private void ReportLexerException(LexerException le)
    {
        var loc = le.Location.HasLocation 
            ? le.Location 
            : new SourceLocation(FilePath, le.Line, le.Column);

        DiagnosticSink?.Report(
            CompilerDiagnostics.LexicalError.Create(loc, le.Message)
        );
    }

    private T FunctionDescent<T>(Func<T> func) where T : AstNode
    {
        _functionDescent++;
        var ret = func();
        _functionDescent--;

        return ret;
    }

    private T LoopDescent<T>(Func<T> func) where T : AstNode
    {
        _loopDescent++;
        var ret = func();
        _loopDescent--;

        return ret;
    }

    private (int, int) Match(Token token, string? customErrorMessage = null)
    {
        var line = CurrentToken.Line;
        var column = CurrentToken.Column;

        if (CurrentToken.Type != token.Type)
        {
            var expected = $"'{token.Value}'";
            if (string.IsNullOrEmpty(token.Value))
            {
                expected = WithArticle(token.Type.ToString());
            }

            var actual = $"'{CurrentToken.Value}'";
            if (string.IsNullOrEmpty(CurrentToken.Value))
            {
                actual = WithArticle(CurrentToken.Type.ToString());
            }

            if (customErrorMessage != null)
            {
                customErrorMessage = customErrorMessage
                    .Replace("$expected", expected)
                    .Replace("$actual", actual);
            }
            else
            {
                customErrorMessage = $"Expected {expected}, got {actual}.";
            }
                
            throw new ParserException(
                customErrorMessage,
                new SourceLocation(FilePath, line, column)
            );
        }

        _lexer.NextToken();
        return (line, column);
    }

    private static string WithArticle(string word)
    {
        var result = word.ToLower();

        return "aeiou".Contains(result[0])
            ? $"an {result}"
            : $"a {result}";
    }
}
