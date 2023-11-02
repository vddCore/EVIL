using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.ExecutionEngine.Collections
{
    public class Table : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
    {
        private ConcurrentDictionary<DynamicValue, DynamicValue> _values = new();

        public DynamicValue this[DynamicValue key]
        {
            get => Index(key);
            set => Set(key, value);
        }

        public bool IsFrozen { get; private set; }

        public double Length
        {
            get
            {
                lock (_values)
                {
                    return _values.Count;
                }
            }
        }

        public void Add(DynamicValue key, DynamicValue value)
            => Set(key, value);

        public void Set(DynamicValue key, DynamicValue value)
        {
            if (IsFrozen)
                return;

            (key, value) = OnSet(key, value);

            if (key == DynamicValue.Nil)
                return;

            lock (_values)
            {
                if (value == DynamicValue.Nil)
                {
                    _values.TryRemove(key, out _);
                }
                else
                {
                    _values.AddOrUpdate(key, (_) => value, (_, _) => value);
                }
            }
        }

        protected virtual (DynamicValue Key, DynamicValue Value) OnSet(DynamicValue key, DynamicValue value)
        {
            return (key, value);
        }

        public DynamicValue Index(DynamicValue key) 
            => OnIndex(key);

        protected virtual DynamicValue OnIndex(DynamicValue key)
        {
            lock (_values)
            {
                if (!_values.TryGetValue(key, out var value))
                    return DynamicValue.Nil;

                return value;
            }
        }

        public bool Contains(DynamicValue key)
        {
            return OnContains(key);
        }

        protected virtual bool OnContains(DynamicValue key)
        {
            lock (_values)
            {
                return _values.ContainsKey(key);
            }
        }

        public void Clear()
        {
            lock (_values)
            {
                _values.Clear();
            }
        }

        public Table Freeze(bool deep = false)
        {
            IsFrozen = true;

            if (deep)
            {
                lock (_values)
                {
                    foreach (var value in _values.Values)
                    {
                        if (value.Type == DynamicValueType.Table)
                            value.Table!.Freeze(deep);
                    }
                }
            }

            return this;
        }

        public Table Unfreeze(bool deep = false)
        {
            IsFrozen = false;

            if (deep)
            {
                lock (_values)
                {
                    foreach (var value in _values.Values)
                    {
                        if (value.Type == DynamicValueType.Table)
                            value.Table!.Freeze();
                    }
                }
            }

            return this;
        }

        public Table GetKeys()
        {
            var keys = new Table();

            lock (_values)
            {
                var i = 0;

                foreach (var kvp in _values)
                {
                    keys.Set(i++, kvp.Key);
                }
            }

            return keys;
        }

        public Table GetValues()
        {
            var values = new Table();

            lock (_values)
            {
                var i = 0;

                foreach (var kvp in _values)
                {
                    values.Set(i++, kvp.Value);
                }
            }
            
            return values;
        }

        public Table ShallowCopy()
        {
            var copy = new Table();
            
            foreach (var kvp in this) 
                copy[kvp.Key] = kvp.Value;

            return copy;
        }

        public Table DeepCopy()
        {
            var copy = new Table();

            foreach (var kvp in this)
            {
                if (kvp.Value.Type == DynamicValueType.Table)
                {
                    copy[kvp.Key] = kvp.Value.Table!.DeepCopy();
                }
                else
                {
                    copy[kvp.Key] = kvp.Value;
                }
            }
            
            return copy;
        }

        public bool IsDeeplyEqualTo(Table other)
        {
            if (Length != other.Length)
                return false;
            
            lock (_values)
            {
                for (var i = 0; i < _values.Keys.Count; i++)
                {
                    var k = _values.Keys.ElementAt(i);

                    if (this[k].Type == DynamicValueType.Table)
                    {
                        if (!DynamicValue.IsTruth(this[k].IsDeeplyEqualTo(other[k])))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!DynamicValue.IsTruth(this[k].IsEqualTo(other[k])))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public DynamicValue IndexUsingFullyQualifiedName(string fullyQualifiedName)
        {
            var segments = new Stack<string>(fullyQualifiedName.Split('.'));
            var currentTable = this;
            var ret = DynamicValue.Nil;

            lock (_values)
            {
                while (segments.Any())
                {
                    ret = currentTable[segments.Pop()];

                    if (ret == DynamicValue.Nil)
                        return ret;

                    if (ret.Type != DynamicValueType.Table && segments.Any())
                        return ret;

                    currentTable = ret.Table!;
                }
            }

            return ret;
        }

        public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>> GetEnumerator()
        {
            lock (_values)
            {
                return _values.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}