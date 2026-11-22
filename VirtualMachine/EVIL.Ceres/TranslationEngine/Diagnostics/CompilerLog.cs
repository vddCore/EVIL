namespace EVIL.Ceres.TranslationEngine.Diagnostics;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Marshaling;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Diagnostics;
using EVIL.Diagnostics.Formatters;

using EvilArray = EVIL.Ceres.ExecutionEngine.Collections.Array;

public sealed class CompilerLog : IDynamicValueProvider, IDiagnosticSink
{
    private readonly List<CompilerMessage> _messages = [];
    private readonly List<Diagnostic> _diagnostics = [];

    public CompilerMessageSeverity MinimumSeverityLevel { get; set; } = CompilerMessageSeverity.Warning;
    public bool WarningsAsErrors { get; set; }
    public HashSet<string> WarningsAsErrorsCodes { get; }
    public HashSet<string> SuppressedCodes { get; }
        
    public IReadOnlyList<CompilerMessage> Messages => _messages;
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public event EventHandler<CompilerMessageEmitEventArgs>? MessageEmitted;

    public int VerboseCount => _messages.Count(x => x.Severity == CompilerMessageSeverity.Verbose);
    public int WarningCount => _messages.Count(x => x.Severity == CompilerMessageSeverity.Warning);
    public int FatalCount => _messages.Count(x => x.Severity == CompilerMessageSeverity.Fatal);
    public int InternalFailureCount => _messages.Count(x => x.Severity == CompilerMessageSeverity.InternalFailure);

    public int ErrorCount => FatalCount + InternalFailureCount;
    public bool HasErrors => ErrorCount > 0;
    public bool HasAnyMessages => _messages.Count > 0;

    public CompilerLog(Compiler compiler)
    {
        WarningsAsErrors = compiler.Options.WarningsAsErrors;
        WarningsAsErrorsCodes = compiler.Options.WarningsAsErrorsCodes;
        SuppressedCodes =  compiler.Options.SuppressedCodes;
    }
        
    public void Clear() 
    {
        _messages.Clear();
        _diagnostics.Clear();
    }

    public string FormatWith(DiagnosticFormatter formatter)
        => formatter.FormatAll(_diagnostics);

    public void Report(Diagnostic diagnostic)
    {
        if (IsSuppressed(diagnostic.Id, GetCode(diagnostic.Id)))
            return;

        if (diagnostic.Severity == DiagnosticSeverity.Warning && IsWarningAsError(diagnostic.Id, GetCode(diagnostic.Id)))
        {
            diagnostic = diagnostic with { Severity = DiagnosticSeverity.Error };
        }

        _diagnostics.Add(diagnostic);

        var messageObject = CompilerMessage.FromDiagnostic(diagnostic);
        if (messageObject.Severity < MinimumSeverityLevel)
            return;

        _messages.Add(messageObject);
        MessageEmitted?.Invoke(this, new(messageObject));
    }

    [DoesNotReturn]
    public void TerminateWithFatal(Diagnostic diagnostic, Exception? innerException = null)
    {
        Report(diagnostic);
        throw new CompilerException(this, innerException);
    }

    [DoesNotReturn]
    public T TerminateWithInternalFailure<T>(Diagnostic diagnostic, Exception? innerException = null, T dummyReturn = default!)
    {
        Report(diagnostic);
        throw new CompilerException(this, innerException);
    }

    public void EmitVerbose(
        string message,
        string? fileName = null,
        int messageCode = 0,
        int line = 0,
        int column = 0)
    {
        if (MinimumSeverityLevel > CompilerMessageSeverity.Verbose)
            return;
            
        var messageObject = new CompilerMessage(
            CompilerMessageSeverity.Verbose,
            message,
            fileName,
            messageCode,
            line,
            column
        );

        _messages.Add(messageObject);
        _diagnostics.Add(messageObject.ToDiagnostic());
        MessageEmitted?.Invoke(this, new(messageObject));
    }
        
