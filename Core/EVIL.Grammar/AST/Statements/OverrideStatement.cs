using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public class OverrideStatement : Statement
    {
        public Expression Target { get; }
        public TableOverride Override { get; }
        public ParameterList? ParameterList { get; }
        public Statement Statement { get; }

        public OverrideStatement(
            Expression target,
            TableOverride @override,
            ParameterList? parameterList,
            Statement statement
        )
        {
            Target = target;
            Override = @override;
            ParameterList = parameterList;
            Statement = statement;
            
            Reparent(Target);
            Reparent(ParameterList);
            Reparent(Statement);
        }
    }
}