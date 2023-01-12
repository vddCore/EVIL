namespace EVIL.Grammar.AST.Nodes
{
    public class EachStatement : Statement
    {
        public VariableDefinitionNode KeyDefinition { get; }
        public VariableDefinitionNode ValueDefinition { get; }
        public Expression Iterable { get; }
        
        public Statement Body { get; }

        public EachStatement(VariableDefinitionNode keyDefinition, VariableDefinitionNode valueDefinition, Expression iterable, Statement body)
        {
            KeyDefinition = keyDefinition;
            ValueDefinition = valueDefinition;
            Iterable = iterable;
            Body = body;

            Reparent(KeyDefinition, ValueDefinition, Iterable, Body);
        }
    }
}