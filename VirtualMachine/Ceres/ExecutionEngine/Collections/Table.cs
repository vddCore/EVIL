using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine.TypeSystem;

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

        public void Freeze(bool deep = false)
        {
            IsFrozen = true;

            if (deep)
            {
                lock (_values)
                {
                    foreach (var value in _values.Values)
                    {
                        if (value.Type == DynamicValue.DynamicValueType.Table)
                            value.Table!.Freeze(deep);
                    }
                }
            }
        }

        public void Unfreeze(bool deep = false)
        {
            IsFrozen = false;

            if (deep)
            {
                lock (_values)
                {
                    foreach (var value in _values.Values)
                    {
                        if (value.Type == DynamicValue.DynamicValueType.Table)
                            value.Table!.Freeze();
                    }
                }
            }
        }

        public Table GetKeys()
        {
            var keys = new Table();

            lock (_values)
            {
                for (var i = 0; i < _values.Keys.Count; i++)
                    keys.Set(i, _values.Keys.ElementAt(i));
            }

            return keys;
        }

        public bool IsDeeplyEqualTo(Table other)
        {
            lock (_values)
            {
                for (var i = 0; i < _values.Keys.Count; i++)
                {
                    var k = _values.Keys.ElementAt(i);

                    if (this[k].Type == DynamicValue.DynamicValueType.Table)
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

                    if (ret.Type != DynamicValue.DynamicValueType.Table && segments.Any())
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