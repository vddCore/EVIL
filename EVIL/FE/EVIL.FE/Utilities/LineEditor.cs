namespace EVIL.FE.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public sealed class LineEditor
{
    private readonly List<string> _history = [];

    public async Task<string?> ReadLineAsync(
        TextReader input,
        TextWriter output,
        Func<ConsoleKeyInfo>? readKey = null,
        Func<string, int, int, string>? render = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        if (readKey == null)
        {
            var line = await input
                .ReadLineAsync()
                .ConfigureAwait(false);
            
            return AddToHistory(line);
        }

        var buffer = new StringBuilder();
        var caret = 0;
        var historyIndex = _history.Count;
        
        string? draft = null;

        while (true)
        {
            var key = readKey();
            if (key.KeyChar == '\u0004'&& buffer.Length == 0)
            {
                return null;
            }

            var oldLength = buffer.Length;
            var oldCaret = caret;

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                {
                    var submitted = AddToHistory(
                        buffer.ToString()
                    );

                    await output
                        .WriteLineAsync()
                        .ConfigureAwait(false);

                    return submitted;
                }

                case ConsoleKey.LeftArrow:
                {
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        while (caret > 0 && char.IsWhiteSpace(buffer[caret - 1]))
                        {
                            caret--;
                        }

                        while (caret > 0 && !char.IsWhiteSpace(buffer[caret - 1]))
                        {
                            caret--;
                        }

                        if (caret < oldCaret)
                        {
                            await output
                                .WriteAsync(new string('\b', oldCaret - caret))
                                .ConfigureAwait(false);
                        }
                    }
                    else if (caret > 0)
                    {
                        caret--;

                        await output
                            .WriteAsync('\b')
                            .ConfigureAwait(false);
                    }

                    break;
                }

                case ConsoleKey.RightArrow:
                {
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        while (caret < buffer.Length && !char.IsWhiteSpace(buffer[caret]))
                        {
                            caret++;
                        }

                        while (caret < buffer.Length && char.IsWhiteSpace(buffer[caret]))
                        {
                            caret++;
                        }

                        await output
                            .WriteAsync(Render(buffer, oldCaret, caret - oldCaret, render))
                            .ConfigureAwait(false);
                    }
                    else if (caret < buffer.Length)
                    {
                        await output
                            .WriteAsync(Render(buffer, caret, 1, render))
                            .ConfigureAwait(false);

                        caret++;
                    }

                    break;
                }

                case ConsoleKey.Home:
                {
                    while (caret > 0)
                    {
                        caret--;

                        await output
                            .WriteAsync('\b')
                            .ConfigureAwait(false);
                    }
                    
                    break;
                }

                case ConsoleKey.End:
                {
                    await output
                        .WriteAsync(Render(buffer, caret, buffer.Length - caret, render))
                        .ConfigureAwait(false);

                    caret = buffer.Length;
                    
                    break;
                }

                case ConsoleKey.Backspace:
                {
                    if (caret > 0)
                    {
                        buffer.Remove(caret - 1, 1);
                        caret--;

                        await RedrawAsync(
                            output,
                            buffer,
                            oldLength,
                            oldCaret,
                            caret,
                            render
                        ).ConfigureAwait(false);
                    }

                    break;
                }

                case ConsoleKey.Delete:
                {
                    if (caret < buffer.Length)
                    {
                        buffer.Remove(caret, 1);

                        await RedrawAsync(
                            output,
                            buffer,
                            oldLength,
                            oldCaret,
                            caret,
                            render
                        ).ConfigureAwait(false);
                    }

                    break;
                }

                case ConsoleKey.UpArrow:
                {
                    if (_history.Count > 0 && historyIndex > 0)
                    {
                        if (historyIndex == _history.Count)
                        {
                            draft = buffer.ToString();
                        }

                        buffer.Clear().Append(
                            _history[--historyIndex]
                        );

                        caret = buffer.Length;

                        await RedrawAsync(
                            output,
                            buffer,
                            oldLength,
                            oldCaret,
                            caret,
                            render
                        ).ConfigureAwait(false);
                    }

                    break;
                }

                case ConsoleKey.DownArrow:
                {
                    if (historyIndex < _history.Count)
                    {
                        historyIndex++;
                        
                        buffer.Clear().Append(
                            historyIndex == _history.Count
                                ? draft ?? string.Empty
                                : _history[historyIndex]
                        );
                        
                        caret = buffer.Length;
                        
                        await RedrawAsync(
                            output,
                            buffer,
                            oldLength,
                            oldCaret,
                            caret,
                            render
                        ).ConfigureAwait(false);
                    }

                    break;
                }

                default:
                {
                    if (!char.IsControl(key.KeyChar))
                    {
                        buffer.Insert(
                            caret++,
                            key.KeyChar
                        );
                        
                        await RedrawAsync(
                            output,
                            buffer,
                            oldLength,
                            oldCaret,
                            caret,
                            render
                        ).ConfigureAwait(false);
                    }

                    break;
                }
            }
        }
    }

    private string? AddToHistory(string? line)
    {
        if (!string.IsNullOrEmpty(line))
        {
            _history.Add(line);
        }

        return line;
    }

    private static async Task RedrawAsync(
        TextWriter output,
        StringBuilder buffer,
        int oldLength,
        int oldCaret,
        int caret,
        Func<string, int, int, string>? render
    )
    {
        await output.WriteAsync(
            new string('\b', oldCaret)
        ).ConfigureAwait(false);
        
        await output.WriteAsync(
            Render(buffer, 0, buffer.Length, render)
        ).ConfigureAwait(false);

        var renderedLength = Math.Max(
            oldLength,
            buffer.Length
        );
        
        if (renderedLength > buffer.Length)
        {
            await output.WriteAsync(
                new string(' ', renderedLength - buffer.Length)
            ).ConfigureAwait(false);
        }

        await output.WriteAsync(
            new string('\b', renderedLength - caret)
        ).ConfigureAwait(false);
    }

    private static string Render(
        StringBuilder buffer,
        int start,
        int length,
        Func<string, int, int, string>? render)
    {
        var source = buffer.ToString();
        return render?.Invoke(source, start, length)
            ?? source.Substring(start, length);
    }
}