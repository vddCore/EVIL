namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionDefinitionAnonymousNode : AstNode
    {
        public ParameterListNode Parameters { get; }
        public BlockStatementNode Statements { get; }

        public FunctionDefinitionAnonymousNode(ParameterListNode parameters, BlockStatementNode statements)
        {
            Parameters = parameters;
            Statements = statements;

            Reparent(Parameters);
            Reparent(Statements);
        }
    }
}