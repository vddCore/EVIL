namespace EVIL.Core.Grammar.AST.Statements;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class ValStatement : Statement
{
    public Dictionary<IdentifierNode, Expression?> Definitions { get; }
    public bool ReadWrite { get; }

    public ValStatement(
        Dictionary<IdentifierNode, Expression?> definitions,
        bool readWrite)
    {
        Definitions = definitions;
        ReadWrite = readWrite;

        Reparent(Definitions.Keys);
        Reparent(Definitions.Values);
    }
}