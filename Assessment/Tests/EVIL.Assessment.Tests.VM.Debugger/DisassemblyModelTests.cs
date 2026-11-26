namespace EVIL.Assessment.Tests.VM.Debugger;

using System;
using System.Linq;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.Assessment.Tests.Base;
using NUnit.Framework;

[TestFixture]
public class DisassemblyModelTests : EvilTest
{
    [Test]
    public void InstructionDecoder_DecodesBasicArithmeticAndVariables()
    {
        var script = """
            fn test(a, b) {
                rw val c = a + b;
                ret c * 2;
            }
            """;

        var chunk = Compiler.Compile(script, "test_arithmetic");
        var testChunk = chunk["test"]!;

        var disasm = testChunk.Disassemble();

        Assert.That(disasm, Is.Not.Null);
        Assert.That(disasm.InstructionCount, Is.GreaterThan(0));

        var opcodes = disasm.Instructions.Select(i => i.OpCode).ToList();
        Assert.That(opcodes, Contains.Item(OpCode.GETARG_0));
        Assert.That(opcodes, Contains.Item(OpCode.GETARG_1));
        Assert.That(opcodes, Contains.Item(OpCode.ADD));
        Assert.That(opcodes, Contains.Item(OpCode.SETLOCAL_0));
        Assert.That(opcodes, Contains.Item(OpCode.GETLOCAL_0));
        Assert.That(opcodes, Contains.Item(OpCode.MUL));
        Assert.That(opcodes, Contains.Item(OpCode.RET));

        var getArg0 = disasm.Instructions.First(i => i.OpCode == OpCode.GETARG_0);
        Assert.That(getArg0.SymbolicAnnotation, Is.EqualTo("a"));

        var getArg1 = disasm.Instructions.First(i => i.OpCode == OpCode.GETARG_1);
        Assert.That(getArg1.SymbolicAnnotation, Is.EqualTo("b"));

        var setLocal0 = disasm.Instructions.First(i => i.OpCode == OpCode.SETLOCAL_0);
        Assert.That(setLocal0.SymbolicAnnotation, Is.EqualTo("c"));
    }

    [Test]
    public void InstructionDecoder_DecodesSliceFlagsWithoutDesynchronizing()
    {
        var script = """
            fn from_end(values) {
                ret values[#-1];
            }
            fn range(values) {
                ret values[..#-1];
            }
            """;

        var chunk = Compiler.Compile(script, "test_slice_disassembly");
        var fromEndInstructions = chunk["from_end"]!.Disassemble().Instructions;
        var rangeInstructions = chunk["range"]!.Disassemble().Instructions;
        var fromEndSlice = fromEndInstructions.Single(i => i.OpCode == OpCode.SLICE);
        var rangeSlice = rangeInstructions.Single(i => i.OpCode == OpCode.SLICE);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fromEndSlice.Length, Is.EqualTo(2));
            Assert.That(fromEndSlice.Operands, Has.Count.EqualTo(1));
            Assert.That(fromEndSlice.Operands[0].Kind, Is.EqualTo(OperandKind.ImmediateByte));
            Assert.That(fromEndSlice.Operands[0].RawValue, Is.EqualTo((byte)21));
            Assert.That(
                fromEndSlice.Operands[0].FormattedText,
                Is.EqualTo("START|START_FROM_END|SCALAR")
            );

