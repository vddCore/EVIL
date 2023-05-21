﻿using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Expressions
{
    public class YieldExpression : Expression
    {
        public Expression Target { get; }
        public ArgumentList ArgumentList { get; }

        public YieldExpression(Expression target, ArgumentList argumentList)
        {
            Target = target;
            ArgumentList = argumentList;

            Reparent(Target);
            Reparent(ArgumentList);
        }
    }
}