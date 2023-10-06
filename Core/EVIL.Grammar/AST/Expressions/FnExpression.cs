using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class FnExpression : Expression
    {
        public ParameterList ParameterList { get; }
        public Statement Statement { get; }

        public FnExpression(
            ParameterList parameterList,
            Statement statement)
        {
            ParameterList = parameterList;
            Statement = statement;

            Reparent(ParameterList);
            Reparent(Statement);
        }
    }
}