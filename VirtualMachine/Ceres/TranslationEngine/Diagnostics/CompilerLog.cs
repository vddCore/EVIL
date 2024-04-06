using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Marshaling;
using Ceres.ExecutionEngine.TypeSystem;
using EvilArray = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.TranslationEngine.Diagnostics
{
    public sealed class CompilerLog : IDynamicValueProvider
    {
        private readonly List<CompilerMessage> _messages = new();

        public CompilerMessageSeverity MinimumSeverityLevel { get; set; } = CompilerMessageSeverity.Warning;
        
        public IReadOnlyList<CompilerMessage> Messages => _messages;

        public event EventHandler<CompilerMessageEmitEventArgs>? MessageEmitted;

        public int VerboseCount => _messages.Count(x => x.Severity == CompilerMessageSeverity.Verbose);
        public int WarningCount => _messages.Count(x => x.Severity == CompilerMessageSeverity.Warning);

        public bool HasAnyMessages => VerboseCount > 0 || WarningCount > 0;
        
        public void Clear() 
            => _messages.Clear();

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
            MessageEmitted?.Invoke(this, new(messageObject));
        }
        
        public void EmitWarning(
            string message, 
            string? fileName = null,
            int messageCode = 0,
            int line = 0,
            int column = 0)
        {
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
    }
}