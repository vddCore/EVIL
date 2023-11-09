using System.IO;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.Runtime.EvilDocPropertyMode;
using static EVIL.CommonTypes.TypeSystem.DynamicValueType;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule : RuntimeModule
    {
        public override string FullyQualifiedName => "fs";

        private static DynamicValue _error = DynamicValue.Nil;

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
        [EvilDocProperty(Get,
            "Retrieves the error message set by the last file system function call, " +
            "or `nil` if the last call was successful.",
            ReturnType = String
        )]
        private static DynamicValue GetError(DynamicValue _)
            => _error;

        private static void ClearError()
            => _error = DynamicValue.Nil;
        
        private static void SetError(string error)
            => _error = error;
        
    }
}