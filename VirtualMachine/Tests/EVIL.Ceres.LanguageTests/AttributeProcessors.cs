namespace EVIL.Ceres.LanguageTests;

using System;
using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.TypeSystem;

public static class AttributeProcessors
{
    public static void ApproximateAttribute(ChunkAttribute approximateAttribute, Chunk chunk)
    {
        if (!chunk.TryGetAttribute("test", out var testAttribute))
        {
            throw new Exception("Only valid on functions marked as a test.");
        }

        if (testAttribute.Values.Count == 0)
        {
            throw new Exception("Only valid on tests returning a value.");
        }

        if (testAttribute.Values[0].Type != DynamicValueType.Number)
        {
            throw new Exception("Only valid on tests returning a number.");
        }

        if (approximateAttribute.Values.Count != 0)
        {
            if (approximateAttribute.Values.Count > 1)
            {
                throw new Exception("Accepts at most 1 parameter.");
            }

            if (approximateAttribute.Values[0].Number % 1 != 0)
            {
                throw new Exception("Decimal places parameter needs to be an integer.");
            }
        }
    }

    public static void DisasmAttribute(ChunkAttribute disasmAttribute, Chunk chunk)
    {
        if (disasmAttribute.Values.Count != 0)
        {
            if (disasmAttribute.Values[0].Type != DynamicValueType.String)
            {
                throw new Exception("Only accepts a string.");
            }

            switch (disasmAttribute.Values[0].String)
            {
                case "always":
                case "failure":
                case "never":
                    break;
                    
                default: throw new Exception("Accepts only 'always', 'failure' or 'never'.");
            }
        }
        else
        {
            disasmAttribute.Values.Add(new("always"));
        }
    }
}