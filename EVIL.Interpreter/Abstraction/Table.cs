using System;
using System.Collections.Generic;


namespace EVIL.Interpreter.Abstraction
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

        public DynValue this[double key]
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
            if (key == null)
                return null;

            if (key.Type == DynValueType.Number)
                return GetKeyByNumber(key.Number);
            else if (key.Type == DynValueType.String)
                return GetKeyByString(key.String);

            throw new KeyNotFoundException($"A {key.Type} cannot be used as a key.");
        }

        public new bool ContainsKey(DynValue key)
            => GetKeyByDynValue(key) != null;

        public DynValue GetKeyByString(string key)
        {
            foreach (var k in Keys)
            {
                if (k.Type == DynValueType.String && k.String == key)
                    return k;
            }

            return null;
        }

        public DynValue GetValueByString(string key)
        {
            var dynKey = GetKeyByString(key);

            if (dynKey != null)
                return base[dynKey];

            throw new KeyNotFoundException($"Key '{key}' was not found in the table.");
        }

        public DynValue GetKeyByNumber(double key)
        {
            foreach (var k in Keys)
            {
                if (k.Type == DynValueType.Number && k.Number == key)
                    return k;
            }

            return null;
        }

        public DynValue GetValueByNumber(double key)
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
    }
}