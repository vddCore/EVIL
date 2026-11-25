namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Base;

public partial class Compiler
{
    protected override void Visit(Expression? expression)
    {
        if (expression != null)
        {
            Visit((AstNode)expression);
        }
    }
}