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
                if (_isUnreachable)
                {
                    break;
                }

                Visit(node);
            }
        });
    }
}