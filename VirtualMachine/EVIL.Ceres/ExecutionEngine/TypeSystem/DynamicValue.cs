namespace EVIL.Ceres.ExecutionEngine.TypeSystem;

using Collections_Array = EVIL.Ceres.ExecutionEngine.Collections.Array;

using System;
using System.IO;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Collections.Serialization;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.TypeSystem;

public readonly struct DynamicValue : IEquatable<DynamicValue>
{
    public static readonly DynamicValue Nil = new();
    public static readonly DynamicValue One = new((double)1);
    public static readonly DynamicValue Zero = new((double)0);
    public static readonly DynamicValue True = new(true);
    public static readonly DynamicValue False = new(false);
    public static readonly DynamicValue EmptyString = new("");
        
    public readonly double Number;
    public readonly DynamicValueType Type;
    private readonly object? _reference;

    public bool Boolean => Number != 0;
    public DynamicValueType TypeCode => (DynamicValueType)(int)Number;
    public string? String => (string?)_reference;
    public Table? Table => (Table?)_reference;
    public Collections_Array? Array => (Collections_Array?)_reference;
    public Fiber? Fiber => (Fiber?)_reference;
    public Chunk? Chunk => (Chunk?)_reference;
    public Error? Error => (Error?)_reference;
    public NativeFunction? NativeFunction => (NativeFunction?)_reference;
    public object? NativeObject => _reference;

    public DynamicValue()
    {
        _reference = null;
        
        Type = DynamicValueType.Nil;
        Number = 0;
    }
        
    public DynamicValue(double value)
    {
        _reference = null;
        
        Number = value;
        Type = DynamicValueType.Number;
    }

    public DynamicValue(string value)
    {
        _reference = value;
        
        Type = DynamicValueType.String;
        Number = 0;
    }

    public DynamicValue(char value)
    {
        _reference = value.ToString();
        
        Type = DynamicValueType.String;
        Number = 0;
    }

    public DynamicValue(bool value)
    {
        _reference = null;
        
        Number = value ? 1.0 : 0.0;
        Type = DynamicValueType.Boolean;
    }

    public DynamicValue(Table value)
    {
        _reference = value;
        
        Type = DynamicValueType.Table;
        Number = 0;
    }

    public DynamicValue(Collections_Array value)
    {
        _reference = value;
        
        Type = DynamicValueType.Array;
        Number = 0;
    }

    public DynamicValue(Fiber value)
    {
        _reference = value;
        
        Type = DynamicValueType.Fiber;
        Number = 0;
    }

    public DynamicValue(Chunk value)
    {
        _reference = value;
        
        Type = DynamicValueType.Chunk;
        Number = 0;
    }

    public DynamicValue(Error value)
    {
        _reference = value;
    
        Type = DynamicValueType.Error;
        Number = 0;
    }

    public DynamicValue(DynamicValueType value)
    {
        _reference = null;
        
        Number = (double)(int)value;
        Type = DynamicValueType.TypeCode;
    }

    public DynamicValue(NativeFunction value)
    {
        Type = DynamicValueType.NativeFunction;
        Number = 0;
        _reference = value;
    }

    public DynamicValue(object? value)
    {
        _reference = value;
        
        Type = DynamicValueType.NativeObject;
        Number = 0;
    }

    public void Serialize(Stream stream, bool throwOnUnsupported = false)
        => DynamicValueSerializer.Serialize(this, stream, throwOnUnsupported);

    public static DynamicValue Deserialize(Stream stream)
        => DynamicValueSerializer.Deserialize(stream);
        
    public static bool IsTruth(DynamicValue value) 
        => value != Nil
           && value != Zero
           && value != False;

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"<{Type}>");

        switch (Type)
        {
            case DynamicValueType.Number:
                sb.Append(' ');
                sb.Append(Number);
                break;
                
            case DynamicValueType.String:
                sb.Append(' ');
                sb.Append($"\"{String}\"");
                break;
                
            case DynamicValueType.Boolean:
                sb.Append(' ');
                sb.Append(Boolean);
                break;

            case DynamicValueType.Error:
            {
                var msg = Error!["msg"];

                if (msg == DynamicValueType.String)
                {
                    sb.Append($"'{msg.String}'");
                }
                else
                {
                    sb.Append($"[{msg.Error!.Length} userdata record(s)]");
                }

                break;
            }
                
            case DynamicValueType.Nil:
                break;
                
            default:
                sb.Append(' ');
                sb.Append(this.ConvertToString());
                break;
        }

        return sb.ToString();
    }

    public bool Equals(DynamicValue other) 
    {
        if (Type != other.Type) return false;
        return Type switch
        {
            DynamicValueType.Nil => true,
            DynamicValueType.Number => Number.Equals(other.Number),
            DynamicValueType.Boolean => (Number != 0) == (other.Number != 0),
            DynamicValueType.TypeCode => (int)Number == (int)other.Number,
            DynamicValueType.String => ReferenceEquals(_reference, other._reference) || String == other.String,
            DynamicValueType.NativeObject => Equals(_reference, other._reference) 
                                          || (_reference != null 
                                              && other._reference != null 
                                              && _reference.GetType() == other._reference.GetType() 
                                              && _reference.ToString() == other._reference.ToString()),
            _ => Equals(_reference, other._reference)
        };
    }

    public override bool Equals(object? obj) 
        => obj is DynamicValue other 
           && Equals(other);

    public override int GetHashCode()
    {
        return Type switch
        {
            DynamicValueType.Nil => 0,
            DynamicValueType.Number => HashCode.Combine((int)Type, Number),
            DynamicValueType.Boolean => HashCode.Combine((int)Type, Boolean),
            DynamicValueType.TypeCode => HashCode.Combine((int)Type, (int)TypeCode),
            _ => HashCode.Combine((int)Type, _reference)
        };
    }

    public static bool operator ==(DynamicValue left, DynamicValue right) 
        => left.Equals(right);

    public static bool operator !=(DynamicValue left, DynamicValue right) 
        => !left.Equals(right);

    public static implicit operator DynamicValue(double value) => new(value);
    public static implicit operator DynamicValue(string value) => new(value);
    public static implicit operator DynamicValue(char value) => new(value.ToString());
    public static implicit operator DynamicValue(bool value) => new(value);
    public static implicit operator DynamicValue(Table value) => new(value);
    public static implicit operator DynamicValue(Collections_Array value) => new(value);
    public static implicit operator DynamicValue(Fiber value) => new(value);
    public static implicit operator DynamicValue(Chunk value) => new(value);
    public static implicit operator DynamicValue(Error value) => new(value);
    public static implicit operator DynamicValue(DynamicValueType value) => new(value);
    public static implicit operator DynamicValue(NativeFunction value) => new(value);
    public static DynamicValue FromObject(object value) => new(value);

    public static explicit operator double(DynamicValue value)
    {
        return value.Type != DynamicValueType.Number 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.") 
            : value.Number;
    }
        
    public static explicit operator float(DynamicValue value)
    {
        return value.Type != DynamicValueType.Number
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.")
            : (float)value.Number;
    }
        
    public static explicit operator int(DynamicValue value)
    {
        return value.Type != DynamicValueType.Number
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.")
            : (int)value.Number;
    }
        
    public static explicit operator uint(DynamicValue value)
    {
        return value.Type != DynamicValueType.Number
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.")
            : (uint)value.Number;
    }
        
    public static explicit operator long(DynamicValue value)
    {
        return value.Type != DynamicValueType.Number
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.")
            : (long)value.Number;
    }
        
    public static explicit operator ulong(DynamicValue value)
    {
        return value.Type != DynamicValueType.Number
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.")
            : (ulong)value.Number;
    }
        
    public static explicit operator string(DynamicValue value)
        => value.ConvertToString().String!;

    public static explicit operator bool(DynamicValue value)
    {
        return value.Type != DynamicValueType.Boolean 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Boolean.") 
            : value.Boolean;
    }

    public static explicit operator Table(DynamicValue value)
    {
        return value.Type != DynamicValueType.Table 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Table.")
            : value.Table!;
    }

    public static explicit operator Collections_Array(DynamicValue value)
    {
        return value.Type != DynamicValueType.Array 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to an Array.") 
            : value.Array!;
    }

    public static explicit operator Fiber(DynamicValue value)
    {
        return value.Type != DynamicValueType.Fiber 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Fiber.") 
            : value.Fiber!;
    }
        
    public static explicit operator Chunk(DynamicValue value)
    {
        return value.Type != DynamicValueType.Chunk
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Chunk.") 
            : value.Chunk!;
    }

    public static explicit operator Error(DynamicValue value)
    {
        return value.Type != DynamicValueType.Error 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to an Error.")
            : value.Error!;
    }
        
    public static explicit operator DynamicValueType(DynamicValue value)
    {
        return value.Type != DynamicValueType.TypeCode 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a TypeCode") 
            : value.TypeCode;
    }
        
    public static explicit operator NativeFunction(DynamicValue value)
    {
        return value.Type != DynamicValueType.NativeFunction 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a NativeFunction.") 
            : value.NativeFunction!;
    }

    public static object ToObject(DynamicValue value)
    {
        return value.Type != DynamicValueType.NativeObject 
            ? throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a NativeObject.") 
            : value.NativeObject!;
    }        
}