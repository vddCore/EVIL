using System.Linq;
using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        private void EmitConstantLoad(CodeGenerator cg, string constant)
        {
            cg.Emit(
                OpCode.LDCS,
                CurrentChunk.Constants.FetchOrAddConstant(constant)
            );
        }

        private void EmitConstantLoad(CodeGenerator cg, double constant)
        {
            cg.Emit(
                OpCode.LDCN,
                CurrentChunk.Constants.FetchOrAddConstant(constant)
            );
        }

        private void EmitGlobalStore(CodeGenerator cg, string identifier)
        {
            cg.Emit(
                OpCode.STG,
                CurrentChunk.Constants.FetchOrAddConstant(
                    identifier
                )
            );
        }

        private void EmitGlobalLoad(CodeGenerator cg, string identifier)
        {
            cg.Emit(
                OpCode.LDG,
                CurrentChunk.Constants.FetchOrAddConstant(
                    identifier
                )
            );
        }

        private void EmitByteOp(CodeGenerator cg, OpCode opCode, byte symId)
        {
            cg.Emit(
                opCode,
                symId
            );
        }

        private void EmitVariableStore(CodeGenerator cg, VariableReferenceExpression varRef)
        {
            Scope localScope = null;
            Scope ownerScope;
            SymbolInfo sym;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            if (localScope != null)
            {
                (ownerScope, sym) = localScope.Find(varRef.Identifier);

                if (sym.Type == SymbolInfo.SymbolType.Global)
                {
                    EmitGlobalStore(cg, varRef.Identifier);
                    return;
                }

                if (localScope.Chunk.Equals(ownerScope.Chunk))
                {
                    if (sym.Type == SymbolInfo.SymbolType.Local)
                    {
                        EmitByteOp(cg, OpCode.STL, (byte)sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                    {
                        EmitByteOp(cg, OpCode.STA, (byte)sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Extern)
                    {
                        cg.Emit(OpCode.STX, sym.Id);
                    }
                }
                else 
                {
                    if (sym.Type == SymbolInfo.SymbolType.Local)
                    {
                        sym = localScope.DefineExtern(
                            varRef.Identifier,
                            ownerScope.Chunk.Name,
                            sym.Id,
                            ExternInfo.ExternType.Local
                        );
                        cg.Emit(OpCode.STX, sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                    {
                        sym = localScope.DefineExtern(
                            varRef.Identifier,
                            ownerScope.Chunk.Name,
                            sym.Id,
                            ExternInfo.ExternType.Parameter
                        );
                        cg.Emit(OpCode.STX, sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Extern)
                    {
                        sym = localScope.DefineExtern(
                            varRef.Identifier,
                            ownerScope.Chunk.Name,
                            sym.Id,
                            ExternInfo.ExternType.Extern
                        );
                        cg.Emit(OpCode.STX, sym.Id);
                    }
                }
            }
            else
            {
                EmitGlobalStore(cg, varRef.Identifier);
            }
        }

        private void EmitVariableLoad(CodeGenerator cg, VariableReferenceExpression varRef)
        {
            Scope localScope = null;
            Scope ownerScope;
            SymbolInfo sym;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            if (localScope != null)
            {
                (ownerScope, sym) = localScope.Find(varRef.Identifier);

                if (ownerScope == null)
                {
                    EmitGlobalLoad(cg, varRef.Identifier);
                    return;
                }

                if (localScope.Chunk.Equals(ownerScope.Chunk))
                {
                    if (sym.Type == SymbolInfo.SymbolType.Local)
                    {
                        EmitByteOp(cg, OpCode.LDL, (byte)sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                    {
                        EmitByteOp(cg, OpCode.LDA, (byte)sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Extern)
                    {
                        cg.Emit(OpCode.LDX, sym.Id);
                    }
                    else if (sym == SymbolInfo.Global)
                    {
                        EmitGlobalLoad(cg, varRef.Identifier);
                    }
                }
                else
                {
                    if (sym.Type == SymbolInfo.SymbolType.Local)
                    {
                        sym = localScope.DefineExtern(
                            varRef.Identifier,
                            ownerScope.Chunk.Name,
                            sym.Id,
                            ExternInfo.ExternType.Local
                        );
                        cg.Emit(OpCode.LDX, sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                    {
                        sym = localScope.DefineExtern(
                            varRef.Identifier,
                            ownerScope.Chunk.Name,
                            sym.Id,
                            ExternInfo.ExternType.Parameter
                        );
                        cg.Emit(OpCode.LDX, sym.Id);
                    }
                    else if (sym.Type == SymbolInfo.SymbolType.Extern)
                    {
                        sym = localScope.DefineExtern(
                            varRef.Identifier,
                            ownerScope.Chunk.Name,
                            sym.Id,
                            ExternInfo.ExternType.Extern
                        );
                        cg.Emit(OpCode.LDX, sym.Id);
                    }
                }
            }
            else
            {
                EmitGlobalLoad(cg, varRef.Identifier);
            }
        }

        private void EmitCompoundAssignment(CodeGenerator cg, AssignmentOperationType op)
        {
            switch (op)
            {
                case AssignmentOperationType.Direct:
                    break;

                case AssignmentOperationType.Add:
                    cg.Emit(OpCode.ADD);
                    break;

                case AssignmentOperationType.Subtract:
                    cg.Emit(OpCode.SUB);
                    break;

                case AssignmentOperationType.Divide:
                    cg.Emit(OpCode.DIV);
                    break;

                case AssignmentOperationType.Multiply:
                    cg.Emit(OpCode.MUL);
                    break;

                case AssignmentOperationType.Modulo:
                    cg.Emit(OpCode.MOD);
                    break;

                case AssignmentOperationType.BitwiseAnd:
                    cg.Emit(OpCode.AND);
                    break;

                case AssignmentOperationType.BitwiseOr:
                    cg.Emit(OpCode.OR);
                    break;

                case AssignmentOperationType.BitwiseXor:
                    cg.Emit(OpCode.XOR);
                    break;

                case AssignmentOperationType.ShiftRight:
                    cg.Emit(OpCode.SHR);
                    break;

                case AssignmentOperationType.ShiftLeft:
                    cg.Emit(OpCode.SHL);
                    break;
            }
        }
    }
}