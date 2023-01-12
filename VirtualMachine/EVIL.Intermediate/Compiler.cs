using System.Collections.Generic;
using EVIL.Grammar;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Grammar.Traversal;

namespace EVIL.Intermediate
{
    public class Compiler : AstVisitor
    {
        private Executable _executable;

        private int _currentLocalFunctionIndex = 0;
        
        private Stack<Scope> ScopeStack { get; } = new();
        private Stack<Chunk> ChunkDefinitionStack { get; } = new();
        
        private Chunk CurrentChunk => ChunkDefinitionStack.Peek();

        public Executable Compile(Program program)
        {
            ChunkDefinitionStack.Clear();
            ScopeStack.Clear();
            
            _executable = new Executable();
            ChunkDefinitionStack.Push(_executable.MainChunk);
            {
                Visit(program);
            }

            _executable.MainChunk.GetCodeGenerator()
                                 .Emit(OpCode.HLT);
            
            ChunkDefinitionStack.Pop();
            return _executable;
        }

        public override void Visit(Program program)
        {
            foreach (var node in program.Statements)
                Visit(node);
        }

        public override void Visit(BlockStatement blockStatement)
        {
            EnterScope();
            {
                foreach (var node in blockStatement.Statements)
                    Visit(node);
            }
            LeaveScope();
        }

        public override void Visit(ConditionalExpression conditionalExpression)
        {
        }

        public override void Visit(NumberExpression numberExpression)
        {
            var constId = _executable.ConstPool.FetchOrAddConstant(numberExpression.Value);
            
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.LDCONST, constId);
        }

