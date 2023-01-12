using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionDefinitionNamedNode : Statement
    {
        public string Identifier { get; }
        
        public List<string> Parameters { get; }
        public BlockStatement Statements { get; }

        public FunctionDefinitionNamedNode(string identifier, List<string> parameters, BlockStatement statements)
        {
            Identifier = identifier;
            
            Parameters = parameters;
            Statements = statements;
            
            Reparent(Statements);
        }
    }
}