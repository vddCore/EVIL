using System.Globalization;
using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ConstantExpression Constant()
        {
            var token = CurrentToken;

            switch (token.Type)
            {
                case TokenType.Minus:
                case TokenType.Number:
                {
                    var (line, col) = (0, 0);
                    var sign = 1;
                    if (token.Type == TokenType.Minus)
                    {
                        sign = -1;
                        (line, col) = Match(Token.Minus);
                        token = CurrentToken;
                        Match(Token.Number);
                    }
                    else
                    {
                        (line, col) = Match(Token.Number);
                    }

                    return new NumberConstant(sign * double.Parse(token.Value))
                        { Line = line, Column = col };
                }
                case TokenType.NaN:
                {
                    var (line, col) = Match(Token.NaN);

                    return new NumberConstant(double.NaN)
                        { Line = line, Column = col };
                }
                case TokenType.Infinity:
                {
                    var (line, col) = Match(Token.Infinity);

                    return new NumberConstant(double.PositiveInfinity)
                        { Line = line, Column = col };
                }
                case TokenType.HexInteger:
                {
                    var (line, col) = Match(Token.HexInteger);

                    return new NumberConstant(long.Parse(token.Value, NumberStyles.HexNumber))
                        { Line = line, Column = col };
                }
                case TokenType.True:
                {
                    var (line, col) = Match(Token.True);

                    return new BooleanConstant(true)
                        { Line = line, Column = col };
                }
                case TokenType.False:
                {
                    var (line, col) = Match(Token.False);

                    return new BooleanConstant(false)
                        { Line = line, Column = col };
                }
                case TokenType.String:
                {
                    var (line, col) = Match(Token.String);

                    return new StringConstant(token.Value)
                        { Line = line, Column = col };
                }
                case TokenType.Nil:
                {
                    var (line, col) = Match(Token.Nil);

                    return new NilConstant
                        { Line = line, Column = col };
                }
                case TokenType.NilTypeCode:
                {
                    var (line, col) = Match(Token.NilTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.Nil)
                        { Line = line, Column = col };
                }
                case TokenType.NumberTypeCode:
                {
                    var (line, col) = Match(Token.NumberTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.Number)
                        { Line = line, Column = col };
                }
                case TokenType.StringTypeCode:
                {
                    var (line, col) = Match(Token.StringTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.String)
                        { Line = line, Column = col };
                }
                case TokenType.BooleanTypeCode:
                {
                    var (line, col) = Match(Token.BooleanTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.Boolean)
                        { Line = line, Column = col };
                }
                case TokenType.TableTypeCode:
                {
                    var (line, col) = Match(Token.TableTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.Table)
                        { Line = line, Column = col };
                }
                case TokenType.FiberTypeCode:
                {
                    var (line, col) = Match(Token.FiberTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.Fiber)
                        { Line = line, Column = col };
                }
                case TokenType.ChunkTypeCode:
                {
                    var (line, col) = Match(Token.ChunkTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.Chunk)
                        { Line = line, Column = col };
                }
                case TokenType.TypeCodeTypeCode:
                {
                    var (line, col) = Match(Token.TypeCodeTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.Type)
                        { Line = line, Column = col };
                }
                case TokenType.NativeFunctionTypeCode:
                {
                    var (line, col) = Match(Token.NativeFunctionTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.NativeFunction)
                        { Line = line, Column = col };
                }
                case TokenType.NativeObjectTypeCode:
                {
                    var (line, col) = Match(Token.NativeObjectTypeCode);
                    
                    return new TypeCodeConstant(DynamicValueType.NativeObject)
                        { Line = line, Column = col };
                }
                default:
                {
                    throw new ParserException(
                        $"Unexpected token {token}",
                        (_lexer.State.Line, _lexer.State.Column)
                    );
                }
            }
        }
    }
}