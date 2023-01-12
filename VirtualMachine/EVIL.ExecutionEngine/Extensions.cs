using EVIL.ExecutionEngine.Abstraction;

namespace EVIL.ExecutionEngine
{
    internal static class Extensions
    {
        public static string Alias(this DynamicValueType dvt)
            => dvt.ToString().ToLowerInvariant();
    }
}