namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Miscellaneous;

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