namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private TryStatement TryStatement()
    {
        var (line, col) = Match(Token.Try);

        Statement protectedStatement = Statement();
        Statement? handlerStatement = null;
        IdentifierNode? exceptionObjectIdentifier = null;
        
        if (CurrentToken == Token.Catch)
        {
            Match(Token.Catch);

            if (CurrentToken == Token.LParenthesis)
            {
                Match(Token.LParenthesis);
                exceptionObjectIdentifier = Identifier();
                Match(Token.RParenthesis);
            }

            handlerStatement = BlockStatement();
        }

        return new TryStatement(
            protectedStatement,
            exceptionObjectIdentifier,
            handlerStatement
        ) { Line = line, Column = col };
    }
}