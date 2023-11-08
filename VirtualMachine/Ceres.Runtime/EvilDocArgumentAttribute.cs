using System;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class EvilDocArgumentAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        
        public DynamicValueType Type { get; }
        public bool IsAnyType { get; }
        
        public string? DefaultValue { get; init; }

        public EvilDocArgumentAttribute(string name, string description, DynamicValueType type)
        {
            Name = name;
            Description = description;
            
            Type = type;
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