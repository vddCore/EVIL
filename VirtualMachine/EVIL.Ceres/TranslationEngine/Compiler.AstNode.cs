namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(AstNode node)
    {
        Line = node.Line;
        Column = node.Column;

        if (node is Expression expression)
        {
            node = expression.Reduce();
        }
        else if (node is Statement stmt and not BlockStatement)
        {
            MarkStatementBoundary(stmt);
        }

        base.Visit(node);
    }
}