using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionDefinitionNode : AstNode
    {
        public string Identifier { get; }
        public BlockStatementNode Statements { get; }
        public List<string> ParameterNames { get; }

        public FunctionDefinitionNode(string identifier, BlockStatementNode statements, List<string> parameterNames)
        {
            Identifier = identifier;
            
            Statements = statements;
            ParameterNames = parameterNames;

            Reparent(Statements);
        }
    }
}