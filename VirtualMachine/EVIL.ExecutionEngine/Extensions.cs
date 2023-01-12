using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine
{
    public static class Extensions
    {
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
    }
}