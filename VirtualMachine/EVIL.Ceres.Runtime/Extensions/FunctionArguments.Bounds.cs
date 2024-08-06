namespace EVIL.Ceres.Runtime.Extensions;

using EVIL.Ceres.ExecutionEngine.TypeSystem;

public static partial class FunctionArguments
{
    public static DynamicValue[] ExpectExactly(this DynamicValue[] args, int count)
    {
        if (count != args.Length)
            throw new EvilRuntimeException($"Expected {(count == 0 ? "no" : count)} arguments, found {args.Length}.");

        return args;
    }

    public static DynamicValue[] ExpectAtLeast(this DynamicValue[] args, int count)
    {
        if (args.Length < count)
            throw new EvilRuntimeException($"Expected at least {count} arguments, found {args.Length}.");

        return args;
    }

    public static DynamicValue[] ExpectAtMost(this DynamicValue[] args, int count)
    {
        if (args.Length > count)
            throw new EvilRuntimeException($"Expected at most {count} arguments, found {args.Length}.");

        return args;
    }

    public static DynamicValue[] ExpectNone(this DynamicValue[] args)
        => args.ExpectExactly(0);
}