        public override void Visit(StringConstant stringConstant)
        {
            var constId = _executable.ConstPool.FetchOrAddConstant(stringConstant.Value);

            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.LDCONST, constId);
        }

        private void AssingmentToVariable(AssignmentExpression assignmentExpression)
        {
            Visit(assignmentExpression.Left);
            Visit(assignmentExpression.Right);

            switch (assignmentExpression.OperationType)
            {
                case AssignmentOperationType.Direct:
                    break;
            }
        }

        public override void Visit(AssignmentExpression assignmentExpression)
        {
            if (assignmentExpression.Left is VariableReferenceExpression)
            {
                AssingmentToVariable(assignmentExpression);
            }
        }

        public override void Visit(BinaryExpression binaryExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);

            switch (binaryExpression.Type)
            {
                case BinaryOperationType.Plus:
                    cg.Emit(OpCode.ADD);
                    break;

                case BinaryOperationType.Minus:
                    cg.Emit(OpCode.SUB);
                    break;

                case BinaryOperationType.Divide:
                    cg.Emit(OpCode.DIV);
                    break;

                case BinaryOperationType.Multiply:
                    cg.Emit(OpCode.MUL);
                    break;

                case BinaryOperationType.Modulo:
                    cg.Emit(OpCode.MOD);
                    break;

                case BinaryOperationType.BitwiseAnd:
                    cg.Emit(OpCode.AND);
                    break;

                case BinaryOperationType.BitwiseOr:
                    cg.Emit(OpCode.OR);
                    break;

                case BinaryOperationType.BitwiseXor:
                    cg.Emit(OpCode.XOR);
                    break;

                case BinaryOperationType.Less:
                    cg.Emit(OpCode.CLT);
                    break;

                case BinaryOperationType.Greater:
                    cg.Emit(OpCode.CGT);
                    break;

                case BinaryOperationType.LessOrEqual:
                    cg.Emit(OpCode.CLE);
                    break;

                case BinaryOperationType.GreaterOrEqual:
                    cg.Emit(OpCode.CGE);
                    break;

                case BinaryOperationType.Equal:
                    cg.Emit(OpCode.CEQ);
                    break;

                case BinaryOperationType.NotEqual:
                    cg.Emit(OpCode.CNE);
                    break;

                case BinaryOperationType.LogicalOr:
                    cg.Emit(OpCode.LOR);
                    break;

                case BinaryOperationType.LogicalAnd:
                    cg.Emit(OpCode.LAND);
                    break;

                case BinaryOperationType.ShiftRight:
                    cg.Emit(OpCode.SHR);
                    break;

                case BinaryOperationType.ShiftLeft:
                    cg.Emit(OpCode.SHL);
                    break;

                case BinaryOperationType.ExistsIn:
                    cg.Emit(OpCode.XINT);
                    break;

                default:
                    throw new CompilerException(
                        $"Unrecognized binary operation type '{binaryExpression.Type}'",
                        binaryExpression.Line
                    );
            }
        }

        public override void Visit(UnaryExpression unaryExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            
            Visit(unaryExpression.Right);
            
            switch (unaryExpression.Type)
            {
                case UnaryOperationType.Minus:
                    cg.Emit(OpCode.UNM);
                    break;
                
                case UnaryOperationType.Length:
                    cg.Emit(OpCode.LEN);
                    break;
                
                case UnaryOperationType.BitwiseNot:
                    cg.Emit(OpCode.NOT);

                    break;
                
                case UnaryOperationType.Plus:
                    break;
                
                case UnaryOperationType.Negation:
                    cg.Emit(OpCode.LNOT);
                    break;
                
                case UnaryOperationType.ToString:
                    cg.Emit(OpCode.TOSTR);
                    break;
                
                case UnaryOperationType.ToNumber:
                    cg.Emit(OpCode.TONUM);
                    break;
                
                default:
                    throw new CompilerException(
                        $"Unrecognized unary operation type '{unaryExpression.Type}'",
                        unaryExpression.Line
                    );
            }
        }

        public override void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            
            Scope localScope = null;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            if (localScope != null)
            {
                var sym = localScope.Find(variableReferenceExpression.Identifier);

                if (sym == SymbolInfo.Undefined)
                {
                    throw new CompilerException(
                        $"Unknown symbol '{variableReferenceExpression}'.",
                        variableReferenceExpression.Line
                    );
                }

                if (sym.Type == SymbolInfo.SymbolType.Local)
                {
                    cg.Emit(OpCode.LDLOCAL, sym.Id);
                }
                else if (sym.Type == SymbolInfo.SymbolType.Parameter)
                {
                    cg.Emit(OpCode.LDARG, sym.Id);
                }
            }
            else
            {
                cg.Emit(OpCode.LDCONST, _executable.ConstPool.FetchOrAddConstant(variableReferenceExpression.Identifier));
                cg.Emit(OpCode.LDGLOBAL);
            }
        }

        public override void Visit(VariableDefinition variableDefinition)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            
            Scope localScope = null;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }
            
            foreach (var kvp in variableDefinition.Definitions)
            {
                if (localScope != null)
                {
                    var sym = localScope.DefineLocal(kvp.Key);
                    cg.Emit(OpCode.LDLOCAL, sym.Id);
                    Visit(kvp.Value);
                    cg.Emit(OpCode.STLOCAL);
                }
                else
                {
                    var id = _executable.ConstPool.FetchOrAddConstant(kvp.Key);
                    cg.Emit(OpCode.LDCONST, id);

                    Visit(kvp.Value);
                    cg.Emit(OpCode.STGLOBAL);
                }
            }
        }

        public override void Visit(FunctionDefinition functionDefinition)
        {
            var chunk = new Chunk(functionDefinition.Identifier);
            ChunkDefinitionStack.Push(chunk);

            chunk.ParameterCount = functionDefinition.Parameters.Count;
            var cg = CurrentChunk.GetCodeGenerator();
            
            EnterScope();
            {
                var localScope = ScopeStack.Peek();
                
                for (var i = 0; i < chunk.ParameterCount; i++)
                {
                    var param = functionDefinition.Parameters[i];
                    localScope.DefineLocal(param);
                    
                    cg.Emit(OpCode.STARG, chunk.ParameterCount - i - 1);
                }

                foreach (var stmt in functionDefinition.Statements.Statements)
                {
                    Visit(stmt);
                }
                
                cg.Emit(OpCode.RETN);
            }
            LeaveScope();
            _executable.Chunks.Add(ChunkDefinitionStack.Pop());
        }

        public override void Visit(FunctionExpression functionExpression)
        {
        }

        public override void Visit(FunctionCallExpression functionCallExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            
            foreach (var expr in functionCallExpression.Arguments)
                Visit(expr);

            Visit(functionCallExpression.Callee);
            cg.Emit(OpCode.CALL);
        }

        public override void Visit(IfStatement ifStatement)
        {
        }

        public override void Visit(ExitStatement exitStatement)
        {
        }

        public override void Visit(ForStatement forStatement)
        {
        }

        public override void Visit(DoWhileStatement doWhileStatement)
        {
        }

        public override void Visit(WhileStatement whileStatement)
        {
        }

        public override void Visit(ReturnStatement returnStatement)
        {
        }

        public override void Visit(BreakStatement breakStatement)
        {
        }

        public override void Visit(SkipStatement nextStatement)
        {
        }

        public override void Visit(TableExpression tableExpression)
        {
        }

        public override void Visit(IndexerExpression indexerExpression)
        {
        }

        public override void Visit(IncrementationExpression incrementationExpression)
        {
        }

        public override void Visit(DecrementationExpression decrementationExpression)
        {
        }

        public override void Visit(UndefStatement undefStatement)
        {
        }

        public override void Visit(EachStatement eachStatement)
        {
        }

        public override void Visit(NameOfExpression nameOfExpression)
        {
        }

        public override void Visit(ExpressionStatement expressionStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            Visit(expressionStatement.Expression);
            // cg.Emit(OpCode.POP);
        }

        public override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
        {
        }

        private void EnterScope()
        {
            Scope localScope = null;

            if (ScopeStack.Count > 0)
                localScope = ScopeStack.Peek();
            
            var scope = new Scope(CurrentChunk, localScope);
            ScopeStack.Push(scope);
        }

        private void LeaveScope()
        {
            if (ScopeStack.Count == 0)
            {
                throw new CompilerException("Cannot leave global scope.");
            }

            ScopeStack.Pop();
        }
    }
}