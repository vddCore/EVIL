using System;
using System.Collections.Generic;
using EVIL.Interpreter.Execution;

namespace EVIL.Interpreter.Abstraction
{
    public class Table
    {
        private Dictionary<double, DynValue> _doubles = new();
        private Dictionary<string, DynValue> _strings = new();
        private List<object> _keys = new();

        public DynValue this[double key]
        {
            get
            {
                if (!_doubles.ContainsKey(key))
                    return null;

                return _doubles[key];
            }

            set
            {
                if (_doubles.ContainsKey(key))
                {
                    _doubles[key] = value;
                }
                else
                {
                    _doubles.Add(key, value);
                    _keys.Add(key);
                }
            }
        }

        public DynValue this[string key]
        {
            get
            {
                if (!_strings.ContainsKey(key))
                    return null;

                return _strings[key];
            }

            set
            {
                if (_strings.ContainsKey(key))
                {
                    _strings[key] = value;
                }
                else
                {
                    _strings.Add(key, value);
                    _keys.Add(key);
                }
            }
        }

        public DynValue this[DynValue key]
        {
            get
            {
                if (key.Type == DynValueType.Number)
                    return this[key.Number];
                else if (key.Type == DynValueType.String)
                    return this[key.String];
                else
                {
                    throw new InvalidDynValueTypeException(
                        $"Value type '{key.Type}' cannot be used as a table key.",
                        DynValueType.String,
                        key.Type
                    );
                }
            }

            set
            {
                if (key.Type == DynValueType.Number)
                    this[key.Number] = value;
                else if (key.Type == DynValueType.String)
                    this[key.String] = value;
                else
                {
                    throw new InvalidDynValueTypeException(
                        $"Value type '{key.Type}' cannot be used as a table key.",
                        DynValueType.String,
                        key.Type
                    );
                }
            }
        }

        public int Count => _keys.Count;

        public bool ContainsKey(double key)
        {
            return _doubles.ContainsKey(key);
        }

        public bool ContainsKey(string key)
        {
            return _strings.ContainsKey(key);
        }

        public bool ContainsKey(DynValue key)
        {
            if (key.Type == DynValueType.Number)
                return ContainsKey(key.Number);
            else if (key.Type == DynValueType.String)
                return ContainsKey(key.String);
            else
            {
                throw new InvalidDynValueTypeException(
                    $"Value type '{key.Type}' cannot be used as a table key.",
                    DynValueType.String,
                    key.Type
                );
            }
        }

        public void ForEach(Action<DynValue, DynValue> predicate)
        {
            for (var i = 0; i < _keys.Count; i++)
            {
                if (_keys[i] is double dbl)
                {
                    predicate(new DynValue(dbl), this[dbl]);
                }
                else if (_keys[i] is string str)
                {
                    predicate(new DynValue(str), this[str]);
                }
            }
        }

        public DynValue ElementAt(int index)
        {
            if (index < 0 || index >= _keys.Count)
                throw new IndexOutOfRangeException("Index is negative or out of range for this table.");

            var k = _keys[index];

            if (k is double dbl)
                return this[dbl];
            else return this[k as string];
        }

        public bool Remove(string key)
        {
            var ret = _strings.Remove(key);

            if (ret)
            {
                _keys.Remove(key);
            }

            return ret;
        }

        public bool Remove(double key)
        {
            var ret = _doubles.Remove(key);

            if (ret)
            {
                _keys.Remove(key);
            }

            return ret;
        }

        public bool Remove(DynValue key)
        {
            if (key.Type == DynValueType.Number)
                return Remove(key.Number);
            else if (key.Type == DynValueType.String)
                return Remove(key.String);
            else
            {
                throw new InvalidDynValueTypeException(
                    $"Value type '{key.Type}' cannot be used as a table key.",
                    DynValueType.String,
                    key.Type
                );
            }
        }

        public static Table FromString(string str)
        {
            var tbl = new Table();

            for (var i = 0; i < str.Length; i++)
                tbl[i] = new DynValue(str[i].ToString());

            return tbl;
        }
    }
}