using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; } 

        public ProgramNode(List<AstNode> statements)
        {
            Statements = statements;
            Reparent(Statements.ToArray());
        }

        public FunctionDefinitionNamedNode FindChildFunctionDefinition(string fnName)
        {
            for (var i = 0; i < Statements.Count; i++)
            {
                if (Statements[i] is FunctionDefinitionNamedNode fdn)
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