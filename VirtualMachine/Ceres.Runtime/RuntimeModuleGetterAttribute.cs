using System;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeModuleGetterAttribute : Attribute
    {
        public string SubNameSpace { get; }
        public DynamicValueType ReturnType { get; set; }

        public RuntimeModuleGetterAttribute(string subNameSpace)
        {
            SubNameSpace = subNameSpace;
        }
    }
}