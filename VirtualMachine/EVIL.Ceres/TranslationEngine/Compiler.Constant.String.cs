namespace EVIL.Ceres.TranslationEngine;

using System.Linq;
using System.Text.RegularExpressions;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

public partial class Compiler
{
    private static Regex _stringSubstitutionRegex = new(@"(?<substitution>\$(?<symbol_name>[\p{L}_]([\p{L}\p{Nd}_]+)?))");

    public override void Visit(StringConstant stringConstant)
    {
        if (stringConstant.IsInterpolated)
        {
            var additionCount = 0;
            var templateString = stringConstant.Value;
            var matches = _stringSubstitutionRegex.Matches(templateString);

            if (!matches.Any())
            {
                var id = Chunk.StringPool.FetchOrAdd(stringConstant.Value);
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)id
                );

                return;
            }
                
            var endOfLastGroup = -1;
        
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var group = match.Groups["substitution"];
                var symbolName = match.Groups["symbol_name"].Value;
                var before = "";
                endOfLastGroup = group.Index + group.Length;
            
                if (i == 0)
                {
                    before = templateString.Substring(0, group.Index);
                }
                else
                {
                    var prevMatch = matches[i - 1];
                    var prevGroup = prevMatch.Groups["substitution"];
                    var endOfPreviousGroup = prevGroup.Index + prevGroup.Length;
                    before = templateString.Substring(endOfPreviousGroup, group.Index - endOfPreviousGroup);
                }

                if (before.Length > 0)
                {
                    var id = Chunk.StringPool.FetchOrAdd(before);
                    Chunk.CodeGenerator.Emit(
                        OpCode.LDSTR,
                        (int)id
                    );
                }

                EmitVarGet(symbolName);
                Chunk.CodeGenerator.Emit(OpCode.TOSTRING);
                additionCount++;

                if (before.Length > 0)
                {
                    Chunk.CodeGenerator.Emit(OpCode.ADD);
                }
            }

            var tail = templateString.Substring(endOfLastGroup);
            if (tail.Length > 0)
            {
                var id = Chunk.StringPool.FetchOrAdd(tail);
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)id
                );
                Chunk.CodeGenerator.Emit(OpCode.ADD);
            }

            for (var i = 0; i < additionCount - 1; i++)
            {
                Chunk.CodeGenerator.Emit(OpCode.ADD);
            }
        }
        else
        {
            var id = Chunk.StringPool.FetchOrAdd(stringConstant.Value);
            Chunk.CodeGenerator.Emit(
                OpCode.LDSTR,
                (int)id
            );
        }
    }
}