namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EvilRuntimeMemberAttribute : Attribute
    {
        public string FullyQualifiedName { get; }
        public bool AllowRedefinition { get; init; } = true;

        public EvilRuntimeMemberAttribute(string fullyQualifiedName)
        {
            FullyQualifiedName = fullyQualifiedName;
        }
    }
}