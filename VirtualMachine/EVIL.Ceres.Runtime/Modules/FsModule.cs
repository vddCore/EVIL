namespace EVIL.Ceres.Runtime.Modules;

using static EVIL.Ceres.Runtime.EvilDocPropertyMode;
using static EVIL.CommonTypes.TypeSystem.DynamicValueType;

using System.IO;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

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
        "or `nil` if the last operation was successful.",
        ReturnType = String
    )]
    private static DynamicValue GetError(DynamicValue _)
        => _error;

    private static void ClearError()
        => _error = DynamicValue.Nil;
        
    private static void SetError(string error)
        => _error = error;
}