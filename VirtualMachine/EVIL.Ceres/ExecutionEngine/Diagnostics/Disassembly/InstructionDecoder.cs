namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;
using EVIL.CommonTypes.TypeSystem;

public static class InstructionDecoder
{
    public static ChunkDisassembly Decode(Chunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        var code = chunk.Code;
        var reader = chunk.SpawnCodeReader();
        var instructions = new List<Instruction>();
        var jumpTargets = new HashSet<int>();

        while (reader.IP < reader.Length)
        {
            var startIp = (int)reader.IP;
            if (TryDecodeNext(chunk, code, reader, jumpTargets, out var inst))
            {
                instructions.Add(inst);
            }
            else
            {
                var raw = code.AsMemory(startIp, 1);
                reader.IP = startIp + 1;
                
                instructions.Add(new Instruction
                {
                    Offset = startIp,
                    Length = 1,
                    OpCode = (OpCode)code[startIp],
                    RawBytes = raw,
                    Mnemonic = $"INVALID(0x{code[startIp]:X2})",
                    Operands = [],
                    Line = chunk.HasDebugInfo ? chunk.DebugDatabase.GetLineForIP(startIp) : -1
                });
            }
        }

        for (var i = 0; i < instructions.Count; i++)
        {
            var inst = instructions[i];
            if (jumpTargets.Contains(inst.Offset))
            {
                inst.Label = $"L_{inst.Offset:X4}";
            }
        }

        return new ChunkDisassembly(chunk, instructions, jumpTargets);
    }

    public static bool TryDecodeNext(
        Chunk chunk,
        byte[] code,
        ChunkReader reader,
        HashSet<int> jumpTargets,
        out Instruction instruction)
    {
        var startIp = (int)reader.IP;
        if (startIp >= reader.Length)
        {
            instruction = null!;
            return false;
        }

        var opCodeByte = reader.ReadByte();
        var opCode = (OpCode)opCodeByte;
        var operands = new List<InstructionOperand>();
        string? annotation = null;
        int? targetOffset = null;

        switch (opCode)
        {
            case OpCode.LDNUM:
            {
                var num = reader.ReadDouble();
                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateDouble,
                    num,
                    num.ToString(CultureInfo.InvariantCulture)
                ));
                break;
            }

