using System;
using System.Globalization;
using EVIL.ExecutionEngine.Interop;
using EVIL.Intermediate;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine.Abstraction
{
    public class DynamicValue
    {
        private string _string;
        private double _number;
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
                   throw new InvalidTypeUsageException(Type, DynamicValueType.String);

            set
            {
                if (IsReadOnly)
                    return;

                _string = value;
                _number = 0;
                _chunk = null;
                _table = null;
                _clrFunction = null;

                Type = DynamicValueType.String;
            }
        }

        public double Number
        {
            get
            {
                if (Type != DynamicValueType.Number)
                    throw new InvalidTypeUsageException(Type, DynamicValueType.Number);


                return _number;
            }

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
                   throw new InvalidTypeUsageException(Type, DynamicValueType.Function);

            set
            {
                if (IsReadOnly)
                    return;

                _string = null;
                _number = 0;
                _chunk = value;
                _table = null;
                _clrFunction = null;

                Type = DynamicValueType.Function;
            }
        }

        public Table Table
        {
            get => _table ??
                   throw new InvalidTypeUsageException(Type, DynamicValueType.Table);

            set
            {
                if (IsReadOnly)
                    return;

                _string = null;
                _number = 0;
                _chunk = null;
                _table = value;
                _clrFunction = null;

                Type = DynamicValueType.Table;
            }
        }

        public ClrFunction ClrFunction
        {
            get => _clrFunction ??
                   throw new InvalidTypeUsageException(Type, DynamicValueType.ClrFunction);

            set
            {
                if (IsReadOnly)
                    return;

                _string = null;
                _number = 0;
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
            _number = 0;
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

        public DynamicValue(string str)
        {
            _string = str;
            _number = 0;
            _chunk = null;
            _table = null;
            _clrFunction = null;

            Type = DynamicValueType.String;
            IsReadOnly = false;
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

        public DynamicValue(Chunk chunk)
        {
            _string = null;
            _number = 0;
            _chunk = chunk;
            _table = null;
            _clrFunction = null;

            Type = DynamicValueType.Function;
            IsReadOnly = false;
        }

        public DynamicValue(Table table)
        {
            _string = null;
            _number = 0;
            _chunk = null;
            _table = table;
            _clrFunction = null;

            Type = DynamicValueType.Table;
            IsReadOnly = false;
        }

        public DynamicValue(ClrFunction clrFunction)
        {
            _string = null;
            _number = 0;
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
                DynamicValueType.Number => _number.ToString(CultureInfo.InvariantCulture),
                DynamicValueType.String => _string,
                DynamicValueType.Table => $"Table[{_table.Entries.Count}]",
                DynamicValueType.Function => $"Function[{_chunk.Name}@{_chunk.Parameters.Count}]",
                DynamicValueType.ClrFunction =>
                    $"ClrFunction[{_clrFunction.Method.Name}@{_clrFunction.Method.GetParameters().Length}]",
                _ => throw new TypeConversionException(Type, DynamicValueType.String)
            };
        }

        public Table AsTable()
        {
            return Type switch
            {
                DynamicValueType.String => Table.FromString(_string),
                DynamicValueType.Table => _table,
                _ => throw new TypeConversionException(Type, DynamicValueType.Table)
            };
        }

        public DynamicValue Index(DynamicValue key)
        {
            switch (Type)
            {
                case DynamicValueType.String:
                    if (key.Type != DynamicValueType.Number)
                        throw new InvalidKeyTypeException(Type, key.Type);

                    var index = key.AsInteger();

                    if (index >= _string.Length)
                        throw new IndexOutOfBoundsException(index, Type);

                    return new DynamicValue(_string[index].ToString());

                case DynamicValueType.Table:
                    return _table.Get(key);

                default:
                    throw new UnindexableTypeException(Type);
            }
        }

        public DynamicValue Add(DynamicValue b)
        {
            if (Type == DynamicValueType.String)
            {
                if (b.Type != DynamicValueType.String)
                    throw new UnexpectedTypeException(b.Type, DynamicValueType.String);

                return new(String + b.String);
            }
            else if (Type == DynamicValueType.Number)
            {
                if (b.Type != DynamicValueType.Number)
                    throw new UnexpectedTypeException(b.Type, DynamicValueType.Number);

                return new(Number + b.Number);
            }

            throw new UnexpectedTypeException(Type);
        }

        public int Compare(DynamicValue b)
        {
            if (Type == DynamicValueType.String)
            {
                return string.Compare(String, b.String, StringComparison.Ordinal);
            }
            else if (Type == DynamicValueType.Number)
            {
                return Math.Sign(Number - b.Number);
            }

            throw new TypeComparisonException(this, b);
        }

        public bool Contains(DynamicValue other)
        {
            switch (Type)
            {
                case DynamicValueType.String when other.Type == DynamicValueType.String:
                    return _string.Contains(other.String);

                case DynamicValueType.Table:
                    return _table.IsSet(other);
            }

            throw new UnsupportedOperationException($"Attempted to check if a {other} exists in {Type}. " +
                                                    $"Frankly, that is not supported. I am also struggling to find a " +
                                                    $"proper word that describes the act of checking if X exists in Y. If " +
                                                    $"you know what the word is I *beg* you, please, contact me.");
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
                    return new DynamicValue(_number);

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
                DynamicValueType.Number => _number.GetHashCode(),
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