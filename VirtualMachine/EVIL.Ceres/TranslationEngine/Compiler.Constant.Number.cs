namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Constants;

public partial class Compiler
{
    protected override void Visit(NumberConstant numberConstant)
    {
        Chunk.CodeGenerator.EmitNumber(numberConstant.Value);
    }
}