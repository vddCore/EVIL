﻿using System.Collections.Generic;
using System.Threading.Tasks;
using EVIL.Abstraction;
using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;
using EVIL.Parsing;

namespace EVIL.Execution
{
    public partial class Interpreter : AstVisitor
    {
        public bool BreakExecution { get; set; }
        public Environment Environment { get; set; }

        public IMemory Memory { get; set; }
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
                var scope = Environment.EnterScope();
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
                    else if(entryNode.ParameterNames.Count > 1)
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
            return new(() => Execute(sourceCode, entryPoint, args));
        }

        public override DynValue Visit(RootNode rootNode)
        {
            return ExecuteStatementList(rootNode.Children);
        }

        private DynValue ExecuteStatementList(List<AstNode> statements)
        {
            var retVal = DynValue.Zero;

            if (BreakExecution)
            {
                BreakExecution = false;
                throw new ProgramTerminationException("Execution stopped by user.");
            }

            foreach (var statement in statements)
            {
                if (BreakExecution)
                {
                    BreakExecution = false;
                    throw new ProgramTerminationException("Execution stopped by user.");
                }

                if (Environment.IsInsideLoop)
                {
                    var loopStackTop = Environment.LoopStackTop;

                    if (loopStackTop.BreakLoop || loopStackTop.SkipThisIteration)
                        break;
                }

                if (Environment.IsInScriptFunctionScope)
                {
                    var callStackTop = Environment.StackTop;

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
    }
}