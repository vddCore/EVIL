namespace EVIL.Core.Grammar.StringInterpolation;

using System;
using System.Collections.Generic;
using System.Text;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public static class InterpolatedStringParser
{
    public static List<InterpolatedSegment> ParseSegments(StringConstant stringConstant, bool throwOnError = true)
    {
        return ParseSegments(
            stringConstant.Value,
            stringConstant.Line,
            stringConstant.Column,
            throwOnError
        );
    }

    public static List<InterpolatedSegment> ParseSegments(
        string templateString,
        int line = 0,
        int column = 0,
        bool throwOnError = true)
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
                        while (j + 1 < templateString.Length && !(templateString[j] == '*' && templateString[j + 1] == '/'))
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
                        segments.Add(
                            new InterpolatedSegment(
                                InterpolatedSegmentKind.Literal,
                                currentLiteral.ToString()
                            )
                        );
                        
                        currentLiteral.Clear();
                    }

                    try
                    {
                        var parser = new Parser();
                        var exprNode = parser.ParseExpression(exprSource);
                        
                        segments.Add(
                            new InterpolatedSegment(
                                InterpolatedSegmentKind.Expression,
                                exprSource, 
                                exprNode
                            )
                        );
                    }
                    catch (Exception) when (!throwOnError)
                    {
                        segments.Add(
                            new InterpolatedSegment(
                                InterpolatedSegmentKind.Literal,
                                exprSource
                            )
                        );
                    }
                    catch (ParserException pe)
                    {
                        throw new ParserException(
                            $"Error in interpolated expression: {pe.Message}",
                            (line, column)
                        );
                    }
                    catch (LexerException le)
                    {
                        throw new ParserException(
                            $"Error in interpolated expression: {le.Message}",
                            (line, column)
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
                    segments.Add(
                        new InterpolatedSegment(
                            InterpolatedSegmentKind.Literal, 
                            currentLiteral.ToString()
                        )
                    );
                    
                    currentLiteral.Clear();
                }

                var exprNode = new SymbolReferenceExpression(identifier)
                {
                    Line = line,
                    Column = column
                };
                
                segments.Add(
                    new InterpolatedSegment(
                        InterpolatedSegmentKind.Expression,
                        identifier,
                        exprNode
                    )
                );
                
                i = j;
                continue;
            }

            currentLiteral.Append(templateString[i]);
            i++;
        }

        if (currentLiteral.Length > 0)
        {
            segments.Add(
                new InterpolatedSegment(
                    InterpolatedSegmentKind.Literal,
                    currentLiteral.ToString()
                )
            );
        }

        return segments;
    }
}