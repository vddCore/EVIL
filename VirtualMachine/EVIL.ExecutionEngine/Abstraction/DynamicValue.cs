using System;
using EVIL.Intermediate;

namespace EVIL.ExecutionEngine.Abstraction
{
    public struct DynamicValue
    {
        private string _string;
        private double? _number;
        private Chunk _chunk;
        private Table _table;

        public static readonly DynamicValue Zero
            = new(0) { IsReadOnly = true };

        public bool IsReadOnly { get; set; }
        public DynamicValueType Type { get; private set; }

        public string String
        {
            get => _string ??
                   throw new UnexpectedTypeException(
                       $"Attempted to use a {Type} as String.");

            set
            {
                if (IsReadOnly)
                    return;

                _string = value;
                _number = null;
                _chunk = null;
                _table = null;

                Type = DynamicValueType.String;
            }
        }

        public double Number
        {
            get => _number ??
                   throw new UnexpectedTypeException(
                       $"Attempted to use a {Type} as Number");

            set
            {
                if (IsReadOnly)
                    return;

                _string = null;
                _number = value;
                _chunk = null;

                Type = DynamicValueType.Number;
            }
        }

        public Chunk Function
        {
            get => _chunk ??
                   throw new UnexpectedTypeException(
                       $"Attemtped to use a {Type} as a Function");

            set
            {
                if (IsReadOnly)
                    return;

                _string = null;
                _number = null;
                _chunk = value;

                Type = DynamicValueType.Function;
            }
        }

        public DynamicValue(DynamicValue other)
        {
            Type = other.Type;
            IsReadOnly = other.IsReadOnly;

            _string = null;
            _number = null;
            _chunk = null;
            _table = null;

            switch (other.Type)
            {
                case DynamicValueType.String:
                    String = other.String;
                    break;

                case DynamicValueType.Number:
                    Number = other.Number;
                    break;
            }
        }

        public DynamicValue(double num)
        {
            _string = null;
            _number = num;
            _chunk = null;
            _table = null;

            Type = DynamicValueType.Number;
            IsReadOnly = false;
        }

        public DynamicValue(string str)
        {
            _string = str;
            _number = null;
            _chunk = null;
            _table = null;

            Type = DynamicValueType.String;
            IsReadOnly = false;
        }

        public DynamicValue(Chunk chunk)
        {
            _string = null;
            _number = null;
            _chunk = chunk;
            _table = null;

            Type = DynamicValueType.Function;
            IsReadOnly = false;
        }

        public DynamicValue(Table table)
        {
            _string = null;
            _number = null;
            _chunk = null;
            _table = table;

            Type = DynamicValueType.Table;
            IsReadOnly = false;
        }

        public DynamicValue CopyToString()
        {
            return Type switch
            {
                DynamicValueType.Number => new(_number.ToString()),
                DynamicValueType.String => new(_string),
                DynamicValueType.Table => new($"Table[{_table.Entries.Count}]"),
                DynamicValueType.Function => new($"Function[{_chunk.Name}@{_chunk.ParameterCount}]"),
                _ => throw new TypeConversionException(Type, DynamicValueType.String)
            };
        }

        public DynamicValue CopyToNumber()
        {
            switch (Type)
            {
                case DynamicValueType.String:
                    break;
                
                case DynamicValueType.Number:
                    break;
                
                default: throw new TypeConversionException(Type, DynamicValueType.Number);   
            }
        }

        public bool Equals(DynamicValue other)
        {
            if (other.Type != Type)
                return false;

            return Type switch
            {
                DynamicValueType.String => _string == other.String,
                DynamicValueType.Number => Nullable.Equals(_number, other._number),
                DynamicValueType.Function => _chunk == other._chunk,
                DynamicValueType.Table => _table == other._table,
                // todo
                _ => false
            };
        }

        public override bool Equals(object obj)
        {
            return obj is DynamicValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Type switch
            {
                DynamicValueType.String => _string.GetHashCode(),
                DynamicValueType.Number => _number!.Value.GetHashCode(),
                DynamicValueType.Function => _chunk.Instructions.GetHashCode(),
                DynamicValueType.Table => _table.GetHashCode(),
                // todo
                _ => 0
            };
        }

        public static bool operator ==(DynamicValue left, DynamicValue right)
            => left.Equals(right);

        public static bool operator !=(DynamicValue left, DynamicValue right)
            => !left.Equals(right);
    }
}