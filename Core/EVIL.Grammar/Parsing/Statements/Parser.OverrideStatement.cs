using System.Collections.Generic;
using System.Linq;
using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private static readonly Dictionary<Token, TableOverride> _validOverrideTokens = new()
        {
            { Token.AddOverride, TableOverride.Add },
            { Token.SubOverride, TableOverride.Subtract },
            { Token.MulOverride, TableOverride.Multiply },
            { Token.DivOverride, TableOverride.Divide },
            { Token.ModOverride, TableOverride.Modulo },
            { Token.ShlOverride, TableOverride.ShiftLeft },
            { Token.ShrOverride, TableOverride.ShiftRight },
            { Token.AnegOverride, TableOverride.ArithmeticNegate },
            { Token.IncOverride, TableOverride.Increment },
            { Token.DecOverride, TableOverride.Decrement },
            { Token.LnotOverride, TableOverride.LogicalNot },
            { Token.LorOverride, TableOverride.LogicalOr },
            { Token.LandOverride, TableOverride.LogicalAnd },
            { Token.BorOverride, TableOverride.BitwiseOr },
            { Token.BxorOverride, TableOverride.BitwiseXor },
            { Token.BandOverride, TableOverride.BitwiseAnd },
            { Token.BnotOverride, TableOverride.BitwiseNot },
            { Token.DeqOverride, TableOverride.DeepEqual },
            { Token.DneOverride, TableOverride.DeepNotEqual },
            { Token.EqOverride, TableOverride.Equal },
            { Token.NeOverride, TableOverride.NotEqual },
            { Token.GtOverride, TableOverride.GreaterThan },
            { Token.LtOverride, TableOverride.LessThan },
            { Token.GteOverride, TableOverride.GreaterThanOrEqual },
            { Token.LteOverride, TableOverride.LessThanOrEqual },
            { Token.LenOverride, TableOverride.Length },
            { Token.ToNumOverride, TableOverride.ToNumber },
            { Token.ToStrOverride, TableOverride.ToString },
            { Token.InvokeOverride, TableOverride.Invoke },
            { Token.GetOverride, TableOverride.Get },
            { Token.SetOverride, TableOverride.Set },
            { Token.ExistsOverride, TableOverride.Exists }
        };
        
        private OverrideStatement OverrideStatement()
        {
            var (line, col) = Match(Token.Override);
            Match(Token.LParenthesis);
            var indexable = AssignmentExpression();
            Match(Token.RParenthesis);
            Match(Token.DoubleColon);

            if (!_validOverrideTokens.TryGetValue(CurrentToken, out var tableOverride))
            {
                var expectedList = _validOverrideTokens.Keys.Select(x => $"`{x.Value}'");

                throw new ParserException(
                    $"Expected one of {string.Join(',', expectedList)}; found `{CurrentToken.Value}'.",
                    (_lexer.State.Line, _lexer.State.Column)
                );
            }

            Match(CurrentToken);
            
            ParameterList? parameterList = null;
            if (CurrentToken == Token.LParenthesis)
            {
                parameterList = ParameterList();
            }
            
            Statement statement;
            if (CurrentToken == Token.LBrace)
            {
                statement = FunctionDescent(BlockStatement);
            }
            else if (CurrentToken == Token.RightArrow)
            {
                statement = FunctionDescent(ExpressionBodyStatement);
                Match(Token.Semicolon);
            }
            else
            {
                throw new ParserException(
                    $"Expected '{{' or '->', found '{CurrentToken.Value}',",
                    (_lexer.State.Line, _lexer.State.Column)
                );
            }

            return new OverrideStatement(
                indexable,
                tableOverride,
                parameterList,
                statement
            ) { Line = line, Column = col };
        }
    }
}