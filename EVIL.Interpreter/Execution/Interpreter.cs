﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Grammar.Parsing;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter : AstVisitor
    {
        public List<Constraint> Constraints { get; } = new();
        public Environment Environment { get; set; }
        public Parser Parser { get; }

        public Interpreter()
        {
            Environment = new Environment();
            Parser = new Parser();
        }

        public Interpreter(Environment env)
            : this()
        {
            Environment = env;
        }

        public void ImposeConstraint(Constraint constraint)
            => Constraints.Add(constraint);

        public DynValue Execute(string sourceCode)
        {
            Parser.LoadSource(sourceCode);
            var node = Parser.Parse();

            try
            {
                return Visit(node);
            }
            catch (ExitStatementException)
            {
                Environment.Clear();
                return DynValue.Zero;
            }
        }

        public async Task<DynValue> ExecuteAsync(string sourceCode)
        {
            Parser.LoadSource(sourceCode);
            var node = Parser.Parse();

            try
            {
                return await Task.Run(() => Visit(node));
            }
            catch (ExitStatementException)
            {
                Environment.Clear();
                return DynValue.Zero;
            }
        }

        public DynValue Execute(string sourceCode, string entryPoint, string[] args)
        {
            Parser.LoadSource(sourceCode);
            var node = Parser.Parse();

            try
            {
                DynValue result;

                Visit(node);
                var entryNode = node.FindChildFunctionDefinition(entryPoint);

                if (entryNode == null)
                {
                    throw new RuntimeException($"Entry point '{entryPoint}' missing.", null);
                }

                var csi = new StackFrame(entryNode.Name);
                var scope = Environment.EnterScope(true);
                {
                    if (entryNode.ParameterNames.Count == 1)
                    {
                        var tbl = new Table();
                        for (var i = 0; i < args.Length; i++)
                        {
                            tbl[i] = new DynValue(args[i]);
                        }

                        scope.Set(entryNode.ParameterNames[0], new DynValue(tbl));
                    }
                    else if (entryNode.ParameterNames.Count > 1)
                    {
                        throw new RuntimeException("Entry point function can only have 1 argument.", entryNode.Line);
                    }

                    Environment.CallStack.Push(csi);
                    result = ExecuteStatementList(entryNode.StatementList);

                    if (Environment.CallStack.Count > 0)
                    {
                        Environment.CallStack.Pop();
                    }
                }
                Environment.ExitScope();

                return result;
            }
            catch (ExitStatementException)
            {
                Environment.Clear();
                return DynValue.Zero;
            }
        }

        public Task<DynValue> ExecuteAsync(string sourceCode, string entryPoint, string[] args)
        {
            return Task.Factory.StartNew(
                () => Execute(sourceCode, entryPoint, args
                ), TaskCreationOptions.LongRunning);
        }

        public override DynValue Visit(RootNode rootNode)
        {
            return ExecuteStatementList(rootNode.Children);
        }

        public DynValue ExecuteScriptFunction(string frameName, ScriptFunction function, FunctionArguments args)
        {
            var frame = new StackFrame(frameName);
            DynValue retval;
            
            Environment.EnterScope(true);
            {
                for (var i = 0; i < function.ParameterNames.Count; i++)
                {
                    if (i >= args.Count)
                        break;
                    
                    Environment.LocalScope.Set(function.ParameterNames[i], args[i]);
                }

                Environment.CallStack.Push(frame);
                retval = ExecuteStatementList(function.StatementList);
            }
            Environment.CallStack.Pop();
            Environment.ExitScope();

            return retval;
        }

        public void ExecuteClrFunction(ClrFunction func, FunctionArguments args)
        {
            func.Invokable?.Invoke(this, args);
        }

        private DynValue ExecuteStatementList(List<AstNode> statements, Environment env = null)
        {
            env = env ?? Environment;

            var retVal = DynValue.Zero;

            foreach (var statement in statements)
            {
                if (env.IsInsideLoop)
                {
                    var loopStackTop = env.LoopStackTop;

                    if (loopStackTop.BreakLoop || loopStackTop.SkipThisIteration)
                        break;
                }

                if (env.IsInScriptFunctionScope)
                {
                    var callStackTop = env.StackTop;

                    if (callStackTop.ReturnNow)
                    {
                        retVal = callStackTop.ReturnValue;
                        break;
                    }
                }

                retVal = Visit(statement);
            }

            return retVal;
        }

        protected override void ConstraintCheck(AstNode node)
        {
            for (var i = 0; i < Constraints.Count; i++)
            {
                var c = Constraints[i];

                if (!c.Check(this, node))
                {
                    throw new ConstraintUnsatisfiedException(
                        "An imposed execution constraint was unsatisfied.", c
                    );
                }
            }
        }
    }
}