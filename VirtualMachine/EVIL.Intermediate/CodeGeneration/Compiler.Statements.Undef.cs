using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(UndefStatement undefStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (undefStatement.Right is VariableReferenceExpression varRef)
            {
                if (ScopeStack.TryPeek(out var localScope))
                {
                    var (_, sym) = localScope.Find(varRef.Identifier);

                    switch (sym.Type)
                    {
                        case SymbolInfo.SymbolType.Global:
                        case SymbolInfo.SymbolType.Undefined:
                            cg.Emit(
                                OpCode.RGL,
                                CurrentChunk.Constants.FetchOrAddConstant(varRef.Identifier)
                            );
                            break;

                        case SymbolInfo.SymbolType.Extern:
                        case SymbolInfo.SymbolType.Parameter:
                        case SymbolInfo.SymbolType.Local:
                            throw new CompilerException(
                                "You can only undefine globals and table entries.",
                                CurrentLine,
                                CurrentColumn
                            );
                    }
                }
                else
                {
                    cg.Emit(
                        OpCode.RGL,
                        CurrentChunk.Constants.FetchOrAddConstant(varRef.Identifier)
                    );
                }
            }
            else if (undefStatement.Right is IndexerExpression indexerExpression)
            {
                Visit(indexerExpression.Indexable);
                Visit(indexerExpression.KeyExpression);
                cg.Emit(OpCode.RTE);
            }
            else
            {
                throw new CompilerException(
                    "Invalid operand provided for undef.",
                    CurrentLine,
                    CurrentColumn
                );
            }
        }
    }
}