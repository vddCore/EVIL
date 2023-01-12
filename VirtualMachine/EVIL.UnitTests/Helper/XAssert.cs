using System;
using EVIL.ExecutionEngine.Abstraction;
using NUnit.Framework;
using NAssert = NUnit.Framework.Assert;

namespace EVIL.UnitTests.Helper
{
    public static class XAssert
    {
        public static void AreEqual(string expected, DynamicValue actual)
        {
            NAssert.AreEqual(DynamicValueType.String, actual.Type);
            NAssert.AreEqual(expected, actual.String);
        }

        public static void AreEqual(double expected, DynamicValue actual)
        {
            NAssert.AreEqual(DynamicValueType.Number, actual.Type);
            NAssert.AreEqual(expected, actual.Number);
        }

        public static void IsTrue(DynamicValue value)
            => NAssert.IsTrue(value.IsTruth);

        public static void IsFalse(DynamicValue value)
            => NAssert.IsFalse(value.IsTruth);
        
        public static void ExistsIn(Table table, double key)
            => NAssert.IsTrue(table.IsSet(key));

        public static void ExistsIn(Table table, string key)
            => NAssert.IsTrue(table.IsSet(key));

        public static void ExistsIn(Table table, double key, out DynamicValue value)
        {
            ExistsIn(table, key);
            value = table[key];
        }

        public static void ExistsIn(Table table, string key, out DynamicValue value)
        {
            ExistsIn(table, key);
            value = table[key];
        }

        public static void ExistsIn(DynamicValue tableValue, double key)
        {
            NAssert.AreEqual(DynamicValueType.Table, tableValue.Type);
            ExistsIn(tableValue.Table, key);
        }

        public static void ExistsIn(DynamicValue tableValue, string key)
        {
            NAssert.AreEqual(DynamicValueType.Table, tableValue.Type);
            ExistsIn(tableValue.Table, key);
        }

        public static void ExistsIn(DynamicValue tableValue, double key, out DynamicValue value)
        {
            ExistsIn(tableValue, key);
            value = tableValue.Table[key];
        }

        public static void ExistsIn(DynamicValue tableValue, string key, out DynamicValue value)
        {
            ExistsIn(tableValue, key);
            value = tableValue.Table[key];
        }

        public static void DoesNotExistIn(Table table, string key)
            => NAssert.IsFalse(table.IsSet(key));

        public static void DoesNotExistIn(Table table, double key)
            => NAssert.IsFalse(table.IsSet(key));
        
        public static void DoesNotExistIn(DynamicValue tableValue, double key)
        {
            NAssert.AreEqual(DynamicValueType.Table, tableValue.Type);
            DoesNotExistIn(tableValue.Table, key);
        }

        public static void DoesNotExistIn(DynamicValue tableValue, string key)
        {
            NAssert.AreEqual(DynamicValueType.Table, tableValue.Type);
            DoesNotExistIn(tableValue.Table, key);
        }

        public static void ThrowsWithInner<T, U>(TestDelegate code)
            where T : Exception
            where U : Exception
        {
            var exception = NAssert.Catch<T>(code);

            NAssert.NotNull(exception);
            NAssert.IsInstanceOf<U>(exception.InnerException);
        }
    }
}