namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionDefinitionNamedNode : AstNode
    {
        public string Identifier { get; }
        
        public ParameterListNode Parameters { get; }
        public BlockStatementNode Statements { get; }

        public FunctionDefinitionNamedNode(string identifier, ParameterListNode parameters, BlockStatementNode statements)
        {
            Identifier = identifier;
            
            Parameters = parameters;
            Statements = statements;
            
            Reparent(Parameters);
            Reparent(Statements);
        }
    }
}