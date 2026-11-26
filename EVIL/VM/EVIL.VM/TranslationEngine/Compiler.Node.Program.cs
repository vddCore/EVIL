namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.Core.Grammar.AST.Miscellaneous;

public partial class Compiler
{
    public override void Visit(ProgramNode programNode)
    {
        foreach (var statement in programNode.Statements)
        {
            if (_isUnreachable)
            {
                break;
            }

            Visit(statement);
        }
    }
}