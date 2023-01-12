using System.Collections.Generic;
using System.Globalization;

namespace EVIL.Interpreter.Abstraction
{
    public class DynValue
    {
        public static DynValue Zero => new(0);

        private decimal _decimalValue;
        private int _integerValue;
        private string _stringValue;
        private Table _tableValue;
        private ClrFunction _clrFunctionValue;
        private ScriptFunction _scriptFunction;

        public DynValueType Type { get; private set; }

        public bool IsTruth
        {
            get
            {
                if (Type == DynValueType.Decimal)
                    return Decimal != 0;
                else if (Type == DynValueType.Integer)
                    return Integer != 0;

                return true;
            }
        }

        public decimal Decimal
        {
            get
            {
                if (Type != DynValueType.Decimal)
                    throw new InvalidDynValueTypeException("This value is not a decimal.", DynValueType.Decimal, Type);

                return _decimalValue;
            }
        }

        public int Integer
        {
            get
            {
                if (Type != DynValueType.Integer)
                    throw new InvalidDynValueTypeException("This value is not an integer.", DynValueType.Integer, Type);

                return _integerValue;
            }
        }

        public string String
        {
            get
            {
                if (Type != DynValueType.String)
                    throw new InvalidDynValueTypeException("This value is not a string.", DynValueType.String, Type);

                return _stringValue;
            }
        }

        public Table Table
        {
            get
            {
                if (Type != DynValueType.Table)
                    throw new InvalidDynValueTypeException($"This value is not a table.", DynValueType.Table, Type);

                return _tableValue;
            }
        }

        public ScriptFunction ScriptFunction
        {
            get
            {
                if (Type != DynValueType.Function)
                    throw new InvalidDynValueTypeException(
                        $"This value is not a function.", DynValueType.Function, Type);

                return _scriptFunction;
            }
        }

        public ClrFunction ClrFunction
        {
            get
            {
                if (Type != DynValueType.ClrFunction)
                    throw new InvalidDynValueTypeException(
                        $"This value is not a CLR function.", DynValueType.Function, Type);

                return _clrFunctionValue;
            }
        }

        private DynValue(DynValue dynValue)
        {
            CopyFrom(dynValue);
        }

        public DynValue(bool value)
        {
            Type = DynValueType.Integer;
            _integerValue = value ? 1 : 0;
        }

        public DynValue(int value)
        {
            Type = DynValueType.Integer;
            _integerValue = value;
        }

        public DynValue(decimal value)
        {
            Type = DynValueType.Decimal;
            _decimalValue = value;
        }

        public DynValue(string value)
        {
            Type = DynValueType.String;
            _stringValue = value;
        }

        public DynValue(Table value)
        {
            Type = DynValueType.Table;
            _tableValue = value;
        }

        public DynValue(ScriptFunction function)
        {
            Type = DynValueType.Function;
            _scriptFunction = function;
        }

        public DynValue(ClrFunction function)
        {
            Type = DynValueType.ClrFunction;
            _clrFunctionValue = function;
        }

        public DynValue AsDecimal()
        {
            if (Type == DynValueType.Decimal)
                return new DynValue(_decimalValue);

            var success = decimal.TryParse(_stringValue, out var result);

            if (!success)
                throw new DynValueConversionException($"Cannot convert value of type '{Type}' to a decimal.");

            return new DynValue(result);
        }

        public DynValue AsInteger()
        {
            if (Type == DynValueType.Integer)
                return new DynValue(_integerValue);

            var success = int.TryParse(_stringValue, out var result);

            if (!success)
                throw new DynValueConversionException($"Cannot convert value of type '{Type}' to an integer.");

            return new DynValue(result);
        }

        public DynValue AsString()
        {
            if (Type == DynValueType.String)
                return new DynValue(_stringValue);
            else if (Type == DynValueType.Table)
            {
                return new DynValue($"Table({_tableValue.Count})");
            }
            else if (Type == DynValueType.Function)
            {
                return new DynValue($"Function({_scriptFunction.ParameterNames.Count})");
            }
            else if (Type == DynValueType.ClrFunction)
            {
                return new DynValue($"ClrFunction({_clrFunctionValue.Invokable.Method.GetParameters().Length})");
            }
            else if (Type == DynValueType.Decimal)
            {
                return new DynValue(_decimalValue.ToString(CultureInfo.InvariantCulture));
            }
            else return new DynValue(_integerValue.ToString(CultureInfo.InvariantCulture));
        }

        public DynValue AsTable()
        {
            if (Type == DynValueType.String)
                return new DynValue(Table.FromString(_stringValue));
            else if (Type == DynValueType.Table)
                return new DynValue(_tableValue);
            else throw new DynValueConversionException($"Cannot convert value of type '{Type}' to a table.");
        }

        public DynValue Copy()
        {
            return new(this);
        }

        public void CopyFrom(DynValue dynValue)
        {
            Type = dynValue.Type;

            switch (dynValue.Type)
            {
                case DynValueType.Decimal:
                    _decimalValue = dynValue.Decimal;
                    break;
                
                case DynValueType.Integer:
                    _integerValue = dynValue.Integer;
                    break;

                case DynValueType.String:
                    _stringValue = dynValue.String;
                    break;

                case DynValueType.Table:
                    _tableValue = dynValue.Table;
                    break;

                case DynValueType.Function:
                    _scriptFunction = dynValue.ScriptFunction;
                    break;

                case DynValueType.ClrFunction:
                    _clrFunctionValue = dynValue.ClrFunction;
                    break;
            }
        }
    }
}