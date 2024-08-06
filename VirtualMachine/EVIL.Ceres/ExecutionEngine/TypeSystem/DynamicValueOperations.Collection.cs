namespace EVIL.Ceres.ExecutionEngine.TypeSystem;

using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.CommonTypes.TypeSystem;

public partial class DynamicValueOperations
{
    public static DynamicValue SetEntry(this DynamicValue a, DynamicValue key, DynamicValue value)
    {
        if (a.Type == DynamicValueType.Table)
        {
            a.Table!.Set(key, value);
        }
        else if (a.Type == DynamicValueType.Array)
        {
            a.Array![key] = value;
        }
        else if (a.Type == DynamicValueType.Error)
        {
            a.Error![key] = value;
        }
        else if (a.Type == DynamicValueType.NativeObject && a.NativeObject is IWriteableObject writeable)
        {
            writeable.Set(key, value);
        }
        else
        {
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to set an entry of a {a.Type} which is not an Array, a Table, nor a writeable object."
            );
        }

        return value;
    }
}