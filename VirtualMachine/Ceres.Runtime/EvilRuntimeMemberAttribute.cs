namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EvilRuntimeMemberAttribute : Attribute
    {
        public string FullyQualifiedName { get; }
        public bool AllowRedefinition { get; init; }

        public EvilRuntimeMemberAttribute(string fullyQualifiedName)
        {
            FullyQualifiedName = fullyQualifiedName;
        }
    }
}