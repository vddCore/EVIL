using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private BlockStatement BlockStatement()
        {
            var line = Match(Token.LBrace);

            var statements = new List<Statement>();
            while (CurrentToken.Type != TokenType.RBrace)
            {
                if (CurrentToken.Type == TokenType.EOF)
                {
                    throw new ParserException(
                        "Unexpected EOF in a statement block.", 
                        Lexer.State
                    );
                }

                statements.Add(Statement());
            }
            Match(Token.RBrace);

            return new BlockStatement(statements) { Line = line };
        }
    }
}