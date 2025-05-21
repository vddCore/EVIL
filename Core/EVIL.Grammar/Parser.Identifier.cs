namespace EVIL.Grammar;

using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

public partial class Parser
{
    private IdentifierNode Identifier(string? customErrorMessage = null)
    {
        var name = CurrentToken.Value;
        var (line, col) = Match(Token.Identifier, customErrorMessage);

        return new IdentifierNode(name)
            { Line = line, Column = col };
    }
}