using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private TryStatement TryStatement()
        {
            var (line, col) = Match(Token.Try);
            var protectedStatement = BlockStatement();
            Match(Token.Catch);

            IdentifierNode? exceptionObjectIdentifier = null;
            if (CurrentToken == Token.LParenthesis)
            {
                Match(Token.LParenthesis);
                exceptionObjectIdentifier = Identifier();
                Match(Token.RParenthesis);
            }

            var handlerStatement = BlockStatement();

            return new TryStatement(
                protectedStatement,
                exceptionObjectIdentifier,
                handlerStatement
            ) { Line = line, Column = col };
        }
    }
}