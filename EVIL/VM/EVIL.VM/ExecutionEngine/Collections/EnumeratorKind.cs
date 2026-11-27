namespace EVIL.VM.ExecutionEngine.Collections;

internal enum EnumeratorKind : byte
{
    Empty = 0,
    Table = 1,
    Array = 2,
    Custom = 3
}