    public void EmitWarning(
        string message, 
        string? fileName = null,
        int messageCode = 0,
        int line = 0,
        int column = 0)
    {
        var id = messageCode > 0 ? $"EV{messageCode:D4}" : string.Empty;
        if (IsSuppressed(id, messageCode))
            return;

        if (IsWarningAsError(id, messageCode))
        {
            TerminateWithFatal(message, fileName, messageCode, line, column);
            return;
        }

        if (MinimumSeverityLevel > CompilerMessageSeverity.Warning)
            return;
            
        var messageObject = new CompilerMessage(
            CompilerMessageSeverity.Warning,
            message,
            fileName,
            messageCode,
            line,
            column
        );
            
        _messages.Add(messageObject);
        _diagnostics.Add(messageObject.ToDiagnostic());
        MessageEmitted?.Invoke(this, new(messageObject));
    }
        
    [DoesNotReturn]
    public void TerminateWithFatal(
        string message,
        string? fileName = null,
        int messageCode = 0,
        int line = 0,
        int column = 0,
        Exception? innerException = null)
    {
        var messageObject = new CompilerMessage(
            CompilerMessageSeverity.Fatal,
            message,
            fileName,
            messageCode,
            line,
            column
        );
            
        _messages.Add(messageObject);
        MessageEmitted?.Invoke(this, new(messageObject));
            
        throw new CompilerException(this, innerException);
    }
        
    [DoesNotReturn]
    public T TerminateWithInternalFailure<T>(
        string message,
        string? fileName = null,
        int messageCode = 0,
        int line = 0,
        int column = 0,
        Exception? innerException = null,
        T dummyReturn = default!)
    {
        var messageObject = new CompilerMessage(
            CompilerMessageSeverity.InternalFailure,
            message,
            fileName,
            messageCode,
            line,
            column
        );
            
        _messages.Add(messageObject);
        MessageEmitted?.Invoke(this, new(messageObject));

        throw new CompilerException(this, innerException);

        #pragma warning disable CS0162 
        
        // ReSharper disable once HeuristicUnreachableCode
        /* Keeps compiler happy! */
        return dummyReturn;
        #pragma warning restore CS0162
    }

    public string ToString(Func<string, string>? lineProcessor)
    {
        var sb = new StringBuilder();

        foreach (var message in _messages)
        {
            if (lineProcessor != null)
            {
                sb.AppendLine(lineProcessor(message.ToString()));                    
            }
            else
            {
                sb.AppendLine($"{message.ToString()}");
            }
        }

        return sb.ToString();
    }

    public override string ToString()
        => ToString(null);

    public DynamicValue ToDynamicValue()
    {
        var ret = new EvilArray(Messages.Count);

        for (var i = 0; i < Messages.Count; i++)
        {
            var msg = Messages[i];

            ret[i] = new Table
            {
                { "severity", (int)msg.Severity },
                { "message", msg.Message },
                { "file_name", msg.FileName ?? DynamicValue.Nil },
                { "message_code", msg.MessageCode },
                { "line", msg.Line },
                { "column", msg.Column }
            };
        }

        return ret;
    }

    private static int GetCode(string id)
    {
        if (id.StartsWith("EV", StringComparison.OrdinalIgnoreCase) && int.TryParse(id[2..], out var code))
        {
            return code;
        }
        
        return 0;
    }

    private bool IsSuppressed(string id, int messageCode)
    {
        if (SuppressedCodes.Count == 0)
        {
            return false;
        }
        
        foreach (var code in SuppressedCodes)
        {
            if (MatchesCode(id, code))
            {
                return true;
            }
            
            if (messageCode > 0 && MatchesCode($"EV{messageCode:D4}", code))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool IsWarningAsError(string id, int messageCode)
    {
        if (WarningsAsErrors)
        {
            return true;
        }
        
        if (WarningsAsErrorsCodes.Count == 0)
        {
            return false;
        }
        
        foreach (var code in WarningsAsErrorsCodes)
        {
            if (MatchesCode(id, code))
            {
                return true;
            }
            
            if (messageCode > 0 && MatchesCode($"EV{messageCode:D4}", code))
            {
                return true;
            }
        }
        
        return false;
    }

    private static bool MatchesCode(string targetCode, string codeToMatch)
    {
        if (string.Equals(targetCode, codeToMatch, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normTarget = targetCode.StartsWith("EV", StringComparison.OrdinalIgnoreCase)
            ? targetCode[2..]
            : targetCode;
        
        var normMatch = codeToMatch.StartsWith("EV", StringComparison.OrdinalIgnoreCase)
            ? codeToMatch[2..]
            : codeToMatch;

        return string.Equals(normTarget, normMatch, StringComparison.OrdinalIgnoreCase);
    }
}
