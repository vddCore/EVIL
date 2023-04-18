using System;
using System.Collections.Generic;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.Grammar;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.Traversal;

namespace Ceres.TranslationEngine
{
    public class Compiler : AstVisitor
    {
        private Script _script = new();
        
#nullable disable
        private Scope _rootScope;
        private Scope _currentScope;
#nullable enable
        
        private readonly Stack<Chunk> _chunks = new();
        private List<ChunkAttribute> _attributeList = new();
        private Dictionary<string, List<AttributeProcessor>> _attributeProcessors = new();
        
        private readonly Stack<Loop> _loopDescent = new();

        private Chunk Chunk => _chunks.Peek();
        private Loop Loop => _loopDescent.Peek();

        public Script Compile(Program program)
        {
            Visit(program);
            return _script;
        }

        public void RegisterAttributeProcessor(string attributeName, AttributeProcessor processor)
        {
            if (!_attributeProcessors.TryGetValue(attributeName, out var list))
            {
                list = new List<AttributeProcessor>();
                _attributeProcessors.Add(attributeName, list);
            }

            list.Add(processor);
        }

        public override void Visit(Program program)
        {
            _script = new Script();
            _rootScope = Scope.CreateRoot();
            _currentScope = _rootScope;

            foreach (var node in program.Statements)
            {
                Visit(node);
            }
        }

        private void InTopLevelChunk(Action action, string? name = null)
        {
            var chunk = new Chunk { Name = name };

            _chunks.Push(chunk);
            {
                action();
            }

            ApplyAnyPendingAttributes(chunk);
            
            _script.Chunks.Add(chunk);
            _chunks.Pop();
        }

        private void ApplyAnyPendingAttributes(Chunk chunk)
        {
            foreach (var attrib in _attributeList)
            {
                if (_attributeProcessors.TryGetValue(attrib.Name, out var processors))
                {
                    foreach (var processor in processors)
                    {
                        try
                        {
                            processor.Invoke(attrib, chunk);
                        }
                        catch (Exception e)
                        {
                            throw new AttributeProcessorException(attrib, chunk, e);
                        }
                    }
                }
                
                chunk.AddAttribute(attrib);
            }
            _attributeList.Clear();
        }

        private void InNewScopeDo(Action action)
        {
            _currentScope = _currentScope!.Descend();
            {
                action();
            }
            _currentScope = _currentScope.Parent!;
        }

        private void InNewLoopDo(Action action)
        {
            _loopDescent.Push(new Loop(Chunk));
            {
                action();
            }
            _loopDescent.Pop();
        }

        public override void Visit(BlockStatement blockStatement)
        {
            InNewScopeDo(() =>
            {
                foreach (var node in blockStatement.Statements)
                {
                    Visit(node);
                }
            });
        }

        public override void Visit(NilConstant nilConstant)
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }

