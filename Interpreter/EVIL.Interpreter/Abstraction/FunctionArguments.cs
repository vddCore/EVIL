using System.Collections.Generic;

namespace EVIL.Interpreter.Abstraction
{
    public class FunctionArguments : List<DynValue>
    {
        public FunctionArguments ExpectExactly(int count)
        {
            if (Count != count)
                throw new ClrFunctionException($"Expected exactly {count} arguments, found {Count}.");

            return this;
        }

        public FunctionArguments DisallowType(DynValueType type)
        {
            if (Exists(x => x.Type == type))
                throw new ClrFunctionException($"The type '{type}' is invalid for this function call.");

            return this;
        }

        public FunctionArguments ExpectTypeAtIndex(int index, DynValueType type)
        {
            if (index >= Count)
                throw new ClrFunctionException($"Expected type '{type}' at argument index {index}, found no value.");

            if (this[index].Type != type)
                throw new ClrFunctionException(
                    $"Expected type '{type}' at argument index {index}, found '{this[index].Type}");

            return this;
        }

        public FunctionArguments ExpectTableAtIndex(int index)
        {
            ExpectTypeAtIndex(index, DynValueType.Table);
            return this;
        }

        public FunctionArguments ExpectTableAtIndex(int index, int size)
        {
            ExpectTableAtIndex(index);

            if (this[index].Table.Count != size)
                throw new ClrFunctionException(
                    $"Expected a table of size {size} at index {index}. Actual size was {this[index].Table.Count}");

            return this;
        }

        public FunctionArguments ExpectTableAtIndex(int index, int size, DynValueType acceptedType)
        {
            ExpectTableAtIndex(index);
            var tbl = this[index].Table;
            
            if (tbl.Count != size)
                throw new ClrFunctionException(
                    $"Expected a table of size {size} at index {index}. Actual size was {this[index].Table.Count}");

            tbl.ForEach((k, v) =>
            {
                if (v.Type != acceptedType)
                {
                    throw new ClrFunctionException(
                        $"Expected a table containing only {acceptedType}(s).");
                }
            });

            return this;
        }

        public FunctionArguments ExpectAtLeast(int count)
        {
            if (Count < count)
                throw new ClrFunctionException($"Expected at least {count} arguments, found {Count}.");

            return this;
        }

        public FunctionArguments ExpectAtMost(int count)
        {
            if (Count > count)
                throw new ClrFunctionException($"Expected at most {count} arguments, found {Count}.");

            return this;
        }

        public FunctionArguments ExpectNone()
        {
            if (Count != 0)
                throw new ClrFunctionException($"Expected no arguments, found {Count}.");

            return this;
        }

        public FunctionArguments ExpectNumberAtIndex(int index)
        {
            if (this[index].Type != DynValueType.Number && this[index].Type != DynValueType.Number)
                throw new ClrFunctionException($"Expected a number value at argument index {index}.");

            return this;
        }

        public FunctionArguments ExpectIntegerAtIndex(int index)
        {
            ExpectTypeAtIndex(index, DynValueType.Number);

            if (this[index].Number % 1 != 0)
                throw new ClrFunctionException($"Expected integer value at argument index {index}.");

            return this;
        }

        public FunctionArguments ExpectByteAtIndex(int index)
        {
            ExpectIntegerAtIndex(index);

            if (this[index].Number < 0 || this[index].Number > 255)
                throw new ClrFunctionException($"Expected a value limited between 0 and 255.");

            return this;
        }

        public FunctionArguments ExpectCharAtIndex(int index)
        {
            ExpectTypeAtIndex(index, DynValueType.String);

            if (this[index].String.Length > 1)
                throw new ClrFunctionException($"Expected a single character.");

            return this;
        }
    }
}