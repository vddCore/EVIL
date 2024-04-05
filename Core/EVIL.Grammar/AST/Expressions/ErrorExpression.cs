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

            if (ImplicitMessageConstant != null)
            {
                Reparent(ImplicitMessageConstant);
            }

            if (UserDataTable != null)
            {
                Reparent(UserDataTable);
            }
        }
    }
}