namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private ProgramNode Program()
    {
        var statementList = new List<Statement>();

        if (CurrentToken == Token.Empty)
        {
            throw new ParserException(
                "Internal error: lexer is in an invalid state (current token is empty?).",
                new SourceLocation(FilePath, -1, -1)
            );
        }

        while (CurrentToken.Type != TokenType.EOF)
        {
            if (DiagnosticSink != null)
            {
                if (DiagnosticSink.ErrorCount >= MaxErrors)
                {
                    break;
                }

                try
                {
                    statementList.Add(Statement());
                }
                catch (ParserException pe)
                {
                    ReportParserException(pe);
                    statementList.Add(new DiagnosticStatement(pe.Message)
                    {
                        Line = pe.Line,
                        Column = pe.Column
                    });
                    Synchronize();
                }
                catch (LexerException le)
                {
                    ReportLexerException(le);
                    statementList.Add(new DiagnosticStatement(le.Message)
                    {
                        Line = le.Line,
                        Column = le.Column
                    });
                    Synchronize();
                }
            }
            else
            {
                statementList.Add(Statement());
            }
        }

        return new ProgramNode(statementList);
    }
}
