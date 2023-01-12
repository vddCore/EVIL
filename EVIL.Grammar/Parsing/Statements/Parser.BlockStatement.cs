using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private BlockStatementNode BlockStatement()
        {
            var line = Match(TokenType.LBrace);

            var statements = new List<AstNode>();
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
            Match(TokenType.RBrace);

            return new BlockStatementNode(statements) { Line = line };
        }
    }
}