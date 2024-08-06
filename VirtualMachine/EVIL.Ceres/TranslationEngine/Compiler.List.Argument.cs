namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Miscellaneous;

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