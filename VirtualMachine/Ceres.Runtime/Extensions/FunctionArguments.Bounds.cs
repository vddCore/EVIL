using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Extensions
{
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

        public static DynamicValue[] ExpectTypeAt(this DynamicValue[] args, int index, DynamicValueType type)
        {
            args.ExpectAtLeast(index + 1);

            var argType = args[index].Type;
            
            if (argType != type)
                throw new EvilRuntimeException($"Expected a {type} at argument index {index}, found {argType}.");

            return args;
        }
    }
}