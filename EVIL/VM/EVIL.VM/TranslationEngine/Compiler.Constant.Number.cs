namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Constants;

public partial class Compiler
{
    protected override void Visit(NumberConstant numberConstant)
    {
        Chunk.CodeGenerator.EmitNumber(numberConstant.Value);
    }
}