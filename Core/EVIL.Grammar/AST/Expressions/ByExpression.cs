using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Expressions
{
    public class ByExpression : Expression
    {
        public Expression Qualifier { get; }
        
        public List<ByArmNode> Arms { get; }
        public AstNode? ElseArm { get; }

        public ByExpression(Expression qualifier, List<ByArmNode> arms, AstNode? elseArm)
        {
            Qualifier = qualifier;
            Arms = arms;
            ElseArm = elseArm;

            Reparent(Qualifier);
            Reparent(Arms);

            if (ElseArm != null)
            {
                Reparent(ElseArm);
            }
        }
    }
}