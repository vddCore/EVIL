using System.Text;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public sealed record DynamicValue
    {
        public enum DynamicValueType
        {
            Nil,
            Number,
            String,
            Boolean,
            Table,
            Fiber,
            Chunk,
            NativeFunction
        }

        public static readonly DynamicValue Nil = new(DynamicValueType.Nil);
        public static readonly DynamicValue One = new((double)1);
        public static readonly DynamicValue Zero = new((double)0);
        public static readonly DynamicValue True = new(true);
        public static readonly DynamicValue False = new(false);
        public static readonly DynamicValue EmptyString = new("");
        
        public DynamicValueType Type { get; }

        public double Number { get; }
        public string? String { get; }
        public bool Boolean { get; }
        public Table? Table { get; }
        public Fiber? Fiber { get; }
        public Chunk? Chunk { get; }
        public NativeFunction? NativeFunction { get; }

        public bool IsTruth => this != Nil 
                            && this != Zero 
                            && this != False;
        
        private DynamicValue(DynamicValueType type)
        {
            Type = type;
        }

        public DynamicValue(double value) : this(DynamicValueType.Number) => Number = value;
        public DynamicValue(string value) : this(DynamicValueType.String) => String = value;
        public DynamicValue(bool value) : this(DynamicValueType.Boolean) => Boolean = value;
        public DynamicValue(Table value) : this(DynamicValueType.Table) => Table = value;
        public DynamicValue(Fiber value) : this(DynamicValueType.Fiber) => Fiber = value;
        public DynamicValue(Chunk value) : this(DynamicValueType.Chunk) => Chunk = value;
        public DynamicValue(NativeFunction value) : this(DynamicValueType.NativeFunction) => NativeFunction = value;

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append($"<{Type}>");

            switch (Type)
            {
                case DynamicValueType.Number:
                    sb.Append(" ");
                    sb.Append(Number);
                    break;
                
                case DynamicValueType.String:
                    sb.Append(" ");
                    sb.Append($"\"{String}\"");
                    break;
                
                case DynamicValueType.Boolean:
                    sb.Append(" ");
                    sb.Append(Boolean);
                    break;
                
                case DynamicValueType.Nil:
                    break;
                
                default:
                    sb.Append(" ");
                    sb.Append(this.ConvertToString());
                    break;
            }

            return sb.ToString();
        }
    }
}