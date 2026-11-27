namespace EVIL.Assessment.Tests.FE;

using System;
using System.Collections.Generic;
using System.IO;
using EVIL.FE.Utilities;
using Shouldly;

[TestFixture]
public class LineEditorTests
{
    [Test]
    public async Task ReadLineAsync_InsertsAndDeletesAtTheCaret()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('a'),
            Character('c'),
            Special(ConsoleKey.LeftArrow),
            Character('b'),
            Special(ConsoleKey.RightArrow),
            Special(ConsoleKey.LeftArrow),
            Special(ConsoleKey.Delete),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("ab");
    }

    [Test]
    public async Task ReadLineAsync_ControlLeftMovesToThePreviousWordStartForInsertion()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('f'), Character('i'), Character('r'), Character('s'), Character('t'),
            Character(' '), Character(' '),
            Character('s'), Character('e'), Character('c'), Character('o'), Character('n'), Character('d'),
            Character(','), Character('p'), Character('a'), Character('r'), Character('t'),
            Control(ConsoleKey.LeftArrow),
            Character('x'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("first  xsecond,part");
    }

    [Test]
    public async Task ReadLineAsync_ControlLeftFromInsideWordSkipsUnicodeWhitespace()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('f'), Character('i'), Character('r'), Character('s'), Character('t'),
            Character('\u2003'),
            Character('s'), Character('e'), Character('c'), Character('o'), Character('n'), Character('d'),
            Special(ConsoleKey.LeftArrow),
            Control(ConsoleKey.LeftArrow),
            Character('x'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("first\u2003xsecond");
    }

    [Test]
    public async Task ReadLineAsync_ControlLeftRepeatedlyMovesToAndStaysAtLineStart()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('o'), Character('n'), Character('e'), Character(' '),
            Character('t'), Character('w'), Character('o'), Character(' '),
            Character('t'), Character('h'), Character('r'), Character('e'), Character('e'),
            Control(ConsoleKey.LeftArrow),
            Control(ConsoleKey.LeftArrow),
            Control(ConsoleKey.LeftArrow),
            Control(ConsoleKey.LeftArrow),
            Character('!'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("!one two three");
    }

    [Test]
    public async Task ReadLineAsync_ControlLeftWritesBackspacesForTheCursorMovement()
    {
        var editor = new LineEditor();
        var output = new StringWriter();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('w'), Character('o'), Character('r'), Character('d'),
            Control(ConsoleKey.LeftArrow),
            Special(ConsoleKey.Enter)
        ]);
        var movementStart = 0;
        var movementEnd = 0;

        ConsoleKeyInfo ReadKey()
        {
            var key = keys.Dequeue();
            if (key.Key == ConsoleKey.LeftArrow && key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                movementStart = output.GetStringBuilder().Length;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                movementEnd = output.GetStringBuilder().Length;
            }

            return key;
        }

        await editor.ReadLineAsync(TextReader.Null, output, ReadKey);

        output.ToString()[movementStart..movementEnd].ShouldBe(new string('\b', 4));
    }

    [Test]
    public async Task ReadLineAsync_ControlRightMovesToTheNextWordStartForInsertion()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('o'), Character('n'), Character('e'),
            Character(' '), Character(' '), Character(' '),
            Character('t'), Character('w'), Character('o'), Character(','), Character('t'), Character('h'), Character('r'), Character('e'), Character('e'),
            Special(ConsoleKey.Home),
            Special(ConsoleKey.RightArrow),
            Control(ConsoleKey.RightArrow),
            Character('x'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("one   xtwo,three");
    }

    [Test]
    public async Task ReadLineAsync_ControlRightMovesWhitespaceOnlyLinesToTheEnd()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character(' '), Character(' '), Character(' '), Character('\u2003'),
            Special(ConsoleKey.Home),
            Control(ConsoleKey.RightArrow),
            Character('x'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("   \u2003x");
    }

    [Test]
    public async Task ReadLineAsync_ControlRightOnAnEmptyLineDoesNotMovePastTheBuffer()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Control(ConsoleKey.RightArrow),
            Character('x'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("x");
    }

    [Test]
    public async Task ReadLineAsync_ControlRightWithAnotherModifierMovesThroughRecalledHistory()
    {
        var editor = new LineEditor();
        using var historyInput = new StringReader("old  words\n");
        await editor.ReadLineAsync(historyInput, TextWriter.Null);

        var keys = new Queue<ConsoleKeyInfo>([
            Special(ConsoleKey.UpArrow),
            Special(ConsoleKey.Home),
            Control(ConsoleKey.RightArrow, shift: true),
            Character('x'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("old  xwords");
    }

    [Test]
    public async Task ReadLineAsync_ControlRightWritesCrossedCharactersAndStopsAtLineEnd()
    {
        var editor = new LineEditor();
        var output = new StringWriter();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('o'), Character('n'), Character('e'), Character(' '), Character(' '),
            Character('t'), Character('w'), Character('o'),
            Special(ConsoleKey.Home),
            Control(ConsoleKey.RightArrow),
            Control(ConsoleKey.RightArrow),
            Control(ConsoleKey.RightArrow),
            Control(ConsoleKey.RightArrow),
            Special(ConsoleKey.Enter)
        ]);
        var movementStart = 0;
        var movementEnd = 0;

        ConsoleKeyInfo ReadKey()
        {
            var key = keys.Dequeue();
            if (key.Key == ConsoleKey.RightArrow && key.Modifiers.HasFlag(ConsoleModifiers.Control) && movementStart == 0)
            {
                movementStart = output.GetStringBuilder().Length;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                movementEnd = output.GetStringBuilder().Length;
            }

            return key;
        }

        await editor.ReadLineAsync(TextReader.Null, output, ReadKey);

        output.ToString()[movementStart..movementEnd].ShouldBe("one  two");
    }

    [Test]
    public async Task ReadLineAsync_UnmodifiedRightArrowStillMovesOneCharacter()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('a'), Character('b'),
            Special(ConsoleKey.Home),
            Special(ConsoleKey.RightArrow),
            Character('x'),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("axb");
    }

    [Test]
    public async Task ReadLineAsync_BackspaceRemovesTheCharacterBeforeTheCaret()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('a'),
            Character('b'),
            Character('c'),
            Special(ConsoleKey.LeftArrow),
            Special(ConsoleKey.Backspace),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("ac");
    }

    [Test]
    public async Task ReadLineAsync_ArrowAndDeletionKeysRespectLineBoundaries()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Special(ConsoleKey.LeftArrow),
            Special(ConsoleKey.Backspace),
            Special(ConsoleKey.UpArrow),
            Special(ConsoleKey.DownArrow),
            Character('x'),
            Special(ConsoleKey.RightArrow),
            Special(ConsoleKey.Delete),
            Special(ConsoleKey.Enter)
        ]);

        var result = await editor.ReadLineAsync(
            TextReader.Null,
            TextWriter.Null,
            keys.Dequeue
        );

        result.ShouldBe("x");
    }

    [Test]
    public async Task ReadLineAsync_NavigatesHistoryAndRestoresTheDraft()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('o'), Character('l'), Character('d'), Special(ConsoleKey.Enter),
            Character('n'), Character('e'), Character('w'), Special(ConsoleKey.Enter),
            Character('d'), Character('r'), Character('a'), Character('f'), Character('t'),
            Special(ConsoleKey.UpArrow),
            Special(ConsoleKey.UpArrow),
            Special(ConsoleKey.DownArrow),
            Special(ConsoleKey.DownArrow),
            Special(ConsoleKey.Enter)
        ]);

        await editor.ReadLineAsync(TextReader.Null, TextWriter.Null, keys.Dequeue);
        await editor.ReadLineAsync(TextReader.Null, TextWriter.Null, keys.Dequeue);
        var result = await editor.ReadLineAsync(TextReader.Null, TextWriter.Null, keys.Dequeue);

        result.ShouldBe("draft");
    }

    [Test]
    public async Task ReadLineAsync_FallbackRecordsNonEmptyLinesInHistory()
    {
        var editor = new LineEditor();
        using var input = new StringReader("first\n\n");

        (await editor.ReadLineAsync(input, TextWriter.Null)).ShouldBe("first");
        (await editor.ReadLineAsync(input, TextWriter.Null)).ShouldBe(string.Empty);

        var keys = new Queue<ConsoleKeyInfo>([
            Special(ConsoleKey.UpArrow),
            Special(ConsoleKey.Enter)
        ]);
        var recalled = await editor.ReadLineAsync(TextReader.Null, TextWriter.Null, keys.Dequeue);

        recalled.ShouldBe("first");
    }

    [Test]
    public async Task ReadLineAsync_ControlDOnAnEmptyLineReturnsEof()
    {
        var editor = new LineEditor();
        var keys = new Queue<ConsoleKeyInfo>([
            new ConsoleKeyInfo('\u0004', ConsoleKey.D, false, false, true)
        ]);

        var result = await editor.ReadLineAsync(TextReader.Null, TextWriter.Null, keys.Dequeue);

        result.ShouldBeNull();
    }

    private static ConsoleKeyInfo Character(char character)
    {
        return new ConsoleKeyInfo(character, ConsoleKey.NoName, false, false, false);
    }

    private static ConsoleKeyInfo Special(ConsoleKey key)
    {
        return new ConsoleKeyInfo('\0', key, false, false, false);
    }

    private static ConsoleKeyInfo Control(ConsoleKey key, bool shift = false)
    {
        return new ConsoleKeyInfo('\0', key, shift, false, true);
    }
}