        public override void Visit(NumberConstant numberConstant)
        {
            if (numberConstant.Value.Equals(0))
            {
                Chunk.CodeGenerator.Emit(OpCode.LDZERO);
            }
            else if (numberConstant.Value.Equals(1))
            {
                Chunk.CodeGenerator.Emit(OpCode.LDONE);
            }
            else
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDNUM,
                    numberConstant.Value
                );
            }
        }

        public override void Visit(BooleanConstant booleanConstant)
        {
            Chunk.CodeGenerator.Emit(
                booleanConstant.Value
                    ? OpCode.LDTRUE
                    : OpCode.LDFALSE
            );
        }

        public override void Visit(StringConstant stringConstant)
        {
            var id = Chunk.StringPool.FetchOrAdd(stringConstant.Value);
            Chunk.CodeGenerator.Emit(
                OpCode.LDSTR,
                (int)id
            );
        }

        public override void Visit(ExpressionStatement expressionStatement)
        {
            Visit(expressionStatement.Expression);
            Chunk.CodeGenerator.Emit(OpCode.POP);
        }

        public override void Visit(AttributeStatement attributeStatement)
        {
            var attribute = new ChunkAttribute(attributeStatement.Name);

            foreach (var valueNode in attributeStatement.Values)
            {
                DynamicValue dv;
                if (valueNode is BooleanConstant boolConst)
                {
                    dv = new DynamicValue(boolConst.Value);
                }
                else if (valueNode is NilConstant)
                {
                    dv = DynamicValue.Nil;
                }
                else if (valueNode is NumberConstant numConst)
                {
                    dv = new DynamicValue(numConst.Value);
                }
                else if (valueNode is StringConstant stringConst)
                {
                    dv = new DynamicValue(stringConst.Value);
                }
                else
                {
                    throw new CompilerException(
                        $"Internal error: unexpected attribute value node type '{valueNode.GetType().FullName}'."
                    );
                }
                attribute.Values.Add(dv);
            }

            _attributeList.Add(attribute);
        }

        public override void Visit(AttributeListStatement attributeListStatement)
        {
            foreach (var attr in attributeListStatement.Attributes)
                Visit(attr);
        }

        public override void Visit(ReturnStatement returnStatement)
        {
            if (returnStatement.Expression != null)
            {
                Visit(returnStatement.Expression);
            }

            Chunk.CodeGenerator.Emit(OpCode.RET);
        }

        public override void Visit(ConditionalExpression conditionalExpression)
        {
            var elseLabel = Chunk.CreateLabel();
            var endLabel = Chunk.CreateLabel();

            Visit(conditionalExpression.Condition);
            Chunk.CodeGenerator.Emit(OpCode.FJMP, elseLabel);
            
            Visit(conditionalExpression.TrueExpression);
            Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);
            Chunk.UpdateLabel(elseLabel, Chunk.CodeGenerator.IP);
            
            Visit(conditionalExpression.FalseExpression);
            Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
        }

        public override void Visit(AssignmentExpression assignmentExpression)
        {
            if (assignmentExpression.Left is VariableReferenceExpression vre)
            {
                if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
                {
                    EmitVarGet(vre.Identifier);
                    Visit(assignmentExpression.Right);

                    Chunk.CodeGenerator.Emit(
                        assignmentExpression.OperationType switch
                        {
                            AssignmentOperationType.Add => OpCode.ADD,
                            AssignmentOperationType.Subtract => OpCode.SUB,
                            AssignmentOperationType.Divide => OpCode.DIV,
                            AssignmentOperationType.Modulo => OpCode.MOD,
                            AssignmentOperationType.Multiply => OpCode.MUL,
                            AssignmentOperationType.BitwiseAnd => OpCode.BAND,
                            AssignmentOperationType.BitwiseOr => OpCode.BOR,
                            AssignmentOperationType.BitwiseXor => OpCode.BXOR,
                            AssignmentOperationType.ShiftLeft => OpCode.SHL,
                            AssignmentOperationType.ShiftRight => OpCode.SHR,
                            _ => throw new CompilerException("Internal error: invalid assignment operation.")
                        }
                    );
                }
                else
                {
                    Visit(assignmentExpression.Right);
                }
                
                Chunk.CodeGenerator.Emit(OpCode.DUP);
                EmitVarSet(vre.Identifier);
            }
            else if (assignmentExpression.Left is IndexerExpression ie)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new CompilerException("Invalid assignment target.");
            }            
        }

        public override void Visit(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);

            Chunk.CodeGenerator.Emit(
                binaryExpression.Type switch
                {
                    BinaryOperationType.Add => OpCode.ADD,
                    BinaryOperationType.Subtract => OpCode.SUB,
                    BinaryOperationType.Divide => OpCode.DIV,
                    BinaryOperationType.Modulo => OpCode.MOD,
                    BinaryOperationType.Multiply => OpCode.MUL,
                    BinaryOperationType.BitwiseAnd => OpCode.BAND,
                    BinaryOperationType.BitwiseOr => OpCode.BOR,
                    BinaryOperationType.BitwiseXor => OpCode.BXOR,
                    BinaryOperationType.ShiftLeft => OpCode.SHL,
                    BinaryOperationType.ShiftRight => OpCode.SHR,
                    BinaryOperationType.ExistsIn => OpCode.EXISTS,
                    BinaryOperationType.LogicalAnd => OpCode.LAND,
                    BinaryOperationType.LogicalOr => OpCode.LOR,
                    BinaryOperationType.Equal => OpCode.CEQ,
                    BinaryOperationType.NotEqual => OpCode.CNE,
                    BinaryOperationType.Greater => OpCode.CGT,
                    BinaryOperationType.Less => OpCode.CLT,
                    BinaryOperationType.GreaterOrEqual => OpCode.CGE,
                    BinaryOperationType.LessOrEqual => OpCode.CLE,
                    _ => throw new CompilerException("Internal error: invalid binary operation type.")
                }
            );
        }

        public override void Visit(UnaryExpression unaryExpression)
        {
            Visit(unaryExpression.Right);

            Chunk.CodeGenerator.Emit(
                unaryExpression.Type switch
                {
                    UnaryOperationType.Minus => OpCode.ANEG,
                    UnaryOperationType.Length => OpCode.LENGTH,
                    UnaryOperationType.BitwiseNot => OpCode.BNOT,
                    UnaryOperationType.LogicalNot => OpCode.LNOT,
                    UnaryOperationType.TypeOf => OpCode.TYPE,
                    UnaryOperationType.ToString => OpCode.TOSTRING,
                    UnaryOperationType.ToNumber => OpCode.TONUMBER,
                    _ => throw new CompilerException("Internal error: invalid unary operation type.")
                }
            );
        }

        public override void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            EmitVarGet(variableReferenceExpression.Identifier);
        }

        public override void Visit(VariableDefinition variableDefinition)
        {
            foreach (var kvp in variableDefinition.Definitions)
            {
                var sym = _currentScope.DefineLocal(
                    kvp.Key,
                    Chunk.AllocateLocal()
                );

                if (kvp.Value != null)
                {
                    Visit(kvp.Value);

                    Chunk.CodeGenerator.Emit(
                        OpCode.SETLOCAL,
                        sym.Id
                    );
                }
            }
        }

        public override void Visit(FunctionDefinition functionDefinition)
        {
            InTopLevelChunk(() =>
            {
                InNewScopeDo(() =>
                {
                    for (var i = 0; i < functionDefinition.Parameters.Count; i++)
                    {
                        _currentScope.DefineParameter(
                            functionDefinition.Parameters[i],
                            Chunk.AllocateParameter()
                        );
                    }

                    Visit(functionDefinition.Statements);
                });

                if (Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
                {
                    if (opCode == OpCode.RET)
                        return;
                }

                /* Either we have no instructions in chunk, or it's not a RET. */
                Chunk.CodeGenerator.Emit(OpCode.RET);
            }, functionDefinition.Identifier);
        }

        public override void Visit(FunctionCallExpression functionCallExpression)
        {
            for (var i = 0; i < functionCallExpression.Arguments.Count; i++)
            {
                Visit(functionCallExpression.Arguments[i]);
            }

            Visit(functionCallExpression.Callee);
            Chunk.CodeGenerator.Emit(
                OpCode.INVOKE,
                functionCallExpression.Arguments.Count
            );
        }

        public override void Visit(IfStatement ifStatement)
        {
            var statementEnd = Chunk.CreateLabel();

            for (var i = 0; i < ifStatement.Conditions.Count; i++)
            {
                var caseEnd = Chunk.CreateLabel();

                Visit(ifStatement.Conditions[i]);
                Chunk.CodeGenerator.Emit(
                    OpCode.FJMP,
                    caseEnd
                );

                Visit(ifStatement.Statements[i]);
                Chunk.CodeGenerator.Emit(
                    OpCode.JUMP,
                    statementEnd
                );

                Chunk.UpdateLabel(caseEnd, Chunk.CodeGenerator.IP);
            }

            if (ifStatement.ElseBranch != null)
            {
                Visit(ifStatement.ElseBranch);
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.NOOP);
            }

            Chunk.UpdateLabel(
                statementEnd,
                Chunk.CodeGenerator.IP
            );
        }

        public override void Visit(ForStatement forStatement)
        {
            throw new NotImplementedException();
        }

        public override void Visit(DoWhileStatement doWhileStatement)
        {
            InNewLoopDo(() =>
            {
                Visit(doWhileStatement.Statement);
                
                Visit(doWhileStatement.Condition);
                Chunk.CodeGenerator.Emit(OpCode.TJMP, Loop.StartLabel);
                
                Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
            });
        }

        public override void Visit(WhileStatement whileStatement)
        {
            InNewLoopDo(() =>
            {
                Visit(whileStatement.Condition);
                Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);
            
                Visit(whileStatement.Statement);
                Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
            
                Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
            });
        }

        public override void Visit(BreakStatement breakStatement)
        {
            Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.EndLabel);
        }

        public override void Visit(SkipStatement skipStatement)
        {
            Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
        }

        public override void Visit(TableExpression tableExpression)
        {
            throw new NotImplementedException();
        }

        public override void Visit(IndexerExpression indexerExpression)
        {
            throw new NotImplementedException();
        }

        public override void Visit(IncrementationExpression incrementationExpression)
        {
            if (incrementationExpression.Target is VariableReferenceExpression vre)
            {
                EmitVarGet(vre.Identifier);
                if (incrementationExpression.IsPrefix)
                {
                    Chunk.CodeGenerator.Emit(OpCode.INC);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    EmitVarSet(vre.Identifier);
                }
                else
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.INC);
                    EmitVarSet(vre.Identifier);
                }
            }
            else if (incrementationExpression.Target is IndexerExpression ie)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new CompilerException("Illegal incrementation target.");
            }
        }

        public override void Visit(DecrementationExpression decrementationExpression)
        {
            if (decrementationExpression.Target is VariableReferenceExpression vre)
            {
                EmitVarGet(vre.Identifier);
                if (decrementationExpression.IsPrefix) 
                {
                    Chunk.CodeGenerator.Emit(OpCode.DEC);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    EmitVarSet(vre.Identifier);
                }
                else
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.DEC);
                    EmitVarSet(vre.Identifier);
                }
            }
            else if (decrementationExpression.Target is IndexerExpression ie)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new CompilerException("Illegal incrementation target.");
            }
        }

        public override void Visit(EachStatement eachStatement)
        {
            throw new NotImplementedException();
        }

        private void EmitVarGet(string identifier)
        {
            var sym = _currentScope.Find(identifier);

            if (sym != null)
            {
                if (sym.Type == Symbol.SymbolType.Local)
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.GETLOCAL,
                        (int)sym.Id
                    );
                }
                else
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.GETARG,
                        (int)sym.Id
                    );
                }
            }
            else
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)Chunk.StringPool.FetchOrAdd(identifier)
                );

                Chunk.CodeGenerator.Emit(OpCode.GETGLOBAL);
            }
        }

        private void EmitVarSet(string identifier)
        {
            var sym = _currentScope.Find(identifier);

            if (sym != null)
            {
                if (sym.Type == Symbol.SymbolType.Local)
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.SETLOCAL,
                        (int)sym.Id
                    );
                }
                else
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.SETARG,
                        (int)sym.Id
                    );
                }
            }
            else
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)Chunk.StringPool.FetchOrAdd(identifier)
                );

                Chunk.CodeGenerator.Emit(OpCode.SETGLOBAL);
            }
        }
    }
}