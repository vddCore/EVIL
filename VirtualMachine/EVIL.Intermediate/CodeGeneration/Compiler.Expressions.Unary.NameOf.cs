using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        private void UnaryNameOf(UnaryExpression unaryExpression, CodeGenerator cg)
        {
            if (unaryExpression.Right is VariableReferenceExpression varRef)
            {
                if (ScopeStack.TryPeek(out var localScope))
                {
                    var (_, sym) = localScope.Find(varRef.Identifier);

                    switch (sym.Type)
                    {
                        case SymbolInfo.SymbolType.Parameter:
                            cg.Emit(
                                OpCode.PNAME,
                                (byte)sym.Id
                            );
                            break;
                        case SymbolInfo.SymbolType.Local:
                            cg.Emit(
                                OpCode.LNAME,
                                sym.Id
                            );
                            break;
                        case SymbolInfo.SymbolType.Extern:
                            cg.Emit(
                                OpCode.XNAME,
                                sym.Id
                            );
                            break;
                        case SymbolInfo.SymbolType.Global:
                            cg.Emit(
                                OpCode.GNAME,
                                CurrentChunk.Constants.FetchOrAddConstant(varRef.Identifier)
                            );
                            break;
                    }
                }
                else
                {
                    cg.Emit(
                        OpCode.GNAME,
                        CurrentChunk.Constants.FetchOrAddConstant(varRef.Identifier)
                    );
                }
            }
            else
            {
                throw new CompilerException(
                    "'??' operator is only valid for variables.",
                    CurrentLine,
                    CurrentColumn
                );
            }
        }
    }
}