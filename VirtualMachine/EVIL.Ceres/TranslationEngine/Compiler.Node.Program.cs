using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ProgramNode programNode)
        {
            foreach (var statement in programNode.Statements)
            {
                Visit(statement);
            }
        }
    }
}