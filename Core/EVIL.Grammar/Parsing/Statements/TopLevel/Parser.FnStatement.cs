using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements.TopLevel;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FnStatement FnStatement()
        {
            var attributes = new List<AttributeNode>();

            while (CurrentToken == Token.AttributeList)
            {
                attributes.AddRange(AttributeList().Attributes);
            }
            
            var (line, col) = Match(Token.Fn);
            var functionIdentifier = Identifier();
            var parameterList = ParameterList();

            Statement statement;
            if (CurrentToken == Token.LBrace)
            {
                statement = FunctionDescent(BlockStatement);
            }
            else if (CurrentToken == Token.RightArrow)
            {
                statement = FunctionDescent(ExpressionBody);
                Match(Token.Semicolon);
            }
            else
            {
                throw new ParserException(
                    $"Expected '{{' or '->', found '{CurrentToken.Value}',",
                    (_lexer.State.Line, _lexer.State.Column)
                );
            }

            return new FnStatement(
                functionIdentifier,
                parameterList,
                statement,
                attributes
            ) { Line = line, Column = col };
        }
    }
}