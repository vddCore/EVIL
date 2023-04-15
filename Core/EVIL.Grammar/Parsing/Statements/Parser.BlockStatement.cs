using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
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
                        (Lexer.State.Column, Lexer.State.Line)
                    );
                }

                statements.Add(Statement());
            }
            Match(Token.RBrace);

            return new BlockStatement(statements)
                { Line = line, Column = col };
        }
    }
}