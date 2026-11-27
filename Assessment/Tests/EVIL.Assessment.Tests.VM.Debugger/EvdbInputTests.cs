namespace EVIL.Assessment.Tests.VM.Debugger;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EVIL.Assessment.Tests.VM.Debugger.Utilities;
using EVIL.FE.Debugger;
using EVIL.VM.Debugger;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class EvdbInputTests
{
    [Test]
    public async Task Evdb_PromptLoop_ExecutesEditedInputAndCanRecallItFromHistory()
    {
        var fileName = "input_history.vil";
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var debugger = new DebuggerSession(vm);
        var keys = new Queue<ConsoleKeyInfo>([
            Character('p'), Character(' '), Character('2'),
            Special(ConsoleKey.LeftArrow),
            Character('4'),
            Special(ConsoleKey.Enter),
            Special(ConsoleKey.UpArrow),
            Special(ConsoleKey.Enter),
            Character('c'),
            Special(ConsoleKey.Enter)
        ]);
        
        _ = new EvdbSession(
            vm,
            debugger,
            fileName,
            keys.Dequeue
        );
        
        var chunk = compiler.Compile(
            "val value = 0;",
            fileName
        );
        
        var originalOutput = Console.Out;
        await using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            debugger.Pause();
            vm.Run();
            vm.MainFiber.Schedule(chunk);
            await vm.MainFiber.BlockUntilFinishedAsync();
        }
        finally
        {
            Console.SetOut(originalOutput);
            vm.Stop();
        }

        output.ToString()
            .Split("42 = <Number> 42")
            .Length
            .ShouldBe(3);
    }

    private static ConsoleKeyInfo Character(char character)
    {
        return new ConsoleKeyInfo(character, ConsoleKey.NoName, false, false, false);
    }

    private static ConsoleKeyInfo Special(ConsoleKey key)
    {
        return new ConsoleKeyInfo('\0', key, false, false, false);
    }
}