            Assert.That(rangeSlice.Length, Is.EqualTo(2));
            Assert.That(rangeSlice.Operands[0].RawValue, Is.EqualTo((byte)10));
            Assert.That(
                rangeSlice.Operands[0].FormattedText,
                Is.EqualTo("END|END_FROM_END")
            );
            Assert.That(fromEndInstructions[^1].OpCode, Is.EqualTo(OpCode.RET));
            Assert.That(rangeInstructions[^1].OpCode, Is.EqualTo(OpCode.RET));
        }
    }

    [Test]
    public void InstructionDecoder_CorrectlyDecodesXargsWithoutDesynchronizing()
    {
        var script = """
            fn variadic(first) {
                val rest = ...;
                ret rest;
            }
            """;

        var chunk = Compiler.Compile(script, "test_xargs");
        var fnChunk = chunk["variadic"]!;
        var disasm = fnChunk.Disassemble();

        var xargsInst = disasm.Instructions.FirstOrDefault(
            i => i.OpCode == OpCode.XARGS
        );

        Assert.That(xargsInst, Is.Not.Null);

        /* 1 byte opcode + 1 byte mode */
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                xargsInst!.Length,
                Is.EqualTo(2)
            );

            Assert.That(
                xargsInst.Operands,
                Has.Count.EqualTo(1)
            );
        }

        Assert.That(
            xargsInst.Operands[0].Kind,
            Is.EqualTo(OperandKind.ImmediateByte)
        );

        var xargsIndex = disasm.IndexOf(xargsInst.Offset);
        Assert.That(xargsIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(xargsIndex + 1, Is.LessThan(disasm.InstructionCount));

        var nextInst = disasm.Instructions[xargsIndex + 1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(nextInst.Offset, Is.EqualTo(xargsInst.Offset + 2));
            Assert.That(nextInst.OpCode, Is.EqualTo(OpCode.SETLOCAL_0));
        }

        var lastInst = disasm.Instructions[^1];
        Assert.That(lastInst.OpCode, Is.EqualTo(OpCode.RET));
    }

    [Test]
    public void InstructionDecoder_DecodesControlFlowAndLabels()
    {
        var script = """
            fn test(x) {
                if (x > 0) {
                    ret 100;
                } else {
                    ret 200;
                }
            }
            """;

        var chunk = Compiler.Compile(script, "test_branches");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();
        Assert.That(disasm.JumpTargets, Is.Not.Empty);

        var fjmp = disasm.Instructions.FirstOrDefault(i => i.OpCode == OpCode.FJMP);
        Assert.That(fjmp, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fjmp!.TargetOffset, Is.Not.Null);
            Assert.That(fjmp.IsBranch, Is.True);
            Assert.That(fjmp.SymbolicAnnotation, Does.Contain("bytes"));
        }

        var targetInst = disasm.FindAtOffset(fjmp.TargetOffset!.Value);
        Assert.That(targetInst, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(targetInst!.IsJumpTarget, Is.True);
            Assert.That(targetInst.Label, Is.EqualTo($"L_{fjmp.TargetOffset.Value:X4}"));
        }
    }

    [Test]
    public void InstructionDecoder_DecodesSwitchStatement()
    {
        var script = """
            fn test(x) {
                val result = by x {
                    1 -> "one",
                    2 -> "two",
                    else: "other"
                };
                ret result;
            }
            """;

        var chunk = Compiler.Compile(script, "test_switch");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var switchInst = disasm.Instructions.FirstOrDefault(i => i.OpCode == OpCode.SWITCH);
        Assert.That(
            switchInst,
            Is.Not.Null
        );

        using (Assert.EnterMultipleScope())
        {
            /* default + count + cases */
            Assert.That(switchInst!.Operands, Has.Count.GreaterThanOrEqualTo(3));
            Assert.That(switchInst.Operands[0].Kind, Is.EqualTo(OperandKind.JumpTargetAddress));
            Assert.That(switchInst.Operands[1].Kind, Is.EqualTo(OperandKind.ImmediateInt32));

            Assert.That(disasm.JumpTargets, Has.Count.GreaterThanOrEqualTo(2));
        }
    }

    [Test]
    public void ChunkDisassembly_GetWindow_ReturnsFocusedContext()
    {
        var script = """
            fn test() {
                rw val a = 1;
                rw val b = 2;
                rw val c = 3;
                rw val d = 4;
                rw val e = 5;
                rw val f = 6;
                rw val g = 7;
                ret a + b + c + d + e + f + g;
            }
            """;

        var chunk = Compiler.Compile(script, "test_window");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        Assert.That(disasm.InstructionCount, Is.GreaterThan(15));

        var midIndex = disasm.InstructionCount / 2;
        var centerIp = disasm.Instructions[midIndex].Offset;
        var window = disasm.GetWindow(centerIp, radiusBefore: 3, radiusAfter: 3);

        Assert.That(window, Has.Count.EqualTo(7));
        Assert.That(window[3].Offset, Is.EqualTo(centerIp));
    }

    [Test]
    public void ChunkDisassembly_GetWindow_BoundaryClamping()
    {
        var script = """
            fn test() {
                ret 42;
            }
            """;

        var chunk = Compiler.Compile(script, "test_clamp");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var windowStart = disasm.GetWindow(0, radiusBefore: 10, radiusAfter: 10);
        Assert.That(windowStart, Has.Count.EqualTo(disasm.InstructionCount));
        Assert.That(windowStart[0].Offset, Is.EqualTo(0));

        var windowPastEnd = disasm.GetWindow(999999, radiusBefore: 2, radiusAfter: 2);
        Assert.That(windowPastEnd, Is.Not.Empty);
    }

    [Test]
    public void ChunkDisassembly_GetRange_ReturnsRequestedSlice()
    {
        var script = """
            fn test() {
                val a = 1;
                val b = 2;
                val c = 3;
                ret a + b + c;
            }
            """;

        var chunk = Compiler.Compile(script, "test_range");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var range = disasm.GetRange(0, 3);
        Assert.That(range, Has.Count.EqualTo(3));
        Assert.That(range[0].Offset, Is.EqualTo(0));
    }

    [Test]
    public void ChunkDisassembly_GetForLine_ReturnsInstructionsMatchingSourceLine()
    {
        var script = """
            fn test(x) {
                val a = x * 2;
                ret a;
            }
            """;

        var chunk = Compiler.Compile(script, "test_line");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var line2Insts = disasm.GetForLine(2);
        Assert.That(line2Insts, Is.Not.Empty);
        Assert.That(
            line2Insts.All(i => i.Line == 2),
            Is.True
        );
    }

    [Test]
    public void InstructionDecoder_HandlesClosuresAndSubChunks()
    {
        var script = """
            fn outer(x) {
                val inner = fn(y) -> x + y;
                ret inner(10);
            }
            """;

        var chunk = Compiler.Compile(script, "test_closure");
        var outerChunk = chunk["outer"]!;
        var disasm = outerChunk.Disassemble();

        var ldcnk = disasm.Instructions.FirstOrDefault(i => i.OpCode == OpCode.LDCNK);
        Assert.That(ldcnk, Is.Not.Null);
        Assert.That(ldcnk!.SymbolicAnnotation, Does.Contain("subchunk"));

        var innerChunk = outerChunk.SubChunks[0];
        var innerDisasm = innerChunk.Disassemble();

        var getClosure = innerDisasm.Instructions.FirstOrDefault(i =>
            i.OpCode is OpCode.GETCLOSURE
                     or OpCode.GETCLOSURE_0
                     or OpCode.GETCLOSURE_B
        );

        Assert.That(getClosure, Is.Not.Null);
        Assert.That(
            getClosure!.SymbolicAnnotation,
            Does.Contain("'outer'")
        );
    }

    [Test]
    public void ChunkDisassembly_FindAtOffset_HandlesExactAndMissingOffsets()
    {
        var script = """
            fn test() {
                val a = 1;
                val b = 2;
                ret a + b;
            }
            """;

        var chunk = Compiler.Compile(script, "test_find_offset");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var first = disasm.Instructions[0];
        var found = disasm.FindAtOffset(first.Offset);

        Assert.That(found, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(found!.Offset, Is.EqualTo(first.Offset));

            Assert.That(disasm.FindAtOffset(99999), Is.Null);
            Assert.That(disasm.FindAtOffset(-1), Is.Null);
        }
    }

    [Test]
    public void ChunkDisassembly_IndexOf_HandlesExactAndMissingOffsets()
    {
        var script = """
            fn test() {
                val a = 10;
                ret a;
            }
            """;

        var chunk = Compiler.Compile(script, "test_index_of");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        for (var i = 0; i < disasm.Instructions.Count; i++)
        {
            Assert.That(disasm.IndexOf(disasm.Instructions[i].Offset), Is.EqualTo(i));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(disasm.IndexOf(99999), Is.EqualTo(-1));
            Assert.That(disasm.IndexOf(-5), Is.EqualTo(-1));
        }
    }

    [Test]
    public void ChunkDisassembly_GetRange_HandlesEdgeCasesAndCountExceeding()
    {
        var script = """
            fn test() {
                val a = 1;
                val b = 2;
                val c = 3;
                ret a + b + c;
            }
            """;

        var chunk = Compiler.Compile(script, "test_get_range_edges");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(disasm.GetRange(0, 0), Is.Empty);
            Assert.That(disasm.GetRange(0, -5), Is.Empty);
        }

        var all = disasm.GetRange(0, disasm.InstructionCount + 100);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(all, Has.Count.EqualTo(disasm.InstructionCount));
            Assert.That(disasm.GetRange(99999, 5), Is.Empty);
        }
    }

    [Test]
    public void ChunkDisassembly_GetWindow_HandlesAsymmetricRadii()
    {
        var script = """
            fn test() {
                rw val a = 1;
                rw val b = 2;
                rw val c = 3;
                rw val d = 4;
                rw val e = 5;
                rw val f = 6;
                rw val g = 7;
                rw val h = 8;
                ret a + b + c + d + e + f + g + h;
            }
            """;

        var chunk = Compiler.Compile(script, "test_asymmetric_window");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var centerIdx = 5;
        var centerIp = disasm.Instructions[centerIdx].Offset;
        var window = disasm.GetWindow(centerIp, radiusBefore: 2, radiusAfter: 4);

        Assert.That(window, Has.Count.EqualTo(7));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                window[0].Offset,
                Is.EqualTo(disasm.Instructions[centerIdx - 2].Offset)
            );

            Assert.That(
                window[2].Offset,
                Is.EqualTo(centerIp)
            );

            Assert.That(
                window[6].Offset,
                Is.EqualTo(disasm.Instructions[centerIdx + 4].Offset)
            );
        }
    }

    [Test]
    public void ChunkDisassembly_GetForLine_NonExistentLine_ReturnsEmptyList()
    {
        var script = "fn test() { ret 1; }";
        var chunk = Compiler.Compile(script, "test_nonexistent_line");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        Assert.That(disasm.GetForLine(9999), Is.Empty);
    }

    [Test]
    public void InstructionDecoder_DecodesNumericConstantsAndDoubleOperands()
    {
        var script = """
            fn test() {
                val pi = 3.14159;
                ret pi;
            }
            """;

        var chunk = Compiler.Compile(script, "test_double_operands");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var ldnum = disasm.Instructions.FirstOrDefault(i => i.OpCode == OpCode.LDNUM);
        Assert.That(ldnum, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ldnum!.Operands, Has.Count.EqualTo(1));
            Assert.That(ldnum.Operands[0].Kind, Is.EqualTo(OperandKind.ImmediateDouble));
            Assert.That(
                Convert.ToDouble(ldnum.Operands[0].RawValue),
                Is.EqualTo(3.14159).Within(0.00001)
            );
        }
    }

    [Test]
    public void InstructionDecoder_DecodesLoopBackwardBranches()
    {
        var script = """
            fn test() {
                rw val i = 0;
                while (i < 5) {
                    i += 1;
                }
                ret i;
            }
            """;

        var chunk = Compiler.Compile(script, "test_backward_branch");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var backwardJump = disasm.Instructions.FirstOrDefault(
            i => i is { IsBranch: true, TargetOffset: not null }
              && i.TargetOffset.Value < i.Offset
        );

        Assert.That(backwardJump, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(backwardJump!.IsBranch, Is.True);
            Assert.That(backwardJump.TargetOffset, Is.Not.Null);
        }
        Assert.That(backwardJump.TargetOffset!.Value, Is.LessThan(backwardJump.Offset));

        var targetInst = disasm.FindAtOffset(backwardJump.TargetOffset.Value);
        Assert.That(targetInst, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(targetInst!.IsJumpTarget, Is.True);
            Assert.That(targetInst.Label, Is.EqualTo($"L_{backwardJump.TargetOffset.Value:X4}"));
        }
    }
}
