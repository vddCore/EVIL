using System;
using System.Collections.Generic;
using System.Linq;

namespace EVIL.Abstraction
{
    public class Table : Dictionary<DynValue, DynValue>
    {
        public DynValue this[string key]
        {
            get => GetValueByString(key);

            set
            {
                var dynKey = GetKeyByString(key);

                if (dynKey != null)
                    base[dynKey] = value;
                else
                    Add(new DynValue(key), value);
            }
        }

        public DynValue this[decimal key]
        {
            get => GetValueByNumber(key);

            set
            {
                var dynKey = GetKeyByNumber(key);

                if (dynKey != null)
                    base[dynKey] = value;
                else
                    Add(new DynValue(key), value);
            }
        }

        public new DynValue this[DynValue key]
        {
            get
            {
                if (key.Type == DynValueType.Number)
                    return this[key.Number];
                else if (key.Type == DynValueType.String)
                    return this[key.String];
                else throw new Exception($"A {key.Type} cannot be used to index a table.");
            }

            set
            {
                if (key.Type == DynValueType.Number)
                    this[key.Number] = value;
                else if (key.Type == DynValueType.String)
                    this[key.String] = value;
                else throw new Exception($"A {key.Type} cannot be used to index a table.");
            }
        }

        public DynValue GetKeyByDynValue(DynValue key)
        {
            switch (key.Type)
            {
                case DynValueType.Number:
                    return GetKeyByNumber(key.Number);
                case DynValueType.String:
                    return GetKeyByString(key.String);
                default: throw new KeyNotFoundException($"The key '{key.AsString()}' was not found in the dictionary.");
            }
        }

        public new bool ContainsKey(DynValue key)
            => GetKeyByDynValue(key) != null;

        public DynValue GetKeyByString(string key)
            => Keys.FirstOrDefault(k => k.Type == DynValueType.String && k.String == key);

        public DynValue GetValueByString(string key)
        {
            var dynKey = GetKeyByString(key);

            if (dynKey != null)
                return base[dynKey];

            throw new KeyNotFoundException($"Key '{key}' was not found in the table.");
        }

        public DynValue GetKeyByNumber(decimal key)
            => Keys.FirstOrDefault(k => k.Type == DynValueType.Number && k.Number == key);

        public DynValue GetValueByNumber(decimal key)
        {
            var dynKey = GetKeyByNumber(key);

            if (dynKey != null)
                return base[dynKey];

            throw new KeyNotFoundException($"Key '{key}' was not found in the table.");
        }

        public static Table FromString(string s)
        {
            var table = new Table();

            for (var i = 0; i < s.Length; i++)
                table[i] = new DynValue(s[i].ToString());

            return table;
        }

        public static Table FromArray<T>(T[] array)
        {
            var t = new Table();

            for (var i = 0; i < array.Length; i++)
            {
                var e = array[i];

                if (typeof(decimal).IsAssignableFrom(e.GetType()))
                {
                    t[i] = new DynValue(
                        Convert.ToDecimal(e)
                    );
                }
                else if (typeof(Table).IsAssignableFrom(e.GetType()))
                {
                    t[i] = new DynValue(
                        (Table)Convert.ChangeType(e, typeof(Table))
                    );
                }
                else if (typeof(string).IsAssignableFrom(e.GetType()))
                {
                    t[i] = new DynValue(
                        Convert.ToString(e)
                    );
                }
                else if (typeof(ClrFunction).IsAssignableFrom(e.GetType()))
                {
                    t[i] = new DynValue(
                        (ClrFunction)Convert.ChangeType(e, typeof(ClrFunction))
                    );
                }
                else if (typeof(ScriptFunction).IsAssignableFrom(e.GetType()))
                {
                    t[i] = new DynValue(
                        (ScriptFunction)Convert.ChangeType(e, typeof(ScriptFunction))
                    );
                }
                else
                {
                    t[i] = new DynValue(e.ToString());
                }
            }

            return t;
        }
    }
}