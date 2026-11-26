namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.Core.Grammar.AST.Miscellaneous;

public partial class Compiler
{
    protected override void Visit(AttributeList attributeList)
    {
        foreach (var attr in attributeList.Attributes)
            Visit(attr);
    }
}