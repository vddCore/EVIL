namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Diagnostics;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private BlockStatement BlockStatement()
    {
        var (line, col) = Match(Token.LBrace);

        var statements = new List<Statement>();
        while (CurrentToken.Type != TokenType.RBrace)
        {
            if (CurrentToken.Type == TokenType.EOF)
            {
                throw new ParserException(
                    $"Unexpected EOF in a block statement. Unmatched '{{' on line {line}, column {col}.",
                    new SourceLocation(FilePath, _lexer.State.Line, _lexer.State.Column)
                );
            }

            if (DiagnosticSink != null)
            {
                if (DiagnosticSink.ErrorCount >= MaxErrors)
                {
                    break;
                }

                try
                {
                    statements.Add(Statement());
                }
                catch (ParserException pe)
                {
                    ReportParserException(pe);
                    statements.Add(new DiagnosticNode(pe.Message)
                    {
                        Line = pe.Line,
                        Column = pe.Column
                    });
                    Synchronize();
                }
                catch (LexerException le)
                {
                    ReportLexerException(le);
                    statements.Add(new DiagnosticNode(le.Message)
                    {
                        Line = le.Line,
                        Column = le.Column
                    });
                    Synchronize();
                }
            }
            else
            {
                statements.Add(Statement());
            }
        }
        Match(Token.RBrace);

        return new BlockStatement(statements)
            { Line = line, Column = col };
    }
}
