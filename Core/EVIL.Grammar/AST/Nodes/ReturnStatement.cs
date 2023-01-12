﻿namespace EVIL.Grammar.AST.Nodes
{
    public class ReturnStatement : Statement
    {
        public Expression Expression { get; }

        public ReturnStatement(Expression expression)
        {
            Expression = expression;

            if (Expression != null)
            {
                Reparent(Expression);
            }
        }
    }
}
