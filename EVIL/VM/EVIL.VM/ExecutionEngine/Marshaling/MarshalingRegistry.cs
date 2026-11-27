namespace EVIL.VM.ExecutionEngine.Marshaling;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Marshaling.Adapters;
using EVIL.VM.ExecutionEngine.Marshaling.Adapters.Internal;
using EVIL.VM.ExecutionEngine.TypeSystem;

public sealed class MarshalingRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<Type, SimpleAdapter> _adapters = new();

    private bool _isFrozen;
    private MarshalingFallbackMode _fallbackMode;

    public bool IsFrozen
    {
        get
        {
            lock (_gate)
            {
                return _isFrozen;
            }
        }
    }

    public MarshalingFallbackMode FallbackMode
    {
        get
        {
            lock (_gate)
            {
                return _fallbackMode;
            }
        }
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            lock (_gate)
            {
                EnsureMutable();
                _fallbackMode = value;
            }
        }
    }

    public MarshalingRegistry(MarshalingFallbackMode fallbackMode = MarshalingFallbackMode.WrapNativeObject)
    {
        if (!Enum.IsDefined(fallbackMode))
        {
            throw new ArgumentOutOfRangeException(nameof(fallbackMode), fallbackMode, null);
        }

        _fallbackMode = fallbackMode;
        RegisterBuiltInAdapters();
    }

    public void Register<T>(IValueAdapter<T> adapter, bool replaceExisting = false)
    {
        ArgumentNullException.ThrowIfNull(adapter);

        RegisterAdapter(typeof(T), new SimpleAdapter<T>(adapter), replaceExisting);
    }

    public void Register<T>(IContextualValueAdapter<T> adapter, bool replaceExisting = false)
    {
        ArgumentNullException.ThrowIfNull(adapter);

        RegisterAdapter(typeof(T), new ContextualAdapter<T>(adapter), replaceExisting);
    }

    private void RegisterAdapter(Type nativeType, SimpleAdapter adapter, bool replaceExisting)
    {
        if (nativeType.ContainsGenericParameters
            || nativeType.IsByRef
            || nativeType.IsPointer
            || nativeType == typeof(void)
            || Nullable.GetUnderlyingType(nativeType) != null)
        {
            throw new ArgumentException(
                $"'{nativeType}' is not a supported adapter type.",
                nameof(adapter)
            );
        }

        lock (_gate)
        {
            EnsureMutable();

            if (_adapters.ContainsKey(nativeType) && !replaceExisting)
            {
                throw new InvalidOperationException(
                    $"An adapter for CLR type '{nativeType}' is already registered. " +
                    "Pass replaceExisting: true to replace it explicitly."
                );
            }

            _adapters[nativeType] = adapter;
        }
    }

    public void Freeze()
    {
        lock (_gate)
        {
            _isFrozen = true;
        }
    }

    public DynamicValue ToDynamicValue(object? value)
        => ToDynamicValue(value, new MarshalingContext(this));

    internal DynamicValue ToDynamicValue(object? value, MarshalingContext context)
    {
        if (value == null)
        {
            return DynamicValue.Nil;
        }

        if (value is DynamicValue dynamicValue)
        {
            return dynamicValue;
        }

        var sourceType = value.GetType();
        if (context.TryGetDynamicValue(value, out var cachedValue))
        {
            return cachedValue;
        }

        if (!context.TryEnterClrReference(value))
        {
            throw new MarshalingException(
                MarshalingDirection.ClrToEvil,
                sourceType.FullName ?? sourceType.Name,
                null,
                "A cycle was encountered before the adapter registered a placeholder EVIL value."
            );
        }

        try
        {
            var adapter = ResolveForClrType(sourceType, out var ambiguousTypes);
            if (ambiguousTypes.Length > 0)
            {
                throw CreateAmbiguousAdapterException(
                    MarshalingDirection.ClrToEvil,
                    sourceType.FullName ?? sourceType.Name,
                    null,
                    ambiguousTypes
                );
            }

            DynamicValue converted;
            if (adapter != null)
            {
                try
                {
                    converted = adapter.ToDynamicValue(value, context);
                }
                catch (Exception exception)
                {
                    throw new MarshalingException(
                        MarshalingDirection.ClrToEvil,
                        sourceType.FullName ?? sourceType.Name,
                        null,
                        "The registered adapter failed.",
                        exception
                    );
                }
            }
            else if (FallbackMode == MarshalingFallbackMode.WrapNativeObject)
            {
                converted = new DynamicValue(value);
            }
            else
            {
                throw new MarshalingException(
                    MarshalingDirection.ClrToEvil,
                    sourceType.FullName ?? sourceType.Name,
                    null,
                    "No adapter is registered and native-object fallback is disabled."
                );
            }

            context.RegisterDynamicValue(value, converted);
            return converted;
        }
        finally
        {
            context.ExitClrReference(value);
        }
    }

    public T ToNative<T>(DynamicValue value)
        => (T)ToNative(value, typeof(T))!;

    public object? ToNative(DynamicValue value, Type targetType)
        => ToNative(value, targetType, new MarshalingContext(this));

    internal object? ToNative(DynamicValue value, Type targetType, MarshalingContext context)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        if (targetType.ContainsGenericParameters
            || targetType.IsByRef
            || targetType.IsPointer
            || targetType == typeof(void))
        {
            throw new ArgumentException($"'{targetType}' is not a supported target type.", nameof(targetType));
        }

        if (targetType == typeof(DynamicValue))
        {
            return value;
        }

        var effectiveTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        
        var acceptsNull = !targetType.IsValueType 
                          || Nullable.GetUnderlyingType(targetType) != null;
        
        var sourceType = GetEvilSourceType(value);

        if (value.Type == DynamicValueType.Nil)
        {
            if (acceptsNull)
            {
                return null;
            }

            throw new MarshalingException(
                MarshalingDirection.EvilToClr,
                sourceType,
                targetType,
                "EVIL nil cannot be converted to a non-nullable CLR type."
            );
        }

        if (value.Type == DynamicValueType.NativeObject)
        {
            var nativeObject = value.NativeObject;
            if (nativeObject == null)
            {
                if (acceptsNull)
                {
                    return null;
                }

                throw new MarshalingException(
                    MarshalingDirection.EvilToClr,
                    sourceType,
                    targetType,
                    "The wrapped native object is null and the target type is non-nullable."
                );
            }

            if (effectiveTargetType.IsInstanceOfType(nativeObject))
            {
                return nativeObject;
            }
        }

        if (context.TryGetNativeObject(value, effectiveTargetType, out var cachedValue))
        {
            return cachedValue;
        }

        var adapter = ResolveForTargetType(effectiveTargetType, out var ambiguousTypes);
        if (ambiguousTypes.Length > 0)
        {
            throw CreateAmbiguousAdapterException(
                MarshalingDirection.EvilToClr,
                sourceType,
                targetType,
                ambiguousTypes
            );
        }

        if (adapter == null)
        {
            throw new MarshalingException(
                MarshalingDirection.EvilToClr,
                sourceType,
                targetType,
                "No adapter is registered for the requested CLR type."
            );
        }

        if (!context.TryEnterNativeReference(value, effectiveTargetType))
        {
            throw new MarshalingException(
                MarshalingDirection.EvilToClr,
                sourceType,
                targetType,
                "A cycle was encountered before the adapter registered a placeholder CLR object."
            );
        }

        try
        {
            try
            {
                var converted = adapter.FromDynamicValue(value, context);
                if (converted == null)
                {
                    if (acceptsNull)
                    {
                        return null;
                    }

                    throw new InvalidCastException("The adapter returned null for a non-nullable target type.");
                }

                if (!effectiveTargetType.IsInstanceOfType(converted))
                {
                    throw new InvalidCastException(
                        $"The adapter returned '{converted.GetType()}', which is not assignable to '{targetType}'."
                    );
                }

                if (context.HasReference(value))
                {
                    context.RegisterNativeObject(value, converted);
                }

                return converted;
            }
            catch (Exception exception)
            {
                throw new MarshalingException(
                    MarshalingDirection.EvilToClr,
                    sourceType,
                    targetType,
                    "The registered adapter failed.",
                    exception
                );
            }
        }
        finally
        {
            context.ExitNativeReference(value, effectiveTargetType);
        }
    }

    private void RegisterBuiltInAdapters()
    {
        Register(new DelegatingAdapter<bool>(
            value => value,
            value => RequireType(value, DynamicValueType.Boolean).Boolean
        ));
        
        Register(new DelegatingAdapter<char>(
            value => value,
            value =>
            {
                var text = RequireType(value, DynamicValueType.String).String!;
                
                return text.Length == 1
                    ? text[0]
                    : throw new InvalidCastException("An EVIL string must contain exactly one character.");
            }
        ));
        
        Register(new DelegatingAdapter<string>(
            value => value,
            value => RequireType(value, DynamicValueType.String).String!
        ));

        RegisterIntegerAdapter(sbyte.MinValue, sbyte.MaxValue, value => (sbyte)value);
        RegisterIntegerAdapter(byte.MinValue, byte.MaxValue, value => (byte)value);
        RegisterIntegerAdapter(short.MinValue, short.MaxValue, value => (short)value);
        RegisterIntegerAdapter(ushort.MinValue, ushort.MaxValue, value => (ushort)value);
        RegisterIntegerAdapter(int.MinValue, int.MaxValue, value => (int)value);
        RegisterIntegerAdapter(uint.MinValue, uint.MaxValue, value => (uint)value);
        RegisterIntegerAdapter(long.MinValue, long.MaxValue, value => (long)value);
        RegisterIntegerAdapter(ulong.MinValue, ulong.MaxValue, value => (ulong)value);

        Register(new DelegatingAdapter<float>(
            value => new DynamicValue(value),
            value => ToSingle(RequireType(value, DynamicValueType.Number).Number)
        ));
        
        Register(new DelegatingAdapter<double>(
            value => new DynamicValue(value),
            value => RequireType(value, DynamicValueType.Number).Number
        ));
        
        Register(new DelegatingAdapter<decimal>(
            FromDecimal,
            value => ToDecimal(RequireType(value, DynamicValueType.Number).Number)
        ));
    }

    private void RegisterIntegerAdapter<T>(BigInteger minimum, BigInteger maximum, Func<BigInteger, T> convert)
        where T : struct
    {
        Register(new DelegatingAdapter<T>(
            value => FromInteger(value),
            value => convert(RequireInteger(value, minimum, maximum))
        ));
    }

    private SimpleAdapter? ResolveForClrType(Type sourceType, out Type[] ambiguousTypes)
    {
        lock (_gate)
        {
            if (_adapters.TryGetValue(sourceType, out var exactMatch))
            {
                ambiguousTypes = [];
                return exactMatch;
            }

            return SelectMostSpecific(
                _adapters.Values.Where(adapter => adapter.NativeType.IsAssignableFrom(sourceType)),
                out ambiguousTypes
            );
        }
    }

    private SimpleAdapter? ResolveForTargetType(Type targetType, out Type[] ambiguousTypes)
    {
        lock (_gate)
        {
            if (_adapters.TryGetValue(targetType, out var exactMatch))
            {
                ambiguousTypes = [];
                return exactMatch;
            }

            return SelectMostSpecific(
                _adapters.Values.Where(adapter => targetType.IsAssignableFrom(adapter.NativeType)),
                out ambiguousTypes
            );
        }
    }

    private static SimpleAdapter? SelectMostSpecific(
        IEnumerable<SimpleAdapter> candidates,
        out Type[] ambiguousTypes)
    {
        var candidateArray = candidates.ToArray();
        var mostSpecific = candidateArray
            .Where(candidate => !candidateArray.Any(other =>
                !ReferenceEquals(candidate, other)
                && candidate.NativeType.IsAssignableFrom(other.NativeType)))
            .ToArray();

        ambiguousTypes = mostSpecific.Length > 1
            ? mostSpecific.Select(adapter => adapter.NativeType).ToArray()
            : [];

        return mostSpecific.Length == 1 ? mostSpecific[0] : null;
    }

    private static MarshalingException CreateAmbiguousAdapterException(
        MarshalingDirection direction,
        string sourceType,
        Type? targetType,
        IEnumerable<Type> ambiguousTypes)
    {
        var typeNames = string.Join(", ", ambiguousTypes.Select(type => $"'{type.FullName ?? type.Name}'"));
        return new MarshalingException(
            direction,
            sourceType,
            targetType,
            $"Adapter resolution is ambiguous between {typeNames}."
        );
    }

    private static DynamicValue RequireType(DynamicValue value, DynamicValueType expectedType)
    {
        return value.Type == expectedType
            ? value
            : throw new InvalidCastException(
                $"Cannot convert EVIL type '{value.Type}' to expected type '{expectedType}'."
            );
    }

    private static DynamicValue FromInteger(object value)
    {
        var integer = value switch
        {
            sbyte number => new BigInteger(number),
            byte number => new BigInteger(number),
            short number => new BigInteger(number),
            ushort number => new BigInteger(number),
            int number => new BigInteger(number),
            uint number => new BigInteger(number),
            long number => new BigInteger(number),
            ulong number => new BigInteger(number),
            _ => throw new InvalidOperationException($"Unsupported integer type '{value.GetType()}'.")
        };

        var numberValue = Convert.ToDouble(value);
        if (new BigInteger(numberValue) != integer)
        {
            throw new OverflowException($"'{value}' cannot be represented exactly as an EVIL number.");
        }

        return new DynamicValue(numberValue);
    }

    private static BigInteger RequireInteger(DynamicValue value, BigInteger minimum, BigInteger maximum)
    {
        var number = RequireType(value, DynamicValueType.Number).Number;
        if (!double.IsFinite(number) || Math.Truncate(number) != number)
        {
            throw new InvalidCastException("The EVIL number must be a finite integer.");
        }

        var integer = new BigInteger(number);
        if (integer < minimum || integer > maximum)
        {
            throw new OverflowException($"EVIL number '{number}' is outside the target integer range.");
        }

        return integer;
    }

    private static DynamicValue FromDecimal(decimal value)
    {
        var number = (double)value;
        if (!IsExactDecimal(number, value))
        {
            throw new OverflowException($"'{value}' cannot be represented exactly as an EVIL number.");
        }

        return new DynamicValue(number);
    }

    private static decimal ToDecimal(double number)
    {
        if (!double.IsFinite(number))
        {
            throw new OverflowException($"EVIL number '{number}' cannot be represented as a CLR decimal.");
        }

        var value = (decimal)number;
        if (!IsExactDecimal(number, value))
        {
            throw new OverflowException($"EVIL number '{number}' cannot be represented exactly as a CLR decimal.");
        }

        return value;
    }

    private static bool IsExactDecimal(double number, decimal value)
    {
        if (!double.IsFinite(number))
        {
            return false;
        }

        if (value == decimal.Zero)
        {
            return number == 0;
        }

        var decimalBits = decimal.GetBits(value);
        var scale = (decimalBits[3] >> 16) & 0x7F;
        var decimalSignificand = new BigInteger((uint)decimalBits[0])
                                 | (new BigInteger((uint)decimalBits[1]) << 32)
                                 | (new BigInteger((uint)decimalBits[2]) << 64);
        
        if ((decimalBits[3] & int.MinValue) != 0)
        {
            decimalSignificand = -decimalSignificand;
        }

        var bits = BitConverter.DoubleToInt64Bits(number);
        var exponentBits = (int)((bits >> 52) & 0x7FF);
        var fraction = (ulong)bits & 0x000FFFFFFFFFFFFF;
        
        var doubleSignificand = exponentBits == 0
            ? new BigInteger(fraction)
            : new BigInteger(fraction | (1UL << 52));

        if (bits < 0)
        {
            doubleSignificand = -doubleSignificand;
        }

        var exponent = (exponentBits == 0 ? 1 - 1023 : exponentBits - 1023) - 52;
        if (exponent >= 0)
        {
            var numerator = doubleSignificand << exponent;
            return decimalSignificand == numerator * BigInteger.Pow(10, scale);
        }

        return (decimalSignificand << -exponent) == doubleSignificand * BigInteger.Pow(10, scale);
    }

    private static float ToSingle(double number)
    {
        var value = (float)number;
        if (double.IsNaN(number) && float.IsNaN(value)
            || double.IsPositiveInfinity(number) && float.IsPositiveInfinity(value)
            || double.IsNegativeInfinity(number) && float.IsNegativeInfinity(value)
            || double.IsFinite(number) && float.IsFinite(value) && (double)value == number)
        {
            return value;
        }

        throw new OverflowException($"EVIL number '{number}' cannot be represented exactly as a CLR single.");
    }

    private static string GetEvilSourceType(DynamicValue value)
    {
        if (value is { Type: DynamicValueType.NativeObject, NativeObject: not null })
        {
            var type = value.NativeObject.GetType();
            return $"NativeObject ({type.FullName ?? type.Name})";
        }

        return value.Type.ToString();
    }

    private void EnsureMutable()
    {
        if (_isFrozen)
        {
            throw new InvalidOperationException("The marshaling registry is frozen.");
        }
    }
}