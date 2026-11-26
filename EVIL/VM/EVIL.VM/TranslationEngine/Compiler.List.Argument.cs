namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.Core.Grammar.AST.Miscellaneous;

public partial class Compiler
{
    protected override void Visit(ArgumentList argumentList)
    {
        for (var i = 0; i < argumentList.Arguments.Count; i++)
        {
            Visit(argumentList.Arguments[i]);
        }
    }
}