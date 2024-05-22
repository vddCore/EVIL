using EVIL.Grammar.AST.Statements;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(BlockStatement blockStatement)
        {
            InNewLocalScopeDo(() =>
            {
                _blockDescent++;
                {
                    foreach (var node in blockStatement.Statements)
                    {
                        Visit(node);
                    }
                }
                _blockDescent--;
            });
        }
    }
}