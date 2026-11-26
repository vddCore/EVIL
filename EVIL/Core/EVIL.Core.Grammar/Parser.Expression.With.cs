namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private WithExpression WithExpression(Expression left)
    {
        var (line, col) = Match(Token.With);

        var tableExpansion = TableExpression();

        if (!tableExpansion.Keyed)
        {
            throw new ParserException(
                "Table expansion expression must be a keyed table.",
                (tableExpansion.Line, tableExpansion.Column)
            );
        }

        return new WithExpression(left, tableExpansion)
            { Line = line, Column = col };
    }
}