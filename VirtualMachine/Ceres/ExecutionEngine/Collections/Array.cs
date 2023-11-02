using System.Collections;
using System.Collections.Generic;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.ExecutionEngine.Collections
{
    public class Array : IEnumerable<KeyValuePair<DynamicValue, DynamicValue>>
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

        public Array(int size)
        {
            if (size < 0)
            {
                throw new ArrayException($"Attempt to initialize an Array using a negative size ({size}).");
            }
            
            _values = new DynamicValue[size];
            System.Array.Fill(_values, DynamicValue.Nil);
        }

        public int IndexOf(DynamicValue value)
            => System.Array.IndexOf(_values, value);
        
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
                        if (!DynamicValue.IsTruth(this[i].IsDeeplyEqualTo(other[i])))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!DynamicValue.IsTruth(this[i].IsEqualTo(other[i])))
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