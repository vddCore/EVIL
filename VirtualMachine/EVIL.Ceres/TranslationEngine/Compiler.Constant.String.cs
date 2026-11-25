namespace EVIL.Ceres.TranslationEngine;

using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.StringInterpolation;

public partial class Compiler
{
    protected override void Visit(StringConstant stringConstant)
    {
        if (stringConstant.IsInterpolated)
        {
            var segments = InterpolatedStringParser.ParseSegments(stringConstant);

            var hasExpressions = segments.Any(s => s.Kind == InterpolatedSegmentKind.Expression);
            if (!hasExpressions)
            {
                var combinedText = string.Concat(segments.Select(s => s.Text));
                var id = Chunk.StringPool.FetchOrAdd(combinedText);
                
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)id
                );

                return;
            }

            var meaningfulSegments = segments.Where(
                s => s.Kind != InterpolatedSegmentKind.Literal 
                  || s.Text.Length > 0
            ).ToList();
            
            if (meaningfulSegments.Count == 0)
            {
                var id = Chunk.StringPool.FetchOrAdd(string.Empty);
                Chunk.CodeGenerator.Emit(OpCode.LDSTR, (int)id);
                return;
            }

            if (meaningfulSegments.Count == 1)
            {
                var segment = meaningfulSegments[0];
                if (segment.Kind == InterpolatedSegmentKind.Literal)
                {
                    var id = Chunk.StringPool.FetchOrAdd(segment.Text);
                    
                    Chunk.CodeGenerator.Emit(
                        OpCode.LDSTR, 
                        (int)id
                    );
                }
                else
                {
                    Visit(segment.Expression!);
                    Chunk.CodeGenerator.Emit(OpCode.TOSTRING);
                }

                return;
            }

            foreach (var segment in meaningfulSegments)
            {
                if (segment.Kind == InterpolatedSegmentKind.Literal)
                {
                    var id = Chunk.StringPool.FetchOrAdd(segment.Text);
                    
                    Chunk.CodeGenerator.Emit(
                        OpCode.LDSTR,
                        (int)id
                    );
                }
                else
                {
                    Visit(segment.Expression!);
                    Chunk.CodeGenerator.Emit(OpCode.TOSTRING);
                }
            }

            Chunk.CodeGenerator.Emit(OpCode.STRCAT, meaningfulSegments.Count);
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