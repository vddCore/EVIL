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

            var (line, col) = (-1, -1);
            var isLocalDefinition = false;
            if (CurrentToken == Token.Loc)
            {
                (line, col) = Match(Token.Loc);
                isLocalDefinition = true;
                
                Match(Token.Fn);
            }
            else
            {
                (line, col) = Match(Token.Fn);
            }

            if (_functionDescent > 0 && !isLocalDefinition)
            {
                throw new ParserException(
                    "Global function definitions may only appear outside of any other functions.",
                    (line, col)
                );
            }
            
            var functionIdentifier = Identifier();
            var parameterList = ParameterList();

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

            return new FnStatement(
                functionIdentifier,
                parameterList,
                statement,
                attributes,
                isLocalDefinition
            ) { Line = line, Column = col };
        }
    }
}