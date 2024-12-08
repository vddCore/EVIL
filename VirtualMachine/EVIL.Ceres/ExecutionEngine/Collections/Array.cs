namespace EVIL.Ceres.ExecutionEngine.Collections;

using static EVIL.Ceres.ExecutionEngine.TypeSystem.DynamicValue;

using System.Collections;
using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

public class Array : IDynamicValueCollection, IIndexableObject, IWriteableObject
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
        get
        {
            if (index.Type != DynamicValueType.Number)
            {
                throw new ArrayException($"Attempt to index an Array using a {index.Type}.");
            }
                
            return this[(int)index.Number];
        }
        set
        {
            if (index.Type != DynamicValueType.Number)
            {
                throw new ArrayException($"Attempt to index an Array using a {index.Type}.");
            }
                
            this[(int)index.Number] = value;
        }
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
        => System.Array.Fill(_values, value);

    public int Resize(int size)
    {
        if (size < 0)
            return -1;

        System.Array.Resize(ref _values, size);
        return size;
    }
        
    public int Push(params DynamicValue[] values)
    {
        if (values.Length == 0)
            return _values.Length;

        var prevSize = _values.Length;
        System.Array.Resize(ref _values, prevSize + values.Length);
        System.Array.Copy(values, 0, _values, prevSize, values.Length);

        return _values.Length;
    }

    public int Insert(int index, params DynamicValue[] values)
    {
        if (index < 0 || index > _values.Length || values.Length == 0)
            return -1;

        var newSize = _values.Length + values.Length;
        System.Array.Resize(ref _values, newSize);

        var elementsToShift = _values.Length - values.Length - index;
        if (elementsToShift > 0)
        {
            System.Array.Copy(
                _values, 
                index, 
                _values, 
                index + values.Length, 
                elementsToShift
            );
        }

        System.Array.Copy(
            values, 
            0, 
            _values, 
            index, 
            values.Length
        );

        return newSize;
    }
    
    public int Delete(int index)
    {
        if (index < 0 || index >= _values.Length)
            return -1;

        System.Array.Copy(_values, index + 1, _values, index, _values.Length - index - 1);
        System.Array.Resize(ref _values, _values.Length - 1);
    
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
        if (_values.Length == 0)
            return Nil;

        var ret = _values[0];
        System.Array.Copy(_values, 1, _values, 0, _values.Length - 1);
        System.Array.Resize(ref _values, _values.Length - 1);
            
        return ret;
    }
        
    public bool IsDeeplyEqualTo(Array other)
    {
        if (Length != other.Length)
            return false;

        for (var i = 0; i < Length; i++)
        {
            var thisElement = this[i];
            var otherElement = other[i];

            if (thisElement.Type == DynamicValueType.Table || thisElement.Type == DynamicValueType.Array)
            {
                if (!IsTruth(thisElement.IsDeeplyEqualTo(otherElement)))
                    return false;
            }
            else
            {
                if (!IsTruth(thisElement.IsEqualTo(otherElement)))
                    return false;
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

    public DynamicValue Index(DynamicValue key)
        => this[key];

    public void Set(DynamicValue key, DynamicValue value)
        => this[key] = value;
}