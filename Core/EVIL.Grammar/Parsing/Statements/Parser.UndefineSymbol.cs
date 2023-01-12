﻿using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private UndefStatement UndefineSymbol()
        {
            var (line, col) = Match(Token.Undef);
            var expr = PostfixExpression();

            if (expr is not IndexerExpression && expr is not VariableReferenceExpression)
            {
                throw new ParserException(
                    "`undef' is only valid for indexing and variable reference expressions.",
                    (Lexer.State.Line, Lexer.State.Column)
                );
            }

            return new UndefStatement(expr) 
                { Line = line, Column = col };
        }
    }
}