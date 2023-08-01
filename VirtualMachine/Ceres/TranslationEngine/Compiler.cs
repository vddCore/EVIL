using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.AST.Statements.TopLevel;
using EVIL.Grammar.Parsing;
using EVIL.Grammar.Traversal;
using EVIL.Lexical;
using static Ceres.TranslationEngine.Loop;

namespace Ceres.TranslationEngine
{
    public partial class Compiler : AstVisitor
    {
        private Script _script = new();

#nullable disable
        private Scope _rootScope;
        private Scope _currentScope;
#nullable enable

        private readonly Stack<Chunk> _chunks = new();
        private Dictionary<string, List<AttributeProcessor>> _attributeProcessors = new();

        private int _blockDescent;
        private readonly Stack<Loop> _loopDescent = new();

        private Chunk Chunk => _chunks.Peek();
        private Loop Loop => _loopDescent.Peek();
        private bool InsideLoop => _loopDescent.Any();
        
        private int Line { get; set; }
        private int Column { get; set; }

        private string CurrentFileName { get; set; } = string.Empty;

        public CompilerLog Log { get; } = new();

        public Script Compile(string source, string fileName = "")
        {
            CurrentFileName = fileName;

            var parser = new Parser();

            try
            {
                var program = parser.Parse(source);
                return Compile(program, fileName);
            }
            catch (LexerException le)
            {
                Log.TerminateWithFatal(
                    le.Message,
                    CurrentFileName,
                    EvilMessageCode.LexerError,
                    line: Line,
                    column: Column,
                    innerException: le
                );
            }
            catch (ParserException pe)
            {
                Log.TerminateWithFatal(
                    pe.Message,
                    CurrentFileName,
                    EvilMessageCode.ParserError,
                    line: pe.Line,
                    column: pe.Column,
                    innerException: pe
                );
            }

            // Dummy return to keep compiler happy.
            return null!;
        }

        public Script Compile(Program program, string fileName = "")
        {
            CurrentFileName = fileName;

            Visit(program);
            return _script;
        }

        private void AddCurrentLocationToDebugDatabase()
        {
            if (_blockDescent > 0 && _chunks.Any())
            {
                Chunk.DebugDatabase.AddDebugRecord(
                    Line,
                    Chunk.CodeGenerator.IP
                );
            }
        }

        private void InTopLevelChunk(Action action, string? name = null)
        {
            var chunk = new Chunk { Name = name };
            chunk.DebugDatabase.DefinedInFile = CurrentFileName;

            _chunks.Push(chunk);
            {
                action();
            }

            _script.Chunks.Add(chunk);
            _chunks.Pop();
        }

        private void InNewScopeDo(Action action)
        {
            _currentScope = _currentScope!.Descend();
            {
                action();
            }
            _currentScope = _currentScope.Parent!;
        }

        private void InNewLoopDo(LoopKind kind, Action action, bool needsExtraLabel)
        {
            _loopDescent.Push(new Loop(Chunk, kind, needsExtraLabel));
            {
                action();
            }
            _loopDescent.Pop();
        }

        public override void Visit(BlockStatement blockStatement)
        {
            InNewScopeDo(() =>
            {
                _blockDescent++;
                {
                    foreach (var node in blockStatement.Statements)
                    {
                        Visit(node);
                    }
                }
                _blockDescent--;
            });
        }

        public override void Visit(AstNode node)
        {
            Line = node.Line;
            Column = node.Column;

            AddCurrentLocationToDebugDatabase();
            base.Visit(node);
            AddCurrentLocationToDebugDatabase();
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

        public override void Visit(ParameterList parameterList)
        {
            for (var i = 0; i < parameterList.Parameters.Count; i++)
            {
                var parameter = parameterList.Parameters[i];
                var parameterId = Chunk.AllocateParameter();

                try
                {
                    _currentScope.DefineParameter(
                        parameter.Identifier.Name,
                        parameterId,
                        parameter.ReadWrite,
                        parameter.Line,
                        parameter.Column
                    );

                    Chunk.DebugDatabase.SetParameterName(
                        parameterId,
                        parameter.Identifier.Name,
                        parameter.ReadWrite
                    );
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        dse.Message,
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        Line,
                        Column,
                        dse
                    );
                }

                if (parameter.Initializer != null)
                {
                    Chunk.InitializeParameter(
                        parameterId,
                        ExtractConstantValueFrom(parameter.Initializer)
                    );
                }
            }
        }

