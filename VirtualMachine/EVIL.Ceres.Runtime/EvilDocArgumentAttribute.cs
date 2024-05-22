using System;
using EVIL.CommonTypes.TypeSystem;

namespace EVIL.Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class EvilDocArgumentAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public DynamicValueType PrimaryType { get; }
        public DynamicValueType[]? OtherTypes { get; }
        public bool IsAnyType { get; }
        
        public bool CanBeNil { get; init; }
        public string? DefaultValue { get; init; }

        public EvilDocArgumentAttribute(string name, string description, DynamicValueType primaryType, params DynamicValueType[] types)
        {
            Name = name;
            Description = description;
            PrimaryType = primaryType;
            OtherTypes = types;
        }
        
        public EvilDocArgumentAttribute(string name, string description, DynamicValueType type)
        {
            Name = name;
            Description = description;

            PrimaryType = type;
            IsAnyType = false;
        }
        
        public EvilDocArgumentAttribute(string name, string description)
        {
            Name = name;
            Description = description;
            IsAnyType = true;
        }
    }
}