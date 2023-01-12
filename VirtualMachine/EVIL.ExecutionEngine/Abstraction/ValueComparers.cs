using System;
using System.Collections.Generic;
using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    internal static class ValueComparers
    {
        internal delegate int ValueComparer(DynamicValue a, DynamicValue b);
        private static Dictionary<DynamicValueType, Dictionary<DynamicValueType, ValueComparer>> Comparers { get; }

        static ValueComparers()
        {
            Comparers = new Dictionary<DynamicValueType, Dictionary<DynamicValueType, ValueComparer>>
            {
                { 
                    DynamicValueType.String, 
                    new Dictionary<DynamicValueType, ValueComparer>
                    {
                        { 
                            DynamicValueType.String, 
                            (a, b) => string.Compare(a.String, b.String, StringComparison.InvariantCulture) 
                        }
                    }
                },

                {
                    DynamicValueType.Number,
                    new Dictionary<DynamicValueType, ValueComparer>
                    {
                        {
                            DynamicValueType.Number,
                            (a, b) => Math.Sign(a.Number - b.Number)
                        }
                    }
                }
            };
        }

        internal static ValueComparer RetrieveComparer(DynamicValue a, DynamicValue b)
        {
            if (!Comparers.ContainsKey(a.Type))
                throw new TypeComparisonException(a, b);

            if (!Comparers[a.Type].ContainsKey(b.Type))
                throw new TypeComparisonException(a, b);

            return Comparers[a.Type][b.Type];
        }
    }
}