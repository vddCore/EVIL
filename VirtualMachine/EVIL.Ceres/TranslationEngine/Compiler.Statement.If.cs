namespace EVIL.Ceres.TranslationEngine;

using System.Collections.Generic;
using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(IfStatement ifStatement)
    {
        var statementEnd = Chunk.CreateLabel();
            
        for (var i = 0; i < ifStatement.Conditions.Count; i++)
        {
            var statementStart = Chunk.CreateLabel();
            var caseEnd = Chunk.CreateLabel();
            var currentConditionExpression = ifStatement.Conditions[i];
                
            if (currentConditionExpression is BinaryExpression orBex
                && orBex.Type == BinaryOperationType.LogicalOr)
            {
                var stack = new Stack<Expression>();
                stack.Push(orBex.Right);
                stack.Push(orBex.Left);

                while (stack.Count != 0)
                {
                    var node = stack.Pop();

                    if (node is BinaryExpression innerBex && innerBex.Type == BinaryOperationType.LogicalOr)
                    {
                        stack.Push(innerBex.Right);
                        stack.Push(innerBex.Left);
                    }
                    else
                    {
                        Visit(node);
                        Chunk.CodeGenerator.Emit(
                            OpCode.TJMP,
                            statementStart
                        );
                    }
                }
                    
                Chunk.CodeGenerator.Emit(
                    OpCode.JUMP,
                    caseEnd
                );
            }
            else
            {
                Visit(currentConditionExpression);
                Chunk.CodeGenerator.Emit(
                    OpCode.FJMP,
                    caseEnd
                );
            }

            Chunk.UpdateLabel(statementStart, Chunk.CodeGenerator.IP);

            Visit(ifStatement.Statements[i]);
            Chunk.CodeGenerator.Emit(
                OpCode.JUMP,
                statementEnd
            );

            Chunk.UpdateLabel(caseEnd, Chunk.CodeGenerator.IP);
        }

        if (ifStatement.ElseBranch != null)
        {
            Visit(ifStatement.ElseBranch);
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.NOOP);
        }

        Chunk.UpdateLabel(
            statementEnd,
            Chunk.CodeGenerator.IP
        );
    }
}