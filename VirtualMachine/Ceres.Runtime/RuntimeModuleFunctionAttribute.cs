using System;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeModuleFunctionAttribute : Attribute
    {
        public string SubNameSpace { get; }
        public DynamicValueType ReturnType { get; set; }

        public RuntimeModuleFunctionAttribute(string subNameSpace)
        {
            SubNameSpace = subNameSpace;
        }
    }
}