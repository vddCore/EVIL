namespace EVIL.CommonTypes.Diagnostics;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class DiagnosticBag : IDiagnosticSink
{
    private readonly List<Diagnostic> _diagnostics = [];
    private readonly object _lock = new();

    public event EventHandler<Diagnostic>? DiagnosticReported;

    public IReadOnlyList<Diagnostic> Diagnostics
    {
        get
        {
            lock (_lock)
            {
                return _diagnostics.ToArray();
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _diagnostics.Count;
            }
        }
    }

    public int InfoCount
    {
        get
        {
            lock (_lock)
            {
                return _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Info);
            }
        }
    }

    public int WarningCount
    {
        get
        {
            lock (_lock)
            {
                return _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
            }
        }
    }

    public int ErrorCount
    {
        get
        {
            lock (_lock)
            {
                return _diagnostics.Count(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Fatal or DiagnosticSeverity.InternalError);
            }
        }
    }

    public int FatalCount
    {
        get
        {
            lock (_lock)
            {
                return _diagnostics.Count(d => d.Severity is DiagnosticSeverity.Fatal or DiagnosticSeverity.InternalError);
            }
        }
    }

    public bool HasErrors => ErrorCount > 0;
    public bool HasAnyMessages => Count > 0;

    public void Report(Diagnostic diagnostic)
    {
        lock (_lock)
        {
            _diagnostics.Add(diagnostic);
        }

        DiagnosticReported?.Invoke(this, diagnostic);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _diagnostics.Clear();
        }
    }

    public override string ToString()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine, _diagnostics.Select(d => d.ToString()));
        }
    }
}
