using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionDefinitionNode : AstNode
    {
        public string Identifier { get; }
        public List<string> ParameterNames { get; }
        
        public BlockStatementNode Statements { get; }

        public FunctionDefinitionNode(string identifier, List<string> parameterNames, BlockStatementNode statements)
        {
            Identifier = identifier;
            ParameterNames = parameterNames;
            
            Statements = statements;
            Reparent(Statements);
        }
    }
}