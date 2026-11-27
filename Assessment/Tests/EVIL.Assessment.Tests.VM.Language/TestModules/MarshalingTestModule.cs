namespace EVIL.Assessment.Tests.VM.Language.TestModules;

using System;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Collections;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Marshaling.Adapters;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.Runtime;
using EVIL.VM.Runtime.Extensions;

public sealed class MarshalingTestModule : RuntimeModule
{
    public override string FullyQualifiedName => "marshaling";

    [RuntimeModuleFunction("create_point")]
    private static DynamicValue CreatePoint(Fiber fiber, params DynamicValue[] args)
    {
        args.ExpectExactly(2)
            .ExpectNumberAt(0, out var x)
            .ExpectNumberAt(1, out var y);

        return fiber.VirtualMachine.Marshal.ToDynamicValue(new MarshalingTestPoint(x, y));
    }

    [RuntimeModuleFunction("sum_point")]
    private static DynamicValue SumPoint(Fiber fiber, params DynamicValue[] args)
    {
        args.ExpectExactly(1);
        var point = fiber.VirtualMachine.Marshal.ToNative<MarshalingTestPoint>(args[0]);

        return point.X + point.Y;
    }
}

public sealed record MarshalingTestPoint(double X, double Y);

public sealed class MarshalingTestPointAdapter : IValueAdapter<MarshalingTestPoint>
{
    public DynamicValue ToDynamicValue(MarshalingTestPoint value)
    {
        var table = new Table
        {
            ["x"] = value.X,
            ["y"] = value.Y
        };

        return table;
    }

    public MarshalingTestPoint FromDynamicValue(DynamicValue value)
    {
        if (value.Type != DynamicValueType.Table)
        {
            throw new InvalidCastException($"Expected an EVIL table, found '{value.Type}'.");
        }

        var table = value.Table!;
        if (table["x"].Type != DynamicValueType.Number || table["y"].Type != DynamicValueType.Number)
        {
            throw new InvalidCastException("The EVIL table must contain numeric 'x' and 'y' fields.");
        }

        return new MarshalingTestPoint(table["x"].Number, table["y"].Number);
    }
}