using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;
using EVIL.Grammar.Parsing;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Lexical;

namespace EVIL.ExecutionEngine
{
    public static class Extensions
    {
        private static Lexer _lexer = new();
        private static Parser _parser = new(_lexer);
        private static Compiler _compiler = new();
        
        public static string Alias(this DynamicValueType dvt)
            => dvt.ToString().ToLowerInvariant();

        public static Table Export(this Executable executable)
        {
            var table = new Table();
            
            foreach (var chunk in executable.Chunks)
            {
                if (chunk.IsRoot)
                    continue;

                table[chunk.Name] = new DynamicValue(chunk);
            }

            return table;
        }

        public static double Set(this Table table, string key, double value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static double Set(this Table table, double key, double value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static string Set(this Table table, string key, string value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static string Set(this Table table, double key, string value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static Table Set(this Table table, string key, Table value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static Table Set(this Table table, double key, Table value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static Chunk Set(this Table table, string key, Chunk value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static Chunk Set(this Table table, double key, Chunk value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static ClrFunction Set(this Table table, string key, ClrFunction value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }
        
        public static ClrFunction Set(this Table table, double key, ClrFunction value, bool readOnly = false)
        {
            table.Set(key, new(value) { IsReadOnly = readOnly });
            return value;
        }

        public static double Add(this Table table, double value)
        {
            table.Add(new(value));
            return value;
        }
        
        public static string Add(this Table table, string value)
        {
            table.Add(new(value));
            return value;
        }
        
        public static Table Add(this Table table, Table value)
        {
            table.Add(new(value));
            return value;
        }
        
        public static Chunk Add(this Table table, Chunk value)
        {
            table.Add(new(value));
            return value;
        }
        
        public static ClrFunction Add(this Table table, ClrFunction value)
        {
            table.Add(new(value));
            return value;
        }

        public static DynamicValue Evaluate(this EVM evm, string expression, CompilerOptions compilerOptions = null, params DynamicValue[] args)
        {
            _lexer.LoadSource(expression);
            var program = _parser.Parse(true);

            if (compilerOptions != null)
            {
                _compiler.Options.OptimizeBytecode = compilerOptions.OptimizeBytecode;
                
            }
            
            var exe = _compiler.Compile(program);
            evm.RunExecutable(exe, args);

            if (!evm.MainExecutionContext.EvaluationStack.TryPop(out var value))
            {
                return DynamicValue.Null;
            }

            return value;
        }

        public static void SetEnvironmentVariable(this EVM evm, EvilEnvironmentVariable var, string value)
            => evm.SetEnvironmentVariable(var, new(value));
        
        public static void SetEnvironmentVariable(this EVM evm, EvilEnvironmentVariable var, double value)
            => evm.SetEnvironmentVariable(var, new(value));
        
        public static void SetEnvironmentVariable(this EVM evm, EvilEnvironmentVariable var, Table value)
            => evm.SetEnvironmentVariable(var, new(value));
        
        public static void SetEnvironmentVariable(this EVM evm, EvilEnvironmentVariable var, Chunk value)
            => evm.SetEnvironmentVariable(var, new(value));
        
        public static void SetEnvironmentVariable(this EVM evm, EvilEnvironmentVariable var, ClrFunction value)
            => evm.SetEnvironmentVariable(var, new(value));
    }
}