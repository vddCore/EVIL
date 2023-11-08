using System.IO;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule : RuntimeModule
    {
        public override string FullyQualifiedName => "fs";

        private static DynamicValue _error = Nil;

        private static readonly Table _origin = new Table()
        {
            { "BEGIN", (long)SeekOrigin.Begin },
            { "CURRENT", (long)SeekOrigin.Current },
            { "END", (long)SeekOrigin.End }
        }.Freeze();
        
        public FsModule()
        {
            Set("origin", _origin);
        }

        [RuntimeModuleGetter("error")]
        private static DynamicValue GetError(DynamicValue _)
            => _error;

        private static void ClearError()
            => _error = Nil;
        
        private static void SetError(string error)
            => _error = error;
        
    }
}