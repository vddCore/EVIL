using System.Collections.Generic;
using EVIL.Grammar.Parsing;
using EVIL.Grammar.Traversal.Generic;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;
using EVIL.Lexical;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter : AstVisitor<DynValue>
    {
        public string MainFilePath { get; private set; }

        public int CallStackLimit { get; set; } = 72;

        public Stack<StackFrame> CallStack { get; } = new();
        public StackFrame StackTop => CallStack.Peek();

        public Environment Environment { get; set; }

        public Lexer Lexer { get; private set; } = new();
        public Parser Parser { get; private set; }

        public Interpreter()
            : this(new())
        {
        }

        public Interpreter(Environment env)
        {
            Environment = env;
        }

        public void Execute(string sourceCode, bool preserveEnvironment = false)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer, true);

            var node = Parser.Parse();

            try
            {
                Begin();
                Visit(node);
            }
            finally
            {
                End();

                if (!preserveEnvironment)
                    Environment.Clear();
            }
        }

        public DynValue Execute(string sourceCode, string entryPoint, string[] args, string filePath = null,
            bool preserveEnvironment = false)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer, false);

            var node = Parser.Parse();
            DynValue ret;

            MainFilePath = filePath;
            
            Begin();
            Visit(node);
            End();

            try
            {
                var entryNode = node.FindChildFunctionDefinition(entryPoint);

                if (entryNode == null)
                {
                    throw new RuntimeException(
                        $"Entry point '{entryPoint}' missing.",
                        this,
                        null
                    );
                }

                if (entryNode.Parameters.Count > 1)
                {
                    throw new RuntimeException(
                        "Entry point function can only have 1 argument.",
                        this,
                        entryNode.Line
                    );
                }

                var stackFrame = new StackFrame(entryNode.Identifier)
                {
                    DefinedAtLine = entryNode.Line
                };

                for (var i = 0; i < entryNode.Parameters.Count; i++)
                {
                    stackFrame.Parameters.Add(entryNode.Parameters[i]);
                }

                var scope = Environment.EnterScope();
                {
                    if (stackFrame.Parameters.Count == 1)
                    {
                        var tbl = new Table();
                        for (var i = 0; i < args.Length; i++)
                        {
                            tbl[i] = new DynValue(args[i]);
                        }

                        scope.Set(stackFrame.Parameters[0], new DynValue(tbl));
                    }

                    Begin();
                    CallStack.Push(stackFrame);
                    Visit(entryNode.Statements);
                    ret = CallStack.Peek().ReturnValue;
                    End();
                }
                Environment.ExitScope();
            }
            finally
            {
                if (!preserveEnvironment)
                    Environment.Clear();
            }

            return ret;
        }

        public DynValue ExecuteGlobalScriptFunction(string name, FunctionArguments args)
        {
            if (!Environment.GlobalScope.Members.TryGetValue(name, out var dynValue))
                throw new RuntimeException($"'{name}' is not a member in global scope.", this, null);

            if (dynValue.Type != DynValueType.Function)
                throw new RuntimeException($"'{name}' is not a script function.", this, null);

            var function = dynValue.ScriptFunction;
            DynValue retVal;

            var stackFrame = new StackFrame(name)
            {
                DefinedAtLine = function.DefinedAtLine
            };

            for (var i = 0; i < function.Parameters.Count; i++)
            {
                stackFrame.Parameters.Add(function.Parameters[i]);
            }

            var scope = Environment.EnterScope();
            {
                for (var i = 0; i < stackFrame.Parameters.Count; i++)
                {
                    if (i >= args.Count)
                        break;

                    scope.Set(stackFrame.Parameters[i], args[i].Copy());
                }

                CallStack.Push(stackFrame);
                {
                    Visit(function.Statements);
                    retVal = CallStack.Peek().ReturnValue;
                }
                CallStack.Pop();
            }
            Environment.ExitScope();

            return retVal;
        }

        private void Begin(string internalChunkName = "!root_chunk!")
        {
            CallStack.Push(new StackFrame(internalChunkName));
        }

        private void End()
        {
            CallStack.Pop();
        }

        public List<StackFrame> StackTrace()
            => new(CallStack);
    }
}