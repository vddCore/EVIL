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

public struct DynamicValue : IEquatable<DynamicValue>
{
    public static readonly DynamicValue Nil = new();
    public static readonly DynamicValue One = new((double)1);
    public static readonly DynamicValue Zero = new((double)0);
    public static readonly DynamicValue True = new(true);
    public static readonly DynamicValue False = new(false);
    public static readonly DynamicValue EmptyString = new("");
        
    public DynamicValueType Type { get; }

    public double Number { get; }
    public string? String { get; }
    public bool Boolean { get; }
    public Table? Table { get; }
    public Collections_Array? Array { get; }
    public Fiber? Fiber { get; }
    public Chunk? Chunk { get; }
    public Error? Error { get; }
    public DynamicValueType TypeCode { get; }
    public NativeFunction? NativeFunction { get; }
    public object? NativeObject { get; }

    public DynamicValue()
    {
        Type = DynamicValueType.Nil;
    }
        
    public DynamicValue(double value)
    {
        Number = value;
        Type = DynamicValueType.Number;
    }

    public DynamicValue(string value)
    {
        String = value;
        Type = DynamicValueType.String;
    }

    public DynamicValue(char value)
    {
        String = value.ToString();
        Type = DynamicValueType.String;
    }

    public DynamicValue(bool value)
    {
        Boolean = value;
        Type = DynamicValueType.Boolean;
    }

    public DynamicValue(Table value)
    {
        Table = value;
        Type = DynamicValueType.Table;
    }

    public DynamicValue(Collections_Array value)
    {
        Array = value;
        Type = DynamicValueType.Array;
    }

    public DynamicValue(Fiber value)
    {
        Fiber = value;
        Type = DynamicValueType.Fiber;
    }

    public DynamicValue(Chunk value)
    {
        Chunk = value;
        Type = DynamicValueType.Chunk;
    }

    public DynamicValue(Error value)
    {
        Error = value;
        Type = DynamicValueType.Error;
    }

    public DynamicValue(DynamicValueType value)
    {
        TypeCode = value;
        Type = DynamicValueType.TypeCode;
    }

    public DynamicValue(NativeFunction value)
    {
        NativeFunction = value;
        Type = DynamicValueType.NativeFunction;
    }

    public DynamicValue(object? value)
    {
        NativeObject = value;
        Type = DynamicValueType.NativeObject;
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
        => Type == other.Type 
           && Number.Equals(other.Number) 
           && String == other.String
           && Boolean == other.Boolean
           && Equals(Table, other.Table) 
           && Equals(Array, other.Array)
           && Equals(Fiber, other.Fiber)
           && Equals(Chunk, other.Chunk)
           && Equals(Error, other.Error)
           && TypeCode == other.TypeCode
           && Equals(NativeFunction, other.NativeFunction);

    public override bool Equals(object? obj) 
        => obj is DynamicValue other 
           && Equals(other);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
            
        hashCode.Add((int)Type);
        hashCode.Add(Number);
        hashCode.Add(String);
        hashCode.Add(Boolean);
        hashCode.Add(Table);
        hashCode.Add(Array);
        hashCode.Add(Fiber);
        hashCode.Add(Chunk);
        hashCode.Add(Error);
        hashCode.Add(TypeCode);
        hashCode.Add(NativeFunction);
        hashCode.Add(NativeObject);
            
        return hashCode.ToHashCode();
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
        if (value.Type != DynamicValueType.Number)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.");

        return value.Number;
    }
        
    public static explicit operator float(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Number)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.");

        return (float)value.Number;
    }
        
    public static explicit operator int(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Number)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.");

        return (int)value.Number;
    }
        
    public static explicit operator uint(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Number)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.");

        return (uint)value.Number;
    }
        
    public static explicit operator long(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Number)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.");

        return (long)value.Number;
    }
        
    public static explicit operator ulong(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Number)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Number.");

        return (ulong)value.Number;
    }
        
    public static explicit operator string(DynamicValue value)
    {
        return value.ConvertToString().String!;
    }
        
    public static explicit operator bool(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Boolean)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Boolean.");

        return value.Boolean;
    }

    public static explicit operator Table(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Table)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Table.");

        return value.Table!;
    }

    public static explicit operator Collections_Array(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Array)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to an Array.");

        return value.Array!;
    }

    public static explicit operator Fiber(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Fiber)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Fiber.");

        return value.Fiber!;
    }
        
    public static explicit operator Chunk(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Chunk)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a Chunk.");

        return value.Chunk!;
    }

    public static explicit operator Error(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Error)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to an Error.");

        return value.Error!;
    }
        
    public static explicit operator DynamicValueType(DynamicValue value)
    {
        if (value.Type != DynamicValueType.TypeCode)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a TypeCode");

        return value.TypeCode;
    }
        
    public static explicit operator NativeFunction(DynamicValue value)
    {
        if (value.Type != DynamicValueType.NativeFunction)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a NativeFunction.");

        return value.NativeFunction!;
    }

    public static object ToObject(DynamicValue value)
    {
        if (value.Type != DynamicValueType.NativeObject)
            throw new InvalidCastException($"Cannot cast dynamic type '{value.Type}' to a NativeObject.");

        return value.NativeObject!;
    }        
}