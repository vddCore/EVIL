using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.RT
{
    public static class ArgsExtensions
    {
        public static DynamicValue[] ExpectExactly(this DynamicValue[] args, int count)
        {
            if (args.Length != count)
                throw new EvilRuntimeException($"Expected exactly {count} arguments, found {args.Length}.");

            return args;
        }

        public static DynamicValue[] DisallowType(this DynamicValue[] args, DynamicValueType type)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].Type == type)
                    throw new EvilRuntimeException($"The type '{type}' is invalid for this function call.");
            }

            return args;
        }

        public static DynamicValue[] ExpectTypeAtIndex(this DynamicValue[] args, int index, DynamicValueType type)
        {
            if (index >= args.Length)
                throw new EvilRuntimeException($"Expected type '{type}' at argument index {index}, found no value.");

            if (args[index].Type != type)
                throw new EvilRuntimeException(
                    $"Expected type '{type}' at argument index {index}, found '{args[index].Type}");

            return args;
        }

        public static DynamicValue[] ExpectTableAtIndex(this DynamicValue[] args, int index)
        {
            ExpectTypeAtIndex(args, index, DynamicValueType.Table);
            return args;
        }

        public static DynamicValue[] ExpectTableAtIndex(this DynamicValue[] args, int index, int size)
        {
            ExpectTableAtIndex(args, index);

            var tbl = args[index].Table;

            if (tbl.Entries.Count != size)
                throw new EvilRuntimeException(
                    $"Expected a table of size {size} at index {index}. Actual size was {tbl.Entries.Count}");

            return args;
        }

        public static DynamicValue[] ExpectTableAtIndex(this DynamicValue[] args, int index, int size,
            DynamicValueType acceptedType)
        {
            ExpectTableAtIndex(args, index);
            var tbl = args[index].Table;

            if (tbl.Entries.Count != size)
                throw new EvilRuntimeException(
                    $"Expected a table of size {size} at index {index}. Actual size was {tbl.Entries.Count}");

            foreach (var kvp in tbl.Entries)
            {
                if (kvp.Value.Type != acceptedType)
                {
                    throw new EvilRuntimeException(
                        $"Expected a table containing only {acceptedType}(s).");
                }
            }

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
        {
            if (args.Length != 0)
                throw new EvilRuntimeException($"Expected no arguments, found {args.Length}.");

            return args;
        }

        public static DynamicValue[] ExpectNumberAtIndex(this DynamicValue[] args, int index)
        {
            if (args[index].Type != DynamicValueType.Number && args[index].Type != DynamicValueType.Number)
                throw new EvilRuntimeException($"Expected a number value at argument index {index}.");

            return args;
        }

        public static DynamicValue[] ExpectIntegerAtIndex(this DynamicValue[] args, int index)
        {
            ExpectTypeAtIndex(args, index, DynamicValueType.Number);

            if (args[index].Number % 1 != 0)
                throw new EvilRuntimeException($"Expected integer value at argument index {index}.");

            return args;
        }

        public static DynamicValue[] ExpectByteAtIndex(this DynamicValue[] args, int index)
        {
            ExpectIntegerAtIndex(args, index);

            if (args[index].Number < 0 || args[index].Number > 255)
                throw new EvilRuntimeException($"Expected a value limited between 0 and 255.");

            return args;
        }

        public static DynamicValue[] ExpectCharAtIndex(this DynamicValue[] args, int index)
        {
            ExpectTypeAtIndex(args, index, DynamicValueType.String);

            if (args[index].String.Length > 1)
                throw new EvilRuntimeException($"Expected a single character.");

            return args;
        }

        public static DynamicValue[] ExpectStringAtIndex(this DynamicValue[] args, int index)
        {
            ExpectTypeAtIndex(args, index, DynamicValueType.String);
            return args;
        }
    }
}