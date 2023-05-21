using System;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }

        public BinaryOperationType Type { get; }

        public BinaryExpression(Expression left, Expression right, BinaryOperationType type)
        {
            Left = left;
            Right = right;

            Type = type;

            Reparent(Left, Right);
        }

        public override Expression Reduce()
        {
            Expression ret = this;

            var left = Left.Reduce();
            var right = Right.Reduce();
            
            if (left is NumberConstant lnc && right is NumberConstant rnc)
            {
                switch (Type)
                {
                    case BinaryOperationType.Divide when rnc.Value != 0:
                        ret = new NumberConstant(lnc.Value / rnc.Value);
                        break;
                    
                    case BinaryOperationType.Modulo when rnc.Value != 0:
                        ret = new NumberConstant(lnc.Value - rnc.Value * Math.Floor(lnc.Value / rnc.Value));
                        break;

                    case BinaryOperationType.Equal:
                        ret = new NumberConstant(lnc.Value == rnc.Value ? 1 : 0);
                        break;

                    case BinaryOperationType.Greater:
                        ret =  new NumberConstant(lnc.Value >= rnc.Value ? 1 : 0);
                        break;

                    case BinaryOperationType.Less:
                        ret =  new NumberConstant(lnc.Value < rnc.Value ? 1 : 0);
                        break;

                    case BinaryOperationType.Subtract:
                        ret = new NumberConstant(lnc.Value - rnc.Value);
                        break;

                    case BinaryOperationType.Multiply:
                        ret = new NumberConstant(lnc.Value * rnc.Value);
                        break;

                    case BinaryOperationType.Add:
                        ret = new NumberConstant(lnc.Value + rnc.Value);
                        break;

                    case BinaryOperationType.BitwiseAnd:
                        ret = new NumberConstant((long)lnc.Value & (long)rnc.Value);
                        break;

                    case BinaryOperationType.BitwiseOr:
                        ret = new NumberConstant((long)lnc.Value | (long)rnc.Value);
                        break;

                    case BinaryOperationType.BitwiseXor:
                        ret = new NumberConstant((long)lnc.Value ^ (long)rnc.Value);
                        break;

                    case BinaryOperationType.LogicalAnd:
                        ret = new NumberConstant(lnc.Value != 0 && rnc.Value != 0 ? 1 : 0);
                        break;

                    case BinaryOperationType.LogicalOr:
                        ret = new NumberConstant(lnc.Value != 0 || rnc.Value != 0 ? 1 : 0);
                        break;

                    case BinaryOperationType.NotEqual:
                        ret = new NumberConstant(lnc.Value != rnc.Value ? 1 : 0);
                        break;
                    
                    case BinaryOperationType.ShiftLeft:
                        ret = new NumberConstant((long)lnc.Value << (int)rnc.Value);
                        break;
                    
                    case BinaryOperationType.ShiftRight:
                        ret = new NumberConstant((long)lnc.Value >> (int)rnc.Value);
                        break;
                    
                    case BinaryOperationType.GreaterOrEqual:
                        ret = new NumberConstant(lnc.Value >= rnc.Value ? 1 : 0);
                        break;
                    
                    case BinaryOperationType.LessOrEqual:
                        ret = new NumberConstant(lnc.Value <= rnc.Value ? 1 : 0);
                        break;
                }
            }

            if (ret != this)
            {
                ret.Line = Line;
                ret.Column = Column;
            }
            
            return ret;
        }
    }
}