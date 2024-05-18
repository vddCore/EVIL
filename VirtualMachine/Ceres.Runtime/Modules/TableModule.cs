using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public class TableModule : RuntimeModule
    {
        public override string FullyQualifiedName => "tbl";

        [RuntimeModuleFunction("clear")]
        [EvilDocFunction(
            "Removes all values from the provided table."
        )]
        private static DynamicValue Clear(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);
            
            table.Clear();
           
            return DynamicValue.Nil;
        }

        [RuntimeModuleFunction("rawset")]
        [EvilDocFunction(
            "Sets the given Table's key to the provided value, ignoring any script-defined `__set` meta-table entries.",
            Returns = "The value that has been set.",
            IsAnyReturn = true
        )]
        [EvilDocArgument("table", "A Table whose key is to be set.", DynamicValueType.Table)]
        [EvilDocArgument("key", "A key to use when setting the value.", CanBeNil = true)]
        [EvilDocArgument("value", "A value to set the Table's key to.", CanBeNil = true)]
        private static DynamicValue RawSet(Fiber _, params DynamicValue[] args)
        {
            args.ExpectTableAt(0, out var table)
                .ExpectAnyAt(1, out var key)
                .ExpectAnyAt(2, out var value);

            table[key] = value;
            return value;
        }
        
        [RuntimeModuleFunction("rawget")]
        [EvilDocFunction(
            "Gets the given Table's key to the provided value, ignoring any script-defined `__get` meta-table entries.",
            Returns = "The value that resides in the provided Table under the given key.",
            IsAnyReturn = true
        )]
        [EvilDocArgument("table", "A Table whose value to retrieve.", DynamicValueType.Table)]
        [EvilDocArgument("key", "A key to use when retrieving the value.", CanBeNil = true)]
        private static DynamicValue RawGet(Fiber _, params DynamicValue[] args)
        {
            args.ExpectTableAt(0, out var table)
                .ExpectAnyAt(1, out var key);

            return table[key];
        }

        [RuntimeModuleFunction("freeze")]
        [EvilDocFunction(
            "Freezes the provided Table so that no keys can be set or removed.",
            Returns = "The frozen Table. This value is returned by reference.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument("table", "A Table to be frozen.", DynamicValueType.Table)]
        private static DynamicValue Freeze(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            table.Freeze();
            
            return table;
        }
        
        [RuntimeModuleFunction("unfreeze")]
        [EvilDocFunction(
            "Unfreezes the provided Table so that keys can be set or removed.",
            Returns = "The unfrozen Table. This value is returned by reference.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument("table", "A Table to be unfrozen.", DynamicValueType.Table)]
        private static DynamicValue Unfreeze(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            table.Unfreeze();

            return table;
        }
        
        [RuntimeModuleFunction("is_frozen")]
        [EvilDocFunction(
            "Checks if the given Table is frozen.",
            Returns = "`true` if the provided Table is frozen, `false` otherwise.",
            ReturnType = DynamicValueType.Boolean
        )]
        [EvilDocArgument("table", "A Table whose freeze status to check.", DynamicValueType.Table)]
        private static DynamicValue /*IsFrozen is already a symbol...*/ _IsFrozen(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            return table.IsFrozen;  
        }

        [RuntimeModuleFunction("keys")]
        [EvilDocFunction(
            "Retrieves all keys present in the given Table.",
            Returns = "An Array containing all keys in the provided Table, in no particular order.",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument("table", "A Table whose keys to retrieve.", DynamicValueType.Table)]
        private static DynamicValue Keys(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            return table.GetKeys();
        }

        [RuntimeModuleFunction("values")]
        [EvilDocFunction(
            "Retrieves all values present in the given Table.",
            Returns = "An Array containing all values in the provided Table, in no particular order.",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument("table", "A Table whose keys to retrieve.", DynamicValueType.Table)]
        private static DynamicValue Values(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTableAt(0, out var table);

            return table.GetValues();
        }

        [RuntimeModuleFunction("cpy")]
        [EvilDocFunction(
            "Copies the given Table, returning a new instance.",
            Returns = "A new Table containing the values from the original one.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument("table", "A Table to be copied.", DynamicValueType.Table)]
        [EvilDocArgument(
            "deep", 
            "Whether the copy should be deep (nested Tables are also completely copied) " +
            "or shallow (nested Table references are copied).",
            DynamicValueType.Boolean,
            DefaultValue = "false"
        )]
        private static DynamicValue Copy(Fiber _, params DynamicValue[] args)
        {
            args.ExpectTableAt(0, out var table)
                .OptionalBooleanAt(1, false, out var deep);

            return deep 
                ? table.DeepCopy() 
                : table.ShallowCopy();
        }
        
        [RuntimeModuleFunction("invert")]
        [EvilDocFunction(
            "Copies a Table in a way that its values become its keys and vice-versa.",
            Returns = "A shallow copy of the provided Table with its values and keys swapped.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument("tbl", "A Table to be inverted.", DynamicValueType.Table)]
        private static DynamicValue Invert(Fiber _, params DynamicValue[] args)
        {
            args.ExpectTableAt(0, out var tbl);

            var newTable = new Table();

            foreach (var (key, value) in tbl)
            {
                newTable[value] = key;
            }
            
            return newTable;
        }

        [RuntimeModuleFunction("set_mt")]
        [EvilDocFunction("Sets a meta-table for the provided Table.", ReturnType = DynamicValueType.Nil)]
        [EvilDocArgument("table", "A Table whose meta-table to modify.", DynamicValueType.Table, CanBeNil = false)]
        [EvilDocArgument("mt", "A Table whose meta-table to modify.", DynamicValueType.Table, CanBeNil = false)]
        private static DynamicValue SetMetaTable(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectTableAt(0, out var table)
                .ExpectTableAt(1, out var mt);

            if (table.MetaTable != null)
            {
                if (table.MetaTable["__metatable"] != DynamicValue.Nil)
                {
                    return fiber.ThrowFromNative("Unable to set a protected metatable.");
                }
            }
            
            table.MetaTable = mt;
            return DynamicValue.Nil;
        }

        [RuntimeModuleFunction("get_mt")]
        [EvilDocFunction(
            "Gets a meta-table for the provided Table.",
            Returns = "A Table acting as the current meta-table for the provided Table, or Nil if it wasn't set.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument(
            "table",
            "A table whose meta-table to retrieve.",
            DynamicValueType.Table,
            CanBeNil = false
        )]
        private static DynamicValue GetMetaTable(Fiber _, params DynamicValue[] args)
        {
            args.ExpectTableAt(0, out var table);

            if (table.MetaTable != null)
            {
                var metaField = table.MetaTable["__metatable"];

                if (metaField != DynamicValue.Nil)
                {
                    return metaField;
                }

                return table.MetaTable;
            }
            
            return DynamicValue.Nil;
        }
    }
}