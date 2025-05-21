namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

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