namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Lexical;

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