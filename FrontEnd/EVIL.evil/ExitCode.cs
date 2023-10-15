namespace EVIL.evil
{
    public enum ExitCode
    {
        OK                    = 0000,
        GenericError          = 0001,
        ArgumentError         = 0002,
        NoInputFiles          = 0003,
        TooManyInputFiles     = 0004,
        LexerError            = 0005,
        ParserError           = 0006,
        CompilerError         = 0007,
        MissingEntryPoint     = 0008,
        RuntimeError          = 0009,
        InputFileDoesNotExist = 0010
    }
}