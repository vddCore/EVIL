﻿using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class DoWhileStatement : Statement
    {
        public Expression Condition { get; }
        public Statement Statement { get; }

        public DoWhileStatement(Expression condition, Statement statement)
        {
            Condition = condition;
            Statement = statement;

            Reparent(Condition, Statement);
        }
    }
}