using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public class ErrorExpression : Expression
    {
        public TableExpression UserDataTable { get; }

        public ErrorExpression(TableExpression userDataTable)
        {
            UserDataTable = userDataTable;
            Reparent(UserDataTable);
        }
    }
}