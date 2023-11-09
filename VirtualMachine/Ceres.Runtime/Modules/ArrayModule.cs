using System.Linq;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using static EVIL.CommonTypes.TypeSystem.DynamicValueType;

namespace Ceres.Runtime.Modules
{
    public class ArrayModule : RuntimeModule
    {
        public override string FullyQualifiedName => "arr";

        [RuntimeModuleFunction("indof")]
        [EvilDocFunction(
            "Searches the given array for an index of the given value.",
            Returns = "0-based index of the first matching element in the given array or -1 if not found.",
            ReturnType = Number
        )]
        [EvilDocArgument("array", "Array to be searched.", Array)]
        [EvilDocArgument("value", "Value to be searched for.")]
        private static DynamicValue IndexOf(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectAnyAt(1, out var value);

            return array.IndexOf(value);
        }
        
        [RuntimeModuleFunction("fill")]
        [EvilDocFunction("Fills the given array with the given value.")]
        [EvilDocArgument("array", "Array to be filled with the given value.", Array)]
        [EvilDocArgument("value", "Value to fill the given array with.")]
        private static DynamicValue Fill(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectAnyAt(1, out var value);

            array.Fill(value);
            
            return DynamicValue.Nil;
        }
        
        [RuntimeModuleFunction("resize")]
        [EvilDocFunction(
            "Attempts to resize the given array to match the given size. Existing contents are preserved.", 
            Returns = "New size of the array or -1 if the operation fails.",
            ReturnType = Number
        )]
        [EvilDocArgument("array", "Array to be resized.", Array)]
        [EvilDocArgument("size", "Integer specifying the new size.", Number)]
        private static DynamicValue Resize(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectIntegerAt(1, out var size);

            return array.Resize((int)size);
        }
        
        [RuntimeModuleFunction("push")]
        [EvilDocFunction(
            "Appends the given values at the end of the given array.",
            Returns = "Size of the array after the values have been appended.",
            ReturnType = Number,
            IsVariadic = true
        )]
        [EvilDocArgument("array", "Array to append the given values to.", Array)]
        [EvilDocArgument("...", "Arbitrary amount of values to be appended to the given array.")]
        private static DynamicValue Push(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array);

            var values = args.Skip(1).ToArray();
            return array.Push(values);
        }
        
        [RuntimeModuleFunction("insert")]
        [EvilDocFunction(
            "Inserts the given values at the given index of the given array",
            Returns = "Size of the array after the values have been inserted or -1 if the operation fails.",
            ReturnType = Number,
            IsVariadic = true
        )]
        [EvilDocArgument("array", "Array into which the values will be inserted.", Array)]
        [EvilDocArgument("index", "Integer specifying the index at which to insert the given values.", Number)]
        [EvilDocArgument("...", "Arbitrary amount of values to be inserted into the given array.")]
        private static DynamicValue Insert(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array)
                .ExpectIntegerAt(1, out var index);

            var values = args.Skip(2).ToArray();
            return array.Insert((int)index, values);
        }
        
        [RuntimeModuleFunction("rsh")]
        [EvilDocFunction(
            "Removes the last element of the given array. This operation changes (shrinks) the size of the array.",
            Returns = "Array element that has been removed or `nil` if the array was empty.",
            IsAnyReturn = true
        )]
        [EvilDocArgument("array", "Array to remove an element from.", Array)]
        private static DynamicValue RightShift(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array);
            return array.RightShift();
        }
        
        [RuntimeModuleFunction("lsh")]
        [EvilDocFunction(
            "Removes the first element of the given array. This operation changes (shrinks) the size of the array.",
            Returns = "Array element that has been removed or `nil` if the array was empty.",
            IsAnyReturn = true
        )]
        [EvilDocArgument("array", "Array to remove an element from.", Array)]
        private static DynamicValue Shift(Fiber _, params DynamicValue[] args)
        {
            args.ExpectArrayAt(0, out var array);
            return array.LeftShift();
        }
    }
}