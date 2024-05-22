using System.Text;

namespace EVIL.Ceres.TranslationEngine.Diagnostics
{
    public sealed record CompilerMessage(
        CompilerMessageSeverity Severity,
        string Message,
        string? FileName = null,
        int MessageCode = 0,
        int Line = 0,
        int Column = 0
    )
    {
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (MessageCode > 0)
            {
                sb.Append($"EV{MessageCode:D4}");
            }

            sb.Append($" :: {Severity}");
            if (Line > 0)
            {
                sb.Append($" :: line {Line}");

                if (Column > 0)
                {
                    sb.Append($", column {Column}");
                }
            }
            
            if (!string.IsNullOrEmpty(FileName))
            {
                sb.Append($" :: {FileName}");
            }
            
            sb.Append($" :: {Message}");
            return sb.ToString();
        }
    }
}