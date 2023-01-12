using System;
using System.Collections;
using System.Collections.Generic;

namespace EVIL.Intermediate.Collections
{
    public class TwoWayDictionary<TKey, TValue> : IEnumerable<TwoWayDictionary<TKey, TValue>.KeyValuePair>
    {
        public struct KeyValuePair
        {
            public TKey Key;
            public TValue Value;
        }

        public struct Enumerator : IEnumerator<KeyValuePair>
        {
            public Enumerator(Dictionary<TKey, TValue>.Enumerator dictEnumerator)
            {
                _dictEnumerator = dictEnumerator;
            }

            public KeyValuePair Current
            {
                get
                {
                    KeyValuePair keyValuePair;
                    keyValuePair.Key = _dictEnumerator.Current.Key;
                    keyValuePair.Value = _dictEnumerator.Current.Value;
                    return keyValuePair;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _dictEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return _dictEnumerator.MoveNext();
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private Dictionary<TKey, TValue>.Enumerator _dictEnumerator;
        }

        private Dictionary<TKey, TValue> _keyToValue = new();
        private Dictionary<TValue, TKey> _valueToKey = new();

        public int Count => _keyToValue.Count;

        public IReadOnlyDictionary<TKey, TValue> Forward => _keyToValue;
        public IReadOnlyDictionary<TValue, TKey> Reverse => _valueToKey;

        public void Add(TKey key, TValue value)
        {
            if (_keyToValue.ContainsKey(key) || _valueToKey.ContainsKey(value))
                throw new ArgumentException("Duplicate key or value");

            _keyToValue.Add(key, value);
            _valueToKey.Add(value, key);
        }

        public TValue GetByKey(TKey key)
        {
            TValue value;
            if (!_keyToValue.TryGetValue(key, out value))
                throw new ArgumentException(nameof(key));

            return value;
        }

        public TKey GetByValue(TValue value)
        {
            TKey key;
            if (!_valueToKey.TryGetValue(value, out key))
                throw new ArgumentException(nameof(value));

            return key;
        }

        public void RemoveByKey(TKey key)
        {
            TValue value;
            if (!_keyToValue.TryGetValue(key, out value))
                throw new ArgumentException(nameof(key));

            _keyToValue.Remove(key);
            _valueToKey.Remove(value);
        }

        public void RemoveByValue(TValue value)
        {
            TKey key;
            if (!_valueToKey.TryGetValue(value, out key))
                throw new ArgumentException(nameof(value));

            _valueToKey.Remove(value);
            _keyToValue.Remove(key);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (_keyToValue.ContainsKey(key) || _valueToKey.ContainsKey(value))
                return false;

            _keyToValue.Add(key, value);
            _valueToKey.Add(value, key);
            return true;
        }


        public bool TryGetByKey(TKey key, out TValue value)
            => _keyToValue.TryGetValue(key, out value);

        public bool TryGetByValue(TValue value, out TKey key)
            => _valueToKey.TryGetValue(value, out key);

        public bool TryRemoveByKey(TKey key)
        {
            TValue value;
            if (!_keyToValue.TryGetValue(key, out value))
                return false;

            _keyToValue.Remove(key);
            _valueToKey.Remove(value);
            return true;
        }

        public bool TryRemoveByValue(TValue value)
        {
            TKey key;
            if (!_valueToKey.TryGetValue(value, out key))
                return false;

            _valueToKey.Remove(value);
            _keyToValue.Remove(key);
            return true;
        }

        public void Clear()
        {
            _keyToValue.Clear();
            _valueToKey.Clear();
        }

        public Enumerator GetEnumerator()
            => new(_keyToValue.GetEnumerator());

        IEnumerator<KeyValuePair> IEnumerable<KeyValuePair>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}