namespace EVIL.Ceres.ExecutionEngine.TypeSystem;

using System;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.TypeSystem;

public static partial class DynamicValueOperations
{
    private const double LongRangeUpperExclusive = 9223372036854775808d;
    private const double LongRangeLowerInclusive = -9223372036854775808d;
    
    public static DynamicValue Contains(this DynamicValue a, DynamicValue b)
    {
        if (a.Type == DynamicValueType.String)
        {
            if (b.Type == DynamicValueType.String)
            {
                return new(a.String!.Contains(b.String!, StringComparison.InvariantCulture));
            }
        }

        if (a.Type == DynamicValueType.Table)
        {
            if (b.Type == DynamicValueType.String || b.Type == DynamicValueType.Number)
            {
                return new(a.Table!.Contains(b));
            }
        }

        if (a.Type == DynamicValueType.Array)
        {
            return new(a.Array!.IndexOf(b) >= 0);
        }

        if (a.Type == DynamicValueType.Error)
        {
            return new(a.Error!.Contains(b));
        }

        throw new UnsupportedDynamicValueOperationException(
            $"Attempt to check if a {a.Type} contains a {b.Type}."
        );
    }

    public static DynamicValue GetLength(this DynamicValue a)
    {
        if (a.Type == DynamicValueType.String)
        {
            return new(a.String!.Length);
        }

        if (a.Type == DynamicValueType.Array)
        {
            return new(a.Array!.Length);
        }

        if (a.Type == DynamicValueType.Table)
        {
            return new(a.Table!.Length);
        }

        if (a.Type == DynamicValueType.Error)
        {
            return new(a.Error!.Length);
        }

        throw new UnsupportedDynamicValueOperationException(
            $"Attempt to get length of a {a.Type}."
        );
    }

    public static DynamicValue Index(this DynamicValue a, DynamicValue key)
    {
        switch (a.Type)
        {
            case DynamicValueType.Table:
                return a.Table![key];
                
            case DynamicValueType.NativeObject when a.NativeObject is IIndexableObject indexable:
                return indexable.Index(key);

            case DynamicValueType.Array when key.Type != DynamicValueType.Number:
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to index an {a.Type} using a {key.Type}."
                );

            case DynamicValueType.Array:
                return a.Array![(int)key.Number];

            case DynamicValueType.String when key.Type != DynamicValueType.Number:
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to index a {a.Type} using a {key.Type}."
                );

