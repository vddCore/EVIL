using EVIL.CommonTypes.TypeSystem;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public partial class DynamicValueOperations
    {
        public static DynamicValue SetEntry(this DynamicValue a, DynamicValue key, DynamicValue value)
        {
            if (a.Type != DynamicValueType.Table)
            {
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to use {a.Type} as a Table."
                );
            }

            a.Table!.Set(key, value);
            return value;
        }
    }
}