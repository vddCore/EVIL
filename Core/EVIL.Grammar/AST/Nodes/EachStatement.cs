namespace EVIL.Grammar.AST.Nodes
{
    public class EachStatement : Statement
    {
        public VariableDefinition Initialization { get; }
        public Expression Iterable { get; }
        
        public Statement Body { get; }

        public EachStatement(VariableDefinition initialization, Expression iterable, Statement body)
        {
            Initialization = initialization;
            Iterable = iterable;
            Body = body;

            Reparent(Initialization, Iterable, Body);
        }
    }
}