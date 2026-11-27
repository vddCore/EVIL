namespace EVIL.VM.Runtime.Modules;

using System.IO;
using EVIL.VM.ExecutionEngine.Collections;
using EVIL.VM.ExecutionEngine.TypeSystem;
using static EvilDocPropertyMode;
using static EVIL.Core.Common.TypeSystem.DynamicValueType;

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