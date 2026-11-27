namespace EVIL.VM.ExecutionEngine.Diagnostics.Disassembly.Rendering;

public enum AddressFormat
{
    Hex4,          /* e.g. 001A       */
    Hex8,          /* e.g. 0000001A   */
    HexPrefixed    /* e.g. 0x0000001A */
}