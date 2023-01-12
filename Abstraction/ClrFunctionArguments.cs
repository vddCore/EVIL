using System.Collections.Generic;

namespace EVIL.Abstraction
{
    public class ClrFunctionArguments : List<DynValue>
    {
        public ClrFunctionArguments ExpectExactly(int count)
        {
            if (Count != count)
                throw new ClrFunctionException($"Expected exactly {count} arguments, found {Count}.");

            return this;
        }

        public ClrFunctionArguments DisallowType(DynValueType type)
        {
            if (Exists(x => x.Type == type))
                throw new ClrFunctionException($"The type '{type}' is invalid for this function call.");

            return this;
        }

        public ClrFunctionArguments ExpectTypeAtIndex(int index, DynValueType type)
        {
            if (index >= Count)
                throw new ClrFunctionException($"Expected type '{type}' at argument index {index}, found no value.");

            if (this[index].Type != type)
                throw new ClrFunctionException($"Expected type '{type}' at argument index {index}, found '{this[index].Type}");

            return this;
        }

        public ClrFunctionArguments ExpectAtLeast(int count)
        {
            if (Count < count)
                throw new ClrFunctionException($"Expected at least {count} arguments, found {Count}.");

            return this;
        }

        public ClrFunctionArguments ExpectAtMost(int count)
        {
            if (Count > count)
                throw new ClrFunctionException($"Expected at most {count} arguments, found {Count}.");

            return this;
        }

        public ClrFunctionArguments ExpectNone()
        {
            if (Count != 0)
                throw new ClrFunctionException($"Expected no arguments, found {Count}.");

            return this;
        }

        public ClrFunctionArguments ExpectIntegerAtIndex(int index)
        {
            ExpectTypeAtIndex(index, DynValueType.Number);

            if (this[index].Number % 1 != 0)
                throw new ClrFunctionException($"Expected integer value at argument index {index}.");

            return this;
        }

        public ClrFunctionArguments ExpectByteAtIndex(int index)
        {
            ExpectIntegerAtIndex(index);

            if (this[index].Number < 0 || this[index].Number > 255)
                throw new ClrFunctionException($"Expected a value limited between 0 and 255.");

            return this;
        }
    }
}
