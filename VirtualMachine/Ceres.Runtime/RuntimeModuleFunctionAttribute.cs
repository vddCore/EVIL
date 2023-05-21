namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeModuleFunctionAttribute : Attribute
    {
        public string Name { get; }

        public RuntimeModuleFunctionAttribute(string name)
        {
            Name = name;
        }
    }
}