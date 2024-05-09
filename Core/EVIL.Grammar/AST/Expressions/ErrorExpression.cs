using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

namespace EVIL.Grammar.AST.Expressions
{
    public class ErrorExpression : Expression
    {
        public StringConstant? ImplicitMessageConstant { get; }
        public TableExpression? UserDataTable { get; }

        public ErrorExpression(StringConstant? implicitMessageConstant, TableExpression? userDataTable)
        {
            ImplicitMessageConstant = implicitMessageConstant;
            UserDataTable = userDataTable;

            Reparent(ImplicitMessageConstant);
            Reparent(UserDataTable);
        }
    }
}