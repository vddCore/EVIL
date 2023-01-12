using EVIL.Grammar;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        private void EmitConstantLoadSequence(CodeGenerator cg, string constant)
        {
            cg.Emit(
                OpCode.LDC,
                _executable.ConstPool.FetchOrAddConstant(constant)
            );
        }

        private void EmitConstantLoadSequence(CodeGenerator cg, double constant)
        {
            cg.Emit(
                OpCode.LDC,
                _executable.ConstPool.FetchOrAddConstant(constant)
            );
        }
        
        private void EmitGlobalStoreSequence(CodeGenerator cg, string identifier)
        {
            cg.Emit(
                OpCode.STG,
                _executable.ConstPool.FetchOrAddConstant(
                    identifier
                )
            );
        }

        private void EmitGlobalLoadSequence(CodeGenerator cg, string identifier)
        {
            cg.Emit(
                OpCode.LDG,
                _executable.ConstPool.FetchOrAddConstant(
                    identifier
                )
            );
        }

        private void EmitVariableStoreSequence(CodeGenerator cg, VariableReferenceExpression varRef)
        {
            Scope localScope = null;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            if (localScope != null)
            {
                var sym = localScope.Find(varRef.Identifier);

                if (sym == SymbolInfo.Undefined)
                {
                    throw new CompilerException(
                        $"Undefined symbol '{varRef.Identifier}'.",
                        varRef.Line
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
                    EmitGlobalStoreSequence(cg, varRef.Identifier);
                }
            }
            else
            {
                EmitGlobalStoreSequence(cg, varRef.Identifier);
            }
        }

        private void EmitVariableLoadSequence(CodeGenerator cg, VariableReferenceExpression varRef)
        {
            Scope localScope = null;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            if (localScope != null)
            {
                var sym = localScope.Find(varRef.Identifier);

                if (sym.Type == SymbolInfo.SymbolType.Local)
                {
                    cg.Emit(OpCode.LDL, sym.Id);
                }
                else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                {
                    cg.Emit(OpCode.LDA, sym.Id);
                }
                else if (sym == SymbolInfo.Undefined || sym == SymbolInfo.Global)
                {
                    EmitGlobalLoadSequence(cg, varRef.Identifier);
                }
            }
            else
            {
                EmitGlobalLoadSequence(cg, varRef.Identifier);
            }
        }

        private void EmitCompoundAssignmentSequence(CodeGenerator cg, VariableReferenceExpression varRef,
            AssignmentOperationType op)
        {
            switch (op)
            {
                case AssignmentOperationType.Direct:
                    break;

                case AssignmentOperationType.Add:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.ADD);
                    break;

                case AssignmentOperationType.Subtract:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.SUB);
                    break;

                case AssignmentOperationType.Divide:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.DIV);
                    break;

                case AssignmentOperationType.Multiply:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.MUL);
                    break;

                case AssignmentOperationType.Modulo:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.MOD);
                    break;

                case AssignmentOperationType.BitwiseAnd:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.AND);
                    break;

                case AssignmentOperationType.BitwiseOr:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.OR);
                    break;

                case AssignmentOperationType.BitwiseXor:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.XOR);
                    break;

                case AssignmentOperationType.ShiftRight:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.SHR);
                    break;

                case AssignmentOperationType.ShiftLeft:
                    EmitVariableLoadSequence(cg, varRef);
                    cg.Emit(OpCode.SHL);
                    break;
            }
        }
    }
}