            case DynamicValueType.String when key.Number % 1 != 0:
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to index a {a.Type} using a fractional number."
                );

            case DynamicValueType.String when key.Number < 0 || key.Number >= a.String!.Length:
                return DynamicValue.Nil;

            case DynamicValueType.String:
                return new(a.String![(int)key.Number]);

            case DynamicValueType.Chunk when key.Type != DynamicValueType.String:
                throw new UnsupportedDynamicValueOperationException(
                    "Chunks may only be indexed using a String."
                );

            case DynamicValueType.Chunk:
                return a.Chunk![key.String!] ?? DynamicValue.Nil;

            case DynamicValueType.Error:
                return a.Error![key];
                
            default:
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to index a non-indexable {a.Type} value."
                );
        }
    }

    internal static DynamicValue Slice(
        this DynamicValue value,
        DynamicValue? startValue,
        DynamicValue? endValue,
        SliceFlags flags)
    {
        const SliceFlags knownFlags = SliceFlags.StartPresent
                                      | SliceFlags.EndPresent
                                      | SliceFlags.StartFromEnd
                                      | SliceFlags.EndFromEnd
                                      | SliceFlags.ScalarResult;

        if ((flags & ~knownFlags) != 0
            || (flags.HasFlag(SliceFlags.StartFromEnd) && !flags.HasFlag(SliceFlags.StartPresent))
            || (flags.HasFlag(SliceFlags.EndFromEnd) && !flags.HasFlag(SliceFlags.EndPresent))
            || (flags.HasFlag(SliceFlags.StartPresent) != startValue.HasValue)
            || (flags.HasFlag(SliceFlags.EndPresent) != endValue.HasValue))
        {
            throw new UnsupportedDynamicValueOperationException(
                "Invalid SLICE instruction flags."
            );
        }

        if (value.Type is not (DynamicValueType.Array or DynamicValueType.String))
        {
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to slice a non-sliceable {value.Type} value."
            );
        }

        var length = value.Type == DynamicValueType.Array
            ? value.Array!.Length
            : value.String!.Length;
        var isScalar = flags.HasFlag(SliceFlags.ScalarResult);

        if (isScalar && (!flags.HasFlag(SliceFlags.StartPresent) || flags.HasFlag(SliceFlags.EndPresent)))
        {
            throw new UnsupportedDynamicValueOperationException(
                "Invalid scalar SLICE instruction flags."
            );
        }

        var start = flags.HasFlag(SliceFlags.StartPresent)
            ? ResolveSliceBound(startValue!.Value, flags.HasFlag(SliceFlags.StartFromEnd), length)
            : 0L;
        
        var end = flags.HasFlag(SliceFlags.EndPresent)
            ? ResolveSliceBound(endValue!.Value, flags.HasFlag(SliceFlags.EndFromEnd), length)
            : length;

        if (isScalar)
        {
            if (start < 0 || start >= length)
            {
                throw new UnsupportedDynamicValueOperationException(
                    $"Index {start} was out of bounds for a collection of length {length}."
                );
            }

            return value.Type == DynamicValueType.Array
                ? value.Array![(int)start]
                : new DynamicValue(value.String![(int)start]);
        }

        if (start < 0 || end < 0 || start > end || end > length)
        {
            throw new UnsupportedDynamicValueOperationException(
                $"Slice bounds [{start}..{end}] are invalid for a collection of length {length}."
            );
        }

        var sliceLength = (int)(end - start);
        if (value.Type == DynamicValueType.String)
        {
            return new DynamicValue(value.String!.Substring((int)start, sliceLength));
        }

        var items = new DynamicValue[sliceLength];
        for (var i = 0; i < sliceLength; i++)
        {
            items[i] = value.Array![(int)start + i];
        }

        return new DynamicValue(new Collections.Array(items));
    }

    private static long ResolveSliceBound(DynamicValue value, bool fromEnd, int length)
    {
        if (value.Type != DynamicValueType.Number)
        {
            throw new UnsupportedDynamicValueOperationException(
                $"A slice bound must be a Number, not a {value.Type}."
            );
        }

        var number = value.Number;
        if (!double.IsFinite(number))
        {
            throw new UnsupportedDynamicValueOperationException("Slice bounds must be finite numbers.");
        }

        if (number % 1 != 0)
        {
            throw new UnsupportedDynamicValueOperationException("Slice bounds must be integral numbers.");
        }
        
        if (number < LongRangeLowerInclusive || number >= LongRangeUpperExclusive)
        {
            throw new UnsupportedDynamicValueOperationException(
                "Slice bound is outside the range of representable indices."
            );
        }

        var bound = (long)number;
        if (!fromEnd)
        {
            return bound;
        }

        try
        {
            return checked(length + bound);
        }
        catch (OverflowException)
        {
            throw new UnsupportedDynamicValueOperationException(
                "Resolved from-end slice bound is outside the range of representable indices."
            );
        }
    }

    public static DynamicValue ShiftLeft(this DynamicValue a, DynamicValue b)
    {
        if (b.Type != DynamicValueType.Number)
        {
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to left-shift a {a.Type} by a {b.Type}."
            );
        }

        if (a.Type == DynamicValueType.Number)
        {
            return new((long)a.Number << (int)b.Number);
        }

        if (a.Type == DynamicValueType.String)
        {
            var amount = (int)b.Number;
            var str = a.String!;

            if (amount >= str.Length)
            {
                return DynamicValue.EmptyString;
            }

            return new(str[amount..]);
        }

        throw new UnsupportedDynamicValueOperationException(
            $"Attempt to left-shift a {a.Type}."
        );
    }

    public static DynamicValue ShiftRight(this DynamicValue a, DynamicValue b)
    {
        if (b.Type != DynamicValueType.Number)
        {
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to right-shift a {a.Type} by a {b.Type}."
            );
        }

        if (a.Type == DynamicValueType.Number)
        {
            return new((long)a.Number >> (int)b.Number);
        }

        if (a.Type == DynamicValueType.String)
        {
            var str = a.String!;
            var amount = str.Length - (int)b.Number;

            if (amount <= 0)
            {
                return DynamicValue.EmptyString;
            }

            return new(str.Substring(0, amount));
        }

        throw new UnsupportedDynamicValueOperationException(
            $"Attempt to right-shift a {a.Type}."
        );
    }
}