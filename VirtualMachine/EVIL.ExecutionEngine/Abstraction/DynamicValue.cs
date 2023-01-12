using System;
using System.Globalization;
using EVIL.Intermediate;

namespace EVIL.ExecutionEngine.Abstraction
{
    public struct DynamicValue
    {
        private string _string;
        private double? _number;
        private Chunk _chunk;
        private Table _table;
        private ClrFunction _clrFunction;

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
                _clrFunction = null;

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
                _table = null;
                _clrFunction = null;

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
                _table = null;
                _clrFunction = null;

                Type = DynamicValueType.Function;
            }
        }

        public Table Table
        {
            get => _table ??
                   throw new UnexpectedTypeException(
                       $"Attempted to use a {Type} as a Table");

            set
            {
                if (IsReadOnly)
                    return;

                _string = null;
                _number = null;
                _chunk = null;
                _table = value;
                _clrFunction = null;

                Type = DynamicValueType.Table;
            }
        }

        public ClrFunction ClrFunction
        {
            get => _clrFunction ??
                   throw new UnexpectedTypeException(
                       $"Attempted to use a {Type} as a ClrFunction");

            set
            {
                if (IsReadOnly)
                    return;

                _string = null;
                _number = null;
                _chunk = null;
                _table = null;
                _clrFunction = value;

                Type = DynamicValueType.ClrFunction;
            }
        }

        public int Length
        {
            get
            {
                return Type switch
                {
                    DynamicValueType.String => _string.Length,
                    DynamicValueType.Table => _table.Entries.Count,
                    _ => throw new UnmeasurableTypeException(Type)
                };
            }
        }

        public bool IsTruth
        {
            get
            {
                if (Type == DynamicValueType.Number)
                    return _number != 0;
                
                return true;
            }
        }

        public DynamicValue(bool nativeBoolValue)
            : this(nativeBoolValue ? 1 : 0)
        {
        }

        public DynamicValue(DynamicValue other, bool copyReadOnlyState)
        {
            Type = other.Type;

            IsReadOnly = other.IsReadOnly && copyReadOnlyState;

            _string = null;
            _number = null;
            _chunk = null;
            _table = null;
            _clrFunction = null;

            switch (other.Type)
            {
                case DynamicValueType.String:
                    _string = other._string;
                    break;

                case DynamicValueType.Number:
                    _number = other._number;
                    break;
                
                case DynamicValueType.Function:
                    _chunk = other._chunk;
                    break;
                
                case DynamicValueType.Table:
                    _table = other._table;
                    break;
                
                case DynamicValueType.ClrFunction:
                    _clrFunction = other._clrFunction;
                    break;
            }
        }

        public DynamicValue(double num)
        {
            _string = null;
            _number = num;
            _chunk = null;
            _table = null;
            _clrFunction = null;

            Type = DynamicValueType.Number;
            IsReadOnly = false;
        }

        public DynamicValue(string str)
        {
            _string = str;
            _number = null;
            _chunk = null;
            _table = null;
            _clrFunction = null;

            Type = DynamicValueType.String;
            IsReadOnly = false;
        }

        public DynamicValue(Chunk chunk)
        {
            _string = null;
            _number = null;
            _chunk = chunk;
            _table = null;
            _clrFunction = null;

            Type = DynamicValueType.Function;
            IsReadOnly = false;
        }

        public DynamicValue(Table table)
        {
            _string = null;
            _number = null;
            _chunk = null;
            _table = table;
            _clrFunction = null;

            Type = DynamicValueType.Table;
            IsReadOnly = false;
        }

        public DynamicValue(ClrFunction clrFunction)
        {
            _string = null;
            _number = null;
            _chunk = null;
            _table = null;
            _clrFunction = clrFunction;

            Type = DynamicValueType.ClrFunction;
            IsReadOnly = false;
        }

        public int AsInteger()
            => (int)Number;

        public string AsString()
        {
            return Type switch
            {
                DynamicValueType.Number => _number.ToString(),
                DynamicValueType.String => _string,
                DynamicValueType.Table => $"Table[{_table.Entries.Count}]",
                DynamicValueType.Function => $"Function[{_chunk.Name}@{_chunk.ParameterCount}]",
                DynamicValueType.ClrFunction => $"ClrFunction[{_clrFunction.Method.Name}@{_clrFunction.Method.GetParameters().Length}]",
                _ => throw new TypeConversionException(Type, DynamicValueType.String)
            };
        }

        public DynamicValue CopyToString()
            => new(AsString());

        public DynamicValue CopyToNumber()
        {
            switch (Type)
            {
                case DynamicValueType.String:
                    if (!double.TryParse(
                            _string, 
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out var dbl))
                    {
                        throw new NumberFormatException(this);
                    }

                    return new DynamicValue(dbl);
                
                case DynamicValueType.Number:
                    return new DynamicValue(_number!.Value);
                
                default: 
                    throw new TypeConversionException(Type, DynamicValueType.Number);   
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
                DynamicValueType.ClrFunction => _clrFunction == other._clrFunction,
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
                DynamicValueType.ClrFunction => _clrFunction.GetHashCode(),
                _ => 0
            };
        }

        public static bool operator ==(DynamicValue left, DynamicValue right)
            => left.Equals(right);

        public static bool operator !=(DynamicValue left, DynamicValue right)
            => !left.Equals(right);
    }
}