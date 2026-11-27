namespace EVIL.Assessment.Tests.VM.Optimization;

using System.Collections.Generic;
using EVIL.Assessment.Tests.Base;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Diagnostics;
using NUnit.Framework;

[TestFixture]
public class DeadCodeEliminationTests : EvilTest
{

    [Test]
    public void DeadCodeElimination_Ret_StripsSubsequentSiblingStatements()
    {
        var script = """
            fn test() {
                ret 42;
                val dead1 = 100;
                val dead2 = "unreachable";
                ret 999;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_ret_test");
        var testChunk = chunk["test"]!;

        var opcodes = new List<OpCode>();
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_B));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_0));
        Assert.That(opcodes, Does.Not.Contain(OpCode.LDSTR));
        Assert.That(opcodes.FindAll(op => op == OpCode.RET), Has.Count.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(42.0));
        }
    }

    [Test]
    public void DeadCodeElimination_Throw_StripsSubsequentSiblingStatements()
    {
        var script = """
            fn test() {
                throw "intentional_err";
                val dead = 100;
                ret 200;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_throw_test");
        var testChunk = chunk["test"]!;

        var opcodes = new List<OpCode>();
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        Assert.That(opcodes, Does.Not.Contain(OpCode.RET));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_B));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_0));
        Assert.That(opcodes, Does.Contain(OpCode.THROW));
    }

    [Test]
    public void DeadCodeElimination_Break_StripsSubsequentStatementsInLoop()
    {
        var script = """
            fn test() {
                rw val sum = 0;
                while (true) {
                    sum += 10;
                    break;
                    sum += 1000;
                    val dead = 500;
                }
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_break_test");
        var testChunk = chunk["test"]!;

        var opcodes = new List<OpCode>();
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        Assert.That(opcodes.FindAll(op => op == OpCode.ADD), Has.Count.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(10.0));
        }
    }

    [Test]
    public void DeadCodeElimination_Skip_StripsSubsequentStatementsInLoop()
    {
        var script = """
            fn test() {
                rw val sum = 0;
                for (rw val i = 0; i < 5; i++) {
                    sum += i;
                    skip;
                    sum += 1000;
                }
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_skip_test");
        var testChunk = chunk["test"]!;

        var opcodes = new List<OpCode>();
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        Assert.That(opcodes.FindAll(op => op == OpCode.ADD), Has.Count.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(10.0));
        }
    }

    [Test]
    public void DeadCodeElimination_IfElse_BothBranchesExit_StripsSubsequentStatements()
    {
        var script = """
            fn test(x) {
                if (x > 0) {
                    ret 100;
                    val dead1 = 1;
                } else {
                    ret 200;
                    val dead2 = 2;
                }
                ret 300;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_ifelse_test");
        var testChunk = chunk["test"]!;

        var opcodes = new List<OpCode>();
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(opcodes.FindAll(op => op == OpCode.RET), Has.Count.EqualTo(2));
            Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL));
        }
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_B));

        var res1 = EvilTestResult(script, 5);
        Assert.That(res1.Number, Is.EqualTo(100.0));

        var res2 = EvilTestResult(script, -5);
        Assert.That(res2.Number, Is.EqualTo(200.0));
    }

    [Test]
    public void DeadCodeElimination_IfElse_OneBranchExits_PreservesSubsequentStatements()
    {
        var script = """
            fn test(x) {
                if (x > 0) {
                    ret 100;
                    val dead = 1;
                } else {
                    val a = 5;
                }
                ret 300;
            }
            """;

        var res1 = EvilTestResult(script, 5);
        Assert.That(res1.Number, Is.EqualTo(100.0));

        var res2 = EvilTestResult(script, -5);
        Assert.That(res2.Number, Is.EqualTo(300.0));
    }

    [Test]
    public void DeadCodeElimination_Retry_StripsSubsequentStatementsInCatchBlock()
    {
        var script = """
            fn test() {
                rw val attempts = 0;
                try {
                    attempts++;
                    if (attempts < 3) {
                        throw "err";
                    }
                } catch (e) {
                    retry;
                    attempts += 1000;
                    val dead = 42;
                }
                ret attempts;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(3.0));
        }
    }
}
