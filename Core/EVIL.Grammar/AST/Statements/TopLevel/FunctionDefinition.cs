using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements.TopLevel
{
    public sealed class FunctionDefinition : TopLevelStatement
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