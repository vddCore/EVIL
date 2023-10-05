using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class VarStatement : Statement
    {
        public Dictionary<IdentifierNode, Expression?> Definitions { get; }
        public bool ReadWrite { get; }

        public VarStatement(
            Dictionary<IdentifierNode, Expression?> definitions,
            bool readWrite)
        {
            Definitions = definitions;
            ReadWrite = readWrite;

            Reparent(Definitions.Keys);
            
            foreach (var kvp in Definitions)
            {
                if (kvp.Value != null)
                    Reparent(kvp.Value);
            }
        }
    }
}