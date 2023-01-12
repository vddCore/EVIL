namespace EVIL.Grammar.AST.Nodes
{
    public class EachStatement : Statement
    {
        public VariableDefinition KeyDefinition { get; }
        public VariableDefinition ValueDefinition { get; }
        public Expression Iterable { get; }
        
        public Statement Body { get; }

        public EachStatement(VariableDefinition keyDefinition, VariableDefinition valueDefinition, Expression iterable, Statement body)
        {
            KeyDefinition = keyDefinition;
            ValueDefinition = valueDefinition;
            Iterable = iterable;
            Body = body;

            Reparent(KeyDefinition, ValueDefinition, Iterable, Body);
        }
    }
}