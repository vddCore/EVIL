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
        private readonly Stack<StackFrame> _preCallStack = new();
        private readonly Stack<FunctionArguments> _argumentStack = new();

        public string MainFilePath { get; private set; }

        public Environment Environment { get; set; } = new();

        public Lexer Lexer { get; private set; } = new();
        public Parser Parser { get; private set; }

        public Interpreter()
        {
        }

        public Interpreter(Environment env)
            => Environment = env;

        public DynValue Execute(string sourceCode)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer);

            var node = Parser.Parse();

            try
            {
                return Visit(node);
            }
            catch (ExitStatementException)
            {
                Environment.Clear();
                throw;
            }
        }

        public async Task<DynValue> ExecuteAsync(string sourceCode)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer);

            var node = Parser.Parse();

            try
            {
                return await Task.Run(() => Visit(node));
            }
            catch (ExitStatementException)
            {
                Environment.Clear();
                throw;
            }
        }

        public DynValue Execute(string sourceCode, string entryPoint, string[] args, bool restrictTopLevelCode = false,
            string filePath = null)
        {
            Lexer.LoadSource(sourceCode);
            Parser = new Parser(Lexer);

            var node = Parser.Parse();

            MainFilePath = filePath;

            try
            {
                DynValue result;

                Visit(node);
                var entryNode = node.FindChildFunctionDefinition(entryPoint);

                if (entryNode == null)
                {
                    throw new RuntimeException(
                        $"Entry point '{entryPoint}' missing.",
                        Environment,
                        null
                    );
                }

                _preCallStack.Push(new StackFrame(entryNode.Identifier)
                {
                    DefinedAtLine = entryNode.Line
                });
                var preCallStackFrame = _preCallStack.Peek();

                Visit(entryNode.Parameters);

                var scope = Environment.EnterScope(true);
                {
                    if (preCallStackFrame.Parameters.Count == 1)
                    {
                        var tbl = new Table();
                        for (var i = 0; i < args.Length; i++)
                        {
                            tbl[i] = new DynValue(args[i]);
                        }

                        scope.Set(preCallStackFrame.Parameters[0], new DynValue(tbl));
                    }
                    else if (preCallStackFrame.Parameters.Count > 1)
                    {
                        throw new RuntimeException(
                            "Entry point function can only have 1 argument.",
                            Environment,
                            entryNode.Line
                        );
                    }

                    Environment.CallStack.Push(_preCallStack.Pop());
                    result = Visit(entryNode.Statements);

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
                throw;
            }
        }

        public Task<DynValue> ExecuteAsync(string sourceCode, string entryPoint, string[] args)
        {
            return Task.Factory.StartNew(
                () => Execute(
                    sourceCode, entryPoint, args
                ),
                TaskCreationOptions.LongRunning
            );
        }

        public DynValue ExecuteScriptFunction(string frameName, ScriptFunction function, FunctionArguments args)
        {
            _preCallStack.Push(new StackFrame(frameName));
            var preCallStackFrame = _preCallStack.Peek();

            Visit(function.Parameters);
            DynValue retval;

            Environment.EnterScope(true);
            {
                for (var i = 0; i < preCallStackFrame.Parameters.Count; i++)
                {
                    if (i >= args.Count)
                        break;

                    Environment.LocalScope.Set(preCallStackFrame.Parameters[i], args[i]);
                }

                Environment.CallStack.Push(preCallStackFrame);
                retval = Visit(function.Statements);
            }
            Environment.CallStack.Pop();
            Environment.ExitScope();

            return retval;
        }
    }
}