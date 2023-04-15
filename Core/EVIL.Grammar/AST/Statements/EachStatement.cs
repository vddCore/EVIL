namespace EVIL.Grammar.AST.Statements
{
    public sealed class EachStatement : Statement
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