using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class VariableDefinition : Statement
    {
        public Dictionary<string, Expression?> Definitions { get; }
        public bool ReadWrite { get; }

        public VariableDefinition(Dictionary<string, Expression?> definitions, bool readWrite)
        {
            Definitions = definitions;
            ReadWrite = readWrite;
            
            foreach (var kvp in Definitions)
            {
                if (kvp.Value != null)
                    Reparent(kvp.Value);
            }
        }
    }
}