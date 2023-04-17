using System;
using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;

namespace Insitor
{
    public static class AttributeProcessors
    {
        public static void ApproximateAttribute(ChunkAttribute attribute, Chunk chunk)
        {
            if (!chunk.TryGetAttribute("test", out var testAttribute))
            {
                throw new Exception("Only valid on functions marked as a test.");
            }

            if (!testAttribute.Values.Any())
            {
                throw new Exception("Only valid on tests returning a value.");
            }

            if (testAttribute.Values[0].Type != DynamicValue.DynamicValueType.Number)
            {
                throw new Exception("Only valid on tests returning a number.");
            }

            if (attribute.Values.Any())
            {
                if (attribute.Values.Count > 1)
                {
                    throw new Exception("Accepts at most 1 parameter.");
                }

                if (attribute.Values[0].Number % 1 != 0)
                {
                    throw new Exception("Decimal places parameter needs to be an integer.");
                }
            }
        }
    }
}