        public override void Visit(ArgumentList argumentList)
        {
            for (var i = 0; i < argumentList.Arguments.Count; i++)
            {
                Visit(argumentList.Arguments[i]);
            }
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

        public override void Visit(AttributeNode attributeNode)
        {
            var attribute = new ChunkAttribute(attributeNode.Identifier.Name);

            foreach (var valueNode in attributeNode.Values)
            {
                attribute.Values.Add(
                    ExtractConstantValueFrom(valueNode)
                );
            }

            foreach (var propertyKvp in attributeNode.Properties)
            {
                attribute.Properties.Add(
                    propertyKvp.Key.Name,
                    ExtractConstantValueFrom(propertyKvp.Value)
                );
            }

            Chunk.AddAttribute(attribute);

            if (_attributeProcessors.TryGetValue(attribute.Name, out var processors))
            {
                foreach (var processor in processors)
                {
                    try
                    {
                        processor.Invoke(attribute, Chunk);
                    }
                    catch (Exception e)
                    {
                        throw new AttributeProcessorException(attribute, Chunk, e);
                    }
                }
            }
        }

        public override void Visit(ReturnStatement returnStatement)
        {
            if (returnStatement.Expression != null)
            {
                Visit(returnStatement.Expression);

                if (Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
                {
                    if (opCode == OpCode.TAILINVOKE)
                        return;
                }
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.LDNIL);
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
                ThrowIfVarReadOnly(vre.Identifier);

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
                            _ => Log.TerminateWithInternalFailure(
                                $"Invalid assignment operation type '{assignmentExpression.OperationType}'.",
                                CurrentFileName,
                                line: Line,
                                column: Column,
                                dummyReturn: OpCode.NOOP
                            )
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
                if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
                {
                    Visit(ie);
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
                            _ => Log.TerminateWithInternalFailure(
                                $"Invalid assignment operation type '{assignmentExpression.OperationType}'.",
                                CurrentFileName,
                                line: Line,
                                column: Column,
                                dummyReturn: OpCode.NOOP
                            )
                        }
                    );
                }
                else
                {
                    Visit(assignmentExpression.Right);
                }

                Chunk.CodeGenerator.Emit(OpCode.DUP);

                Visit(ie.Indexable);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.TABSET);
            }
            else
            {
                Log.TerminateWithFatal(
                    "Illegal assignment target.",
                    CurrentFileName,
                    EvilMessageCode.IllegalAssignmentTarget,
                    Line,
                    Column
                );
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
                    BinaryOperationType.DeepEqual => OpCode.DEQ,
                    BinaryOperationType.DeepNotEqual => OpCode.DNE,
                    BinaryOperationType.Equal => OpCode.CEQ,
                    BinaryOperationType.NotEqual => OpCode.CNE,
                    BinaryOperationType.Greater => OpCode.CGT,
                    BinaryOperationType.Less => OpCode.CLT,
                    BinaryOperationType.GreaterOrEqual => OpCode.CGE,
                    BinaryOperationType.LessOrEqual => OpCode.CLE,
                    _ => Log.TerminateWithInternalFailure(
                        $"Invalid binary operation type '{binaryExpression.Type}'.",
                        CurrentFileName,
                        line: Line,
                        column: Column,
                        dummyReturn: OpCode.NOOP
                    )
                }
            );
        }

        public override void Visit(TypeOfExpression typeOfExpression)
        {
            Visit(typeOfExpression.Target);
            Chunk.CodeGenerator.Emit(OpCode.TYPE);
        }

        public override void Visit(YieldExpression yieldExpression)
        {
            Visit(yieldExpression.ArgumentList);
            Visit(yieldExpression.Target);

            Chunk.CodeGenerator.Emit(
                OpCode.YIELD,
                yieldExpression.ArgumentList.Arguments.Count
            );

            Chunk.CodeGenerator.Emit(OpCode.YRET);
        }

        public override void Visit(ExpressionBodyStatement expressionBodyStatement)
        {
            _blockDescent++;
            Visit(expressionBodyStatement.Expression);
            Chunk.CodeGenerator.Emit(OpCode.RET);
            _blockDescent--;
        }

        public override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
        {
            Chunk.CodeGenerator.Emit(OpCode.XARGS);
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
                    UnaryOperationType.ToString => OpCode.TOSTRING,
                    UnaryOperationType.ToNumber => OpCode.TONUMBER,
                    _ => Log.TerminateWithInternalFailure(
                        $"Invalid unary operation type '{unaryExpression.Type}'.",
                        CurrentFileName,
                        line: Line,
                        column: Column,
                        dummyReturn: OpCode.NOOP
                    )
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
                Symbol sym;
                try
                {
                    var localId = Chunk.AllocateLocal();

                    sym = _currentScope.DefineLocal(
                        kvp.Key.Name,
                        localId,
                        variableDefinition.ReadWrite,
                        variableDefinition.Line,
                        variableDefinition.Column
                    );

                    Chunk.DebugDatabase.SetLocalName(localId, kvp.Key.Name, variableDefinition.ReadWrite);
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        dse.Message,
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        Line,
                        Column,
                        dse
                    );

                    // Dummy return to keep compiler happy.
                    return;
                }

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
                Chunk.DebugDatabase.DefinedOnLine = functionDefinition.Line;

                InNewScopeDo(() =>
                {
                    Visit(functionDefinition.ParameterList);
                    Visit(functionDefinition.Statement);
                });

                if (Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
                {
                    if (opCode == OpCode.RET || opCode == OpCode.TAILINVOKE)
                    {
                        foreach (var attr in functionDefinition.Attributes)
                            Visit(attr);

                        return;
                    }
                }

                /* Either we have no instructions in chunk, or it's not a RET. */
                Chunk.CodeGenerator.Emit(OpCode.LDNIL);
                Chunk.CodeGenerator.Emit(OpCode.RET);

                foreach (var attr in functionDefinition.Attributes)
                    Visit(attr);
            }, functionDefinition.Identifier.Name);
        }

        public override void Visit(FunctionCallExpression functionCallExpression)
        {
            Visit(functionCallExpression.ArgumentList);

            if (functionCallExpression.Parent is ReturnStatement
                && functionCallExpression.Callee is VariableReferenceExpression varRef
                && varRef.Identifier == Chunk.Name)
            {
                Chunk.CodeGenerator.Emit(OpCode.TAILINVOKE);
            }
            else
            {
                Visit(functionCallExpression.Callee);
                Chunk.CodeGenerator.Emit(
                    OpCode.INVOKE,
                    functionCallExpression.ArgumentList.Arguments.Count
                );
            }
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
            InNewScopeDo(() =>
            {
                foreach (var statement in forStatement.Assignments)
                    Visit(statement);

                InNewLoopDo(LoopKind.For, () =>
                {
                    Visit(forStatement.Condition);
                    Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);

                    Visit(forStatement.Statement);
                    Chunk.UpdateLabel(Loop.StartLabel, Chunk.CodeGenerator.IP);

                    foreach (var iterationStatement in forStatement.IterationStatements)
                        Visit(iterationStatement);

                    Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.ExtraLabel);
                    Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
                }, true);
            });
        }

        public override void Visit(EachStatement eachStatement)
        {
            InNewScopeDo(() =>
            {
                int keyLocal = Chunk.AllocateLocal();
                int valueLocal = -1;
                var isKeyValue = false;
                
                _currentScope.DefineLocal(
                    eachStatement.KeyIdentifier.Name,
                    keyLocal,
                    false,
                    eachStatement.KeyIdentifier.Line,
                    eachStatement.KeyIdentifier.Column
                );

                if (eachStatement.ValueIdentifier != null)
                {
                    valueLocal = Chunk.AllocateLocal();

                    _currentScope.DefineLocal(
                        eachStatement.ValueIdentifier.Name,
                        valueLocal,
                        false,
                        eachStatement.ValueIdentifier.Line,
                        eachStatement.ValueIdentifier.Column
                    );

                    isKeyValue = true;
                }

                Visit(eachStatement.Iterable);
                Chunk.CodeGenerator.Emit(OpCode.EACH);
                
                InNewLoopDo(LoopKind.Each, () =>
                {
                    Chunk.UpdateLabel(Loop.StartLabel, Chunk.CodeGenerator.IP);
                    Chunk.CodeGenerator.Emit(OpCode.NEXT, isKeyValue ? 1 : 0);
                    Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);
                    Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, keyLocal);

                    if (isKeyValue)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, valueLocal);
                    }

                    Visit(eachStatement.Statement);
                    Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
                    Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
                    Chunk.CodeGenerator.Emit(OpCode.EEND);
                }, false);
            });
        }

        public override void Visit(DoWhileStatement doWhileStatement)
        {
            InNewLoopDo(LoopKind.DoWhile, () =>
            {
                Visit(doWhileStatement.Statement);

                Visit(doWhileStatement.Condition);
                Chunk.CodeGenerator.Emit(OpCode.TJMP, Loop.StartLabel);

                Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
            }, false);
        }

        public override void Visit(WhileStatement whileStatement)
        {
            InNewLoopDo(LoopKind.While, () =>
            {
                Visit(whileStatement.Condition);
                Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);

                Visit(whileStatement.Statement);
                Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);

                Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
            }, false);
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
            Chunk.CodeGenerator.Emit(OpCode.TABNEW);

            if (tableExpression.Keyed)
            {
                foreach (var expr in tableExpression.Initializers)
                {
                    var kvpe = (KeyValuePairExpression)expr;
                    Visit(kvpe.ValueNode);
                    Visit(kvpe.KeyNode);
                    Chunk.CodeGenerator.Emit(OpCode.TABINIT);
                }
            }
            else
            {
                for (var i = 0; i < tableExpression.Initializers.Count; i++)
                {
                    Visit(tableExpression.Initializers[i]);

                    if (i == 0)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.LDZERO);
                    }
                    else if (i == 1)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.LDONE);
                    }
                    else
                    {
                        Chunk.CodeGenerator.Emit(OpCode.LDNUM, (double)i);
                    }

                    Chunk.CodeGenerator.Emit(OpCode.TABINIT);
                }
            }
        }

        public override void Visit(IndexerExpression indexerExpression)
        {
            Visit(indexerExpression.Indexable);
            Visit(indexerExpression.KeyExpression);
            Chunk.CodeGenerator.Emit(OpCode.INDEX);
        }

        public override void Visit(IncrementationExpression incrementationExpression)
        {
            if (incrementationExpression.Target is VariableReferenceExpression vre)
            {
                ThrowIfVarReadOnly(vre.Identifier);

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
                Visit(ie);

                if (incrementationExpression.IsPrefix)
                {
                    Chunk.CodeGenerator.Emit(OpCode.INC);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }
                else
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.INC);
                }

                Visit(ie.Indexable);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.TABSET);
            }
            else
            {
                Log.TerminateWithFatal(
                    "Illegal incrementation target.",
                    CurrentFileName,
                    EvilMessageCode.IllegalIncrementationTarget,
                    Line,
                    Column
                );
            }
        }

        public override void Visit(DecrementationExpression decrementationExpression)
        {
            if (decrementationExpression.Target is VariableReferenceExpression vre)
            {
                ThrowIfVarReadOnly(vre.Identifier);

                EmitVarGet(vre.Identifier);
                {
                    if (decrementationExpression.IsPrefix)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.DEC);
                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                    }
                    else
                    {
                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                        Chunk.CodeGenerator.Emit(OpCode.DEC);
                    }
                }
                EmitVarSet(vre.Identifier);
            }
            else if (decrementationExpression.Target is IndexerExpression ie)
            {
                Visit(ie);

                if (decrementationExpression.IsPrefix)
                {
                    Chunk.CodeGenerator.Emit(OpCode.DEC);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }
                else
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.DEC);
                }

                Visit(ie.Indexable);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.TABSET);
            }
            else
            {
                Log.TerminateWithFatal(
                    "Illegal decrementation target.",
                    CurrentFileName,
                    EvilMessageCode.IllegalDecrementationTarget,
                    Line,
                    Column
                );
            }
        }

        private DynamicValue ExtractConstantValueFrom(AstNode valueNode)
        {
            if (valueNode is BooleanConstant boolConst)
            {
                return boolConst.Value;
            }
            else if (valueNode is NilConstant)
            {
                return DynamicValue.Nil;
            }
            else if (valueNode is NumberConstant numConst)
            {
                return numConst.Value;
            }
            else if (valueNode is StringConstant stringConst)
            {
                return stringConst.Value;
            }
            else
            {
                Log.TerminateWithInternalFailure(
                    $"Unexpected constant value node type '{valueNode.GetType().FullName}'.",
                    CurrentFileName,
                    line: valueNode.Line,
                    column: valueNode.Column,
                    dummyReturn: DynamicValue.Zero
                );

                // Dummy return to keep compiler happy.
                return DynamicValue.Zero;
            }
        }
    }
}