using System.Collections.Generic;
using System.Threading.Tasks;
using EVIL.Grammar.AST;
using EVIL.Grammar.Parsing;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;
using EVIL.Lexical;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter : AstVisitor
    {
        private List<Constraint> _constraints = new();

        public string MainFilePath { get; private set; }
        
        public IReadOnlyList<Constraint> Constraints => _constraints;
        public Environment Environment { get; set; } = new();

        public Lexer Lexer { get; private set; } = new();
        public Parser Parser { get; private set; }

        public Interpreter()
        {
        }

        public Interpreter(Environment env)
        {
            Environment = env;
        }

        public void ImposeConstraint(Constraint constraint)
            => _constraints.Add(constraint);

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

        public DynValue Execute(string sourceCode, string entryPoint, string[] args, bool restrictTopLevelCode = false, string filePath = null)
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

                var csi = new StackFrame(entryNode.Identifier, entryNode.ParameterNames)
                {
                    DefinedAtLine = entryNode.Line
                };

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
                        throw new RuntimeException(
                            "Entry point function can only have 1 argument.",
                            Environment,
                            entryNode.Line
                        );
                    }

                    Environment.CallStack.Push(csi);
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
            var frame = new StackFrame(frameName, function.ParameterNames);
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
                retval = Visit(function.Statements);
            }
            Environment.CallStack.Pop();
            Environment.ExitScope();

            return retval;
        }

        protected override void ConstraintCheck(AstNode node)
        {
            for (var i = 0; i < Constraints.Count; i++)
            {
                var constraint = Constraints[i];

                if (!constraint.Check(this, node))
                {
                    throw new ConstraintUnsatisfiedException(
                        "An imposed execution constraint was unsatisfied.",
                        constraint
                    );
                }
            }
        }
    }
}