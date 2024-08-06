namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Statements.TopLevel;

public partial class Compiler
{
    public override void Visit(AttributeList attributeList)
    {
        foreach (var attr in attributeList.Attributes)
            Visit(attr);
    }
}