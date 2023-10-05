using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ArgumentList argumentList)
        {
            for (var i = 0; i < argumentList.Arguments.Count; i++)
            {
                Visit(argumentList.Arguments[i]);
            }
        }
    }
}