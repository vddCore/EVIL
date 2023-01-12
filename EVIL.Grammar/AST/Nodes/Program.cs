using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class Program : Statement
    {
        public List<Statement> Statements { get; } 

        public Program(List<Statement> statements)
        {
            Statements = statements;

            for (var i = 0; i < Statements.Count; i++)
            {
                Reparent(Statements[i]);
            }
        }

        public FunctionDefinition FindChildFunctionDefinition(string fnName)
        {
            for (var i = 0; i < Statements.Count; i++)
            {
                if (Statements[i] is FunctionDefinition fdn)
                {
                    if (fdn.Identifier == fnName)
                    {
                        return fdn;
                    }
                }
            }

            return null;
        }
    }
}