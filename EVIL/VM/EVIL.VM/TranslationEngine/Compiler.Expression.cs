namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Base;

public partial class Compiler
{
    protected void Visit(Expression? expression)
    {
        if (expression != null)
        {
            Visit((AstNode)expression);
        }
    }
}