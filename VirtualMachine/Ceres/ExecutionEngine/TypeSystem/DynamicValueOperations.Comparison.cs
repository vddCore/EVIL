using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public static partial class DynamicValueOperations
    {
        public static DynamicValue IsDeeplyEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Table && b.Type == DynamicValueType.Table)
            {
                return new(a.Table!.IsDeeplyEqualTo(b.Table!));
            }

            if (a.Type == DynamicValueType.Array && b.Type == DynamicValueType.Array)
            {
                return new(a.Array!.IsDeeplyEqualTo(b.Array!));
            }

            if (a.Type == DynamicValueType.Error && b.Type == DynamicValueType.Error)
            {
                return new(a.Error!.IsDeeplyEqualTo(b.Error!));
            }

            try
            {
                return IsEqualTo(a, b);
            }
            catch (UnsupportedDynamicValueOperationException)
            {
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to deeply compare (<==>) a {a.Type} with a {b.Type}."
                );
            }
        }

        public static DynamicValue IsDeeplyNotEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Table && b.Type == DynamicValueType.Table)
            {
                return new(!a.Table!.IsDeeplyEqualTo(b.Table!));
            }
            
            if (a.Type == DynamicValueType.Array && b.Type == DynamicValueType.Array)
            {
                return new(!a.Array!.IsDeeplyEqualTo(b.Array!));
            }
            
            if (a.Type == DynamicValueType.Error && b.Type == DynamicValueType.Error)
            {
                return new(!a.Error!.IsDeeplyEqualTo(b.Error!));
            }

            try
            {
                return IsNotEqualTo(a, b);
            }
            catch (UnsupportedDynamicValueOperationException)
            {
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to deeply compare (<!=>) a {a.Type} with a {b.Type}."
                );
            }
        }

        public static DynamicValue IsEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type != b.Type)
                return False;

            switch (a.Type)
            {
                case DynamicValueType.Nil:
                    return b.Type == DynamicValueType.Nil;

                case DynamicValueType.Number:
                    return a.Number.Equals(b.Number);

                case DynamicValueType.String:
                    return a.String == b.String;

                case DynamicValueType.Boolean:
                    return a.Boolean == b.Boolean;

                case DynamicValueType.Table:
                    return a.Table == b.Table;

                case DynamicValueType.Fiber:
                    return a.Fiber == b.Fiber;

                case DynamicValueType.Chunk:
                    return a.Chunk == b.Chunk;
                
                case DynamicValueType.Error:
                    return a.Error == b.Error;
                
                case DynamicValueType.TypeCode:
                    return a.TypeCode == b.TypeCode;

                case DynamicValueType.NativeFunction:
                    return a.NativeFunction == b.NativeFunction;
                
                case DynamicValueType.NativeObject:
                    return a.NativeObject == b.NativeObject;
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (==) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsNotEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type != b.Type)
                return True;

            switch (a.Type)
            {
                case DynamicValueType.Nil:
                    return b.Type != DynamicValueType.Nil;

                case DynamicValueType.Number:
                    return !a.Number.Equals(b.Number);

                case DynamicValueType.String:
                    return a.String != b.String;

                case DynamicValueType.Boolean:
                    return a.Boolean != b.Boolean;

                case DynamicValueType.Table:
                    return a.Table != b.Table;

                case DynamicValueType.Fiber:
                    return a.Fiber != b.Fiber;

                case DynamicValueType.Chunk:
                    return a.Chunk != b.Chunk;
                
                case DynamicValueType.Error:
                    return a.Error != b.Error;
                
                case DynamicValueType.TypeCode:
                    return a.TypeCode != b.TypeCode;

                case DynamicValueType.NativeFunction:
                    return a.NativeFunction != b.NativeFunction;
                
                case DynamicValueType.NativeObject:
                    return a.NativeObject != b.NativeObject;
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (!=) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsGreaterThan(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number > b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (>) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsLessThan(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number < b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (<) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsGreaterThanOrEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number >= b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (>=) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsLessThanOrEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number <= b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (<=) a {a.Type} with a {b.Type}."
            );
        }
    }
}