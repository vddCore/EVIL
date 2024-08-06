namespace EVIL.Ceres.Runtime;

using System;
using EVIL.CommonTypes.TypeSystem;

[AttributeUsage(AttributeTargets.Method)]
public class EvilDocFunctionAttribute : Attribute
{
    public string Description { get; }

    public bool IsVariadic { get; set; } = false;
    public bool IsAnyReturn { get; set; } = false;
        
    public DynamicValueType ReturnType { get; set; } = DynamicValueType.Nil;
    public string? Returns { get; set; } = "Nothing.";

    public EvilDocFunctionAttribute(string description)
    {
        Description = description;
    }
}