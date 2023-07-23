using System.Text;

namespace Ceres.TranslationEngine.Diagnostics
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

            sb.Append($" :: {Severity} :: {Message}");

            if (FileName != null)
            {
                sb.Append($" :: {FileName}");
            }
            
            if (Line > 0)
            {
                sb.Append($" (line {Line}");

                if (Column > 0)
                {
                    sb.Append($", column {Column}");
                }

                sb.Append(")");
            }
            
            return sb.ToString();
        }
    }
}