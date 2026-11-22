namespace EVIL.Ceres.RuntimeTests;

using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;

[TestFixture]
public class DisassemblyModelTests : TestBase
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
        Assert.That(line2Insts.All(i => i.Line == 2), Is.True);
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
}
