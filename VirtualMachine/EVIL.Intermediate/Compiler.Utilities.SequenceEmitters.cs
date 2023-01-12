using EVIL.Grammar;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        private void EmitConstantLoad(CodeGenerator cg, string constant)
        {
            cg.Emit(
                OpCode.LDC,
                _executable.ConstPool.FetchOrAddConstant(constant)
            );
        }

        private void EmitConstantLoad(CodeGenerator cg, double constant)
        {
            cg.Emit(
                OpCode.LDC,
                _executable.ConstPool.FetchOrAddConstant(constant)
            );
        }

        private void EmitGlobalStore(CodeGenerator cg, string identifier)
        {
            cg.Emit(
                OpCode.STG,
                _executable.ConstPool.FetchOrAddConstant(
                    identifier
                )
            );
        }

        private void EmitGlobalLoad(CodeGenerator cg, string identifier)
        {
            cg.Emit(
                OpCode.LDG,
                _executable.ConstPool.FetchOrAddConstant(
                    identifier
                )
            );
        }

        private void EmitVariableStore(CodeGenerator cg, VariableReferenceExpression varRef)
        {
            Scope localScope = null;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            if (localScope != null)
            {
                var (scope, sym) = localScope.Find(varRef.Identifier);

                if (sym == SymbolInfo.Undefined)
                {
                    throw new CompilerException(
                        $"Undefined symbol '{varRef.Identifier}'.",
                        varRef.Line,
                        varRef.Column
                    );
                }

                if (sym.Type == SymbolInfo.SymbolType.Local)
                {
                    cg.Emit(OpCode.STL, sym.Id);
                }
                else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                {
                    cg.Emit(OpCode.STA, sym.Id);
                }
                else if (sym.Type == SymbolInfo.SymbolType.Global)
                {
                    EmitGlobalStore(cg, varRef.Identifier);
                }
                else if (sym.Type == SymbolInfo.SymbolType.Extern)
                {
                    cg.Emit(OpCode.STX, sym.Id);
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

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            if (localScope != null)
            {
                var (_, sym) = localScope.Find(varRef.Identifier);

                if (sym.Type == SymbolInfo.SymbolType.Local)
                {
                    cg.Emit(OpCode.LDL, sym.Id);
                }
                else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                {
                    cg.Emit(OpCode.LDA, sym.Id);
                }
                else if (sym.Type == SymbolInfo.SymbolType.Extern)
                {
                    cg.Emit(OpCode.LDX, sym.Id);
                }
                else if (sym == SymbolInfo.Undefined || sym == SymbolInfo.Global)
                {
                    EmitGlobalLoad(cg, varRef.Identifier);
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