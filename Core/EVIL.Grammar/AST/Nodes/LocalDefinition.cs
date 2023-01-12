using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class LocalDefinition : Statement
    {
        public Dictionary<string, Expression> Definitions { get; }

        public LocalDefinition(Dictionary<string, Expression> definitions)
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