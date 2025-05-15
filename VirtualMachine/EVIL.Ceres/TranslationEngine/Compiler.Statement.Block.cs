namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(BlockStatement blockStatement)
    {
        InNewLocalScopeDo(() =>
        {
            foreach (var node in blockStatement.Statements)
            {
                Visit(node);
            }
        });
    }
}