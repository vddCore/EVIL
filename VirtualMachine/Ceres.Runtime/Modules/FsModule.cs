using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule : RuntimeModule
    {
        public override string FullyQualifiedName => "fs";

        private static DynamicValue _error = Nil;

        [RuntimeModuleGetter("error", ReturnType = DynamicValueType.String)]
        private static DynamicValue GetError(DynamicValue key)
            => _error;

        private static void ClearError()
            => _error = Nil;
        
        private static void SetError(string error)
            => _error = error;
    }
}