            case OpCode.LDNUM_I8:
            {
                var numI8 = (sbyte)reader.ReadByte();
                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateByte,
                    numI8,
                    numI8.ToString(CultureInfo.InvariantCulture)
                ));
                break;
            }

            case OpCode.LDSTR:
            {
                var strId = reader.ReadInt32();
                string? strVal = null;
                if (strId >= 0 && strId < chunk.StringPool.Count)
                {
                    strVal = chunk.StringPool[strId];
                    annotation = strVal != null ? Literalize(strVal) : "<null>";
                }
                else
                {
                    annotation = "<???>";
                }

                operands.Add(new InstructionOperand(
                    OperandKind.StringPoolIndex,
                    strId,
                    strId.ToString(),
                    strVal
                ));
                break;
            }

            case OpCode.LDCNK:
            {
                var chunkId = reader.ReadInt32();
                string? chunkName = null;
                if (chunkId >= 0 && chunkId < chunk.SubChunks.Count)
                {
                    chunkName = chunk.SubChunks[chunkId].Name;
                    annotation = $"subchunk {chunkId} '{chunkName}'";
                }

                operands.Add(new InstructionOperand(
                    OperandKind.SubChunkIndex,
                    chunkId,
                    chunkId.ToString(),
                    chunkName
                ));
                break;
            }

            case OpCode.LDTYPE:
            {
                var typeByte = reader.ReadByte();
                var dynamicType = (DynamicValueType)typeByte;
                annotation = dynamicType.ToString();

                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateByte,
                    typeByte,
                    typeByte.ToString(),
                    dynamicType.ToString()
                ));
                break;
            }

            case OpCode.SETLOCAL_0:
            case OpCode.SETLOCAL_1:
            case OpCode.SETLOCAL_2:
            case OpCode.SETLOCAL_3:
            case OpCode.GETLOCAL_0:
            case OpCode.GETLOCAL_1:
            case OpCode.GETLOCAL_2:
            case OpCode.GETLOCAL_3:
            {
                var slot = opCode switch
                {
                    OpCode.SETLOCAL_0 or OpCode.GETLOCAL_0 => 0,
                    OpCode.SETLOCAL_1 or OpCode.GETLOCAL_1 => 1,
                    OpCode.SETLOCAL_2 or OpCode.GETLOCAL_2 => 2,
                    _ => 3
                };

                string? localName = null;
                if (chunk.HasDebugInfo && chunk.DebugDatabase.TryGetLocalName(slot, out var name))
                {
                    localName = name;
                    annotation = name;
                }

                operands.Add(new InstructionOperand(
                    OperandKind.LocalSlot,
                    slot,
                    slot.ToString(),
                    localName
                ));
                break;
            }

            case OpCode.SETLOCAL_B:
            case OpCode.GETLOCAL_B:
            {
                int slot = reader.ReadByte();
                string? localName = null;
                if (chunk.HasDebugInfo && chunk.DebugDatabase.TryGetLocalName(slot, out var name))
                {
                    localName = name;
                    annotation = name;
                }

                operands.Add(new InstructionOperand(
                    OperandKind.LocalSlot,
                    slot,
                    slot.ToString(),
                    localName
                ));
                break;
            }

            case OpCode.SETLOCAL:
            case OpCode.GETLOCAL:
            {
                var slot = reader.ReadInt32();
                string? localName = null;
                if (chunk.HasDebugInfo && chunk.DebugDatabase.TryGetLocalName(slot, out var name))
                {
                    localName = name;
                    annotation = name;
                }

                operands.Add(new InstructionOperand(
                    OperandKind.LocalSlot,
                    slot,
                    slot.ToString(),
                    localName
                ));
                break;
            }

            case OpCode.SETARG_0:
            case OpCode.SETARG_1:
            case OpCode.SETARG_2:
            case OpCode.SETARG_3:
            case OpCode.GETARG_0:
            case OpCode.GETARG_1:
            case OpCode.GETARG_2:
            case OpCode.GETARG_3:
            {
                var slot = opCode switch
                {
                    OpCode.SETARG_0 or OpCode.GETARG_0 => 0,
                    OpCode.SETARG_1 or OpCode.GETARG_1 => 1,
                    OpCode.SETARG_2 or OpCode.GETARG_2 => 2,
                    _ => 3
                };

                string? paramName = null;
                if (chunk.HasDebugInfo && chunk.DebugDatabase.TryGetParameterName(slot, out var name))
                {
                    paramName = name;
                    annotation = name;
                }
                else if (chunk.IsSelfAware && slot == 0)
                {
                    paramName = "self";
                    annotation = "self";
                }

                operands.Add(new InstructionOperand(
                    OperandKind.ArgumentSlot,
                    slot,
                    slot.ToString(),
                    paramName
                ));
                break;
            }

            case OpCode.SETARG_B:
            case OpCode.GETARG_B:
            {
                int slot = reader.ReadByte();
                string? paramName = null;
                if (chunk.HasDebugInfo && chunk.DebugDatabase.TryGetParameterName(slot, out var name))
                {
                    paramName = name;
                    annotation = name;
                }
                else if (chunk.IsSelfAware && slot == 0)
                {
                    paramName = "self";
                    annotation = "self";
                }

                operands.Add(new InstructionOperand(
                    OperandKind.ArgumentSlot,
                    slot,
                    slot.ToString(),
                    paramName
                ));
                break;
            }

            case OpCode.SETARG:
            case OpCode.GETARG:
            {
                var slot = reader.ReadInt32();
                string? paramName = null;
                if (chunk.HasDebugInfo && chunk.DebugDatabase.TryGetParameterName(slot, out var name))
                {
                    paramName = name;
                    annotation = name;
                }
                else if (chunk.IsSelfAware && slot == 0)
                {
                    paramName = "self";
                    annotation = "self";
                }

                operands.Add(new InstructionOperand(
                    OperandKind.ArgumentSlot,
                    slot,
                    slot.ToString(),
                    paramName
                ));
                break;
            }

            case OpCode.GETCLOSURE_0:
            case OpCode.GETCLOSURE_1:
            case OpCode.GETCLOSURE_2:
            case OpCode.GETCLOSURE_3:
            case OpCode.SETCLOSURE_0:
            case OpCode.SETCLOSURE_1:
            case OpCode.SETCLOSURE_2:
            case OpCode.SETCLOSURE_3:
            {
                var slot = opCode switch
                {
                    OpCode.GETCLOSURE_0 or OpCode.SETCLOSURE_0 => 0,
                    OpCode.GETCLOSURE_1 or OpCode.SETCLOSURE_1 => 1,
                    OpCode.GETCLOSURE_2 or OpCode.SETCLOSURE_2 => 2,
                    _ => 3
                };

                annotation = GetClosureInfo(chunk, slot);
                operands.Add(new InstructionOperand(
                    OperandKind.ClosureSlot,
                    slot,
                    slot.ToString(),
                    annotation
                ));
                break;
            }

            case OpCode.GETCLOSURE_B:
            case OpCode.SETCLOSURE_B:
            {
                int slot = reader.ReadByte();
                annotation = GetClosureInfo(chunk, slot);
                operands.Add(new InstructionOperand(
                    OperandKind.ClosureSlot,
                    slot,
                    slot.ToString(),
                    annotation
                ));
                break;
            }

            case OpCode.GETCLOSURE:
            case OpCode.SETCLOSURE:
            {
                var slot = reader.ReadInt32();
                annotation = GetClosureInfo(chunk, slot);
                operands.Add(new InstructionOperand(
                    OperandKind.ClosureSlot,
                    slot,
                    slot.ToString(),
                    annotation
                ));
                break;
            }

            case OpCode.FJMP:
            case OpCode.TJMP:
            case OpCode.JUMP:
            case OpCode.NJMP:
            case OpCode.VJMP:
            {
                var target = reader.ReadInt32();
                targetOffset = target;
                jumpTargets.Add(target);

                var delta = target - startIp;
                var sign = delta >= 0 ? "+" : "";
                annotation = $"{sign}{delta} bytes";

                operands.Add(new InstructionOperand(
                    OperandKind.JumpTargetAddress,
                    target,
                    $"0x{target:X8}",
                    $"L_{target:X4}"
                ));
                break;
            }

            case OpCode.INVOKE:
            {
                var argCount = reader.ReadInt32();
                var isVariadic = reader.ReadByte();

                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateInt32,
                    argCount,
                    argCount.ToString()
                ));

                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateByte,
                    isVariadic,
                    isVariadic == 1 ? "VARARGS" : "EXACT"
                ));
                break;
            }

            case OpCode.SWITCH:
            {
                var defaultIp = reader.ReadInt32();
                var count = reader.ReadInt32();
                jumpTargets.Add(defaultIp);

                operands.Add(new InstructionOperand(
                    OperandKind.JumpTargetAddress,
                    defaultIp,
                    $"default: 0x{defaultIp:X8}",
                    $"L_{defaultIp:X4}"
                ));

                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateInt32,
                    count,
                    $"count: {count}"
                ));

                for (var i = 0; i < count; i++)
                {
                    var type = (DynamicValueType)reader.ReadByte();
                    string valStr;
                    
                    switch (type)
                    {
                        case DynamicValueType.Nil:
                            valStr = "nil";
                            break;
                        case DynamicValueType.Boolean:
                            valStr = reader.ReadBoolean().ToString().ToLowerInvariant();
                            break;
                        case DynamicValueType.Number:
                            valStr = reader.ReadDouble().ToString(CultureInfo.InvariantCulture);
                            break;
                        case DynamicValueType.String:
                            var strId = reader.ReadInt32();
                            var s = strId >= 0 && strId < chunk.StringPool.Count ? chunk.StringPool[strId] : null;
                            valStr = s != null ? Literalize(s) : "<null>";
                            break;
                        case DynamicValueType.TypeCode:
                            valStr = ((DynamicValueType)reader.ReadByte()).ToString();
                            break;
                        default:
                            valStr = $"<{type}>";
                            break;
                    }

                    var caseTargetIp = reader.ReadInt32();
                    jumpTargets.Add(caseTargetIp);

                    operands.Add(new InstructionOperand(
                        OperandKind.SwitchTableRef,
                        caseTargetIp,
                        $"case {valStr} => 0x{caseTargetIp:X8}",
                        $"L_{caseTargetIp:X4}"
                    ));
                }
                break;
            }

            case OpCode.XARGS:
            {
                var mode = reader.ReadByte();
                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateByte,
                    mode,
                    mode == 0 ? "ARRAY" : "UNPACKED"
                ));
                break;
            }

            case OpCode.TABCLN:
            {
                var isDeep = reader.ReadByte();
                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateByte,
                    isDeep,
                    isDeep == 1 ? "DEEP" : "SHALLOW"
                ));
                break;
            }

            case OpCode.YIELD:
            {
                var count = reader.ReadInt32();
                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateInt32,
                    count,
                    count.ToString()
                ));
                break;
            }

            case OpCode.NEXT:
            {
                var isKeyValue = reader.ReadInt32();
                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateInt32,
                    isKeyValue,
                    isKeyValue != 0 ? "KEY_VALUE" : "VALUE_ONLY"
                ));
                break;
            }

            case OpCode.ENTER:
            {
                var blockId = reader.ReadInt32();
                operands.Add(new InstructionOperand(
                    OperandKind.ProtectedBlockId,
                    blockId,
                    blockId.ToString()
                ));
                break;
            }

            case OpCode.ARRINIT:
            case OpCode.TABINIT:
            case OpCode.STRCAT:
            {
                var count = reader.ReadInt32();
                operands.Add(new InstructionOperand(
                    OperandKind.ImmediateInt32,
                    count,
                    count.ToString()
                ));
                break;
            }
        }

        var endIp = (int)reader.IP;
        var length = endIp - startIp;
        var rawBytes = code.AsMemory(startIp, length);

        StatementDebugRecord? stmt = null;
        var line = -1;
        if (chunk.HasDebugInfo)
        {
            chunk.DebugDatabase.TryGetStatementAtIP(startIp, out stmt);
            line = chunk.DebugDatabase.GetLineForIP(startIp);
        }

        instruction = new Instruction
        {
            Offset = startIp,
            Length = length,
            OpCode = opCode,
            RawBytes = rawBytes,
            Mnemonic = opCode.ToString(),
            Operands = operands,
            TargetOffset = targetOffset,
            SymbolicAnnotation = annotation,
            Statement = stmt,
            Line = line
        };

        return true;
    }

    private static string GetClosureInfo(Chunk chunk, int closureId)
    {
        if (closureId < 0 || closureId >= chunk.Closures.Count)
        {
            return $"closure {closureId}";
        }

        var closure = chunk.Closures[closureId];
        var closureType = closure.Type switch
        {
            ClosureType.Parameter => "parameter",
            ClosureType.Local => "local",
            ClosureType.Closure => "closure",
            _ => "???"
        };

        var names = new List<string>();
        var closureFrom = chunk;
        var nesting = closure.NestingLevel;

        while (closureFrom?.Parent != null)
        {
            closureFrom = closureFrom.Parent;
            names.Add($"'{closureFrom?.Name ?? "???"}'");
        }

        for (var i = 0; i < nesting - 1; i++)
        {
            if (names.Count > 0)
            {
                names.RemoveAt(0);
            }
        }

        names.Reverse();

        var scopePath = names.Count > 0 ? " in " + string.Join(" -> ", names) : string.Empty;
        return $"{closureType} {closure.EnclosedId}{scopePath}";
    }

    private static string Literalize(string input)
    {
        var literal = new StringBuilder(input.Length + 2);
        literal.Append('"');

        foreach (var c in input)
        {
            switch (c)
            {
                case '\"': literal.Append("\\\""); break;
                case '\\': literal.Append(@"\\"); break;
                case '\0': literal.Append(@"\0"); break;
                case '\a': literal.Append(@"\a"); break;
                case '\b': literal.Append(@"\b"); break;
                case '\f': literal.Append(@"\f"); break;
                case '\n': literal.Append(@"\n"); break;
                case '\r': literal.Append(@"\r"); break;
                case '\t': literal.Append(@"\t"); break;
                case '\v': literal.Append(@"\v"); break;
                default:
                    if (c >= 0x20 && c <= 0x7E)
                    {
                        literal.Append(c);
                    }
                    else
                    {
                        literal.Append(@"\u");
                        literal.Append(((int)c).ToString("x4"));
                    }
                    break;
            }
        }

        literal.Append('"');
        return literal.ToString();
    }
}
