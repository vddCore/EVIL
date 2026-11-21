namespace EVIL.Ceres.TranslationEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Compiler
{
    private enum InterpolatedSegmentKind
    {
        Literal,
        Expression
    }

    private sealed class InterpolatedSegment
    {
        public InterpolatedSegmentKind Kind { get; }
        public string Text { get; }
        public Expression? Expr { get; }

        public InterpolatedSegment(InterpolatedSegmentKind kind, string text, Expression? expr = null)
        {
            Kind = kind;
            Text = text;
            Expr = expr;
        }
    }

    private List<InterpolatedSegment> ParseInterpolatedSegments(string templateString, StringConstant stringConstant)
    {
        var segments = new List<InterpolatedSegment>();
        var currentLiteral = new StringBuilder();
        var i = 0;

        while (i < templateString.Length)
        {
            if (templateString[i] == '\\' && i + 1 < templateString.Length && templateString[i + 1] == '$')
            {
                currentLiteral.Append('$');
                i += 2;
                continue;
            }

            if (templateString[i] == '$' && i + 1 < templateString.Length && templateString[i + 1] == '{')
            {
                var exprStart = i + 2;
                var braceDepth = 1;
                var j = exprStart;

                while (j < templateString.Length && braceDepth > 0)
                {
                    if (templateString[j] == '\\')
                    {
                        j++;
                        
                        if (j < templateString.Length)
                        {
                            j++;
                        }
                        
                        continue;
                    }

                    if (templateString[j] == '/' && j + 1 < templateString.Length && templateString[j + 1] == '/')
                    {
                        j += 2;
                        
                        while (j < templateString.Length && templateString[j] != '\n')
                        {
                            j++;
                        }

                        continue;
                    }

                    if (templateString[j] == '/' && j + 1 < templateString.Length && templateString[j + 1] == '*')
                    {
                        j += 2;
                        while (j + 1 < templateString.Length &&
                               !(templateString[j] == '*' && templateString[j + 1] == '/'))
                        {
                            j++;
                        }

                        if (j + 1 < templateString.Length)
                        {
                            j += 2;
                        }

                        continue;
                    }

                    if (templateString[j] == '"' || templateString[j] == '\'')
                    {
                        var quote = templateString[j];
                        j++;
                        
                        while (j < templateString.Length && templateString[j] != quote)
                        {
                            if (templateString[j] == '\\')
                            {
                                j++;
                            }

                            if (j < templateString.Length)
                            {
                                j++;
                            }
                        }

                        if (j < templateString.Length && templateString[j] == quote)
                        {
                            j++;
                        }

                        continue;
                    }

                    if (templateString[j] == '{')
                    {
                        braceDepth++;
                        j++;
                        
                        continue;
                    }

                    if (templateString[j] == '}')
                    {
                        braceDepth--;

                        if (braceDepth == 0)
                        {
                            break;
                        }
                    }

                    j++;
                }

                if (braceDepth == 0)
                {
                    var exprSource = templateString.Substring(exprStart, j - exprStart);
                    if (currentLiteral.Length > 0)
                    {
                        segments.Add(new InterpolatedSegment(InterpolatedSegmentKind.Literal, currentLiteral.ToString()));
                        currentLiteral.Clear();
                    }

                    try
                    {
                        var parser = new Parser();
                        var exprNode = parser.ParseExpression(exprSource);
                        segments.Add(new InterpolatedSegment(InterpolatedSegmentKind.Expression, exprSource, exprNode));
                    }
                    catch (ParserException pe)
                    {
                        throw new ParserException(
                            $"Error in interpolated expression: {pe.Message}",
                            (stringConstant.Line, stringConstant.Column)
                        );
                    }
                    catch (LexerException le)
                    {
                        throw new ParserException(
                            $"Error in interpolated expression: {le.Message}",
                            (stringConstant.Line, stringConstant.Column)
                        );
                    }

                    i = j + 1; /* skip closing '}' */
                    continue;
                }
            }

            if (templateString[i] == '$' && i + 1 < templateString.Length && (char.IsLetter(templateString[i + 1]) || templateString[i + 1] == '_'))
            {
                var idStart = i + 1;
                var j = idStart;
                
                while (j < templateString.Length && (char.IsLetterOrDigit(templateString[j]) || templateString[j] == '_'))
                {
                    j++;
                }

                var identifier = templateString.Substring(idStart, j - idStart);
                if (currentLiteral.Length > 0)
                {
                    segments.Add(new InterpolatedSegment(InterpolatedSegmentKind.Literal, currentLiteral.ToString()));
                    currentLiteral.Clear();
                }

                var exprNode = new SymbolReferenceExpression(identifier);
                segments.Add(new InterpolatedSegment(InterpolatedSegmentKind.Expression, identifier, exprNode));
                i = j;
                
                continue;
            }

            currentLiteral.Append(templateString[i]);
            i++;
        }

        if (currentLiteral.Length > 0)
        {
            segments.Add(new InterpolatedSegment(InterpolatedSegmentKind.Literal, currentLiteral.ToString()));
        }

        return segments;
    }

    protected override void Visit(StringConstant stringConstant)
    {
        if (stringConstant.IsInterpolated)
        {
            var segments = ParseInterpolatedSegments(stringConstant.Value, stringConstant);

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

            var isFirst = true;
            foreach (var segment in segments)
            {
                if (segment.Kind == InterpolatedSegmentKind.Literal)
                {
                    if (segment.Text.Length == 0)
                    {
                        continue;
                    }

                    var id = Chunk.StringPool.FetchOrAdd(segment.Text);
                    Chunk.CodeGenerator.Emit(
                        OpCode.LDSTR,
                        (int)id
                    );
                }
                else
                {
                    Visit(segment.Expr!);
                    Chunk.CodeGenerator.Emit(OpCode.TOSTRING);
                }

                if (!isFirst)
                {
                    Chunk.CodeGenerator.Emit(OpCode.ADD);
                }
                else
                {
                    isFirst = false;
                }
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