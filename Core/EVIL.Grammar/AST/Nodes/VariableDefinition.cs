using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class VariableDefinition : Statement
    {
        public Dictionary<string, Expression> Definitions { get; }

        public VariableDefinition(Dictionary<string, Expression> definitions)
        {
            Definitions = definitions;

            foreach (var kvp in Definitions)
            {
                if (kvp.Value != null)
                    Reparent(kvp.Value);
            }
        }
    }
}