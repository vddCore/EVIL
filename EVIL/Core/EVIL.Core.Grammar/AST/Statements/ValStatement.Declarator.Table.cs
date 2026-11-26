namespace EVIL.Core.Grammar.AST.Statements;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class TablePatternValDeclarator : ValDeclarator
{
    public IReadOnlyList<TablePatternField> Fields { get; }

    public TablePatternValDeclarator(
        IReadOnlyList<TablePatternField> fields,
        Expression initializer
    ) : base(initializer)
    {
        Fields = [..fields];

        foreach (var field in Fields)
        {
            Reparent(field);
        }
    }
}