namespace EVIL.VM.ExecutionEngine.Marshaling;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal sealed class ReferenceComparer : IEqualityComparer<object>
{
    public static readonly ReferenceComparer Instance = new();

    public new bool Equals(object? x, object? y)
        => ReferenceEquals(x, y);

    public int GetHashCode(object value)
        => RuntimeHelpers.GetHashCode(value);
}