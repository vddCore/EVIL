using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.Collections
{
    public class Array : IDynamicValueCollection
    {
        private DynamicValue[] _values;
        
        public int Length => _values.Length;

        public DynamicValue this[int index]
        {
            get
            {
                if (index < 0 || index >= _values.Length)
                {
                    throw new ArrayException($"Index {index} was out of bounds for this array.");
                }

                return _values[index];
            }

            set
            {
                if (index < 0 || index >= _values.Length)
                {
                    throw new ArrayException($"Index {index} was out of bounds for this array.");
                }
                
                _values[index] = value;
            }
        }

        public DynamicValue this[DynamicValue index]
        {
            get => this[(int)index.Number];
            set => this[(int)index.Number] = value;
        }
        public Array(Array array)
        {
            _values = new DynamicValue[array.Length];
            
            for (var i = 0; i < array.Length; i++)
            {
                _values[i] = array[i];
            }
        }

        public Array(int size)
        {
            if (size < 0)
            {
                throw new ArrayException($"Attempt to initialize an Array using a negative size ({size}).");
            }
            
            _values = new DynamicValue[size];
            System.Array.Fill(_values, Nil);
        }

        public int IndexOf(DynamicValue value)
            => System.Array.IndexOf(_values, value);

        public void Fill(DynamicValue value)
        {
            for (var i = 0; i < _values.Length; i++)
            {
                _values[i] = value;
            }
        }

        public int Resize(int size)
        {
            if (size < 0)
            {
                return -1;
            }

            System.Array.Resize(ref _values, size);
            return _values.Length;
        }
        
        public int Push(params DynamicValue[] values)
        {
            if (values.Length == 0)
                return _values.Length;
            
            System.Array.Resize(
                ref _values,
                _values.Length + values.Length
            );

            for (var i = 0; i < values.Length; i++)
            {
                _values[_values.Length - values.Length + i] = values[i];
            }

            return _values.Length;
        }

        public int Insert(int index, params DynamicValue[] values)
        {
            if (index < 0)
                return -1;

            if (index > _values.Length)
                return -1;

            if (values.Length == 0)
                return _values.Length;
            
            System.Array.Resize(
                ref _values, 
                _values.Length + values.Length
            );

            var start = _values.Length - values.Length - 1;
            
            for (var i = start; i >= index; i--)
            {
                _values[_values.Length - values.Length + i] = _values[i];
            }

            for (var i = 0; i < values.Length; i++)
            {
                _values[index + i] = values[i];
            }

            return _values.Length;
        }

        public DynamicValue RightShift()
        {
            if (_values.Length <= 0)
                return Nil;

            var ret = _values[_values.Length - 1];
            System.Array.Resize(ref _values, _values.Length - 1);

            return ret;
        }
        
        public DynamicValue LeftShift()
        {
            if (_values.Length <= 0)
                return Nil;

            var ret = _values[0];

            for (var i = 1; i < _values.Length; i++)
            {
                _values[i - 1] = _values[i];
            }

            System.Array.Resize(ref _values, _values.Length - 1);
            
            return ret;
        }
        
        public bool IsDeeplyEqualTo(Array other)
        {
            if (Length != other.Length)
                return false;
            
            lock (_values)
            {
                for (var i = 0; i < Length; i++)
                {
                    if (this[i].Type == DynamicValueType.Table || this[i].Type == DynamicValueType.Array)
                    {
                        if (!IsTruth(this[i].IsDeeplyEqualTo(other[i])))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!IsTruth(this[i].IsEqualTo(other[i])))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        
        public static Array FromString(string s)
        {
            var a = new Array(s.Length);
            
            for (var i = 0; i < s.Length; i++)
            {
                a[i] = s[i].ToString();
            }

            return a;
        }

        public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>> GetEnumerator()
            => new ArrayEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}