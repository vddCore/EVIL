namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class SelfFnExpression : Expression
{
    public ParameterList? ParameterList { get; }
    public Statement Statement { get; }

    public SelfFnExpression(
        ParameterList? parameterList,
        Statement statement)
    {
        ParameterList = parameterList;
        Statement = statement;
            
        Reparent(ParameterList);
        Reparent(Statement);
    }
}