using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(NameOfExpression nameOfExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (nameOfExpression.Right is VariableReferenceExpression varRef)
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
                        case SymbolInfo.SymbolType.Undefined:
                            cg.Emit(
                                OpCode.GNAME,
                                _executable.ConstPool.FetchOrAddConstant(varRef.Identifier)
                            );
                            break;
                    }
                }
                else
                {
                    cg.Emit(
                        OpCode.GNAME,
                        _executable.ConstPool.FetchOrAddConstant(varRef.Identifier)
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