using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.Collections
{
    public class Table : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
    {
        private ConcurrentDictionary<DynamicValue, DynamicValue> _values = new();
        private ConcurrentDictionary<TableOverride, Chunk> _overrides = new();

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

        public bool TryGetOverride(TableOverride op, [MaybeNullWhen(false)] out Chunk chunk)
        {
            if (!_overrides.TryGetValue(op, out chunk))
            {
                chunk = null;
                return false;
            }

            return true;
        }

        public DynamicValue GetOverride(TableOverride op)
        {
            if (_overrides.TryGetValue(op, out var chunk))
                return chunk;

            return Nil;
        }

        public void SetOverride(TableOverride op, Chunk chunk)
        {
            if (!_overrides.TryAdd(op, chunk))
            {
                _overrides[op] = chunk;
            }
        }

        public bool RemoveOverride(TableOverride op)
            => _overrides.TryRemove(op, out _);

        public void Add(DynamicValue key, DynamicValue value)
            => Set(key, value);

        public void Set(DynamicValue key, DynamicValue value)
        {
            if (IsFrozen)
                return;

            (key, value) = OnSet(key, value);

            if (key == Nil)
                return;

            lock (_values)
            {
                if (value == Nil)
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
                    return Nil;

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

        public Array GetKeys()
        {
            lock (_values)
            {
                var keys = new Array(_values.Count);
                var i = 0;

                foreach (var kvp in _values)
                {
                    keys[i++] = kvp.Key;
                }

                return keys;
            }
        }

        public Array GetValues()
        {
            lock (_values)
            {
                var values = new Array(_values.Count);
                var i = 0;

                foreach (var kvp in _values)
                {
                    values[i++] = kvp.Value;
                }

                return values;
            }
        }

        public Table ShallowCopy()
        {
            var copy = new Table();

            foreach (var ovr in _overrides)
                copy._overrides.TryAdd(ovr.Key, ovr.Value);

            foreach (var kvp in this)
                copy[kvp.Key] = kvp.Value;

            return copy;
        }

        public Table DeepCopy()
        {
            var copy = new Table();

            foreach (var ovr in _overrides)
                copy.SetOverride(ovr.Key, ovr.Value);

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
                        if (!IsTruth(this[k].IsDeeplyEqualTo(other[k])))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!IsTruth(this[k].IsEqualTo(other[k])))
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
            var ret = Nil;

            lock (_values)
            {
                while (segments.Any())
                {
                    ret = currentTable[segments.Pop()];

                    if (ret == Nil)
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