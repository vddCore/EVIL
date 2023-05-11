namespace EVIL.Grammar.AST.Statements
{
    public sealed class FunctionDefinition : Statement
    {
        public string Identifier { get; }

        public ParameterList ParameterList { get; }
        public Statement Statement { get; }

        public FunctionDefinition(string identifier, ParameterList parameterList, Statement statement)
        {
            Identifier = identifier;

            ParameterList = parameterList;
            Statement = statement;

            Reparent(Statement);
        }
    }
}