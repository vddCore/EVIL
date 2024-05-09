using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
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
}