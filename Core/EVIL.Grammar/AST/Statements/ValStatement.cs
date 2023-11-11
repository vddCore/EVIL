using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class ValStatement : Statement
    {
        public Dictionary<IdentifierNode, Expression?> Definitions { get; }
        public bool ReadWrite { get; }
        public bool NilAccepting { get; }

        public ValStatement(
            Dictionary<IdentifierNode, Expression?> definitions,
            bool readWrite,
            bool nilAccepting)
        {
            Definitions = definitions;
            ReadWrite = readWrite;
            NilAccepting = nilAccepting;

            Reparent(Definitions.Keys);
            
            foreach (var kvp in Definitions)
            {
                if (kvp.Value != null)
                    Reparent(kvp.Value);
            }
        }
    }
}