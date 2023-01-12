using System.Collections.Generic;
using System.Threading.Tasks;
using EVIL.Grammar.Traversal.Generic;
using EVIL.Grammar.Parsing;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;
using EVIL.Lexical;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter : AstVisitor<DynValue>
    {
        public string MainFilePath { get; private set; }

        public Environment Environment { get; set; } = new();

        public Lexer Lexer { get; private set; } = new();
        public Parser Parser { get; private set; }

        public Interpreter()
        {
        }

        public Interpreter(Environment env)
            => Environment = env;

        public void Execute(string sourceCode, bool preserveEnvironment = false)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer);

            var node = Parser.Parse();

            try
            {
                Environment.Begin();
                Visit(node);
            }
            finally
            {
                Environment.End();
                
                if (!preserveEnvironment)
                    Environment.Clear();
            }
        }

        public async Task ExecuteAsync(string sourceCode)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer);

            var node = Parser.Parse();

            try
            {
                Environment.Begin();
                await Task.Run(() => Visit(node));
            }
            finally
            {
                Environment.End();
                Environment.Clear();
            }
        }

        public void Execute(string sourceCode, string entryPoint, string[] args, string filePath = null)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer);

            var node = Parser.Parse();

            MainFilePath = filePath;

            try
            {
                Environment.Begin();
                Visit(node);
                Environment.End();
                
                var entryNode = node.FindChildFunctionDefinition(entryPoint);

                if (entryNode == null)
                {
                    throw new RuntimeException(
                        $"Entry point '{entryPoint}' missing.",
                        Environment,
                        null
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
                    else if (stackFrame.Parameters.Count > 1)
                    {
                        throw new RuntimeException(
                            "Entry point function can only have 1 argument.",
                            Environment,
                            entryNode.Line
                        );
                    }

                    Environment.Begin();
                    Environment.CallStack.Push(stackFrame);
                    Visit(entryNode.Statements);

                    if (Environment.CallStack.Count > 0)
                    {
                        Environment.CallStack.Pop();
                    }
                }
                Environment.ExitScope();
            }
            finally
            {
                Environment.End();
                Environment.Clear();
            }
        }

        public Task ExecuteAsync(string sourceCode, string entryPoint, string[] args)
        {
            return Task.Factory.StartNew(
                () => Execute(
                    sourceCode, entryPoint, args
                ),
                TaskCreationOptions.LongRunning
            );
        }
    }
}