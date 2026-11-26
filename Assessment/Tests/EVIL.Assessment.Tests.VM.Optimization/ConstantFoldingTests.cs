namespace EVIL.Assessment.Tests.VM.Optimization;

using System.Collections.Generic;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Assessment.Tests.Base;
using EVIL.Core.Common.TypeSystem;
using NUnit.Framework;

[TestFixture]
public class ConstantFoldingTests : EvilTest
{

    [Test]
    public void ConstantFolding_ArithmeticExpressions_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val timeout = 10 * 60 * 1000;
                ret timeout;
            }
            """;

        var chunk = Compiler.Compile(script, "fold_arithmetic_test");
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.MUL));
        Assert.That(opcodes, Contains.Item(OpCode.LDNUM));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(600000.0));
        }
    }

    [Test]
    public void ConstantFolding_StringConcatenation_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val tag = "item_" + 1;
                val s2 = "hello " + "world";
                val s3 = 42 + "_id";
                val s4 = "flag_" + true;
                ret [tag, s2, s3, s4];
            }
            """;

        var chunk = Compiler.Compile(script, "fold_string_test");
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.ADD));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].String, Is.EqualTo("item_1"));
            Assert.That(result.Array![1].String, Is.EqualTo("hello world"));
            Assert.That(result.Array![2].String, Is.EqualTo("42_id"));
            Assert.That(result.Array![3].String, Is.EqualTo("flag_true"));
        }
    }

    [Test]
    public void ConstantFolding_BitwiseAndBooleanLogic_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val bAnd = 12 & 10;
                val bOr = 12 | 10;
                val bXor = 12 ^ 10;
                val shl = 1 << 4;
                val shr = 32 >> 2;
                val lAnd = true && false;
                val lOr = true || false;
                val notVal = !true;
                val notNil = !nil;
                ret [bAnd, bOr, bXor, shl, shr, lAnd, lOr, notVal, notNil];
            }
            """;

        var chunk = Compiler.Compile(script, "fold_bitwise_bool_test");
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
            Assert.That(opcodes, Does.Not.Contain(OpCode.BAND));
            Assert.That(opcodes, Does.Not.Contain(OpCode.BOR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.BXOR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.SHL));
            Assert.That(opcodes, Does.Not.Contain(OpCode.SHR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.LAND));
            Assert.That(opcodes, Does.Not.Contain(OpCode.LOR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.LNOT));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(8.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(14.0));
            Assert.That(result.Array![2].Number, Is.EqualTo(6.0));
            Assert.That(result.Array![3].Number, Is.EqualTo(16.0));
            Assert.That(result.Array![4].Number, Is.EqualTo(8.0));
            Assert.That(result.Array![5].Boolean, Is.False);
            Assert.That(result.Array![6].Boolean, Is.True);
            Assert.That(result.Array![7].Boolean, Is.False);
            Assert.That(result.Array![8].Boolean, Is.True);
        }
    }

    [Test]
    public void ConstantFolding_RelationalAndEquality_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val gtTrue = 10 > 5;
                val gtFalse = 10 > 10;
                val gteTrue = 10 >= 10;
                val ltTrue = 5 < 10;
                val lteTrue = 5 <= 5;
                val eqTrue = 42 == 42;
                val eqFalse = 42 == 43;
                val strEq = "evil" == "evil";
                val strNeq = "evil" != "good";
                val crossEq = "evil" == 42;
                ret [gtTrue, gtFalse, gteTrue, ltTrue, lteTrue, eqTrue, eqFalse, strEq, strNeq, crossEq];
            }
            """;

        var chunk = Compiler.Compile(script, "fold_relational_test");
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
            Assert.That(opcodes, Does.Not.Contain(OpCode.CGT));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CGE));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CLT));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CLE));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CEQ));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CNE));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Boolean, Is.True);
            Assert.That(result.Array![1].Boolean, Is.False);
            Assert.That(result.Array![2].Boolean, Is.True);
            Assert.That(result.Array![3].Boolean, Is.True);
            Assert.That(result.Array![4].Boolean, Is.True);
            Assert.That(result.Array![5].Boolean, Is.True);
            Assert.That(result.Array![6].Boolean, Is.False);
            Assert.That(result.Array![7].Boolean, Is.True);
            Assert.That(result.Array![8].Boolean, Is.True);
            Assert.That(result.Array![9].Boolean, Is.False);
        }
    }

    [Test]
    public void ConstantFolding_SubexpressionFoldingWithVariable_FoldsConstantPart()
    {
        var script = """
            fn test() {
                val base = 5;
                ret base + 10 * 60 * 1000;
            }
            """;

        var chunk = Compiler.Compile(script, "fold_subexpr_test");
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.MUL));
        Assert.That(opcodes.FindAll(op => op == OpCode.ADD).Count, Is.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(600005.0));
        }
    }
}
